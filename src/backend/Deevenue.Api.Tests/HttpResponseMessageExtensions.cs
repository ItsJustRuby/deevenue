using System.Text.Json;
using Deevenue.Domain;

namespace Deevenue.Api.Tests;

internal static class HttpResponseMessageExtensions
{
    public static async Task<T> JsonAsync<T>(this HttpResponseMessage message)
    {
        using var contentStream = await message.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        return (await JsonSerializer.DeserializeAsync<T>(
                contentStream, JsonSerialization.DefaultOptions, TestContext.Current.CancellationToken))!;
    }
}
