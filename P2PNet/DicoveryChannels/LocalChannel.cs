﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using P2PNet.DicoveryChannels;
using static P2PNet.PeerNetwork;

namespace P2PNet.DiscoveryChannels
{
    /// <summary>
    /// Conducts LAN casting to broadcast address to grow network.
    /// </summary>
    internal class LocalChannel : LANDiscoveryChannelBase
    {
        public int DESIGNATED_PORT { get; set; }
        public LocalChannel(int port_designation)
        {
            DESIGNATED_PORT = port_designation;
        }

        public async Task OpenLocalChannel()
            {
            cancelBroadcaster = new CancellationTokenSource();
            cancelListener = new CancellationTokenSource();

            Task.Run(() => StartBroadcaster(cancelBroadcaster.Token));
            Task.Run(() => StartListener(cancelListener.Token, DESIGNATED_PORT));
            }

        public override async Task StartBroadcaster(CancellationToken cancellationToken)
        {
            UdpClient broadcaster = new UdpClient();
            broadcaster.AllowNatTraversal(true);

            IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, BroadcasterPort);
            
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Loopback, BroadcasterPort);

            while (true)
            {
                    byte[] message = UniqueIdentifier();
                    broadcaster.Send(message, message.Length, broadcastEndPoint);
                    Thread.Sleep(500);
                    broadcaster.Send(message, message.Length, localEndPoint);
                    Thread.Sleep(BroadcastRateControl.GetCurrentInterval());
            }
        }

        public override async Task StartListener(CancellationToken cancellationToken, int designated_port)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, designated_port);
            UdpClient listener = new UdpClient(remoteEndPoint);

            while (true)
            {
                byte[] receivedData = listener.Receive(ref remoteEndPoint);
                string packet = Encoding.UTF8.GetString(receivedData);
                HandlePacket(packet);
            }
        }
    }
}