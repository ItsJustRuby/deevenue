
namespace Deevenue.Api.Tests.Framework;

internal class ThenFixture
{
    public static readonly ThenFixture Then = new();
    public readonly ResponseFixture Response = new();
}
