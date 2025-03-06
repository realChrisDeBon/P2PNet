using static P2PNet.PeerNetwork;
using P2PNet.NetworkPackets;
using static P2PNet.Distribution.DistributionHandler;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;


namespace P2PNet.Peers
{
    /// <summary>
    /// Represents a communication channel with a peer in the P2P network.
    /// </summary>
    public class PeerChannel : PeerChannel_Base
        {
        /// <summary>
        /// Gets the DateTime value of when the last piece of data or information was received from this peer.
        /// </summary>
        public DateTime LastIncomingDataReceived { get; internal set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the peer associated with this channel.
        /// </summary>
        public IPeer peer { get; set; }

        private int RETRIES = 0; // for retrying connections
        /// <summary>
        /// Gets the number of good pings received from the peer.
        /// This value is incremented each time a ping is received from the peer, 
        /// </summary>
        public int GOODPINGS { get; internal set; } = 0; // readonly
        private const int MAX_RETRIES = 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeerChannel"/> class with the specified peer.
        /// </summary>
        /// <param name="peer_">The peer to associate with this channel.</param>
        public PeerChannel(IPeer peer_)
            {
            peer = peer_;
            }

        /// <summary>
        /// Opens the communication channel with the peer and starts handling incoming and outgoing data.
        /// </summary>
        public async void OpenPeerChannel()
            {
            cancelSender = new CancellationTokenSource();        
            cancelReceiver = new CancellationTokenSource();


            DebugMessage($"Successfully opened channel with new peer: {peer.IP.ToString()}:{peer.Client.Client.RemoteEndPoint.ToString()} port {peer.Port}:{peer.Client.Client.RemoteEndPoint.ToString()}", ConsoleColor.Cyan);


            peer.Client.ReceiveTimeout = 60000;
            peer.Client.SendTimeout = 20000;

            StartPacketHandling();
            receiveTask = Task.Run(() => ReadIncoming(cancelReceiver.Token));
            sendTask = Task.Run(() => SendOutgoing(cancelSender.Token));

            if (IncomingPeerTrustPolicy.AllowDefaultCommunication == true)
                {
                Task.Run(() => CreatePing());
                }
            }

        /// <summary>
        /// Closes the communication channel with the peer and terminates all associated tasks.
        /// </summary>
        public void ClosePeerChannel()
            {
            if ((cancelSender != null) && (cancelReceiver != null))
                {

                DebugMessage($"\t-- Ending peer connection with {peer.IP.ToString()} port {peer.Port}", ConsoleColor.DarkBlue);

                BreakAndRemovePeer();

                }
            }

        protected async void CreatePing()
            {
            while(IsTrustedPeer == false)
                {
                PureMessagePacket pingMessage = new PureMessagePacket();
                pingMessage.Message = $"Ping from {PublicIPV4Address.ToString()}";
                string outgoing = Serialize<PureMessagePacket>(pingMessage);
                WrapPacket(PacketType.PureMessage, ref outgoing);
                OutgoingDataQueue.Enqueue(outgoing);
                Thread.Sleep(3000);
                }
            }

        /// <summary>
        /// The default task for sending outgoing data and information to the network stream.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token provided to the send outgoing task.</param>
        protected async void SendOutgoing(CancellationToken cancellationToken)
            {
            try
                {
                NetworkStream stream = peer.Client.GetStream();
                while (!cancellationToken.IsCancellationRequested)
                    {
                    while (OutgoingDataQueue.TryDequeue(out string message))
                        {
                        try
                            {
                            byte[] buffer = Encoding.UTF8.GetBytes(message);

                            DebugMessage($"Sending: {message}", ConsoleColor.Cyan);

                                stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
                            }
                        catch (Exception e)
                            {

                            DebugMessage(e.ToString(), MessageType.Critical);

                            }
                        finally
                            {
                            Thread.Sleep(100);
                            }
                        }

                    }
                
                }


            catch (ObjectDisposedException ex)
                {
                HandleException(ex, RETRIES, MAX_RETRIES);
                
                }
            catch (IOException ex) when (ex.InnerException is SocketException socket_ex)
                {
                HandleException(ex, RETRIES, MAX_RETRIES);
                }
            catch (InvalidOperationException ex)
                {
                HandleException(ex, RETRIES, MAX_RETRIES);
                }
            catch (Exception ex)
                {
                DebugMessage($"Thread: {Environment.CurrentManagedThreadId.ToString()}\nPeer channel connection issue (sender): {peer.IP.ToString()} port {peer.Port}" + Environment.NewLine + ex.Message, MessageType.Critical);
                BreakAndRemovePeer(); // Stop after generic exceptions
                }
            finally
                {
                Thread.Sleep(3000);
                }
            }

        /// <summary>
        /// The default task for reading incoming network stream data.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token provided to the read incoming task.</param>
        public async void ReadIncoming(CancellationToken cancellationToken)
            {
            int ReceiverErrors = 0;
            try
                {
                NetworkStream stream = peer.Client.GetStream();
                string receivedData = ""; // Top-level string

                while (!cancellationToken.IsCancellationRequested)
                    {
                    try
                        {
                        byte[] buffer = new byte[4096];
                        int bytesRead = 0;

                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                        if (bytesRead > 0)
                            {
                            string newData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            receivedData = receivedData + newData;
                            LastIncomingDataReceived = DateTime.Now;

                            while (IsValidMessageFormat(receivedData))
                                {
                                PacketTypeRelay receivedPacket = ExtractWholeMessage(receivedData);
                                if (!string.IsNullOrEmpty(receivedPacket.Data))
                                    {
                                    packetQueue.Enqueue(receivedPacket); // Add to the packet queue to be processed
                                    OnDataReceived(newData); // event test
                                    }
                                receivedData = ""; // wipe receivedData ~ ~ ~
                                }
                            }
                        }
                    catch (Exception e)
                        {
                        // BreakAndRemovePeer();
                        DebugMessage($"Encountered an error. Error code: {(int)e.HResult} {e.Message}\n{e.Data}", MessageType.Critical);
                        HandleException(e, RETRIES, MAX_RETRIES);
                        }
                    finally
                        {
                        stream.Flush();
                        }
                    Thread.Sleep(250);
                    }
                TerminateCurrentReceiver();
                } catch
                {

                }
            }
        private void HandleException(Exception ex, int retries, int maxRetries)
            {
            RETRIES++;

            DebugMessage($"Exception in PeerChannel ({peer.IP.ToString()} port {peer.Port}) : {ex.Message} - Attempt {RETRIES}/{MAX_RETRIES}", MessageType.Critical);

            if (RETRIES >= MAX_RETRIES)
                {
                BreakAndRemovePeer();
                }
            else
                {
                Thread.Sleep(4000); // Retry pause of 4 seconds
                }
            }
        private void BreakAndRemovePeer()
            {
            TerminateChannel();
            }
        }
}