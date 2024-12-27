using Deevenue.Cli;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.AddCommand<Import>("import");
    config.AddCommand<TestConnection>("test");
});

return app.Run(args);
