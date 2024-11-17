using Deevenue.Infrastructure.Jobs;
using Microsoft.AspNetCore.Mvc;

namespace Deevenue.Api.Controllers;

public class JobController(
    IJobResultService jobsService,
    IJobsSummary jobsSummary) : DeevenueApiControllerBase
{
    [HttpGet("running", Name = "getRunningJobs")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<JobsViewModel>> GetRunning()
    {
        var jobSummary = await jobsSummary.GetRunningAsync();

        var jobKindViewModels = new List<JobKindViewModel>();
        foreach (var kvp in jobSummary)
        {
            var jobKindName = kvp.Key;
            var jobs = kvp.Value;

            var jobViewModels = jobs.Select(j => new JobViewModel(j.MediumId)).ToList();
            jobKindViewModels.Add(new(jobKindName, jobViewModels));
        }

        return Ok(new JobsViewModel(jobKindViewModels));
    }

    [HttpGet("result", Name = "getJobResults")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<JobResultsViewModel>> GetResults()
    {
        var results = await jobsService.GetAllAsync();
        return Ok(new JobResultsViewModel(results.OrderBy(r => r.InsertedAt).ToList()));
    }

    [HttpDelete("result/{id:Guid}", Name = "deleteJobResult")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteResult(Guid id)
    {
        var success = await jobsService.DeleteAsync(id);
        if (!success)
            return NotFound();
        return Ok();
    }
}

public record JobViewModel(Guid? MediumId);
public record JobKindViewModel(string Kind, IReadOnlyList<JobViewModel> Jobs);
public record JobsViewModel(IReadOnlyList<JobKindViewModel> Kinds);
public record JobResultsViewModel(IReadOnlyList<JobResultViewModel> Results);
