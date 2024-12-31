using System.Text.Json;
using Deevenue.Domain;

namespace Deevenue.Api.Tests.Framework;

internal class ResponseFixture
{
    private HttpResponseMessage? response;

    public HttpResponseMessage Value
    {
        get
        {
            if (response == null)
                throw new InvalidOperationException("You must execute a request before expecting a response");
            return response;
        }
    }

    public TViewModel AsViewModel<TViewModel>()
    {
        var task = GetJsonAsync<TViewModel>(Value);
        task.Wait();
        return task.Result;
    }

    internal void UpdateWith(HttpResponseMessage response) => this.response = response;

    private static async Task<T> GetJsonAsync<T>(HttpResponseMessage message)
    {
        using var contentStream = await message.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        return (await JsonSerializer.DeserializeAsync<T>(
                contentStream, JsonSerialization.DefaultOptions, TestContext.Current.CancellationToken))!;
    }
}
