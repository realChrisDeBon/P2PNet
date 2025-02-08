global using static P2PNet.PeerNetwork;
global using P2PNet.Distribution;
global using static P2PNet.Distribution.Distribution_Protocol;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using P2PNet;
using P2PNet.Distribution;
using P2PNet.NetworkPackets;
using P2PNet.Peers;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace P2PBootstrap
{
    public class Program
    {
        // get the appsettings.json file imported
        public static IConfiguration AppSettings;
        public const string ConfigFile = "appsettings.json";

        public static void Main(string[] args)
        {
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

            app.UseRouting();

            // Endpoint to Get Peers
            app.MapGet("/api/Bootstrap/peers", () =>
            {
                string serialized = Serialize(new CollectionSharePacket(100, KnownPeers));
                return Results.Content(serialized, "application/json");
            });


            KnownPeers.Add(new GenericPeer() { Address = "127.0.0.1", Port = 5000 });

            string test = Serialize(new CollectionSharePacket(100, KnownPeers));
            Console.WriteLine(test);

           
            app.Run();
        }

    }

    [JsonSerializable(typeof(CollectionSharePacket))]
    [JsonSerializable(typeof(IPeer))]
    [JsonSerializable(typeof(GenericPeer))]
    [JsonDerivedType(typeof(GenericPeer), typeDiscriminator: "GenericPeer")]
    internal partial class AppJsonSerializerContext : JsonSerializerContext
    {
    }
}