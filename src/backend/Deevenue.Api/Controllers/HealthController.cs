using System.Text.Json.Serialization;
using Deevenue.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Deevenue.Api.Controllers;

public class HealthController : DeevenueApiControllerBase
{
    private static readonly HealthViewModel healthy = new(HealthStatus.Pass);

    [HttpGet("", Name = "getHealthStatus")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400, Type = typeof(HealthViewModel))]
    public ActionResult<HealthViewModel> Get()
    {
        return Ok(healthy);
    }

    public record HealthViewModel(HealthStatus Status);

    [JsonConverter(typeof(LowercaseJsonStringEnumConverter))]
    public enum HealthStatus
    {
        Pass,
        Warn,
        Fail
    }
}
