using Deevenue.Cli.Networking;
using Spectre.Console.Cli;

namespace Deevenue.Cli;

internal class TestConnection : Command
{
    public override int Execute(CommandContext context)
    {
        using var reverseProxy = ReverseProxy.StartNew();
        Console.WriteLine("Connection to backend successful!");
        return 0;
    }
}
