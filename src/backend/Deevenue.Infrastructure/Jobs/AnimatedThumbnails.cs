using System.Diagnostics;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Deevenue.Domain;
using Deevenue.Domain.Media;
using Deevenue.Domain.Thumbnails;
using Deevenue.Domain.Video;
using Deevenue.Infrastructure.MediaProcessing;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Logging;

namespace Deevenue.Infrastructure.Jobs;

internal class AnimatedThumbnails(
    IThumbnailStorage thumbnailStorage,
    IVideoInspector videoInspector,
    ILogger<AnimatedThumbnails> logger) : IAnimatedThumbnails
{
    private static readonly CsvConfiguration csvConfiguration = new(CultureInfo.InvariantCulture)
    {
        NewLine = "\n",
        HasHeaderRecord = true,
    };

    public async Task CreateAsync(Guid mediumId, MediumData mediumData)
    {
        if (!CanHandle(mediumData, mediumId)) return;

        using var tempDirectory = new TemporaryDirectory(
            nameof(AnimatedThumbnails),
            Guid.NewGuid().ToString()
        );

        var inputMediumFilePath = await Setup(tempDirectory, mediumData, mediumId);
        var scenes = await DetectScenesAsync(tempDirectory, inputMediumFilePath);

        var thumbnailVideoPath = Path.Combine(tempDirectory, "thumbnailVideo.webm");
        if (scenes.Count() < 5)
            await CreateNaiveThumbnailAsync(inputMediumFilePath, thumbnailVideoPath);
        else
            await CreateSceneBasedThumbnail(tempDirectory, inputMediumFilePath, thumbnailVideoPath, scenes);

        await DownscaleAndPersistAsync(mediumId, thumbnailVideoPath);
    }

    private async Task DownscaleAndPersistAsync(Guid mediumId, string thumbnailVideoPath)
    {
        bool isLandscape;
        using (var fileStream = File.OpenRead(thumbnailVideoPath))
        {
            var dimensions = await videoInspector.MeasureAsync(fileStream)
                ?? throw new Exception($"Could not measure video {mediumId}.");
            isLandscape = dimensions.Width > dimensions.Height;
        }

        await Task.WhenAll(ThumbnailSizes.All.Select(async size =>
        {
            var downscaledVideoPath = await DownscaleAsync(thumbnailVideoPath, isLandscape, size);
            await PersistAsync(mediumId, downscaledVideoPath, size);
        }));
    }

    private async Task<IEnumerable<ScenesCsv>> DetectScenesAsync(string tempPath, string inputMediumFilePath)
    {
        var parametersToTry = new Queue<string[]>([
            ["--min-scene-len=3.0", "--drop-short-scenes"],
            // Merging short scenes instead of dropping them is the default behavior
            ["--min-scene-len=3.0"],
            ["--min-scene-len=1.5", "--drop-short-scenes"],
        ]);

        IEnumerable<ScenesCsv> scenes = [];
        while (scenes.Count() < 5 && parametersToTry.Count > 0)
        {
            var currentParameters = parametersToTry.Dequeue();
            scenes = await GetScenes(tempPath, inputMediumFilePath, currentParameters);
            logger.LogDebug("Detected {n} scenes", scenes.Count());
        }

        return scenes;
    }

    private async Task CreateNaiveThumbnailAsync(string inputFilePath, string outputFilePath)
    {
        var arguments = FFMpegArguments
            .FromFileInput(inputFilePath)
            .OutputToFile(outputFilePath, overwrite: true, options =>
            {
                options
                    .DisableChannel(Channel.Audio)
                    .WithVideoCodec("libvpx-vp9")
                    .WithFramerate(60)
                    .WithDuration(TimeSpan.FromSeconds(15));
            })
            .WithLogLevel(logger.GetFFMpegLogLevel());

        logger.LogDebug("Naive thumbnail arguments: {a}", arguments.Arguments);
        await arguments.ProcessAsynchronously();
    }

    private async Task CreateSceneBasedThumbnail(
        string tempPath, string inputMediumFilePath, string outputPath, IEnumerable<ScenesCsv> scenes)
    {
        var sceneArray = scenes.ToArray();
        var sceneIndicesToPick = Stride.GetEvenlySpacedIndices(scenes.Count(), 5);
        var pickedScenes = sceneIndicesToPick.Select(index => sceneArray[index]);

        int i = 1;
        foreach (var scene in pickedScenes)
            scene.SceneNumber = i++;

        var selectedScenesCsvPath = Path.Combine(tempPath, "selectedScenes.csv");

        // Not using "using var" for once because StreamWriter only (implicitly) flushes on Dispose!
        using (var selectedScenesCsvFile = new StreamWriter(selectedScenesCsvPath))
        {
            using var csvWriter = new CsvWriter(selectedScenesCsvFile, csvConfiguration);
            await csvWriter.WriteRecordsAsync(pickedScenes);
        }

        // Give file to `scenedetect load-scenes` combined with `split-video`,
        // once again capturing the output files in a temp directory and probably with a better custom name.
        var splitScenesDirectory = await SplitVideoAsync(inputMediumFilePath, selectedScenesCsvPath);

        var processedScenesDirectory = await TrimAndMuteScenesAsync(splitScenesDirectory);

        // Concatenate all the scenes into one webm
        await ContatenateScenesIntoThumbnailAsync(processedScenesDirectory, outputPath);
    }

    private async Task<string> DownscaleAsync(
        string inputVideoFilePath, bool isLandscape, IThumbnailSize thumbnailSize)
    {
        var outputFilePath = Path.Combine(
            Path.GetDirectoryName(inputVideoFilePath)!,
            Path.GetFileNameWithoutExtension(inputVideoFilePath) + $"_{thumbnailSize.Abbreviation}.webm");

        logger.LogDebug("Downscaling {in} to {out}, size {size}",
            inputVideoFilePath, outputFilePath, thumbnailSize.Abbreviation);

        var arguments = FFMpegArguments.FromFileInput(inputVideoFilePath)
            .OutputToFile(outputFilePath, overwrite: true, o =>
            {
                o.WithVideoFilters(filters =>
                {
                    if (isLandscape)
                        filters.Scale(thumbnailSize.PixelCount, -1);
                    else
                        filters.Scale(-1, thumbnailSize.PixelCount);
                })
                .WithVideoCodec("libvpx-vp9")
                .WithFramerate(60);
            })
            .WithLogLevel(logger.GetFFMpegLogLevel());

        logger.LogDebug("Calling ffmpeg: {cmd}", arguments.Arguments);
        await arguments.ProcessAsynchronously();
        return outputFilePath;
    }

    private async Task PersistAsync(Guid mediumId, string downscaledVideoPath, IThumbnailSize size)
    {
        logger.LogDebug("Persisting {p}", downscaledVideoPath);
        using var fileStream = File.OpenRead(downscaledVideoPath);
        await thumbnailStorage.StoreAsync(mediumId, isAnimated: true, size, fileStream);
    }

    private async Task<string> TrimAndMuteScenesAsync(string splitScenesDirectoryPath)
    {
        var outputDirectory =
            Path.Combine(Directory.GetParent(splitScenesDirectoryPath)!.FullName, "processedScenes");
        Directory.CreateDirectory(outputDirectory);

        var orderedSceneFileNames = Directory.GetFiles(splitScenesDirectoryPath).OrderBy(f => f).ToList();

        await Task.WhenAll(orderedSceneFileNames.Select(async sceneFile =>
        {
            var fileOutputPath = Path.Combine(outputDirectory, Path.GetFileName(sceneFile));

            var arguments = FFMpegArguments.FromFileInput(sceneFile)
                .OutputToFile(fileOutputPath, overwrite: true, options =>
                {
                    options.CopyChannel(Channel.Video)
                        .DisableChannel(Channel.Audio)
                        .WithDuration(TimeSpan.FromSeconds(3));
                })
            .WithLogLevel(logger.GetFFMpegLogLevel());

            return await arguments.ProcessAsynchronously();
        }));

        return outputDirectory;
    }

    private async Task ContatenateScenesIntoThumbnailAsync(
        string processedScenesDirectoryPath,
        string outputPath)
    {
        var orderedSceneFileNames = Directory.GetFiles(processedScenesDirectoryPath).OrderBy(f => f).ToList();

        var arguments = FFMpegArguments
            .FromDemuxConcatInput(orderedSceneFileNames)
            .OutputToFile(outputPath)
            .WithLogLevel(logger.GetFFMpegLogLevel());

        logger.LogDebug("Calling ffmpeg: {cmd}", arguments.Arguments);
        await arguments.ProcessAsynchronously(throwOnError: true);
    }

    private async Task<string> SplitVideoAsync(string inputMediumFilePath, string selectedScenesCsvPath)
    {
        var tempDirectoryPath = Path.GetDirectoryName(inputMediumFilePath)!;
        var outputDirectoryPath = Path.Combine(tempDirectoryPath, "scenes");
        Directory.CreateDirectory(outputDirectoryPath);

        using var sceneListingProcess = new Process();

        sceneListingProcess.StartInfo = new ProcessStartInfo(
            "scenedetect",
            [
                "-i", inputMediumFilePath,
                "--quiet",
                "load-scenes",
                "-i", selectedScenesCsvPath,
                "split-video",
                "--output", outputDirectoryPath,
                "--filename", "$SCENE_NUMBER"
            ]
        )
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        sceneListingProcess.Start();
        await sceneListingProcess.WaitForExitAsync();
        logger.LogDebug("SceneDetect stdout: {o}", sceneListingProcess.StandardOutput.ReadToEnd());
        var stderr = sceneListingProcess.StandardError.ReadToEnd();
        if (stderr.Length > 0)
            logger.LogWarning("SceneDetect errors: {stderr}", stderr);
        return outputDirectoryPath;
    }

    private async Task<IEnumerable<ScenesCsv>> GetScenes(
        string tempPath,
        string inputMediumFilePath,
        string[] sceneDetectionParameters)
    {
        using var sceneListingProcess = new Process();

        sceneListingProcess.StartInfo = new ProcessStartInfo(
            "scenedetect",
            [
                "-i", inputMediumFilePath,
                ..sceneDetectionParameters,
                "--min-scene-len=3.0",
                "--drop-short-scenes",
                "--quiet",
                "detect-content",
                "list-scenes",
                "--output", tempPath,
                "--filename", "scenes.csv",
                "--skip-cuts" // generate more compliant CSV
            ]
        )
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        sceneListingProcess.Start();
        await sceneListingProcess.WaitForExitAsync();

        logger.LogDebug("SceneDetect stdout: {o}", sceneListingProcess.StandardOutput.ReadToEnd());
        var stderr = sceneListingProcess.StandardError.ReadToEnd();
        if (stderr.Length > 0)
            logger.LogWarning("SceneDetect errors: {stderr}", stderr);

        // Note: Even if there are errors in stderr, that still means that SceneDetect
        // was a nice enough to just output at least 1 scene in scenes.csv,
        // so we can just proceed as normal.

        using var scenesCsvFile = new StreamReader(Path.Combine(tempPath, "scenes.csv"));
        using var csvReader = new CsvReader(scenesCsvFile, csvConfiguration);
        return csvReader.GetRecords<ScenesCsv>().ToList();
    }

    private async Task<string> Setup(string tempPath, MediumData? mediumFile, Guid mediumId)
    {
        var inputMediumFilePath = Path.Combine(tempPath, $"{mediumId}.tmp");
        await File.WriteAllBytesAsync(inputMediumFilePath, mediumFile!.Bytes);
        logger.LogDebug("Wrote {n} bytes to {p}", mediumFile.Bytes.Length, inputMediumFilePath);

        return inputMediumFilePath;
    }

    private bool CanHandle(MediumData? mediumFile, Guid mediumId)
    {
        if (mediumFile == null)
        {
            logger.LogWarning("Tried to create animated thumbnail for non-existant medium {id}", mediumId);
            return false;
        }

        var mediaKind = MediaKinds.Parse(mediumFile.ContentType);
        if (mediaKind != MediaKind.Video)
        {
            logger.LogWarning("Tried to create animated thumbnail for non-video medium {id}", mediumId);
            return false;
        }

        return true;
    }

    // For deserialization, we actually need neither of these,
    // but for serialization, these are a reasonable minimum to use,
    // so we just use the same type for both operations to make our lives easier.
    private class ScenesCsv
    {
        [Name("Scene Number")]
        public required int SceneNumber { get; set; }

        [Name("Start Frame")]
        public required string StartFrame { get; set; }
    }
}
