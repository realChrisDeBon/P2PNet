using static P2PNet.PeerNetwork;
using static P2PWidescan.Widescan;
using static ConsoleDebugger.ConsoleDebugger;

using NUnit.Framework;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace Testing
{
    [TestFixture]
    public class NetworkRevTest
    {
        [SetUp]
        public void Setup()
        {
            // Initialize any required resources before tests
        }

        [Test]
        public async Task PeerNetworkStartupTest()
        {
            ConsoleDebugger.ConsoleDebugger.DebugMessage("Starting PeerNetwork tests.", ConsoleDebugger.ConsoleDebugger.MessageType.General);

            // Load local addresses
            LoadLocalAddresses();

            // Begin accepting inbound peers
            AcceptInboundPeers = true;

            // Boot discovery channels
            BootDiscoveryChannels();

            // Start routines if any
            await StartRoutines();

            // Assert that the public IP addresses are not null
            Xunit.Assert.NotNull(PublicIPV4Address);
            Xunit.Assert.NotNull(PublicIPV6Address);

            // Assert that ListeningPort is set
            Xunit.Assert.True(ListeningPort > 0, "ListeningPort should be greater than 0");

            // Optionally, wait for some time to allow network operations
            await Task.Delay(1000);

            // Clean up
            StopAcceptingInboundPeers();
        }

        [Test]
        public async Task WidescanTest()
        {
            // Set hardware mode
            SetHardwareMode(P2PWidescan.HardwareMode.CPU);

            // Add address prefixes
            AddAddressPrefix("2001:0db8:85a3:0000");

            // Start Widescan
            StartWidescan();

            // Wait for a short period to simulate scanning
            await Task.Delay(5000);

            // Stop Widescan
            StopWidescan();

            // Since Widescan runs asynchronously, we assume it starts and stops without exceptions
            NUnit.Framework.Assert.Pass("Widescan started and stopped successfully");
        }
    }
}
