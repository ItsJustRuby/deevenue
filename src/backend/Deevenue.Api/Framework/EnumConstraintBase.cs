using System.Text.Json;

namespace Deevenue.Api.Framework;

internal abstract class EnumConstraintBase<TEnum> : IRouteConstraint where TEnum : Enum
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

        var stringValue = values[routeKey] as string;

        try
        {
            JsonSerializer.Deserialize<TEnum>($"\"{stringValue}\"");
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
