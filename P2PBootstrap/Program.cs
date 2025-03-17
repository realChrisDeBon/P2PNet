global using static P2PNet.PeerNetwork;
global using static P2PNet.Distribution.DistributionProtocol;
global using static ConsoleDebugger.ConsoleDebugger;
global using static P2PBootstrap.GlobalConfig;
global using static P2PBootstrap.Database.DatabaseService;
global using P2PNet.Distribution;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using P2PNet;
using P2PNet.NetworkPackets;
using P2PNet.Peers;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using P2PBootstrap.CLI;
using System.IO;
using System.Text.Json;
using P2PBootstrap.Database;
using Microsoft.Extensions.FileProviders;
using ConsoleDebugger;
using P2PBootstrap.Encryption;
using P2PNet.Distribution.NetworkTasks;
using System.Text;

namespace P2PBootstrap
{
    public class Program
    {
        public static string PublicKeyToString => Encoding.UTF8.GetString(GlobalConfig.ActiveKeys.Public.KeyData);
        public static void Main(string[] args)
        {
            LoggingConfiguration.LoggerStyle = LogStyle.PlainTextFormat;
            LoggingConfiguration.LoggerActive = true;

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(ConfigFile, optional: false, reloadOnChange: true);

            AppSettings = config.Build();

            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.AddFilter("Microsoft", LogLevel.None);
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // Enable default files and static files
            app.UseDefaultFiles(); // Serves index.html by default
            app.UseStaticFiles();

            var DBdirectory = Path.Combine(Directory.GetCurrentDirectory(), "localdb");
            if (!Directory.Exists(DBdirectory))
            {
                Directory.CreateDirectory(DBdirectory);
            }

            app.UseRouting();

            app.MapPut("/api/Bootstrap/peers", async Task<IResult> (HttpContext context) =>
            {
                try
                {
                    // read the incoming PUT
                    using var reader = new StreamReader(context.Request.Body);
                    var bodyJson = await reader.ReadToEndAsync();

                    // deserialize the input
                    var incomingPacket = Deserialize<DataTransmissionPacket>(bodyJson);

                    if (GlobalConfig.TrustPolicy == TrustPolicies.BootstrapTrustPolicyType.Trustless)
                    {
                        // reply with a CollectionSharePacket
                        var share = new CollectionSharePacket(100, KnownPeers);
                        var responseJson = Serialize(share);
                        return Results.Content(responseJson, "application/json");
                    }
                    else
                    {

                        // reply with a DataTransmissionPacket holding public key and peer list
                        var networkTask = new NetworkTask()
                        {
                            TaskType = TaskType.SendPublicKey,
                            TaskData = new Dictionary<string, string>()
                                {
                                    { "PublicKey", PublicKeyToString },
                                    { "Peers", Serialize(new CollectionSharePacket(100, KnownPeers)) }
                                }
                        };

                        var outPacket = new DataTransmissionPacket()
                        {
                            DataType = DataPayloadFormat.Task,
                            Data = networkTask.ToByte()
                        };

                        var responseJson = Serialize(outPacket);
                        return Results.Content(responseJson, "application/json");
                    }
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            });

            app.MapGet("/api/parser/output", () =>
            {
                if (Parser.OutputQueue.Count > 0)
                {
                    return Results.Text(Parser.OutputQueue.Dequeue(), "text/plain");
                }
                return Results.NoContent();
            });
            app.MapPut("/api/parser/input", async (HttpContext context) =>
            {
                using var reader = new StreamReader(context.Request.Body);
                var input = await reader.ReadToEndAsync();
                Parser.InputQueue.Enqueue(input);
                return Results.Ok();
            });

            Task.Run(() => { Parser.Initialize(); });
            Task.Run(() => { EncryptionService.Initialize(); });
            Task.Run(() => { InitializeDatabase(); });

            app.Run();
        }

        private class InputModel
        {
            public string Input { get; set; }
        }
    }
}
