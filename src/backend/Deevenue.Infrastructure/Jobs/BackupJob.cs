using System.Text.Json;
using Deevenue.Domain;
using Deevenue.Domain.Media;
using Deevenue.Infrastructure.Db;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Deevenue.Infrastructure.Jobs;

[JobKindName("Backup job")]
internal class BackupJob(
    IJobResultRepository jobRepository,
    ITagRepository tagRepository,
    IMediumRepository mediumRepository,
    IMediumStorage mediumStorage,
    ILogger<JobResultCleanupJob> logger) : DeevenueJobBase(jobRepository)
{
    public override JobSummaryData GetSummaryData(IJobExecutionContext context)
        => new(null);

    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        WriteIndented = true,
        // This is implicitly *lowercase* camel case.
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    protected override async Task ActuallyExecute(IJobExecutionContext context)
    {
        logger.LogInformation("Backup job starting");

        var rootDirectory = Config.Backup.Directory;
        if (!Directory.Exists(rootDirectory))
        {
            await FailAsync($"Could not access target directory \"{rootDirectory}\"");
            return;
        }

        // These need to be sequential because they both access the same DbContext
        await BackupTags();
        await BackupMedia();
    }

    private async Task BackupTags()
    {
        logger.LogInformation("Backing up tags");
        var tags = await tagRepository.GetAllAsync();

        var outputFileName = Path.Combine(Config.Backup.Directory, "tags.json");
        using var tagsJsonFile = File.OpenWrite(outputFileName);
        await JsonSerializer.SerializeAsync(tagsJsonFile, tags, jsonSerializerOptions);
        logger.LogInformation("Backed up tags to \"{f}\"", outputFileName);
    }

    private async Task BackupMedia()
    {
        logger.LogInformation("Backing up media");
        var media = await mediumRepository.GetAllSearchableAsync(isSfw: false);

        logger.LogInformation("Found {n} media to back up", media.Count);
        await Task.WhenAll(media.Select(async medium =>
        {
            var mediumDirectory = Path.Combine(Config.Backup.Directory, medium.Id.ToString());
            Directory.CreateDirectory(mediumDirectory);

            async Task writeMetadata()
            {
                using var metadataJsonFile = File.OpenWrite(Path.Combine(mediumDirectory, "metadata.json"));
                await JsonSerializer.SerializeAsync(metadataJsonFile, medium, jsonSerializerOptions);
            }

            async Task writeActualData()
            {
                var mediumData = await mediumStorage.GetAsync(medium.Id);
                await File.WriteAllBytesAsync(Path.Combine(mediumDirectory, medium.Id.ToString()), mediumData!.Bytes);
            }

            await Task.WhenAll(writeMetadata(), writeActualData());
        }));

        logger.LogInformation("Backed up {n} media", media.Count);
    }
}
