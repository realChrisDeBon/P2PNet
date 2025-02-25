using P2PBootstrap.CLI.Command.ICommand;
using P2PBootstrap.Encryption;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

namespace P2PBootstrap.CLI.Command.CommandImplementations
{
    public class Key_cmd : ICommand.ICommand
    {
        public string Command { get; set; }
        public string Description { get; set; }
        public string Help { get; set; }
        public Dictionary<string, ICommandArg> Args { get; set; }
        public Func<List<ICommandArg>, CommandResponse> CommandDelegate { get; set; }

        public Key_cmd()
        {
            Command = "key";
            Description = "Handles key generation and listing.";
            Help = "Use 'key gen' to generate a new key and 'key list' to list all keys.";
            Args = new Dictionary<string, ICommandArg>
            {
                {"gen", new CommandArg { Arg = "gen", Description = "Generate a new key", CommandArgDelegate = GenerateKey  } },
                {"list", new CommandArg { Arg = "list", Description = "List all keys", CommandArgDelegate = ListKeys } }
            };

            CommandDelegate = ExecuteCommand;
        }

        public CommandResponse ExecuteCommand(List<ICommandArg> args)
        {
            if (args == null || args.Count == 0)
            {
                DebugMessage("No arguments provided.");
                return new CommandResponse { Success = false, Response = "No arguments provided." };
            }

            var arg = args[0];


            if (arg != null && Args.ContainsKey(arg.Arg))
            {
                var commandArg = Args[arg.Arg];
                var commandResponse = commandArg.CommandArgDelegate?.Invoke(arg.Arg);
                return commandResponse;
            }
            else
            {
                DebugMessage($"Unknown argument: {arg.Arg}");
                return new CommandResponse { Success = false, Response = $"Unknown argument: {arg.Arg}" };
            }
        }

        private CommandResponse GenerateKey(string arg)
        {
            // Implement key generation logic here
            CommandResponse cr = new CommandResponse();
            EncryptionService.GenNewPGPKey(arg, ref cr);
            return cr;
        }

        private CommandResponse ListKeys(string arg)
        {
            // Implement key listing logic here
            return new CommandResponse { Response = "Listing all keys...", Success = true };
        }
    }
}
