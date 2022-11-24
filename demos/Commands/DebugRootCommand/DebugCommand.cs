using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace demos.Commands.DebugRootCommand
{
    public class DebugCommand : Command
    {
        public DebugCommand() : base("debug", "Helper for debugging purposes")
        {
            this.SetHandler(() =>
            {
                ShowDirectoryInfo();
            });
        }

        private void ShowDirectoryInfo()
        {
            AnsiConsole.MarkupLineInterpolated($"[green]Launched from {Environment.CurrentDirectory}[/]");
            AnsiConsole.MarkupLineInterpolated($"[green]Physical location {AppDomain.CurrentDomain.BaseDirectory}[/]");
            AnsiConsole.MarkupLineInterpolated($"[green]AppContext.BaseDir {AppContext.BaseDirectory}[/]");
            AnsiConsole.MarkupLineInterpolated($"[green]Runtime Call {Path.GetDirectoryName(Process.GetCurrentProcess()?.MainModule?.FileName)}[/]");
        }
    }
}
