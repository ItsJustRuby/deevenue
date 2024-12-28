namespace Deevenue.Api.Framework;

public class HeaderAuthMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        // Health checks from within the same container should succeed.
        if (context.Request.Host.HasValue && context.Request.Host.Host == "localhost")
            return next(context);

        var hasHeader = context.Request.Headers.TryGetValue(Config.Auth.HeaderName, out var actualHeaderValues);

        if (!hasHeader || !actualHeaderValues.Contains(Config.Auth.HeaderValue))
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        }

        SentrySdk.ConfigureScope(scope =>
        {
            scope.User.Id = Config.Auth.HeaderValue;
            scope.User.Username = Config.Auth.HeaderValue;
        });

        return next(context);
    }
}
