# P2PNet
<p align="center">
    <img src="https://github.com/realChrisDeBon/P2PNet/assets/97779307/36f3441a-2905-476e-ac6a-c5fa8a9112b0" width="175" height="175" alt="p2pnet logo">
</p>
P2PNet is meant to facilitate true serverless peer to peer networking. Initial peer discovery is initiated at the local area network level by the PeerNetwork. The PeerNetwork will be able to use a range of interoperable WAN and LAN discovery mechanisms to expand and grow the network. Implementing the P2PNet library, you will be able to integrate your own verification steps and protocols to validate discovered peer members before establishing an enhanced connection that will facilitate the exchange of data and information.

![Static Badge](https://img.shields.io/badge/LAN_discovery-working-darkgreen)

![Static Badge](https://img.shields.io/badge/peer_communication-mostly_working-darkgreen)

![Static Badge](https://img.shields.io/badge/data_transmission-mostly_working-darkgreen)

**Note:** DistributionHandler is still in mid development and more functionality will be rolled out. In its present state it works to transmit, recieve, and store.

![Static Badge](https://img.shields.io/badge/WAN_discovery-IN_PROGRESS-yellow)

**Note:** WAN discovery is in preliminary stages of development. There is a mostly working proof-of-concept Widescan feature which leverages GPU offloading to mass generate IPv6 addresses to send ICMP packets with just enough data to tell the recipient how to connect, and the Bootstrap server has been outlined but still needs work as well. 

Roadmap:
1. Short term implementation to improve reliability and stability of peer connections: ~~**[1]** Rotating broadcast port~~ ~~**[2]** Discern excess local connection and trim them~~ ~~**[3]** Variabalize rate of broadcasting as to a sine wave rather than constant heart beat to lighten computation load~~
2. Full implementation of duty packets (CollectionSharePacket and DisconnectPacket) to enchance peer discovery and connection integrity.
3. ~~Medium term finalization of DistributionHandler to faciliate data transmission among peers.~~ 
4. Initial WAN discovery mechanisms:
   * [P2PNet.Bootstrap](https://github.com/realChrisDeBon/P2PNet.Bootstrap) ![Static Badge](https://img.shields.io/badge/non_working-IN_PROGRESS-yellow)
   * P2PNet.TURN ![Static Badge](https://img.shields.io/badge/non_working-IN_PLANNING-orange)
   * [P2PNet.Widescan](https://github.com/realChrisDeBon/P2PNet/blob/P2PNet.Widescan/P2PNet/DicoveryChannels/WAN/Widescan.cs) ![Static Badge](https://img.shields.io/badge/non_working-IN_PROGRESS-yellow)
5. Thorough documentation published.
6. Public NuGet launch.
