global using static P2PNet.PeerNetwork;
global using static P2PNet.Distribution.Distribution_Protocol;
global using static ConsoleDebugger.ConsoleDebugger;
global using static P2PBootstrap.GlobalConfig;
global using static P2PBootstrap.Database.DatabaseService;
global using P2PNet.Distribution;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using P2PNet;
using P2PNet.Distribution;
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

namespace P2PBootstrap
{
    public class Program
    {

        public static void Main(string[] args)
        {
            LoggingConfiguration.LoggerStyle = LogStyle.PlainTextFormat;
            LoggingConfiguration.LoggerActive = true;

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(ConfigFile, optional: false, reloadOnChange: true);

            AppSettings = config.Build();

            var builder = WebApplication.CreateBuilder(args);
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

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(DBdirectory),
                RequestPath = "/localdb",
                ServeUnknownFileTypes = true,
                DefaultContentType = "application/octet-stream"
            });

            app.UseRouting();

            // Endpoint to Get Peers
            app.MapGet("/api/Bootstrap/peers", () =>
            {
                string serialized = Serialize(new CollectionSharePacket(100, KnownPeers));
                return Results.Content(serialized, "application/json");
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


            KnownPeers.Add(new GenericPeer() { Address = "127.0.0.1", Port = 5000 });

            string test = Serialize(new CollectionSharePacket(100, KnownPeers));
            Console.WriteLine(test);

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
