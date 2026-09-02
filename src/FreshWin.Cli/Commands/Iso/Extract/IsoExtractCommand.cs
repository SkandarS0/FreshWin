
using FreshWin.Deployment.IsoManagement;
using FreshWin.Deployment.IsoManagement.Exceptions;
using FreshWin.Deployment.IsoManagement.Extensions;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace FreshWin.Cli.Commands.Iso.Extract
{
    [Description("Extracts an ISO file to a specified directory")]
    internal class IsoExtractCommand : AsyncCommand<IsoExtractCommandSettings>
    {
        protected override async Task<int> ExecuteAsync(CommandContext context, IsoExtractCommandSettings settings, CancellationToken cancellationToken)
        {
            if (!TryPrepareDestination(settings, out int exitCode))
            {
                return exitCode;
            }

            try
            {
                await AnsiConsole.Progress().StartAsync(async ctx =>
                {
                    var task = ctx.AddTask("Extracting ISO");
                    var progress = new Progress<IsoExtractionProgress>(p => task.Value = p.Percent);
                    await settings.IsoPath.ExtractIsoToDirectory(settings.DestinationPath!, progress, settings.Overwrite, cancellationToken);
                });
            }
            catch (OperationCanceledException)
            {
                AnsiConsole.MarkupLine("[yellow]Extraction cancelled.[/]");
                return 1;
            }

            return 0;
        }

        private static bool TryPrepareDestination(IsoExtractCommandSettings settings, out int exitCode)
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
                Path.Combine(settings.IsoPath.DirectoryName!, Path.GetFileNameWithoutExtension(settings.IsoPath.Name)));

            if (File.Exists(settings.DestinationPath.FullName))
            {
                AnsiConsole.MarkupLine("[red]Extraction cancelled — destination path is a file.[/]");
                exitCode = 1;
                return false;
            }

            if (!settings.DestinationPath.Exists)
            {
                bool shouldCreate = settings.CreateDestination
                    || AnsiConsole.Confirm($"Destination directory [yellow]{Markup.Escape(settings.DestinationPath.FullName)}[/] does not exist. Create it?");

                if (!shouldCreate)
                {
                    AnsiConsole.MarkupLine("[red]Extraction cancelled — destination directory was not created.[/]");
                    exitCode = 1;
                    return false;
                }

                settings.DestinationPath.Create();
            }

            exitCode = 0;
            return true;
        }
    }
}
