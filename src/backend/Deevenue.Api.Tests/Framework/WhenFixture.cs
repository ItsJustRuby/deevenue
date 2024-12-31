
using Microsoft.AspNetCore.Mvc.Testing;

namespace Deevenue.Api.Tests.Framework;

internal class WhenFixture
{
    public static readonly WhenFixture When = new();

    public void UsingApiClient(
        Func<HttpClient, HttpResponseMessage> func,
        Action<WebApplicationFactoryClientOptions>? configureOptions = null)
    {
        using var client = GetClient(configureOptions);
        Then.Response.UpdateWith(func(client));
    }

    public async Task UsingApiClient(
        Func<HttpClient, Task<HttpResponseMessage>> asyncFunc,
        Action<WebApplicationFactoryClientOptions>? configureOptions = null)
    {
        using var client = GetClient(configureOptions);
        Then.Response.UpdateWith(await asyncFunc(client));
    }

    private static HttpClient GetClient(Action<WebApplicationFactoryClientOptions>? configureOptions)
    {
        var options = new WebApplicationFactoryClientOptions();
        configureOptions?.Invoke(options);
        return ApiFixture.Instance.CreateClient(options);
    }
}
