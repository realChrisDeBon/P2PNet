using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PNet.DicoveryChannels.WAN
    {
    /// <summary>
    /// Communicates with a bootstrap server to share known (public) peers and establish identity in network bootstrap.
    /// </summary>
    /// <remarks>Bootstrap server will direct public-facing peers to requesters and pass peers behind NAT to TURN server.</remarks>
    internal class BootstrapChannel
        {
        private string BootstrapServerEndpoint { get; set; }
        public BootstrapChannel(string url)
            {
            BootstrapServerEndpoint = url;
            }

        // TODO :: Will roll implementation out incrementally in tandem with P2PNet.Bootstrap library development

        }
    }
