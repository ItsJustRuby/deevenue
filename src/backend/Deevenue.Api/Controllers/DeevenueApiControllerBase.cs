using Microsoft.AspNetCore.Mvc;

namespace Deevenue.Api.Controllers;

[Route("[controller]")]
public abstract class DeevenueControllerBase : ControllerBase { }

[ApiController]
[Produces("application/json")]
public abstract class DeevenueApiControllerBase : DeevenueControllerBase { }
