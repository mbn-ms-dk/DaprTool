using demos.Commands.AzureUserRootCommand;
using demos.Commands.DaprRootCommand;
using demos.Commands.DebugRootCommand;
using demos.Helpers;
using Spectre.Console;
using System.CommandLine;

var rootCmd = new RootCommand("A variety of different demos");

var showHelp = new Option<bool>(
    name: "--readme", description: "Open browser and shows demos Tool readme");
rootCmd.AddOption(showHelp);

var showFiglets = new Option<bool>(
    name: "--show", description: "Show CSU Figlets");
rootCmd.AddOption(showFiglets);

rootCmd.AddCommand(new AzureUserCommand());
rootCmd.AddCommand(new DaprRootCommand());
rootCmd.AddCommand(new DebugCommand());

rootCmd.SetHandler(async (showFiglet, showHelp) =>
{
    await DisplayInfo(showFiglet, showHelp);
}, showFiglets, showHelp);

async Task DisplayInfo(bool show, bool showHelp)
{
    if (show)
    {
        AnsiConsole.Write(new FigletText(FigletFont.Load($"{AppDomain.CurrentDomain.BaseDirectory}starwars.flf"), "DK CSU").Color(Color.Red));
        AnsiConsole.Write(new FigletText(FigletFont.Load($"{AppDomain.CurrentDomain.BaseDirectory}starwars.flf"), "App Inno").Color(Color.Blue));
    }
    if (showHelp)
    {
        var cmd = $"start msedge https://github.com/mbn-ms-dk/DaprTool/blob/main/demos/documentation/demos.md";
        await Utils.RunCmd(cmd);
    }
}

await rootCmd.InvokeAsync(args);
