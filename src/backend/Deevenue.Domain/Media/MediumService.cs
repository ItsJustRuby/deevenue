using Deevenue.Domain.Search;
using Deevenue.Domain.Thumbnails;

namespace Deevenue.Domain.Media;

internal class MediumService(
    IMediumStorage mediumStorage,
    IThumbnailStorage thumbnailStorage,
    IMediumRepository mediaRepository,
    ISfwService sfwService) : IMediumService
{
    public Task DeleteAsync(Guid mediumId)
    {
        return Task.WhenAll(
            mediaRepository.DeleteAsync(mediumId),
            mediumStorage.RemoveAsync(mediumId),
            thumbnailStorage.RemoveAllAsync(mediumId)
        );
    }

    public async Task<IReadOnlySet<SmallMediumDocument>> GetAllSearchableAsync(bool? isSfwOverride = null)
        => await mediaRepository.GetAllSearchableAsync(isSfwOverride ?? sfwService.IsSfw);

    public async Task<SmallMediumDocument?> GetEnforceableAsync(Guid id)
        => await mediaRepository.GetSearchableAsync(id, sfwService.IsSfw);

    public async Task<PaginationViewModel<SearchResultViewModel>> PaginateAllAsync(PaginationParameters pagination)
    {
        var result = await mediaRepository.PaginateAllAsync(pagination, sfwService.IsSfw);

        return new PaginationViewModel<SearchResultViewModel>(
            result.Page.Select(m =>
            {
                return new SearchResultViewModel(m.Id, MediaKinds.Parse(m.ContentType));
            }
            ).ToList(),
            result.PageCount,
            pagination.PageNumber,
            pagination.PageSize
        );
    }

    public async Task<bool> SetRatingAsync(Guid id, Rating rating)
        => await mediaRepository.SetRatingAsync(id, rating);

    public async Task<ITryGetResult> TryGetAsync(Guid id)
    {
        var dbResult = await mediaRepository.TryGetAsync(id);

        if (dbResult == null)
            return new NotFound();

        if (sfwService.IsSfw && dbResult.Rating != Rating.Safe)
            return new NotSfw();

        var viewModel = new MediumViewModel(
            dbResult.Id,
            MediaKinds.Parse(dbResult.ContentType),
            dbResult.Tags,
            dbResult.AbsentTags,
            dbResult.ThumbnailSheetIds,
            dbResult.Rating
        );

        return new Success(viewModel);
    }

    public Task<Guid?> TryGetByHashAsync(string hash) => mediaRepository.FindByHashAsync(hash);

    private class Success(MediumViewModel medium) : ITryGetResult
    {
        public T Accept<T>(ITryGetResultVisitor<T> visitor) => visitor.VisitSuccess(medium);
    }

    private class NotFound : ITryGetResult
    {
        public T Accept<T>(ITryGetResultVisitor<T> visitor) => visitor.VisitNotFound();
    }

    private class NotSfw : ITryGetResult
    {
        public T Accept<T>(ITryGetResultVisitor<T> visitor) => visitor.VisitNotSfw();
    }
}
