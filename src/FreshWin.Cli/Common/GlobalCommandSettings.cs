using Spectre.Console.Cli;
using System.ComponentModel;

namespace FreshWin.Cli.Common
{
    internal abstract class GlobalCommandSettings : CommandSettings
    {
        [CommandOption("--root <PATH>")]
        [Description("The root directory where the Projects directory is located.")]
        public DirectoryInfo? Root { get; set; }

        public DirectoryInfo ResolveRoot()
        {
            if (Root is not null)
            {
                return Root;
            }

#if DEBUG
            return new(Path.Combine(Environment.CurrentDirectory, "playground"));
#else
            return new(AppContext.BaseDirectory);
#endif
        }
    }
}
