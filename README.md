# P2PNet

<p align="center">
    <img src="https://github.com/realChrisDeBon/P2PNet/assets/97779307/36f3441a-2905-476e-ac6a-c5fa8a9112b0" width="175" height="175" alt="p2pnet logo">
</p>

P2PNet facilitates peer to peer networking with an array of components and options for setting up your network. Initial peer discovery can be initiated in the LAN, and facilitated over a WAN utilizing various methods such as bootstrapping and IPv6 ICMP blasting (widescan). The PeerNetwork will be able to use a range of interoperable WAN and LAN discovery mechanisms to expand and grow the network. Implementing the P2PNet library, you will be able to integrate your own verification steps and protocols to validate discovered peer members before establishing an enhanced connection that will facilitate the exchange of data and information.




**Note:** DistributionHandler is still in mid development and more functionality will be rolled out. In its present state it works to transmit, recieve, and store.

**Note:** WAN discovery is in preliminary stages of development. There is a mostly working IPv6 ICMP Widescan feature which leverages GPU offloading to mass generate IPv6 addresses to send ICMP packets with just enough data to tell the recipient how to connect, and the bootstrap server will be a containerized instance with a configuration panel - this should be ideal for VPS deployments.
