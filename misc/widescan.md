---
uid: p2pnetwidescan
---

### Widescan 📡

---

The `P2PNet.Widescan` class library project is designed to facilitate mass IPv6 address generation and peer discovery within a peer-to-peer network. This project leverages hardware capabilities, such as GPUs, to efficiently generate vast quantities of IPv6 addresses. By utilizing user-defined address prefixes, it allows for a more targeted and narrow scope of addresses to ping, enhancing the efficiency of the discovery process. This can be leveraged with publicly available information on IPv6 prefix registrations, like the [Ripe database](https://apps.db.ripe.net/db-web-ui/query), in order to refine the scope of the widescan. Additionally, the project employs lightweight ICMP packets to broadcast discovery information, ensuring minimal network overhead while effectively communicating with potential peers.

### Initialization

---

The `P2PNet.Widescan` project initializes by setting up the necessary configurations and services required for widescan operations. The main entry point is the `Widescan` class, which configures the application and starts the widescan process.

1. **Configuration**: The application reads configuration settings from the provided parameters. This includes settings for address prefixes, hardware mode (GPU or CPU), and other essential configurations.
2. **Logging**: Logging is configured to use a plain text format and is activated to capture important events and errors.
3. **Hardware Mode Setup**: The application can be configured to use either GPU offloading or parallel CPU capabilities for address generation and peer discovery.

### Operation

---

The `P2PNet.Widescan` project operates by providing several key functionalities:

1. **IPv6 Address Generation**: The widescan application generates vast quantities of IPv6 addresses using either GPU offloading or parallel CPU capabilities. This allows for efficient and rapid address generation.

   - **GPU Offloading**: Utilizes the processing power of GPUs to generate IPv6 addresses in parallel, significantly speeding up the process.
   - **Parallel CPU Capabilities**: Uses multiple CPU cores to generate IPv6 addresses in parallel, providing an alternative for systems without GPU support.
2. **Peer Discovery**: The widescan application pings the generated IPv6 addresses to discover potential peers within the network. It uses lightweight ICMP packets to broadcast discovery information, ensuring minimal network overhead.
3. **Address Prefix Filtering**: By utilizing user-defined address prefixes, the widescan application narrows the scope of addresses to ping, enhancing the efficiency of the discovery process. This can be refined using publicly available information on IPv6 prefix registrations, such as the [Ripe database](https://apps.db.ripe.net/db-web-ui/query).

### Integration

---

The `P2PNet.Widescan` project is designed to be a modular import that can be integrated with other P2P network applications, such as a`P2PNetwork` client application or the `P2PBootstrap` server. It does not run independently and does not use `appsettings.json` for configuration. Instead, it relies on the host application to provide the necessary configuration parameters.

### Diagrams

---

To supplement the information visually, the following diagrams are provided:

1. **Widescan Architecture**: Shows the overall architecture of the widescan application, including its interaction with the P2P network and the hardware components (GPU/CPU).
2. **IPv6 Address Generation Flow**: Illustrates the flow of IPv6 address generation, from configuration to address generation using GPU or CPU, and finally to peer discovery. This is very much a simple consumer-producer pattern, with intermediate in-memory queues to operate as a broadcaster.
3. **ICMP Packet Listener:** Operating independently of the broadcaster, the ICMP Packet Listener operates as a listener for responses from prospective peers and other potential widescan instances. This utilizes a docile form of packet sniffing on the host machine.

<p>
    <img src="https://raw.githubusercontent.com/realChrisDeBon/P2PNet/refs/heads/master/misc/Widescan.png" width="500" height="325" alt="widescan chart">
</p>

**Note:** Widescan project still under construction 🏗️