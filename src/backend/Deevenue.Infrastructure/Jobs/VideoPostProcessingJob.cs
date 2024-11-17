using Deevenue.Domain;
using Deevenue.Domain.Media;
using Deevenue.Infrastructure.Db;
using Deevenue.Infrastructure.MediaProcessing;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Deevenue.Infrastructure.Jobs;

public record PostProcessingJobParameters(Guid MediumId, bool HasInitialThumbnails);

internal class VideoPostProcessingJobFactory : IDeevenueJobFactory<VideoPostProcessingJob, PostProcessingJobParameters>
{
    public static readonly IDeevenueJobFactory<VideoPostProcessingJob, PostProcessingJobParameters> Instance
        = new VideoPostProcessingJobFactory();
    private VideoPostProcessingJobFactory() { }

    public string CreateIdentity(PostProcessingJobParameters parameters) => parameters.MediumId.ToString();

    public JobDataMap StoreParameters(PostProcessingJobParameters parameters)
    {
        var result = new JobDataMap();
        result.Put(VideoPostProcessingJob.MediumIdParamName, parameters.MediumId);
        result.Put(VideoPostProcessingJob.HasInitialThumbnailsParamName, parameters.HasInitialThumbnails);
        return result;
    }
}

[JobKindName("Video postprocessing job")]
internal class VideoPostProcessingJob(
    IJobResultRepository jobRepository,
    IMediumStorage mediumStorage,
    IMediumRepository repository,
    IAnimatedThumbnails animatedThumbnails,
    IVideoPostProcessor videoPostProcessor,
    ILogger<VideoPostProcessingJob> logger) : DeevenueJobBase(jobRepository)
{
    public const string MediumIdParamName = "mediumId";
    public const string HasInitialThumbnailsParamName = "hasInitialThumbnails";

    public override JobSummaryData GetSummaryData(IJobExecutionContext context)
        => new(context.JobDetail.JobDataMap.GetGuid(MediumIdParamName));

    protected override async Task ActuallyExecute(IJobExecutionContext context)
    {
        var mediumId = context.JobDetail.JobDataMap.GetGuid(MediumIdParamName);
        var hasInitialThumbnails = context.JobDetail.JobDataMap.GetBoolean(HasInitialThumbnailsParamName);

        using var temporaryOutputDirectory = new TemporaryDirectory(
            nameof(VideoPostProcessingJob),
            Guid.NewGuid().ToString()
        );

        var mediumData = await mediumStorage.GetAsync(mediumId);

        using var memoryStream = new MemoryStream(mediumData!.Bytes);
        using var inputFile = TemporaryVideoFile.From(memoryStream);

        string newContentType;
        string outputFilePath;
        if (mediumData.ContentType == "image/gif")
        {
            outputFilePath = await TranscodeGif(inputFile, temporaryOutputDirectory);
            newContentType = "video/webm";
        }
        else
        {
            outputFilePath = await TranscodeVideo(inputFile, temporaryOutputDirectory);
            newContentType = "video/mp4";
        }

        string hash;
        using (var outputFile = File.OpenRead(outputFilePath))
        {
            hash = await Hashes.ComputeMD5Async(outputFile);
            outputFile.Seek(0, SeekOrigin.Begin);

            var maybeConflictingMediumId = await repository.FindByHashAsync(hash);
            if (maybeConflictingMediumId != null)
            {
                await FailAsync($"Found duplicates while postprocessing: {mediumId} and {maybeConflictingMediumId}");
                return;
            }

            // No need to delete the old medium file and persist the new file,
            // this should just overwrite it in situ.
            await mediumStorage.StoreAsync(mediumId, newContentType, outputFile.Length, outputFile);
        }

        await repository.UpdateContentMetadataAsync(mediumId, newContentType, hash);
        await animatedThumbnails.CreateAsync(mediumId, mediumData);
        await videoPostProcessor.PersistMeasurementsAsync(mediumId, mediumData);

        if (!hasInitialThumbnails)
            await videoPostProcessor.CreateAndPersistThumbnailsAsync(mediumId, mediumData);
    }

    private async Task<string> TranscodeVideo(TemporaryVideoFile inputFile, string outputDirectory)
    {
        var outputPath = Path.Combine(outputDirectory, "out.mp4");

        var arguments = FFMpegArguments
            .FromFileInput(inputFile)
            .OutputToFile(outputPath, overwrite: true, options =>
            {
                options
                    .WithVideoCodec(VideoCodec.LibX264)
                    // Crop both input dimensions to be divisible by 2.
                    // The codec cannot handle odd width or height.
                    .WithCustomArgument("""
                        -vf "crop=trunc(iw/2)*2:trunc(ih/2)*2"
                    """);
            })
            .WithLogLevel(logger.GetFFMpegLogLevel());

        logger.LogDebug("Calling ffmpeg: {cmd}", arguments.Arguments);
        await arguments.ProcessAsynchronously(throwOnError: true);
        return outputPath;
    }

    private async Task<string> TranscodeGif(TemporaryVideoFile temporaryVideoFile, string outputDirectory)
    {
        var outputPath = Path.Combine(outputDirectory, "out.webm");

        var arguments = FFMpegArguments.FromFileInput(temporaryVideoFile)
            .OutputToFile(outputPath, overwrite: true, options =>
            {
                options
                    .WithVideoCodec("libvpx-vp9");
            })
            .WithLogLevel(logger.GetFFMpegLogLevel());

        await arguments.ProcessAsynchronously();
        return outputPath;
    }
}
