using Deevenue.Domain;

namespace Deevenue.Infrastructure.Jobs;

public interface IVideoPostProcessor
{
    Task PersistMeasurementsAsync(Guid mediumId, MediumData mediumData);
    Task CreateAndPersistThumbnailsAsync(Guid mediumId, MediumData mediumData);
}
