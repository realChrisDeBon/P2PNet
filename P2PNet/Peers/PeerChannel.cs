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
    public class PeerChannel : Peer_Channel_Base
        {
        public DateTime lastIncomingReceived = DateTime.Now;

        public IPeer peer { get; set; }

        private int retries = 0; // for retrying connections
        public int goodpings { get; internal set; } = 0; // readonly
        private const int MAX_RETRIES = 3;

        public PeerChannel(IPeer peer_)
            {
            peer = peer_;
            }

        public async void OpenPeerChannel()
            {
            cancelSender = new CancellationTokenSource();
            cancelReceiver = new CancellationTokenSource();
#if DEBUG
            DebugMessage($"Successfully opened channel with new peer: {peer.IP.ToString()}:{peer.Client.Client.LocalEndPoint.ToString()} port {peer.Port}:{peer.Client.Client.LocalEndPoint.ToString()}", ConsoleColor.Cyan);
#endif
            peer.Client.ReceiveTimeout = 60000;
            peer.Client.SendTimeout = 20000;

            StartPacketHandling();
            receiveTask = Task.Run(() => ReadIncoming(cancelReceiver.Token));
            sendTask = Task.Run(() => SendOutgoing(cancelSender.Token));
            Task.Run(() => CreatePing());
            }
        public void ClosePeerChannel()
            {
            if ((cancelSender != null) && (cancelReceiver != null))
                {
#if DEBUG
                DebugMessage($"\t-- Ending peer connection with {peer.IP.ToString()} port {peer.Port}", ConsoleColor.DarkBlue);
#endif
                cancelSender.Cancel();
                cancelReceiver.Cancel();
                peer.Stream.Close();
                peer.Client.Close();
                }
            }

        protected async void CreatePing()
            {
            while(IsTrustedPeer == false)
                {
                PureMessagePacket pingMessage = new PureMessagePacket();
                pingMessage.Message = $"Ping from {publicip.ToString()}";
                string outgoing = Serialize<PureMessagePacket>(pingMessage);
                WrapPacket(PacketType.PureMessage, ref outgoing);
                outgoingData.Enqueue(outgoing);
                Thread.Sleep(3000);
                }
            }
        protected async void SendOutgoing(CancellationToken cancellationToken)
            {
            try
                {
                NetworkStream stream = peer.Client.GetStream();
                while (!cancellationToken.IsCancellationRequested)
                    {
                    while (outgoingData.TryDequeue(out string message))
                        {
                        try
                            {
                            byte[] buffer = Encoding.UTF8.GetBytes(message);
#if DEBUG
                            DebugMessage($"Sending: {message}", ConsoleColor.Cyan);
#endif
                //            lock (sendLock)
                //                {
                                stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
                //                }
                            }
                        catch (Exception e)
                            {
#if DEBUG
                            DebugMessage(e.ToString(), MessageType.Critical);
#endif
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
                HandleException(ex, retries, MAX_RETRIES);
                
                }
            catch (IOException ex) when (ex.InnerException is SocketException socket_ex)
                {
                HandleException(ex, retries, MAX_RETRIES);
                }
            catch (InvalidOperationException ex)
                {
                HandleException(ex, retries, MAX_RETRIES);
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

        public async void ReadIncoming(CancellationToken cancellationToken)
            {
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
                        lock (receiveLock)
                            {
                            bytesRead = stream.Read(buffer, 0, buffer.Length);
                            }

                        if (bytesRead > 0)
                            {
                            string newData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            receivedData += newData;

                            lastIncomingReceived = DateTime.Now;

                            while (IsValidMessageFormat(receivedData))
                                {
                                PacketTypeRelay receivedPacket = ExtractWholeMessage(receivedData);
                                if (!string.IsNullOrEmpty(receivedPacket.Data))
                                    {
                                    packetQueue.Enqueue(receivedPacket); // Add to the packet queue to be processed
                                    }
                                receivedData = ""; // wipe receivedData ~ ~ ~
                                }
                            }
                        }
                    catch (Exception e)
                        {
                        BreakAndRemovePeer();
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
            retries++;
#if DEBUG
            DebugMessage($"Exception in PeerChannel ({peer.IP.ToString()} port {peer.Port}) : {ex.Message} - Attempt {retries}/{maxRetries}", MessageType.Critical);
#endif
            if (retries >= maxRetries)
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
            RemovePeer(this);
            }
        }
}