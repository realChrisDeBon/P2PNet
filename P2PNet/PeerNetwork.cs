#if DEBUG
global using static ConsoleDebugger.ConsoleDebugger;
#endif
using P2PNet.DiscoveryChannels;
using P2PNet.Distribution;
using P2PNet.NetworkPackets;
using P2PNet.Peers;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Channels;

namespace P2PNet
{
    public static class PeerNetwork
        {

        // Some public facing settings for better user-defined control
        static bool isBroadcaster = false;
        private static Random randomizer = new Random();

        /// <summary>
        /// Indicates whether to automatically throttle outbound broadcast rate when a new peer is discovered.
        /// </summary>
        public static bool AutoThrottleOnConnect { get; set; } = true;

        /// <summary>
        /// Gets or sets the list of designated ports for broadcast and discovery.
        /// </summary>
        public static List<int> DesignatedPorts { get; set; } = new List<int> { 8001, 8002, 8003 }; // Your designated ports

        /// <summary>
        /// Gets or sets the broadcaster port designated for outbound LAN discovery.
        /// </summary>
        public static int BroadcasterPort;

        /// <summary>
        /// Determines if the broadcaster port for LAN discovery will be rotated on a regular interval.
        /// </summary>
        public static bool RunRotateBroadcastPort_Routine
            {
            get { return runningPortRotate; }
            set { runningPortRotate = value; InboundToggle_PortRotate(value); }
            }
        private static bool runningPortRotate = true;
        private static System.Timers.Timer rotationTimer = new System.Timers.Timer();
        private static void InboundToggle_PortRotate(bool status_)
            {
            if(status_ == true)
                {
                rotationTimer.Start();
                rotationTimer.AutoReset = true;
                } else
                {
                rotationTimer.Stop();
                rotationTimer.AutoReset = false;
                }
            }

        /// <summary>
        /// The duration, in minutes, of how often the LAN broadcaster port will rotate.
        /// </summary>
        public static int BroadcastPortRotationDuration { get; set; } = 2;

        /// <summary>
        /// Gets the public IPv4 IP address.
        /// </summary>
        public static IPAddress PublicIPV4Address;

        /// <summary>
        /// Gets the public IPv6 IP address.
        /// </summary>
        public static IPAddress PublicIPV6Address;
        private static bool IPv6AddressFound = false;

        /// <summary>
        /// Gets the system MAC address.
        /// </summary>
        public static PhysicalAddress MAC;

        private static TcpListener listener;

        /// <summary>
        /// Gets or sets whether a designated TCP port will be actively listening for and accepting inbound TCP peer connections.
        /// The default is true, but you may want to toggle this for server instances that serve different network purposes.
        /// </summary>
        public static bool AcceptInboundPeers 
            { 
            get
                {
                return runningListener;
                }
            set
                {
                runningListener = value;
                InboundToggle_Listener(value);
                }
            }
        private static  bool runningListener = true;
        private static void InboundToggle_Listener(bool status_)
            {
            if(status_ == true)
                {
                BeginAcceptingInboundPeers();
                } else
                {
                StopAcceptingInboundPeers();
                }
            }

        /// <summary>
        /// Begins accepting inbound peers connections.
        /// </summary>
        public static void BeginAcceptingInboundPeers()
            {
            if (AcceptInboundPeers == true)
                {
                listener.Start();
                Task.Run(() => AcceptClientsAsync());
                } else
                {
#if DEBUG
                DebugMessage("Attempt was made to start inbound listener while AcceptInboundPeers is false.", MessageType.Warning);
#endif
                }
            }
        
        /// <summary>
        /// Stops accepting inbound peer connections.
        /// </summary>
        public static void StopAcceptingInboundPeers()
            {
            listener.Stop();
            if(AcceptInboundPeers == true)
                {
                AcceptInboundPeers = false; // redundant but never too careful
                }
            }

        /// <summary>
        /// Gets the listening port for inbound TCP peer connections.
        /// </summary>
        public static int ListeningPort { get; private set; }

        /// <summary>
        /// Get or set whether or not the cleanup timer will run on a regular interval.
        /// Cleanup timer scans and removes peers that have been active for the duration
        /// set by <see cref="PeerChannelCleanupDuration"/>
        /// </summary>
        public static bool RunPeerCleanup_Routine { get; set; } = true;
        private static System.Timers.Timer cleanupTimer = new System.Timers.Timer();

