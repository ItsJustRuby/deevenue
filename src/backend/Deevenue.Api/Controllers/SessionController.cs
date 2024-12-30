using Deevenue.Api.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Deevenue.Api.Controllers;

public class SessionController : DeevenueApiControllerBase
{
    [HttpPatch(Name = "updateSession")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public SessionViewModel Update([FromBody] SessionUpdateParameters sessionUpdateParameters)
    {
        var session = HttpContext.DeevenueSession();
        session.IsSfw = sessionUpdateParameters.IsSfw;
        return new SessionViewModel(session.IsSfw);
    }

    [HttpGet(Name = "getSession")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public SessionViewModel Get()
    {
        var session = HttpContext.DeevenueSession();
        return new SessionViewModel(session.IsSfw);
    }

    public class SessionUpdateParameters
    {
        public bool IsSfw { get; set; }
    }
}
