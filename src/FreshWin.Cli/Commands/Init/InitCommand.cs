using FreshWin.Core.Projects;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace FreshWin.Cli.Commands.Init
{
    [Description("Creates a new project")]
    internal sealed class InitCommand : AsyncCommand<InitCommandSettings>
    {

        protected override async Task<int> ExecuteAsync(CommandContext context, InitCommandSettings settings, CancellationToken cancellationToken)
        {
            var root = settings.ResolveRoot();
            if (string.IsNullOrWhiteSpace(settings.Name)
                || settings.Name is "." or ".."
                || settings.Name.Contains('/')
                || settings.Name.Contains('\\')
                || settings.Name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                AnsiConsole.MarkupLine("[red]Project name must be a valid single directory name.[/]");
                return 1;
            }

            var projectRoot = Path.Combine(root.FullName, "Projects", settings.Name);

            if (Directory.Exists(projectRoot))
            {
                AnsiConsole.MarkupLine($"[red]Project '{Markup.Escape(settings.Name)}' already exists.[/]");
                return 1;
            }

            if (File.Exists(projectRoot))
            {
                AnsiConsole.MarkupLine($"[red]A file with the name '{Markup.Escape(settings.Name)}' already exists.[/]");
                return 1;
            }

            ProjectLayoutWriter.Materialize(ProjectLayout.Root, projectRoot);

            AnsiConsole.MarkupLine($"[green]Initialized project '{Markup.Escape(settings.Name)}' at {Markup.Escape(projectRoot)}[/]");
            return 0;
        }
    }

}
