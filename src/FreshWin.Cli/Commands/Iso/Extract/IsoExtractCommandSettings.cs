using Spectre.Console.Cli;
using System.ComponentModel;

namespace FreshWin.Cli.Commands.Iso.Extract
{
    internal class IsoExtractCommandSettings : CommandSettings
    {
        [CommandArgument(0, "<ISO_PATH>")]
        [Description("Path to the source .iso file")]
        public required FileInfo IsoPath { get; set; }

        [CommandArgument(1, "[DESTINATION_PATH]")]
        [Description("Directory to extract into")]
        public DirectoryInfo? DestinationPath { get; set; }

        [CommandOption("-y|--create-destination")]
        [Description("Create the destination directory if it doesn't already exist")]
        public bool CreateDestination { get; set; }

        [CommandOption("--overwrite")]
        [Description("Overwrite existing files in the destination directory")]
        public bool Overwrite { get; set; }
    }
}
