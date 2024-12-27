using System.Text.Json;
using Deevenue.Domain.Thumbnails;

namespace Deevenue.Api.Framework;

internal class ThumbnailSizeConstraint : IRouteConstraint
{
    public bool Match(
        HttpContext? httpContext,
        IRouter? route,
        string routeKey,
        RouteValueDictionary values,
        RouteDirection routeDirection)
    {
        if (!values.ContainsKey(routeKey))
            return false;

        try
        {
            var abbr = JsonSerializer.Deserialize<ThumbnailSizeAbbreviation>($"\"{values[routeKey] as string}\"");
            return ThumbnailSizes.IsKnown(abbr);
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
