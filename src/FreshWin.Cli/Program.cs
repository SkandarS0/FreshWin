using FreshWin.Cli.Commands.Iso.Extract;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(config =>
{
    config
    .SetApplicationName("fwin")
    .UseAssemblyInformationalVersion()
    .AddBranch("deploy", deploy =>
    {
        deploy.AddBranch("iso", iso =>
        {
            iso.AddCommand<IsoExtractCommand>("extract").WithExample(["deploy", "iso", "extract", "path/to/source.iso", "path/to/destination"]);
        });
    });
});

return await app.RunAsync(args);