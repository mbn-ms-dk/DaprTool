using demos.Commands.DaprStatestoreCommand;
using System.CommandLine;

var rootCmd = new RootCommand("Demos as dotnet tool");
rootCmd.AddCommand(new StatestoreRootCommand());
return await rootCmd.InvokeAsync(args);