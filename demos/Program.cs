using demos.Commands.DaprBindingCommand;
using demos.Commands.DaprStatestoreCommand;
using System.CommandLine;

var rootCmd = new RootCommand("Demos...");
rootCmd.AddCommand(new StatestoreRootCommand());
rootCmd.AddCommand(new BindingRootCommand());
 await rootCmd.InvokeAsync(args);
Console.ReadLine();