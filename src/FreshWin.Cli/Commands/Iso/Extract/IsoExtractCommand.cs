
using FreshWin.Deployment.IsoManagement;
using FreshWin.Deployment.IsoManagement.Exceptions;
using FreshWin.Deployment.IsoManagement.Extensions;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace FreshWin.Cli.Commands.Iso.Extract
{
    [Description("Extracts an ISO file to a specified directory")]
    internal class IsoExtractCommand : Command<IsoExtractCommandSettings>
    {
        protected override int Execute(CommandContext context, IsoExtractCommandSettings settings, CancellationToken cancellationToken)
        {
            if (!settings.IsoPath.Exists)
            {
                throw new FileNotFoundException($"The specified file does not exist: {settings.IsoPath.FullName}");
            }

            if (settings.IsoPath.IsNotIsoUdf())
            {
                throw new IsoFileNotUdfException(settings.IsoPath);
            }

            settings.DestinationPath ??= new DirectoryInfo(
                Path.Combine(
                    settings.IsoPath.DirectoryName!,
                    Path.GetFileNameWithoutExtension(settings.IsoPath.Name)));

            if (File.Exists(settings.DestinationPath.FullName))
            {
                AnsiConsole.MarkupLine("[red]Extraction cancelled — destination path is a file.[/]");
                return 1;
            }

            if (!settings.DestinationPath.Exists)
            {
                bool shouldCreate = settings.CreateDestination
                    || AnsiConsole.Confirm(
                        $"Destination directory [yellow]{Markup.Escape(settings.DestinationPath.FullName)}[/] does not exist. Create it?");

                if (!shouldCreate)
                {
                    AnsiConsole.MarkupLine("[red]Extraction cancelled — destination directory was not created.[/]");
                    return 1;
                }

                settings.DestinationPath.Create();
            }

            AnsiConsole.Progress().Start(ctx =>
            {
                var task = ctx.AddTask("Extracting ISO");
                var progress = new Progress<IsoExtractionProgress>(p => task.Value = p.Percent);

                settings.IsoPath.ExtractIsoToDirectory(settings.DestinationPath, progress);
            });

            return 0;
        }
    }
}
