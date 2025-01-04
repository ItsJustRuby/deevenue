namespace Deevenue.Domain.Media;

internal class SimilarMediaService(
    ITracingSpans tracingSpans,
    IMediumRepository repository,
    ISfwService sfwService) : ISimilarMediaService
{
    const int MaxCount = 5;

    public async Task<IReadOnlyList<SimilarMedium>> GetSimilarAsync(Guid mediumId)
    {
        using var tracingSpan = tracingSpans.Create(nameof(SimilarMediaService), nameof(GetSimilarAsync));

        // Get all media that have at least some tags in common
        var all = await repository.GetAllSearchableAsync(sfwService.IsSfw);
        var current = all.Single(m => m.Id == mediumId);
        var relevant = all
            .Where(m => m.Id != mediumId)
            .Where(m => m.InnateTags.Intersect(current.InnateTags).Any()).ToHashSet();

        // Keep a priority queue
        // * of MaxCount with space for 1 extra to eject when the queue is "full"
        // * ordered in reverse (highest priority == similarity first)
        var queue = new PriorityQueue<SmallMediumDocument, float>(
            MaxCount + 1,
            Comparer<float>.Create((a, b) => b.CompareTo(a)));

        foreach (var candidate in relevant)
        {
            var unionSize = candidate.InnateTags.Union(current.InnateTags).Count();

            if (unionSize == 0) continue;

            // Calculate jacquard similarity
            var similarity = (float)candidate.InnateTags.Intersect(current.InnateTags).Count()
                / candidate.InnateTags.Union(current.InnateTags).Count();

            queue.Enqueue(candidate, similarity);
            if (queue.Count > MaxCount)
                queue.Dequeue();
        }

        var result = new List<SimilarMedium>();
        while (queue.Count > 0)
        {
            var smd = queue.Dequeue();
            result.Add(new SimilarMedium(smd.Id, MediaKinds.Parse(smd.ContentType)));
        }
        return result;
    }
}
