namespace Deevenue.Domain.Media;

internal class MediumTagService(
    IOrphanTagsRepository orphanTagsRepository,
    IMediumTagRepository mediumTagRepository) : IMediumTagService
{
    public async Task LinkAsync(Guid mediumId, string tagName)
        => await mediumTagRepository.LinkAsync(mediumId, tagName);

    public async Task UnlinkAsync(Guid mediumId, string tagName)
    {
        await using var orphanDisposable = orphanTagsRepository.GetOrphanDisposableAsync();
        await mediumTagRepository.UnlinkAsync(mediumId, tagName);
    }

    public async Task LinkAbsentAsync(Guid mediumId, string tagName)
        => await mediumTagRepository.LinkAbsentAsync(mediumId, tagName);

    public async Task UnlinkAbsentAsync(Guid mediumId, string tagName)
    {
        await using var orphanDisposable = orphanTagsRepository.GetOrphanDisposableAsync();
        await mediumTagRepository.UnlinkAbsentAsync(mediumId, tagName);
    }
}
