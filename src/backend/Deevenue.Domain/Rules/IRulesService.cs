namespace Deevenue.Domain.Rules;

public interface IRulesService
{
    Task<RulesViewModel?> DeleteAsync(int index);
    Task<RulesViewModel> GetAllAsync();
    Task<IRandomRuleViolationViewModel> GetRandomRuleViolationAsync();
    Task<MediumRuleViolationsViewModel?> GetViolationsAsync(Guid mediumId);
    Task<RulesViewModel?> PutAsync(RulesViewModel rules);
    bool Validate(RulesViewModel viewModel);

    RuleViewModel? Get(int index);
}

public record MediumRuleViolationsViewModel(IEnumerable<RuleViewModel> Rules);
