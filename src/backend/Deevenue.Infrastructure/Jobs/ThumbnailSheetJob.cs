using Deevenue.Domain;
using Deevenue.Domain.Media;
using Deevenue.Domain.Thumbnails;
using Deevenue.Infrastructure.Db;
using Deevenue.Infrastructure.MediaProcessing;
using FFMpegCore;
using FFMpegCore.Arguments;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Deevenue.Infrastructure.Jobs;

internal record ThumbnailSheetJobParameters(Guid SheetId, Guid MediumId, int Count);

internal class ThumbnailSheetJobFactory : IDeevenueJobFactory<ThumbnailSheetJob, ThumbnailSheetJobParameters>
{
    public static readonly IDeevenueJobFactory<ThumbnailSheetJob, ThumbnailSheetJobParameters> Instance
        = new ThumbnailSheetJobFactory();
    private ThumbnailSheetJobFactory() { }

    public string CreateIdentity(ThumbnailSheetJobParameters parameters) => parameters.SheetId.ToString();

    public JobDataMap StoreParameters(ThumbnailSheetJobParameters parameters)
    {
        var result = new JobDataMap();
        result.Put(ThumbnailSheetJob.MediumIdParamName, parameters.MediumId);
        result.Put(ThumbnailSheetJob.SheetIdParamName, parameters.SheetId);
        result.Put(ThumbnailSheetJob.CountParamName, parameters.Count);
        return result;
    }
}

[JobKindName("Thumbnail sheet job")]
internal class ThumbnailSheetJob(
    IJobResultRepository jobRepository,
    IBucketStorage storage,
    IMediumStorage mediumStorage,
    IVideoAnalysis videoAnalysis,
    IThumbnailSheetStorage thumbnailSheetStorage,
    IThumbnailSheetRepository repository,
    ILogger<ThumbnailSheetJob> logger) : DeevenueJobBase(jobRepository)
{
    public const string MediumIdParamName = "mediumId";
    public const string SheetIdParamName = "sheetId";
    public const string CountParamName = "count";

    public override JobSummaryData GetSummaryData(IJobExecutionContext context)
        => new(context.JobDetail.JobDataMap.GetGuid(MediumIdParamName));

    protected override async Task ActuallyExecute(IJobExecutionContext context)
    {
        var mediumId = context.JobDetail.JobDataMap.GetGuid(MediumIdParamName);
        var sheetId = context.JobDetail.JobDataMap.GetGuid(SheetIdParamName);
        var count = context.JobDetail.JobDataMap.GetInt(CountParamName);

        count = Math.Clamp(count, 0, 100);

        var mediumFile = await mediumStorage.GetAsync(mediumId);

        if (mediumFile == null)
        {
            logger.LogWarning("Tried to create thumbnail sheet for non-existant medium {id}", mediumId);
            return;
        }

        using var memoryStream = new MemoryStream(mediumFile.Bytes);
        using var inputFile = TemporaryVideoFile.From(memoryStream);

        var analysis = await videoAnalysis.ForAsync(inputFile);

        var lengthInMs = (int)Math.Floor(analysis.Duration.TotalMilliseconds);
        var isLandscape = analysis.PrimaryVideoStream!.Width > analysis.PrimaryVideoStream!.Height;

        using var parentOutputDirectory = new TemporaryDirectory(
            nameof(ThumbnailSheetJob),
            Guid.NewGuid().ToString()
        );

        await storage.CreateAsync(new ThumbnailSheetBucket(sheetId));

        foreach (var thumbnailSize in ThumbnailSizes.All)
        {
            var outputDirectory = Path.Combine(parentOutputDirectory, thumbnailSize.PixelCount.ToString());
            Directory.CreateDirectory(outputDirectory);

            var arguments = FFMpegArguments
                .FromFileInput(inputFile)
                .OutputToFile(Path.Combine(outputDirectory, $"out_%03d.jpg"), overwrite: true, options =>
                {
                    options
                        .WithVideoFilters(o =>
                        {
                            if (isLandscape)
                                o.Scale(thumbnailSize.PixelCount, -1);
                            else
                                o.Scale(-1, thumbnailSize.PixelCount);
                            o.Arguments.Add(new FpsArgument(lengthInMs, count));
                        });
                })
                .WithLogLevel(logger.GetFFMpegLogLevel());

            logger.LogDebug("Calling ffmpeg: {cmd}", arguments.Arguments);
            await arguments.ProcessAsynchronously(throwOnError: true);

            var i = 0;
            foreach (var thumbnailPath in Directory.EnumerateFiles(outputDirectory).OrderBy(s => s))
            {
                logger.LogDebug("Created thumbnail at {p}", thumbnailPath);

                using var fileStream = new FileStream(thumbnailPath, FileMode.Open);
                await thumbnailSheetStorage.StoreAsync(sheetId, i, fileStream, thumbnailSize);
                i++;
            }
        }

        await repository.CompleteAsync(sheetId);
    }

    private class FpsArgument(int lengthInMs, int count) : IVideoFilterArgument
    {
        public string Key => "fps";
        public string Value => $"{count * 1000}/{lengthInMs}";
    }
}
