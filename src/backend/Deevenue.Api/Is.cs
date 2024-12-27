using System.Reflection;

namespace Deevenue.Api;

internal static class Is
{
    // See https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-9.0&tabs=visual-studio#customizing-run-time-behavior-during-build-time-document-generation
    public static bool OnlyGeneratingOpenAPIDefinitions =>
        Assembly.GetEntryAssembly()!.GetName().Name == "GetDocument.Insider";
}
