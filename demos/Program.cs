using demos.Commands.AzureUserRootCommand;
using demos.Commands.DaprRootCommand;
using demos.Commands.DebugRootCommand;
using Spectre.Console;
using System.CommandLine;


var rootCmd = new RootCommand("A variaty of different demos");

var showFiglets = new Option<bool>(
    name: "--show", description: "Show CSU Figlets");
rootCmd.AddOption(showFiglets);


rootCmd.AddCommand(new AzureUserCommand());
rootCmd.AddCommand(new DaprRootCommand());
rootCmd.AddCommand(new DebugCommand());

rootCmd.SetHandler((show) =>
{
    DisplayFiglets(show);
}, showFiglets);

void DisplayFiglets(bool show)
{
    if(show)
    {
        AnsiConsole.Write(new FigletText(FigletFont.Load("starwars.flf"), "DK CSU").Color(Color.Red));
        AnsiConsole.Write(new FigletText(FigletFont.Load("starwars.flf"), "App Inno").Color(Color.Blue));
    }
}

await rootCmd.InvokeAsync(args);
//Console.ReadLine();