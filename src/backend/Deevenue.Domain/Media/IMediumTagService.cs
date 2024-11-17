namespace Deevenue.Domain.Media;

public interface IMediumTagService
{
    Task LinkAsync(Guid mediumId, string tagName);
    Task UnlinkAsync(Guid mediumId, string tagName);

    Task LinkAbsentAsync(Guid mediumId, string tagName);
    Task UnlinkAbsentAsync(Guid mediumId, string tagName);
}
