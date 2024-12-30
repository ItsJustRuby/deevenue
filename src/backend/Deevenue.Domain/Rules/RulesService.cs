using System.Text.Json;
using Deevenue.Domain.Media;
using FluentValidation;

namespace Deevenue.Domain.Rules;

internal class RulesService(
    IMediumService mediumService,
    ISfwService sfwService,
    IRulesStorage storage) : IRulesService
{
    public async Task<RulesViewModel?> DeleteAsync(int index)
    {
        var current = await GetAllAsync();

        if (index < 0 || index > current.Rules.Length - 1)
            return null;

        var newRulesArray = current.Rules.Where((_, currentIndex) => currentIndex != index).ToArray();
        var newRules = new RulesViewModel { Rules = newRulesArray };
        await PutAsync(newRules);
        return newRules;
    }

    public RuleViewModel? Get(int index)
    {
        var rulesViewModel = GetAllAsync().Result;
        return rulesViewModel.Rules.ElementAtOrDefault(index);
    }

    public async Task<RulesViewModel> GetAllAsync()
    {
        using var maybeStream = await storage.StreamAsync();
        if (maybeStream == null)
            return RulesViewModel.Empty;

        var deserialized = await DeserializeAsync(maybeStream);
        return deserialized!;
    }

    public async Task<IRandomRuleViolationViewModel> GetRandomRuleViolationAsync()
    {
        static int sfwSorter(SmallMediumDocument medium)
        {
            return medium.Rating switch
            {
                Rating.Safe => 0,
                Rating.Explicit or Rating.Questionable => 1,
                Rating.Unknown or _ => 2
            };
        }

        var random = new Random();
        int randomSorter(SmallMediumDocument medium)
        {
            return random.Next();
        }

        var allMedia = await mediumService.GetAllSearchableAsync(isSfwOverride: false);
        var groupedMedia = allMedia
            .OrderBy(sfwSorter)
            .ThenBy(randomSorter)
            .ToList();

        var rulesViewModel = await GetAllAsync();

        foreach (var medium in groupedMedia)
        {
            var hasViolations = rulesViewModel.Rules
                .Any(r => RuleViolations.ExistBetween(r, medium));

            if (hasViolations)
            {
                if (sfwService.IsSfw && medium.Rating != Rating.Safe)
                    return RandomRuleViolations.Invisible;
                return RandomRuleViolations.Visible(medium.Id);
            }
        }

        return RandomRuleViolations.None;
    }

    public async Task<MediumRuleViolationsViewModel?> GetViolationsAsync(Guid mediumId)
    {
        var rulesViewModel = await GetAllAsync();
        var medium = await mediumService.GetEnforceableAsync(mediumId);

        if (medium == null)
            return null;

        var violatedRules = rulesViewModel.Rules
            .Where(r => RuleViolations.ExistBetween(r, medium))
            .ToList();

        return new MediumRuleViolationsViewModel(violatedRules);
    }

    public async Task<RulesViewModel?> PutAsync(RulesViewModel rules)
    {
        using var memoryStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(memoryStream, rules);
        memoryStream.Seek(0, SeekOrigin.Begin);
        await storage.StoreAsync(memoryStream);
        return rules;
    }

    private static async Task<RulesViewModel?> DeserializeAsync(Stream stream)
    {
        try
        {
            return await JsonSerializer.DeserializeAsync<RulesViewModel>(stream, JsonSerialization.DefaultOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public bool Validate(RulesViewModel viewModel)
    {
        if (viewModel == null)
            return false;

        new RulesViewModelValidator().ValidateAndThrow(viewModel);
        return true;
    }

}
