using System.IO;

namespace P2PBootstrap.CLI
{
    public static class Parser
    {
        public static bool ParserRunning { get; set; } = true;
        private static int lastProcessedLine = 0;

        public static void Initialize()
        {
            if (ParserRunning == true)
            {
                ParserRunning = true;
            }

            while (ParserRunning == true)
            {
                // TODO
                
            }
        }

        public static string ProcessInput(string input)
        {
            if(input.Contains( "test"))
            {
                Console.Beep(500, 500);
            }
            return $"Processed: {input}";
        }
    }
}
