namespace P2PBootstrap.CLI.Command.ICommand
{
    public interface ICommand
    {
        public string Command { get; set; }
        public string Description { get; set; }
        public string Help { get; set; }
        public List<ICommandArg> Args { get; set; }
        public Action<List<ICommandArg>> CommandDelegate { get; set; }
    }
}
