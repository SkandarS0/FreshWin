using FreshWin.Cli.Common;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace FreshWin.Cli.Commands.Init
{
    internal sealed class InitCommandSettings : GlobalCommandSettings
    {
        [CommandArgument(0, "<Name>")]
        [Description("Name of the project to create.")]
        public required string Name { get; init; }
    }
}
