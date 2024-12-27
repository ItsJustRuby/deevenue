using Deevenue.Domain.Thumbnails;
using Microsoft.Extensions.Logging;

namespace Deevenue.Domain.Media;

internal class AddMediumService(
    IAddImageService addImageService,
    IAddVideoService addVideoService,
    IMediumStorage mediumStorage,
    IThumbnailStorage thumbnailStorage,
    IMediumRepository mediaRepository,
    ITagRepository tagRepository,
    ITaggyFileName taggyFileName,
    IMediumOracle mediumOracle,
    ILogger<AddMediumService> logger
) : IAddMediumService
{
    public async Task<ITryAddResult> TryAddAsync(
        string fileName, string contentType, Stream readStream, long size)
    {
        var hashString = await Hashes.ComputeMD5Async(readStream);
        readStream.Seek(0, SeekOrigin.Begin);

        var maybeConflictingMediumId = await mediaRepository.FindByHashAsync(hashString);
        if (maybeConflictingMediumId.HasValue)
            return new ConflictingMedium(maybeConflictingMediumId.Value);

        var mediaKind = mediumOracle.GuessMediaKind(new MediumFileData(readStream, contentType));

        if (mediaKind == MediaKind.Unusable)
            return new UnusableMediaKind(contentType);

        var taggyParseResults = taggyFileName.Parse(fileName);
        logger.LogDebug("File name parsing results: {r}", taggyParseResults);
        await tagRepository.EnsureExistAsync(taggyParseResults.Tags);

        IAddService addService = (mediaKind == MediaKind.Video) ? addVideoService : addImageService;
        var preprocessResult = await addService.PreProcessAsync(readStream);

        var dbPutArgs = new PutAsyncArgs
        {
            ContentType = contentType,
            Hash = hashString,
            Rating = taggyParseResults.Rating,
            ReadStream = readStream,
            Size = size,
            TagNames = taggyParseResults.Tags,
            Width = preprocessResult.Dimensions?.Width,
            Height = preprocessResult.Dimensions?.Height
        };

        var id = await mediaRepository.PutAsync(dbPutArgs);

        await StoreThumbnailsAsync(id, preprocessResult.Thumbnails);
        await mediumStorage.StoreAsync(id, contentType, size, readStream);

        await addService.PostProcessAsync(id);
        return new Success(id);
    }

    private async Task StoreThumbnailsAsync(
        Guid createdMediumId, IReadOnlySet<CreatedThumbnail> thumbnails)
    {
        foreach (var createdThumbnail in thumbnails)
        {
            using var thumbnailStream = createdThumbnail.Stream;
            logger.LogDebug("Thumbnail creation results: {Length} bytes", thumbnailStream.Length);
            await thumbnailStorage.StoreAsync(
                createdMediumId, isAnimated: false, createdThumbnail.Size, thumbnailStream);
        }
    }

    private class Success(Guid createdMediumId) : ITryAddResult
    {
        public T Accept<T>(ITryAddResultVisitor<T> visitor)
            => visitor.VisitSuccess(createdMediumId);
    }

    private class UnusableMediaKind(string contentType) : ITryAddResult
    {
        public T Accept<T>(ITryAddResultVisitor<T> visitor)
            => visitor.VisitUnusableMediaKind(contentType);
    }

    private class ConflictingMedium(Guid conflictingMediumId) : ITryAddResult
    {
        public T Accept<T>(ITryAddResultVisitor<T> visitor)
            => visitor.VisitConflictingMedium(conflictingMediumId);
    }
}
