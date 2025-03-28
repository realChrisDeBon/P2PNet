namespace P2PBootstrap.CLI.Command
{
    public record CommandResponse
    {
        public string Response { get; set; }
        public bool Success { get; set; }
    }
}
