﻿namespace P2PBootstrap.CLI.Command.ICommand
{
    public interface ICommandArg
    {
        public string Arg { get; set; }
        public string Description { get; set; }
        public Func<string, CommandResponse> CommandArgDelegate { get; set; }
    }
}
