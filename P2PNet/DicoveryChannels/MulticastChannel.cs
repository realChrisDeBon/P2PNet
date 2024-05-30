using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using P2PNet.DicoveryChannels;

namespace P2PNet.DiscoveryChannels
{
    internal class MulticastChannel : Discovery_Channel_Base
    {
        private readonly int[] MULTICAST_PORTS = { 3000, 3010, 3020, 3030 };
        public IPAddress multicast_address;

        public MulticastChannel(IPAddress in_multicast_address) 
        {
            multicast_address = in_multicast_address;
        }

        public async Task StartBroadcaster()
        {
            int timevariation = CreateTimeVariation(100, 500);
            UdpClient broadcaster = new UdpClient();
            broadcaster.JoinMulticastGroup(multicast_address);
            broadcaster.EnableBroadcast = true;
            broadcaster.MulticastLoopback = true;
            broadcasterendpoint = new IPEndPoint(multicast_address, MULTICAST_PORTS[0]);
            while (true)
            {
                    // Send a sample message (replace with actual data)
                    byte[] message = UniqueIdentifier();
#if DEBUG
                    DebugMessage($"Multicast channel broadcast:  Endpoint-{broadcasterendpoint.Address.ToString()} MCAddress-{multicast_address.Address.ToString()}");
#endif
                    broadcaster.Send(message, message.Length, new IPEndPoint(multicast_address, MULTICAST_PORTS[0]));
                    Thread.Sleep(BroadcastRateControl.GetCurrentInterval());
            }
        }

        public async Task StartListener()
        {
            UdpClient listener = new UdpClient();
            listener.JoinMulticastGroup(multicast_address);
            listener.MulticastLoopback = true; listener.EnableBroadcast = true;
            
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, MULTICAST_PORTS[0]);
            listenerendpoint = endpoint;
            listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listener.Client.Bind(endpoint);

            while (true)
            {
                byte[] receivedData = listener.Receive(ref endpoint);
                string packet = Encoding.UTF8.GetString(receivedData);
#if DEBUG
                DebugMessage("Packet received from multicast channel!");
#endif
                HandlePacket(packet);   
            }
        }
    }
}
