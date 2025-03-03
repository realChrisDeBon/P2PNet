---
uid: p2pnetbootstrap
---

### Bootstrap 🤝

---

The application serves as a bootstrap node, providing an HTTP endpoint to distribute known peers to new peers joining the network. This setup ensures seamless peer discovery and initialization, enabling efficient and secure distributed data exchange within the peer network. By containerizing the application using Docker, deployment becomes significantly easier and makes quick VPS deployments easy. Additionally, the user control panel offers finer-grained controls over the network, including scaling and monitoring, which enhances the manageability and reliability of the peer network infrastructure.

### Initialization

---

The `P2PBootstrap` project initializes by setting up the necessary configurations and services required for the bootstrap server to operate. The main entry point is the `Program.cs` file, which configures the application and starts the web server.

1. **Configuration**: The application reads configuration settings from the `appsettings.json` file. This includes settings for encryption keys, database paths, and other essential configurations.
2. **Logging**: Logging is configured to use a plain text format and is activated to capture important events and errors.
3. **Web Server Setup**: The application uses ASP.NET Core to set up the web server. It configures the HTTP request pipeline, enabling default files, static files, and routing. This is an AOT compatible application, as is most of the P2PNet library.

### Operation

---

The `P2PBootstrap` project operates by providing several key functionalities:

1. **Peer Distribution**: The bootstrap server provides an HTTP endpoint (`/api/Bootstrap/peers`) to distribute the list of known peers to new peers joining the network. This endpoint can operate in two modes:

   - **Trustless Mode**: Returns the list of known peers directly.
   - **Authority Mode**: Requires the client to receive and store a public key from the bootstrap server before returning the peer list.
2. **Parser Integration**: The server integrates with a parser to handle input and output operations. It provides endpoints to get parser output (`/api/parser/output`) and to submit parser input (`/api/parser/input`).
3. **Encryption Service**: The server initializes an encryption service to handle secure communication. This includes generating and loading PGP keys from the specified directory.
4. **Database Initialization**: The server initializes a local database to store necessary data. It ensures the database directory exists and sets up the required files.

### User Control Panel

---

The user control panel provides a web-based interface for managing the bootstrap server. In this web-based interface is a terminal for executing commands. It includes the following pages for easier management as well:

1. **Overview**: Displays an overview of the server's status and key metrics.
2. **Settings**: Allows users to modify server settings, such as encryption keys and database paths.
3. **Peers**: Displays the list of known peers and provides options to perform actions on them, such as disconnecting or blocking peers.

### Diagrams

---

To supplement the information visually, the following diagrams are provided:

1. **Bootstrap Server Architecture**: Shows the overall architecture of the bootstrap server, including its interaction with the P2P network and the user control panel.
2. **Peer Distribution Flow**: Illustrates the flow of peer distribution, from a new peer requesting the peer list to the server returning the list based on the configured trust policy.

<p>
    <img src="https://raw.githubusercontent.com/realChrisDeBon/P2PNet/refs/heads/master/misc/Bootstrap.png" width="500" height="325" alt="bootstrap chart">
</p>

**Note:** Bootstrap server still under construction 🏗️