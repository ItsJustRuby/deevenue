
using Deevenue.Domain;

namespace Deevenue.Infrastructure.Jobs;

internal interface IAnimatedThumbnails
{
    Task CreateAsync(Guid mediumId, MediumData mediumData);
}
