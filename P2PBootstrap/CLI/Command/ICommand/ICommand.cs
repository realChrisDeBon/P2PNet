using P2PBootstrap.CLI.Command.ICommand;

namespace P2PBootstrap.CLI.Command.ICommand
{
    public interface ICommand
    {
        public string Command { get; set; }
        public string Description { get; set; }
        public string Help { get; set; }
        public Dictionary<string, ICommandArg> Args { get; set; }
        public Func<List<ICommandArg>, CommandResponse> CommandDelegate { get; set; }

        public CommandResponse ExecuteCommand(List<ICommandArg> args)
        {
            if (args == null || args.Count == 0)
            {
                DebugMessage("No arguments provided.");
                return new CommandResponse { Success = false, Response = "No arguments provided." };
            }

            var arg = args[0];
            var commandResponse = new CommandResponse();

            if (arg != null && Args.ContainsKey(arg.Arg))
            {
                var commandArg = Args[arg.Arg];
                commandResponse = commandArg.CommandArgDelegate?.Invoke(arg.Arg);
                return commandResponse;
            }
            else
            {
                DebugMessage($"Unknown argument: {arg.Arg}");
                return new CommandResponse { Success = false, Response = $"Unknown argument: {arg.Arg}" };
            }
        }


    }
}