        /// <summary>
        /// The duration, in minutes, of how often a cleanup of peer channels will run.
        /// This scan checks for likely inactive or disconnected peers, and removes them.
        /// </summary>
        public static int PeerChannelCleanupDuration { get; set; } = 2;

        #region Connection Collections
        private static volatile List<IPeer> activePeers_ = new List<IPeer>();

        /// <summary>
        /// Gets or sets the list of known peers.
        /// KnownPeers do not necessarily have established trust to exchange extensive data and information, but do have an open PeerChannel.
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
        /// Queue of inbound connecting peers that have not been assigned a peer channel.
        /// Pass these peers through your verification pipeline and/or <see cref="AddPeer(IPeer, TcpClient)"/> once verified.
        /// </summary>
        /// <remarks>
        /// Inbound connections opened through <see cref="ListeningPort"/> will automatically be enqueued here. This allows you to implement any additional verification you may want in place before calling <see cref="AddPeer(IPeer, TcpClient)"/> and opening a PeerChannel
        /// </remarks>
        private static volatile InboundConnectingPeersQueue InboundConnectingPeers = new InboundConnectingPeersQueue();

        /// <summary>
        /// Occurs when a new incoming peer connection attempt is detected.
        /// Subscribers can use this event to handle new connections and pass incoming connections through your verification pipeline and/or <see cref="AddPeer(IPeer, TcpClient)"/>.
        /// </summary>
        public static event EventHandler<IncomingPeerEventArgs> IncomingPeerConnectionAttempt
            {
            add { InboundConnectingPeers.IncomingPeerConnectionAttempt += value; }
            remove { InboundConnectingPeers.IncomingPeerConnectionAttempt -= value; }
            }

        /// <summary>
        /// Gets the number of inbound peers that have been enqueued but not yet processed.
        /// </summary>
        public static int InboundPeerCount => InboundConnectingPeers.Count;

        /// <summary>
        /// All active <see cref="PeerChannel"/> connections are stored here. 
        /// </summary>
        /// <remarks>
        /// Inbound connections opened through <see cref="ListeningPort"/> will automatically be enqueued here. This allows you to implement any additional verification you may want in place before calling <see cref="AddPeer(IPeer, TcpClient)"/> and opening a PeerChannel
        /// </remarks>
        public static volatile List<PeerChannel> ActivePeerChannels = new List<PeerChannel>();
        private static List<LocalChannel> ActiveLocalChannels = new List<LocalChannel>();
        private static List<MulticastChannel> ActiveMulticastChannels = new List<MulticastChannel>();

        private static List<IPAddress> multicast_addresses = new List<IPAddress>();
        #endregion

