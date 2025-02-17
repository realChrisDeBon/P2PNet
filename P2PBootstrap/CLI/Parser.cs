using System.IO;

namespace P2PBootstrap.CLI
{
    public static class Parser
    {
        public static bool ParserRunning { get; set; } = true;
        public static Queue<string> InputQueue = new Queue<string>();
        public static Queue<string> OutputQueue = new Queue<string>();
        private static int lastProcessedLine = 0;

        public static void Initialize()
        {
            if (ParserRunning == true)
            {
                ParserRunning = true;
            }

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
            

            DebugMessage($"Admin console input: {input}", MessageType.General);
            return $"Processed: {input}";
        }
    }
}
