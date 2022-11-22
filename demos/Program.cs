using demos.Commands.AzureUserCommand;
using demos.Commands.DaprRootCommand;
using Spectre.Console;
using System.CommandLine;

AnsiConsole.Write(new FigletText("App Inno").Color(Color.Blue));
var rootCmd = new RootCommand("A variaty of different demos");

rootCmd.AddCommand(new AzureUserRootCommand());
rootCmd.AddCommand(new DaprRootCommand());

await rootCmd.InvokeAsync(args);
Console.ReadLine();