        private static void InitiateLocalChannels(int designated_broadcast_port)
            {
            if (AcceptInboundPeers == true)
                {
                listener.Start();
                Task.Run(() => AcceptClientsAsync());
                }

            Queue<LocalChannel> badChannels = new Queue<LocalChannel>();
            bool error_occurred = false;
            foreach (LocalChannel channel_ in ActiveLocalChannels)
                {
                try
                    {
                        channel_.OpenLocalChannel();
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
        private static void InitializeMulticaseChannels()
            {
            Queue<MulticastChannel> badChannels = new Queue<MulticastChannel>();
            foreach (MulticastChannel multicastChannel in ActiveMulticastChannels)
                {
                bool error_occurred = false;
                try
                    {
                    multicastChannel.OpenMulticastChannel();
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
            ListeningPort = randomizer.Next(8051, 9000); // setup a port for listening
            listener = new TcpListener(IPAddress.Any, ListeningPort);

            cleanupTimer.Elapsed += DiscernPeerChannels;
            cleanupTimer.Interval = (PeerChannelCleanupDuration * 60000);

            rotationTimer.Elapsed += RotatePorts;
            rotationTimer.Interval = (BroadcastPortRotationDuration * 60000);
            }

        /// <summary>
        /// Starts timed routines if their run value is true. These routines include:
        /// <list type="bullet">
        /// <item>
        /// <description><see cref="RunRotateBroadcastPort_Routine"/></description>
        /// </item>
        /// <item>
        /// <description><see cref="RunPeerCleanup_Routine"/></description>
        /// </item>
        /// </list>
        /// </summary>
        public static async Task StartRoutines()
            {
            if (RunRotateBroadcastPort_Routine == true)
                {
                rotationTimer.Start();
                }

            if (RunPeerCleanup_Routine == true)
                {
                cleanupTimer.Start();
                }
            }

        static async Task AcceptClientsAsync()
            {
            while (AcceptInboundPeers == true)
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
                    else if (InboundConnectingPeers.PeerIsQueued(peerIP.ToString()))
                        {
#if DEBUG
                        DebugMessage("Duplicate connection attempt from existing peer. Ignoring.", MessageType.Warning);
#endif
                        }
                    else
                        {
                        // Set inboud as GenericPeer - intentional gap before AddPeer to implement additional verifications as needed
                        GenericPeer newPeer = new GenericPeer(((IPEndPoint)client.Client.RemoteEndPoint).Address, ((IPEndPoint)client.Client.RemoteEndPoint).Port);

                        InboundConnectingPeers.Enqueue(newPeer);
                        Task.Run(() => AddPeer(newPeer, client));
                        }
                    }
                }
            

        /// <summary>
        /// Adds a peer to the <see cref="KnownPeers"/> list and establishes a connection if one is not provided.
        /// A new peer channel will be automatically added to <see cref="ActivePeerChannels"/> with standard non-elevated permissions.
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
            else if (peer.IP.ToString() == PublicIPV4Address.ToString() && peer.Port == ListeningPort)
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
#if DEBUG || TRUSTLESS
                    DistributionHandler.AddTrustedPeer(peerChannel); // REMOVE AFTER DEBUGGING & TESTING
#endif
                    peers.Add(peerChannel.peer);
                    KnownPeers = peers;
                    Thread peerThread = new Thread(peerChannel.OpenPeerChannel);
                    peerThread.Start();
                    return;
                    }
                }
            }

        /// <summary>
        /// Terminates a peer connection and removes it from <see cref="KnownPeers"/> and <see cref="ActivePeerChannels"/>.
        /// </summary>
        /// <param name="channel">The peer channel to remove.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
        public static async Task<bool> RemovePeer(PeerChannel channel)
            {
            bool flawless = true; // This is here for more intricate scenarios later
            try
                {

                channel.ClosePeerChannel();
                ActivePeerChannels.Remove(channel);
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

        /// <summary>
        /// Scans all network interface devices and collects essential information needed for peer network.
        /// </summary>
        public static void LoadLocalAddresses()
            {
            PublicIPV6Address = GetLocalIPv6Address();
            if(PublicIPV6Address != null)
                {
                IPv6AddressFound = true; // this will signal to us later if IPv6 is usable or not
#if DEBUG
                DebugMessage($"IPv6 IP address: {PublicIPV6Address.ToString()}");
#endif
                }
            else
                {
#if DEBUG
                DebugMessage("IPv6 IP address not found. IPv6 features will not be available.", MessageType.Warning);
#endif
                }

            // Get the first available network interface
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            var devices = CaptureDeviceList.Instance; // loads active interfaces


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
                    PublicIPV4Address = ip; // grab public IP, typically the last/only one is true
                    MAC = primaryInterface.GetPhysicalAddress();
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
                foreach (var dev in devices)
                    {
                    if ((dev is LibPcapLiveDevice libPcapDevice) && (dev.Description.Equals(adapter.Description, StringComparison.OrdinalIgnoreCase)))
                        {

                        IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
#if DEBUG
                        DebugMessage(adapter.GetPhysicalAddress().ToString());
                        DebugMessage($"{adapter.Name.ToString()}\t {adapter.NetworkInterfaceType.ToString()}");
                        DebugMessage($"Multicast supported: {adapter.SupportsMulticast}");
#endif
                        foreach (MulticastIPAddressInformation address in adapterProperties.MulticastAddresses)
                            {
#if DEBUG
                            DebugMessage($"Multicast address: {address.Address.ToString()}");
#endif
                            multicast_addresses.Add(address.Address);
                            }
                        }
                    }
                }
            // Ensure there's no duplicates
            multicast_addresses = multicast_addresses.Distinct().ToList();

            }

        /// <summary>
        /// Gets the IPv6 address of the host machine.
        /// </summary>
        /// <returns>The non-temporary IPv6 address of the host machine.</returns>
        private static IPAddress GetLocalIPv6Address()
            {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                    {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6 &&
                            !ip.Address.IsIPv6LinkLocal &&
                            !ip.Address.IsIPv6Multicast &&
                            !ip.Address.IsIPv6SiteLocal &&
                            !ip.DuplicateAddressDetectionState.HasFlag(DuplicateAddressDetectionState.Deprecated))
                            {
                            return ip.Address;
                            }
                        }
                    }
                }
            return null;
            }

