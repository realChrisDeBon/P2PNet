# P2PNet
<p align="center">
    <img src="https://github.com/realChrisDeBon/P2PNet/assets/97779307/36f3441a-2905-476e-ac6a-c5fa8a9112b0" width="175" height="175" alt="p2pnet logo">
</p>
P2PNet is meant to facilitate true serverless peer to peer networking. This is an early-stage project. Initial peer discovery is initiated at the local area network level by the PeerNetwork. The PeerNetwork will be able to use a range of interoperable WAN discovery mechanisms to expand and grow the network, such as bootstrapping and TURN. Implementing the P2PNet library, you will be able to integrate your own verification steps and protocols to validate discovered peer members before establishing an enhanced connection that will facilitate the exchange of data and information.

![Static Badge](https://img.shields.io/badge/LAN_discovery-working-darkgreen)

**Note:** If you are on a LAN with two machines and they both startup on the same broadcast port within a minute of eachother, there can be hangups that lead to hindered discovery.

![Static Badge](https://img.shields.io/badge/peer_communication-mostly_working-darkgreen)

**Note:** Generally, with enough time, LAN peers will establish reliable communication and ping each other. There are some instances such as the scenario above where there's a rare exception.

![Static Badge](https://img.shields.io/badge/data_transmission-mostly_working-darkgreen)

**Note:** DistributionHandler is still in mid development and more functionality will be rolled out. In its present state it works to transmit, recieve, and store.

![Static Badge](https://img.shields.io/badge/WAN_discovery-IN_PROGRESS-yellow)

**Note:** WAN discovery still in early development. See [P2PNet.Bootstrap](https://github.com/realChrisDeBon/P2PNet.Bootstrap)

Roadmap:
1. Short term implementation to improve reliability and stability of peer connections: ~~**[1]** Rotating broadcast port~~ ~~**[2]** Discern excess local connection and trim them~~ ~~**[3]** Variabalize rate of broadcasting as to a sine wave rather than constant heart beat to lighten computation load~~
2. Full implementation of duty packets (CollectionSharePacket and DisconnectPacket) to enchance peer discovery and connection integrity.
3. ~~Medium term finalization of DistributionHandler to faciliate data transmission among peers.~~ 
4. Initial WAN discovery mechanisms:
   * [P2PNet.Bootstrap](https://github.com/realChrisDeBon/P2PNet.Bootstrap) ![Static Badge](https://img.shields.io/badge/non_working-IN_PROGRESS-yellow)
   * P2PNet.TURN ![Static Badge](https://img.shields.io/badge/non_working-IN_PLANNING-orange)
   * P2PNet.Widescan ![Static Badge](https://img.shields.io/badge/non_working-IN_PLANNING-orange)
5. Thorough documentation published.
6. Public NuGet launch.
