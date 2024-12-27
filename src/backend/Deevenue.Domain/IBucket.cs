namespace Deevenue.Domain;

public interface IBucket
{
    string Name { get; }
}

public static class StaticBuckets
{
    public static readonly IBucket Media = new StaticBucket("media");
    public static readonly IBucket Thumbs = new StaticBucket("thumbs");
    public static readonly IBucket Rules = new StaticBucket("rules");

    public static readonly IReadOnlySet<IBucket> All = new HashSet<IBucket>([Media, Thumbs, Rules]);
    private record StaticBucket(string Name) : IBucket;
}
