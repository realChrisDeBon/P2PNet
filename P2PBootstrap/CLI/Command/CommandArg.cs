using P2PBootstrap.CLI.Command.ICommand;

namespace P2PBootstrap.CLI.Command
{
    public class CommandArg : ICommandArg
    {
        public string Arg { get; set; }
        public string Description { get; set; }
        public Func<string, CommandResponse> CommandArgDelegate { get; set; }
    }
}
