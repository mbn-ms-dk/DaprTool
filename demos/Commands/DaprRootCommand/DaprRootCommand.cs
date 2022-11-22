using demos.Commands.DaprRootCommand.DaprBindingCommand;
using demos.Commands.DaprRootCommand.DaprStatestoreCommand;
using System.CommandLine;

namespace demos.Commands.DaprRootCommand
{
    public class DaprRootCommand : Command
    {
        public DaprRootCommand() : base("dapr", "Collection of available Dapr demos")
        {
            AddCommand(new StatestoreCommand());
            AddCommand(new BindingCommand());
        }
    }
}
