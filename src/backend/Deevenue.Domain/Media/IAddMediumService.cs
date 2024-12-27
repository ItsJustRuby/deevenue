namespace Deevenue.Domain.Media;

public interface IAddMediumService
{
    Task<ITryAddResult> TryAddAsync(string fileName, string contentType, Stream readStream, long size);
}

public interface ITryAddResult
{
    T Accept<T>(ITryAddResultVisitor<T> visitor);
}

public interface ITryAddResultVisitor<out T>
{
    T VisitSuccess(Guid createdMediumId);
    T VisitUnusableMediaKind(string contentType);
    T VisitConflictingMedium(Guid conflictingMediumId);
}
