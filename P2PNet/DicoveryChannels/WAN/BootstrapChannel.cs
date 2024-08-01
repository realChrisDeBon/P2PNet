using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PNet.DicoveryChannels.WAN
    {
    /// <summary>
    /// Communicates with a bootstrap server to share known peers and establish identity in network.
    /// </summary>
    /// <remarks>Bootstrap server will direct connecting peers with a CollectionSharePacket and other means of conveying network information.</remarks>
    public class BootstrapChannel
        {
        private string BootstrapServerEndpoint { get; set; }
        public BootstrapPeer BootstrapServer { get; set; }
        public BootstrapChannel()
            {
            
            }

        // TODO :: Will roll implementation out incrementally in tandem with P2PNet.Bootstrap library development

        }
    }