#if DEBUG
global using static ConsoleDebugger.ConsoleDebugger;
#endif
using P2PNet.DiscoveryChannels;
using P2PNet.Distribution;
using P2PNet.NetworkPackets;
using P2PNet.Peers;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace P2PNet
    {
    public static class PeerNetwork
        {

        // Some public facing settings for better user-defined control

        public static string NAME = "Test"; // placeholder, ignore for now
        static bool isBroadcaster = false;
        static Random random = new Random();

        /// <summary>
        /// Indicates whether to automatically throttle on connect.
        /// </summary>
        public static bool AutoThrottleOnConnect { get; set; } = true;

        /// <summary>
        /// Gets or sets the list of designated ports.
        /// </summary>
        public static List<int> designatedPorts { get; set; } = new List<int> { 8001, 8002, 8003 }; // Your designated ports

        /// <summary>
        /// Gets or sets the broadcaster port.
        /// </summary>
        public static int broadcasterPort;

        /// <summary>
        /// Gets the public IP address.
        /// </summary>
        public static IPAddress publicip;

        /// <summary>
        /// Gets the system MAC address.
        /// </summary>
        public static string MAC;


        private static Random randomizer = new Random();
        private static TcpListener listener;

        /// <summary>
        /// Gets the listening port for inbound connections.
        /// </summary>
        public static int ListeningPort { get; private set; }

        private static System.Timers.Timer cleanupTimer = new System.Timers.Timer();

        #region Connection Collections
        private static volatile List<IPeer> activePeers_ = new List<IPeer>();

        /// <summary>
        /// Gets or sets the list of known peers.
        /// </summary>
        public static List<IPeer> KnownPeers
            {
            get
                {
                return activePeers_;
                }
            set
                {
                activePeers_ = value;
                }
            }

        /// <summary>
        /// Queue of inbound connecting peers.
        /// </summary>
        /// <remarks>
        /// Inbound connections opened through <see cref="ListeningPort"/> will automatically be enqueued here. This allows you to implement any additional verification you may want in place before calling <see cref="AddPeer(IPeer, TcpClient)"/> and opening a PeerChannel
        /// </remarks>
        public static Queue<GenericPeer> InboundConnectingPeers = new Queue<GenericPeer>();

        internal static volatile List<PeerChannel> ActivePeerChannels = new List<PeerChannel>();
        private static List<LocalChannel> ActiveLocalChannels = new List<LocalChannel>();
        private static List<MulticastChannel> ActiveMulticastChannels = new List<MulticastChannel>();

        private static List<IPAddress> multicast_addresses = new List<IPAddress>();
        #endregion

        public static void InitiateLocalChannels(int designated_broadcast_port)
            {
            Queue<LocalChannel> badChannels = new Queue<LocalChannel>();
            bool error_occurred = false;
            foreach (LocalChannel channel_ in ActiveLocalChannels)
                {
                try
                    {
                    if (channel_.DESIGNATED_PORT != designated_broadcast_port)
                        {
                        Task.Run(() => channel_.Listener(channel_.DESIGNATED_PORT));
                        Task.Run(() => channel_.StartBroadcaster());
                        }
                    else
                        {
                        Task.Run(() => channel_.Listener(channel_.DESIGNATED_PORT));
                        }
                    }
                catch
                    {
                    error_occurred = true;
#if DEBUG
                    DebugMessage($"\tCannot setup local broadcast channel.", MessageType.Warning);
#endif
                    }
                finally
                    {
                    if (error_occurred == false)
                        {
#if DEBUG
                        DebugMessage($"\tSetup local channel on port: {channel_.DESIGNATED_PORT}");
#endif
                        }
                    }
                }
            while (badChannels.Count > 0)
                {
                ActiveLocalChannels.Remove(badChannels.Dequeue());
                }
            }
        public static void InitializeMulticaseChannels()
            {
            Queue<MulticastChannel> badChannels = new Queue<MulticastChannel>();
            foreach (MulticastChannel multicastChannel in ActiveMulticastChannels)
                {
                bool error_occurred = false;
                try
                    {
                    Task.Run(() => multicastChannel.StartListener());
                    Task.Run(() => multicastChannel.StartBroadcaster());
                    }
                catch
                    {
                    error_occurred = true;
#if DEBUG
                    DebugMessage($"\tCannot setup multicast channel on address: {multicastChannel.multicast_address.ToString()}", MessageType.Warning);
#endif
                    badChannels.Enqueue(multicastChannel);
                    }
                finally
                    {
                    if (error_occurred == false)
                        {
#if DEBUG
                        DebugMessage($"\tSetup multicast channel on address: {multicastChannel.multicast_address.ToString()}");
#endif
                        }
                    }
                }
            while (badChannels.Count > 0)
                {
                ActiveMulticastChannels.Remove(badChannels.Dequeue());
                }
            }

        /// <summary>
        /// Throttles the broadcasting down.
        /// </summary>
        /// <remarks>This greatly reduces the speed at which packets are sent out. This may be useful if you want to lessen the computational load while handling newly found connections.</remarks>
        public static void ThrottleBroadcastingDown()
            {
            foreach(MulticastChannel multicastChannel in ActiveMulticastChannels)
                {
                multicastChannel.DownThrottle();
                }
            foreach(LocalChannel localChannel in ActiveLocalChannels)
                {
                localChannel.DownThrottle();
                }
            }

        static PeerNetwork()
            {
            ListeningPort = randomizer.Next(8051, 9000);
            listener = new TcpListener(IPAddress.Any, ListeningPort);

            listener.Start();
            Task.Run(() => AcceptClientsAsync());

            cleanupTimer.Elapsed += DiscernPeerChannels;
            cleanupTimer.Interval = 7000;
            cleanupTimer.Stop(); // redundant engineering at its finest
            }

        static async Task AcceptClientsAsync()
            {
            while (true)
                {
                TcpClient client = await listener.AcceptTcpClientAsync();

                IPAddress peerIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
                // Check for existing peer
                if (KnownPeers.Any(p => p.IP.Equals(peerIP)))
                    {
#if DEBUG
                    DebugMessage("Duplicate connection attempt from existing peer. Ignoring.", MessageType.Warning);
#endif
                    client.Dispose();
                    }
                else
                    {
                    // Set inboud as GenericPeer - intentional gap before AddPeer to implement additional verifications as needed
                    InboundConnectingPeers.Enqueue(new GenericPeer(((IPEndPoint)client.Client.RemoteEndPoint).Address, ((IPEndPoint)client.Client.RemoteEndPoint).Port));
                    }
                }
            }

        /// <summary>
        /// Adds a peer to the known peers list and establishes a connection if a client is not provided.
        /// </summary>
        /// <param name="peer">The peer to add.</param>
        /// <param name="client">The TCP client associated with the peer.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks></remarks>
        public static async Task AddPeer(IPeer peer, TcpClient client = null)
            {
            List<IPeer> peers = KnownPeers;
            if ((KnownPeers.Any(p => p.IP.Equals(peer.IP))) || (KnownPeers.Any(p => p.Identifier.Equals(peer.Identifier))))
                {
#if DEBUG
                DebugMessage("Duplicate connection attempt from existing peer. Ignoring.", MessageType.Warning);
#endif
                return;
                }
            else if (peer.IP.ToString() == publicip.ToString() && peer.Port == ListeningPort)
                {
#if DEBUG
                DebugMessage("Listener broadcasted to iteself.");
#endif
                return;
                }
            else
                {
                if (client == null)
                    { 
                    try
                        {

                        IPEndPoint endpoint = new IPEndPoint(peer.IP, peer.Port);
                        peer.Client = new TcpClient(peer.IP.ToString(), peer.Port);
                        if (peer.Client.Connected == false)
                            {
                            peer.Client.Connect(peer.IP, peer.Port);
                            }

                        peer.Stream = peer.Client.GetStream();
                        await channelize();
                        }
                    catch (Exception e)
                        {
#if DEBUG
                        DebugMessage($"Issue opening trusted peer channel: {peer.IP.ToString()} @ port {peer.Port}\n{e.ToString()}", MessageType.Critical);
#endif
                        return;
                        }
                    }
                else // we're accepting an incoming client
                    {
                    try
                        {
                        peer.Client = client;
                        peer.Stream = peer.Client.GetStream();
                        await channelize();
                        }
                    catch (Exception e)
                        {
#if DEBUG
                        DebugMessage($"Issue accepting incoming peer: {peer.IP.ToString()} @ port {peer.Port}", MessageType.Warning);
#endif
                        return;
                        }
                    }

                async Task channelize()
                    {
                    PeerChannel peerChannel = new PeerChannel(peer);
                    ActivePeerChannels.Add(peerChannel);
                    DistributionHandler.AddTrustedPeer(peerChannel); // REMOVE AFTER DEBUGGING & TESTING
                    peers.Add(peerChannel.peer);
                    KnownPeers = peers;
                    Thread peerThread = new Thread(peerChannel.OpenPeerChannel);
                    peerThread.Start();
                    return;
                    }
                }
            }

        /// <summary>
        /// Removes a peer from the active peer channels and known peers lists.
        /// </summary>
        /// <param name="channel">The peer channel to remove.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
        public static async Task<bool> RemovePeer(PeerChannel channel)
            {
            bool flawless = true;
            try
                {

                channel.ClosePeerChannel();

                // Close connections and stop tasks gracefully
                channel.peer.Stream?.Close();
                channel.peer.Client?.Close();

                // Signal tasks/threads linked to the PeerChannel to stop 
                KnownPeers.RemoveAll(deadconnection => deadconnection.IP.ToString() == channel.peer.IP.ToString() && deadconnection.Port == channel.peer.Port);
                ActivePeerChannels.RemoveAll(deadchannel => deadchannel.peer.Identifier == channel.peer.Identifier);

                }
            catch (Exception ex)
                {
#if DEBUG
                DebugMessage($"Error removing peer: {ex.Message}", MessageType.Warning);
#endif
                return false;
                }
            return flawless;
            }

        /// <summary>
        /// Elevates a peer's permissions and adjusts broadcasting if necessary.
        /// </summary>
        /// <param name="channel">The peer channel to elevate.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
        public static async Task<bool> ElevatePeerPermission(PeerChannel channel)
            {
            bool flawless = true;
            try
                {

                DistributionHandler.AddTrustedPeer(channel);
                
                if (AutoThrottleOnConnect == true)
                    {
                    ThrottleBroadcastingDown();
                    }
                foreach (PeerChannel potentialpeer in ActivePeerChannels)
                    {
                    if (channel.peer.IP.ToString() == potentialpeer.peer.IP.ToString())
                        {
                        // Filter out unnecessary duplicate connections
                        bool removalAttempt = await RemovePeer(potentialpeer);
                        }
                    }

                }
            catch (Exception ex)
                {
                // Log the exception for debugging 
#if DEBUG
                DebugMessage($"Error elevating peer status: {ex.Message}", MessageType.Warning);
#endif
                return false;
                }
            return flawless;
            }

        /// <summary>
        /// Processes and adds peers from a collection share packet to the known peers list.
        /// </summary>
        /// <param name="packet">The collection share packet containing the list of peers.</param>
        public static void ProcessPeerList(CollectionSharePacket packet)
            {
            int x = 0;
            foreach (IPeer newpeer in packet.peers)
                {
                if (!KnownPeers.Contains(newpeer))
                    {
                    KnownPeers.Add(newpeer);
                    x++;
                    }
                }
            if (x > 0)
                {
#if DEBUG
                DebugMessage($"Added {x} peers.");
#endif
                }
            }

        #region Bootup

        // Scans all network interfaces to get some useful info (ie multicast, public facing IP, ect)
        private static void LoadLocalAddresses()
            {
            // Get the first available network interface
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            var relevantInterfaces = networkInterfaces
           .Where(adapter =>
               adapter.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
               adapter.SupportsMulticast);

            NetworkInterface primaryInterface = null;
            foreach (NetworkInterface adapter in relevantInterfaces)
                {
                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();

                // Check if the interface has a non-empty default gateway
                if (adapterProperties.GatewayAddresses.Any())
                    {
                    primaryInterface = adapter;
                    break; // Found the primary interface
                    }
                }

            if (primaryInterface != null)
                {
                IPInterfaceProperties adapterProperties = primaryInterface.GetIPProperties();
                var ipv4Addresses = adapterProperties.UnicastAddresses
                .Where(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(addr.Address))
                .Select(addr => addr.Address);
                foreach (IPAddress ip in ipv4Addresses)
                    {
                    publicip = ip; // grab public IP, typically the last/only one is true
                    MAC = primaryInterface.GetPhysicalAddress().ToString();
                    }
                }
            else
                {
#if DEBUG
                DebugMessage("No primary interface found.");
                Thread.Sleep(1500);
#endif
                return; // accidently'd the internet
                }


            foreach (NetworkInterface adapter in networkInterfaces)
                {
                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                GatewayIPAddressInformationCollection addresses = adapterProperties.GatewayAddresses;
#if DEBUG
                DebugMessage(adapter.GetPhysicalAddress().ToString());
                DebugMessage($"{adapter.Name.ToString()}\t {adapter.NetworkInterfaceType.ToString()}");
                DebugMessage($"Multicast supported: {adapter.SupportsMulticast}");
#endif
                IPAddressCollection addresses_DNS = adapterProperties.DnsAddresses;

                foreach (MulticastIPAddressInformation address in adapterProperties.MulticastAddresses)
                    {
#if DEBUG

                    DebugMessage($"Multicast address: {address.Address.ToString()}");
#endif
                    multicast_addresses.Add(address.Address);
                    }
#if DEBUG
                Console.WriteLine();
#endif
                }
            }

        /// <summary>
        /// Begin LAN broadcast and discovery.
        /// </summary>
        public static void BootDiscoveryChannels()
            {
            cleanupTimer.Start();
            DetermineRole();

            foreach (int port in designatedPorts)
                {
                LocalChannel localChannel = new LocalChannel(port);
                ActiveLocalChannels.Add(localChannel);
                }
            InitiateLocalChannels(broadcasterPort);


            foreach (IPAddress multicaster in multicast_addresses)
                {
                MulticastChannel multicastChannel = new MulticastChannel(multicaster);
                ActiveMulticastChannels.Add(multicastChannel);
                }
            InitializeMulticaseChannels();
            }

        static void DetermineRole()
            {
            broadcasterPort = designatedPorts[random.Next(designatedPorts.Count)];
            isBroadcaster = true;  // Just for testing, in real-world you'd have more logic
#if DEBUG
            Console.WriteLine("Role: {0}, Port: {1}", isBroadcaster ? "Broadcaster" : "Listener", broadcasterPort);
            Console.Title = ($"Broadcast port: {broadcasterPort}");
#endif
            }

        #endregion

        #region Routines

        // Cleanup likely-inactive peers
        private static void DiscernPeerChannels(object sender, System.Timers.ElapsedEventArgs e)
            {
#if DEBUG
            DebugMessage("DISCERNING CHANNELS", ConsoleColor.Magenta);
#endif
            Task.Run(async () =>
            {
                try
                    {
                    List<PeerChannel> peersToRemove = new List<PeerChannel>();
                    List<PeerChannel> peersToTrust = new List<PeerChannel>();

                    foreach (PeerChannel channel in ActivePeerChannels)
                        {
                        if (DateTime.Now - channel.lastIncomingReceived > TimeSpan.FromMinutes(1)) // No activity for 60 seconds ~ subject to change
                            {
                            peersToRemove.Add(channel);
                            }
                        if (channel.goodpings > 2)
                            {
                            peersToTrust.Add(channel);
                            }
                        }

                    // Remove inactive PeerChannels from ActivePeerChannels
                    foreach (PeerChannel channel in peersToRemove)
                        {
                        bool success = await RemovePeer(channel);
                        if (success)
                            {
#if DEBUG
                            DebugMessage($"Removed peer for inactivity: {channel.peer.IP.ToString()} port {channel.peer.Port}", ConsoleColor.DarkCyan);
#endif
                            channel.ClosePeerChannel();
                            }
                        }
                    foreach (PeerChannel channel in peersToTrust)
                        {
                        bool success = await ElevatePeerPermission(channel);
                        if (success == true)
                            {
                            channel.TrustPeer();
                            }
#if DEBUG
                        DebugMessage($"Trusting peer: {channel.peer.IP.ToString()} port {channel.peer.Port}", ConsoleColor.Cyan);
#endif

                        }
                    }
                catch
                    {
                    // nothing here yet TODO
                    }
            });
            }

        #endregion

        }
    }