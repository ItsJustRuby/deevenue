
namespace Deevenue.Domain.Rules;

internal static class RandomRuleViolations
{
    public static readonly IRandomRuleViolationViewModel None = new None();
    public static readonly IRandomRuleViolationViewModel Invisible = new Invisible();
    public static IRandomRuleViolationViewModel Visible(Guid mediumId) => new Visible { MediumId = mediumId };
}

public interface IRandomRuleViolationViewModel
{
    T Accept<T>(IRandomRuleViolationViewModelVisitor<T> visitor);
}

internal class None : IRandomRuleViolationViewModel
{
    public T Accept<T>(IRandomRuleViolationViewModelVisitor<T> visitor)
        => visitor.VisitNone();
}

internal class Visible : IRandomRuleViolationViewModel
{
    public required Guid MediumId { get; init; }

    public T Accept<T>(IRandomRuleViolationViewModelVisitor<T> visitor)
        => visitor.VisitVisible(MediumId);
}

internal class Invisible : IRandomRuleViolationViewModel
{
    public T Accept<T>(IRandomRuleViolationViewModelVisitor<T> visitor)
        => visitor.VisitInvisible();
}

public interface IRandomRuleViolationViewModelVisitor<out T>
{
    T VisitNone();
    T VisitInvisible();
    T VisitVisible(Guid mediumId);
}
