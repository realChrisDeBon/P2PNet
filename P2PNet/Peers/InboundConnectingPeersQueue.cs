﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static P2PNet.PeerNetwork;

namespace P2PNet.Peers
{
    public class IncomingPeerEventArgs : EventArgs
    {
        public GenericPeer Peer { get; }

        public IncomingPeerEventArgs(GenericPeer peer)
        {
            Peer = peer;
        }
    }

    public class InboundConnectingPeersQueue
    {
        private volatile ConcurrentQueue<GenericPeer> _queue = new ConcurrentQueue<GenericPeer>();
        
        public event EventHandler<IncomingPeerEventArgs> IncomingPeerConnectionAttempt;

        public void Enqueue(GenericPeer peer)
        {
           if((TrustPolicies.IncomingPeerTrustPolicy.IncomingPeerPlacement == TrustPolicies.IncomingPeerTrustPolicy.IncomingPeerMode.QueueBased)||(TrustPolicies.IncomingPeerTrustPolicy.IncomingPeerPlacement == TrustPolicies.IncomingPeerTrustPolicy.IncomingPeerMode.QueueAndEventBased))
                {
                _queue.Enqueue(peer);
                }
            if ((TrustPolicies.IncomingPeerTrustPolicy.IncomingPeerPlacement == TrustPolicies.IncomingPeerTrustPolicy.IncomingPeerMode.EventBased) || (TrustPolicies.IncomingPeerTrustPolicy.IncomingPeerPlacement == TrustPolicies.IncomingPeerTrustPolicy.IncomingPeerMode.QueueAndEventBased))
                {
                Task.Run(() => { OnIncomingPeerConnectionAttempt(peer); });
                }
        }

        public bool PeerIsQueued(string IPAddress, string Identifier = null)
            {
            // Check if a peer with the same IP already exists in the queue
            foreach (var existingPeer in _queue)
                {
                if (existingPeer.IP.ToString() == IPAddress)
                    {
                    return true; // Exit the method without adding the new peer
                    }
                }
            return false;
            }
        public bool PeerIsQueued(IPAddress IPAddress, string Identifier = null)
        {
            // Check if a peer with the same IP already exists in the queue
            foreach (var existingPeer in _queue)
            {
                if (existingPeer.IP == IPAddress)
                {
                    return true;
                }
                if(Identifier != null)
                {
                    if (existingPeer.Identifier == Identifier)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public GenericPeer Dequeue()
        {
                _queue.TryDequeue(out var peer);
                return peer;
        }

        public int Count => _queue.Count;

        protected virtual void OnIncomingPeerConnectionAttempt(GenericPeer peer)
        {
            IncomingPeerConnectionAttempt?.Invoke(this, new IncomingPeerEventArgs(peer));
        }
    }
}