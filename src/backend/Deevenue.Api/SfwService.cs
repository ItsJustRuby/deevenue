using Deevenue.Api.Framework;
using Deevenue.Domain;

namespace Deevenue.Api;

internal class SfwService(IHttpContextAccessor httpContext) : ISfwService
{
    public bool IsSfw => httpContext.HttpContext!.DeevenueSession().IsSfw;
}
