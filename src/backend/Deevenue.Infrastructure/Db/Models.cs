using Deevenue.Domain;

namespace Deevenue.Infrastructure.Db;

public class Medium
{
    public Guid Id { get; set; }
    public required Rating Rating { get; set; }
    public required string ContentType { get; set; }
    public required int Width { get; set; }
    public required int Height { get; set; }
    public required decimal FileSize { get; set; }
    public required DateTime InsertedAt { get; set; }

    public ISet<MediumHash> KnownHashes { get; set; } = new HashSet<MediumHash>();
    public ISet<Tag> Tags { get; set; } = new HashSet<Tag>();
    public ISet<Tag> AbsentTags { get; set; } = new HashSet<Tag>();
    public ISet<ThumbnailSheet> ThumbnailSheets { get; set; } = new HashSet<ThumbnailSheet>();
}

public class MediumHash
{
    public Guid Id { get; set; }
    public required Medium Medium { get; set; }
    public Guid MediumId { get; set; }
    public required string Hash { get; set; }
}

internal interface ITagLinkedToMedium
{
    Medium Medium { get; set; }
    Guid MediumId { get; set; }
    Tag Tag { get; set; }
    Guid TagId { get; set; }
}

public class MediumTagAbsence : ITagLinkedToMedium
{
    public required Medium Medium { get; set; }
    public required Guid MediumId { get; set; }
    public required Tag Tag { get; set; }
    public required Guid TagId { get; set; }
}

public class MediumTag : ITagLinkedToMedium
{
    public required Medium Medium { get; set; }
    public required Guid MediumId { get; set; }
    public required Tag Tag { get; set; }
    public required Guid TagId { get; set; }
}

public class Tag
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required Rating Rating { get; set; } = Rating.Unknown;

    public IList<string> Aliases { get; set; } = new List<string>();

    // Note: initializing to [] causes issues due to Fixed Size collections.
    // Either make these properties required and initialize them explicitly,
    // or initialize them to empty but resizable data structures.
    public ISet<Tag> ImplyingThis { get; set; } = new HashSet<Tag>();
    public ISet<Tag> ImpliedByThis { get; set; } = new HashSet<Tag>();

    public ISet<TagImplication> OutgoingImplications { get; set; } = new HashSet<TagImplication>();
    public ISet<TagImplication> IncomingImplications { get; set; } = new HashSet<TagImplication>();

    public ISet<Medium> Media { get; set; } = new HashSet<Medium>();
    public ISet<Medium> AbsentMedia { get; set; } = new HashSet<Medium>();
}

public class TagImplication
{
    public required Tag ImplyingTag { get; set; }
    public required Guid ImplyingTagId { get; set; }
    public required Tag ImpliedTag { get; set; }
    public required Guid ImpliedTagId { get; set; }

}

public class ThumbnailSheet
{
    public Guid Id { get; set; }
    public required Medium Medium { get; set; }
    public Guid MediumId { get; set; }
    public required int ThumbnailCount { get; set; }

    public required DateTime InsertedAt { get; set; }
    public DateTime? CompletedAt { get; set; } = null;
}

public class JobResult
{
    public Guid Id { get; set; }
    public required Guid JobId { get; set; }

    public required string JobTypeName { get; set; }
    public required string ErrorText { get; set; }
    public required DateTime InsertedAt { get; set; }
}
