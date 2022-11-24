using demos.Commands.DaprRootCommand.DaprBindingCommands;
using demos.Commands.DaprRootCommand.DaprPubSubCommands;
using demos.Commands.DaprRootCommand.DaprStatestoreCommands;
using System.CommandLine;

namespace demos.Commands.DaprRootCommand
{
    public class DaprRootCommand : Command
    {
        public DaprRootCommand() : base("dapr", "Collection of available Dapr demos")
        {
            var option = new Option<bool>("bob", "bob");
            AddGlobalOption(option);
            AddCommand(new StatestoreCommand());
            AddCommand(new BindingCommand());
            AddCommand(new PubSubCommand());
        }
    }
}
