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
            AddCommand(new StatestoreCommand());
            AddCommand(new BindingCommand());
            AddCommand(new PubSubCommand());
        }
    }
}