        /// <summary>
        /// Begin LAN broadcast and discovery.
        /// </summary>
        public static void BootDiscoveryChannels()
            {
            if (RunPeerCleanup_Routine == true)
                {
                cleanupTimer.Start();
                } // startup the cleanup timer 

            RandomizeBroadcasterPort(); // randomize a broadcast port

            foreach (int port in DesignatedPorts)
                {
                LocalChannel localChannel = new LocalChannel(port);
                ActiveLocalChannels.Add(localChannel);
                }
            InitiateLocalChannels(BroadcasterPort);

            foreach (IPAddress multicaster in multicast_addresses)
                {
                MulticastChannel multicastChannel = new MulticastChannel(multicaster);
                ActiveMulticastChannels.Add(multicastChannel);
                }
            InitializeMulticaseChannels();
            }

        // Randomly selects a port from the designated port collection to focus on outbound broadcasting
        static void RandomizeBroadcasterPort()
            {
            BroadcasterPort = DesignatedPorts[randomizer.Next(DesignatedPorts.Count)];
#if DEBUG
            Console.WriteLine("Role: {0}, Port: {1}", isBroadcaster ? "Broadcaster" : "Listener", BroadcasterPort);
            Console.Title = ($"Broadcast port: {BroadcasterPort}");
#endif
            }

        #endregion

        #region Routines

        // Cleanup likely-inactive peers
        private static void DiscernPeerChannels(object sender, System.Timers.ElapsedEventArgs e)
            {
            cleanupTimer.Interval = (PeerChannelCleanupDuration * 60000);
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
                        if (DateTime.Now - channel.LastIncomingDataReceived > TimeSpan.FromMinutes(PeerChannelCleanupDuration)) // No activity for 60 seconds ~ subject to change
                            {
                            peersToRemove.Add(channel);
                            }
                        if (channel.GOODPINGS > 2)
                            {
                            peersToTrust.Add(channel);
                            }
                        }

                    // Remove inactive PeerChannels from ActivePeerChannels
                    foreach (PeerChannel channel in peersToRemove)
                        {
                        bool success = await RemovePeer(channel);
                        if (success == true)
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
                catch (Exception ex)
                {
#if DEBUG
                    DebugMessage($"Encountered an error: {ex}", MessageType.Critical);
#endif
                    }
            });
            }

        // Rotate LAN broadcast port
        private static void RotatePorts(object sender, System.Timers.ElapsedEventArgs e)
            {
            rotationTimer.Interval = (BroadcastPortRotationDuration * 60000);
            int currentDesgPort = BroadcasterPort;
            while (BroadcasterPort == currentDesgPort) // make sure we get a new port
                {
                BroadcasterPort = DesignatedPorts[randomizer.Next(DesignatedPorts.Count)];
                }
#if DEBUG
            DebugMessage($"Rotated to new port: {BroadcasterPort}", MessageType.General);
#endif
            }

        #endregion

        #region Trust Definitions

        /// <summary>
        /// Handles trust and permissions in regards to incoming peer connections.
        /// </summary>
        public static class IncomingPeerTrustConfiguration
            {
            private static bool _allowDefaultCommunication = true;
            private static bool _allowEnhancedPacketExchange = false;

            /// <summary>
            /// Gets or sets whether incoming peer connections will be trusted to establish initial communication by default.
            /// This is the initial 'ping' pure message packet sent back and forth to ensure connection communicability.
            /// </summary>
            public static bool AllowDefaultCommunication
                {
                get => _allowDefaultCommunication;
                set => _allowDefaultCommunication = value;
                }

            /// <summary>
            /// Gets or sets whether incoming peer connections will be trusted to exchange all other packet types, like
            /// data transmission packets, before being trusted peers.
            /// </summary>
            public static bool AllowEnhancedPacketExchange
                {
                get => _allowEnhancedPacketExchange;
                set => _allowEnhancedPacketExchange = value;
                }
            }

        #endregion
        }
    }