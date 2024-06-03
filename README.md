# P2PNet
<p align="center">
    <img src="https://github.com/realChrisDeBon/P2PNet/assets/97779307/36f3441a-2905-476e-ac6a-c5fa8a9112b0" width="175" height="175" alt="p2pnet logo">
</p>
P2PNet is meant to facilitate true serverless peer to peer networking. This is an early stage project. Initial peer discovery is initiated as the client randomly selects one port of a fixed collection to be the designated broadcast port, while listening on all the others. The pool of ports are, theoretically, the same for all clients, so as they begin to broadcast to the network they will at some point reach  each other. Multicast is also utilized if available to the network. The clients are broadcasting a unique identifier that tells the reciever [1] Whether or not the packet came from a trusted source [2] How to connect to the sender, which the receiver can use to then establish a reliable TCP connection. Once a TCP connection is esablished, data transmission can begin.

![Static Badge](https://img.shields.io/badge/LAN_discovery-working-darkgreen)

**Note:** If you are on a LAN with two machines and they both startup on the same broadcast port within a minute of eachother, there can be hangups that lead to hindered discovery.

![Static Badge](https://img.shields.io/badge/peer_communication-mostly_working-darkgreen)

**Note:** Generally, with enough time, LAN peers will establish reliable communication and ping each other. There are some instances such as the scenario above where there's a rare exception.

![Static Badge](https://img.shields.io/badge/data_transmission-mostly_working-darkgreen)

**Note:** DistributionHandler is still in mid development and more functionality will be rolled out. In its present state it works to transmit, recieve, and store.

![Static Badge](https://img.shields.io/badge/WAN_discovery-TODO-orange)

**Note:** WAN discovery will likely entail a blend of spaghetti flinging, port scanning, and while not in the spirit of 'true serverless' in the most purist sense, an extension library for setting up trusted nodes with API endpoints to serve as bootstraps, alongside hardcoded bootstraps.

Roadmap:
1. Short term implementation to improve reliability and stability of peer connections: ~~**[1]** Rotating broadcast port~~ ~~**[2]** Discern excess local connection and trim them~~ ~~**[3]** Variabalize rate of broadcasting as to a sine wave rather than constant heart beat to lighten computation load~~
2. Full implementation of duty packets (CollectionSharePacket and DisconnectPacket) to enchance peer discovery and connection integrity.
3. ~~Medium term finalization of DistributionHandler to faciliate data transmission among peers.~~ 
4. Long term implementation of WAN discovery mechanism(s).
