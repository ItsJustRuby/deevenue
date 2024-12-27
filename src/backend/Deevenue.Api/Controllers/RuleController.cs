using Deevenue.Domain.Rules;
using Microsoft.AspNetCore.Mvc;

namespace Deevenue.Api.Controllers;

public class RuleController(IRulesService rulesService) : DeevenueApiControllerBase
{
    [HttpGet("", Name = "getRules")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<RulesViewModel>> Get()
    {
        return await rulesService.GetAllAsync();
    }

    [HttpGet("violation/{mediumId:Guid}", Name = "getRuleViolationsForMedium")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<MediumRuleViolationsViewModel>> GetViolationsFor(Guid mediumId)
    {
        var result = await rulesService.GetViolationsAsync(mediumId);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("violation/random", Name = "getRandomRuleViolation")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<NotificationViewModel>> GetRandomViolation()
    {
        var result = await rulesService.GetRandomRuleViolationAsync();
        return Ok(result.Accept(new Visitor()));
    }

    [HttpPost("", Name = "uploadRules")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<RulesViewModel>> Post(RulesViewModel r)
    {
        var rules = await rulesService.PutAsync(r!);

        if (rules != null)
            return rules;

        return BadRequest();
    }

    [HttpPost("validation", Name = "validateRules")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public ActionResult<bool> Validate(RulesViewModel r)
    {
        // Note: Half of the validation happens during model binding
        // (e.g. everything that already breaks during JSON deserialization),
        // the other half during the call below.

        return rulesService.Validate(r!);
    }

    [HttpDelete("{index:int}", Name = "deleteRuleAtIndex")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<RulesViewModel>> DeleteRule([FromRoute] int index)
    {
        var result = await rulesService.DeleteAsync(index);
        if (result == null)
            return BadRequest();
        return Ok(result);
    }

    private class Visitor : IRandomRuleViolationViewModelVisitor<NotificationViewModel>
    {
        public NotificationViewModel VisitInvisible()
            => Notifications.CreateBuilder(NotificationLevel.Warning)
            .WithText("There are violations, but your SFW settings do not allow you to see them.")
            .Build();

        public NotificationViewModel VisitNone()
            => Notifications.CreateBuilder(NotificationLevel.Info)
            .WithText("No violations found")
            .Build();

        public NotificationViewModel VisitVisible(Guid mediumId)
            => Notifications.CreateBuilder(NotificationLevel.Info)
            .WithEntity(EntityKind.Medium, mediumId)
            .Build();
    }
}
