using FluentValidation;

namespace Deevenue.Domain.Rules;

internal class RulesViewModelValidator : AbstractValidator<RulesViewModel>
{
    public RulesViewModelValidator()
    {
        RuleForEach(c => c.Rules).SetValidator(new RuleViewModelValidator());
    }
}

internal class RuleViewModelValidator : AbstractValidator<RuleViewModel>
{
    public RuleViewModelValidator()
    {
        RuleFor(r => r.Iffs).NotEmpty();
        RuleFor(r => r.Thens).NotEmpty();
    }
}
