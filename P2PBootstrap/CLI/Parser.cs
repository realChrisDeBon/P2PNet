using P2PBootstrap.CLI.Command;
using P2PBootstrap.CLI.Command.CommandImplementations;
using P2PBootstrap.CLI.Command.ICommand;
using System.IO;

namespace P2PBootstrap.CLI
{
    public static class Parser
    {
        public static bool ParserRunning { get; set; } = true;
        public static Queue<string> InputQueue = new Queue<string>();
        public static Queue<string> OutputQueue = new Queue<string>();
        private static int lastProcessedLine = 0;
        public static Dictionary<string, ICommand> Commands = new Dictionary<string, ICommand>()
        {
            {"key", new Key_cmd() },
        };

        public static void Initialize()
        {

            while (ParserRunning == true)
            {
                if (InputQueue.Count > 0)
                {
                    string input = InputQueue.Dequeue();
                    string output = ProcessInput(input);
                    OutputQueue.Enqueue(output);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        public static string ProcessInput(string input)
        {
            // add input to DB log table
            Task.Run(() => { ExecuteTableCommand(LogsCLI_table.RunInsertCommand(input, AdminConsoleLog)); });

            CommandResponse cr = ProcessCommand(input);

            if(cr.Success == true)
            {
                DebugMessage(cr.Success.ToString(), MessageType.Warning);
                UpdateMostRecentLogProcessed(true);
                return $"{cr.Response}";
            }

            return $"{cr.Response}";
        }

        public static CommandResponse ProcessCommand(string command)
        {
            var args = command.Split(' ');
            if (args.Length == 0)
            {
                return new CommandResponse { Response = "No command provided.", Success = false };
            }

            var commandKey = args[0];
            if (Commands.ContainsKey(commandKey))
            {
                var commandArgs = new List<ICommandArg>
                {
                    new CommandArg { Arg = string.Join(' ', args.Skip(1)) }
                };
                CommandResponse cr = Commands[commandKey].ExecuteCommand(commandArgs);
                return cr;
            }
            else
            {
                return new CommandResponse { Response = $"Unknown command: {commandKey}", Success = false };
            }
        }

    }
}
