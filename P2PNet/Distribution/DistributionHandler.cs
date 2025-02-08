using P2PNet.NetworkPackets;
using P2PNet.Peers;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Timers;

namespace P2PNet.Distribution
    {
    // Data structure for entries
    internal record struct MemoryEntry
        {
        public int StartPosition { get; }
        public int Length { get; }
        public DataPayloadFormat DataType { get; }
        public string AutoGenTags {get;}
        public MemoryEntry(int position, int length, DataPayloadFormat datatype)
            {
            StartPosition = position;
            Length = length;
            DataType = datatype;
            }
        }

    /// <summary>
    /// Provides methods and properties to handle data distribution to trusted peer channels.
    /// </summary>
    public static class DistributionHandler
        {
        /// <summary>
        /// Gets or sets the list of trusted peer channels.
        /// </summary>
        static List<PeerChannel> TrustedPeerChannels { get; set; } = new List<PeerChannel>();

        /// <summary>
        /// Queue for outgoing data packets to be distributed to trusted peers.
        /// </summary>
        public static ConcurrentQueue<DataTransmissionPacket> outgoingDataQueue = new ConcurrentQueue<DataTransmissionPacket>();

        /// <summary>
        /// Queue for incoming data packets to be processed.
        /// </summary>
        public static ConcurrentQueue<DataTransmissionPacket> incomingDataQueue = new ConcurrentQueue<DataTransmissionPacket>();


        private static Timer _timer;
        private static Timer queueChecker;


        /// <summary>
        /// Adds a peer channel to the list of trusted peer channels.
        /// </summary>
        /// <param name="peerChannel">The peer channel to add.</param>
        public static void AddTrustedPeer(PeerChannel peerChannel)
            {
            TrustedPeerChannels.Add(peerChannel);
            }

        /// <summary>
        /// Queues a data packet for distribution.
        /// </summary>
        /// <param name="packet">The data packet to queue.</param>
        public static void QueueDataForDistribution(DataTransmissionPacket packet)
            {
            outgoingDataQueue.Enqueue(packet);
            }

        /// <summary>
        /// Queues raw data for distribution by wrapping it in a data transmission packet.
        /// </summary>
        /// <param name="data">The raw data to queue.</param>
        /// <param name="dataType">The type of data being queued.</param>
        public static void QueueDataForDistribution(byte[] data, DataPayloadFormat dataType)
            {
            outgoingDataQueue.Enqueue(new DataTransmissionPacket { Data = data, DataType = dataType });
            } // overload for raw data not wrapped in DTP

        /// <summary>
        /// Enqueues an incoming data packet for processing.
        /// </summary>
        /// <param name="packet">The data packet to enqueue.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task EnqueueIncomingDataPacket(DataTransmissionPacket packet)
            {
            incomingDataQueue.Enqueue(packet);
            }

        /// <summary>
        /// Enqueues a serialized incoming data packet for processing.
        /// </summary>
        /// <param name="packet">The serialized data packet to enqueue.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task EnqueueIncomingDataPacket(string packet)
            {
            DataTransmissionPacket packet_ = Deserialize<DataTransmissionPacket>(packet);
            incomingDataQueue.Enqueue(packet_);
            }

        static DistributionHandler()
            {
            _timer = new System.Timers.Timer(500); // half second
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            queueChecker = new System.Timers.Timer(500); // 10 seconds
            queueChecker.Elapsed += HandleIncomingDataPackets;
            queueChecker.AutoReset = true;
            queueChecker.Enabled = true;
            }
         
        internal static void OnTimedEvent(System.Object source, ElapsedEventArgs e)
            {
            if (outgoingDataQueue.IsEmpty)
                return;
            while (!outgoingDataQueue.IsEmpty)
                {
                outgoingDataQueue.TryDequeue(out DataTransmissionPacket incomingpacket);
                DistributeData(incomingpacket);
                }
            }

        private static async void HandleIncomingDataPackets(System.Object source, ElapsedEventArgs e)
            {
            while (!incomingDataQueue.IsEmpty)
                {
                if (incomingDataQueue.TryDequeue(out DataTransmissionPacket packet))
                    {
                    // Logic for handling the packet based on DataType
                    switch (packet.DataType)
                        {
                        case DataPayloadFormat.File:
                            MemoryHandler.LoadDataToMemory(packet);
                            break;
                        case DataPayloadFormat.Task:

                            break;
                        }
                    }
                }
            }

        internal static class MemoryHandler
            {
            private static Memory<byte> _data = new Memory<byte>(new byte[1024]); // change this later
            private static int _position = 0;
            private static object _lock = new object(); // For thread safety
            private static List<MemoryEntry> _entries = new List<MemoryEntry>();
            private static bool validentry = true;

            public static async Task LoadDataToMemory(DataTransmissionPacket packet)
                {
                lock (_lock)
                    {
                    int startPosition = _position;
                    int totallen = Encoding.UTF8.GetBytes(packet.Data.ToString()).Length;

                    MemoryEntry newentry = new MemoryEntry(startPosition, totallen, packet.DataType);
                    // Resize if needed
                    if (_data.Length - _position < packet.Data.Length)
                        {
                        ResizeInternalMemory(_data.Length + totallen);
                        }

                    // Append data
                    validentry = true;
                    try
                        {
                        Encoding.UTF8.GetBytes(packet.Data.ToString()).AsSpan().CopyTo(_data.Span.Slice(_position));
                        } catch
                        {
                        validentry = false;
                        }
                    finally
                        {
                        if (validentry == true)
                            {
                            _position += totallen; // Adjust position 

                            // Add entry
                            _entries.Add(newentry);

                            DebugMessage($"\nLoaded {totallen} bytes into memory.\n", ConsoleColor.Magenta);

                            }
                        }
                    }
                }

            public static async Task<byte[]> ReadDataFromMemory(int index)
                {
                lock (_lock)
                    {
                    if (index < 0 || index >= _entries.Count)
                        {

                        DebugMessage($"Index out of range.\n", MessageType.Warning);

                        throw new ArgumentOutOfRangeException(nameof(index));
                        }

                    var entry = _entries[index];
                    return _data.Slice(entry.StartPosition, entry.Length).ToArray();
                    }
                }

            public static async Task<byte[]> ReadDataFromMemory(DataPayloadFormat dataType, string dataTag)
                {
                lock (_lock)
                    {
                    var matchingEntry = _entries.FirstOrDefault(e => e.DataType == dataType);

                    if (matchingEntry.StartPosition == 0) // Check if entry was found
                        {

                        DebugMessage($"Entry in memory not found.\n", MessageType.Warning);

                        }

                    return _data.Slice(matchingEntry.StartPosition, matchingEntry.Length).ToArray();
                    }
                }

            private static void ResizeInternalMemory(int newSize)
                {
                var newMemory = new Memory<byte>(new byte[newSize]);
                _data.Span.CopyTo(newMemory.Span);
                _data = newMemory;
                }

            public static ReadOnlySpan<byte> Search(DataPayloadFormat datatype)
                {
                int startIndex = FindBlockStart(DataFormatTagMap[datatype].OpeningTag);
                if (startIndex == -1)
                    {

                    DebugMessage($"Tag not found.\n", MessageType.Warning);

                    }

                int endIndex = FindBlockEnd(startIndex + DataFormatTagMap[datatype].OpeningTag.Length);
                if (endIndex == -1)
                    {

                    DebugMessage($"Invalid data format: Missing TAGEND.\n", MessageType.Warning);

                    }

                return _data.Span.Slice(startIndex + DataFormatTagMap[datatype].OpeningTag.Length, endIndex - startIndex - DataFormatTagMap[datatype].OpeningTag.Length);

                // Helper to find the start of a data block
                  int FindBlockStart(string tag)
                    {
                    return _data.Span.IndexOf(Encoding.UTF8.GetBytes(tag));
                    }

                // Helper to find the end of a data block
                  int FindBlockEnd(int start)
                    {
                    return _data.Span.Slice(start).IndexOf(Encoding.UTF8.GetBytes(DataFormatTagMap[datatype].ClosingTag));
                    }
                }
            }

        private static void DistributeData(DataTransmissionPacket outgoingpacket)
            {
            string outdata = Serialize<DataTransmissionPacket>(outgoingpacket);
            WrapPacket(PacketType.DataTransmissionPacket, ref outdata);
            foreach (var peer in TrustedPeerChannels)
                {
                peer.LoadOutgoingData(outdata);
                }
            }

        /// <summary>
        /// Distributes a file asynchronously by reading its contents and queuing it for distribution.
        /// </summary>
        /// <param name="filePath">The path of the file to distribute.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task DistributeFileAsync(string filePath)
            {
            DataTransmissionPacket outgoingpacket = new DataTransmissionPacket();
            outgoingpacket.DataType = DataPayloadFormat.File;
            outgoingpacket.Data = await File.ReadAllBytesAsync(filePath);
            QueueDataForDistribution(outgoingpacket);
            }

        }

    }