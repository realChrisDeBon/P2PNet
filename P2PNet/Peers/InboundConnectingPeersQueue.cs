using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    internal class InboundConnectingPeersQueue
    {
        private volatile Queue<GenericPeer> _queue = new Queue<GenericPeer>();

        public event EventHandler<IncomingPeerEventArgs> IncomingPeerConnectionAttempt;

        public void Enqueue(GenericPeer peer)
        {
                _queue.Enqueue(peer);
                OnIncomingPeerConnectionAttempt(peer);
        }

        public bool PeerIsQueued(string IPAddress)
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

        public GenericPeer Dequeue()
        {
            return _queue.Dequeue();
        }

        public int Count => _queue.Count;

        protected virtual void OnIncomingPeerConnectionAttempt(GenericPeer peer)
        {
            IncomingPeerConnectionAttempt?.Invoke(this, new IncomingPeerEventArgs(peer));
        }
    }

}