using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace MCQuery
{
    public static class Minecraft
    {
        /// <summary>
        /// Protocol number for latest, stable Minecraft.
        /// </summary>
        public static int Protocol = 498;

        private static Stopwatch stopwatch;
        private static byte[] pingBytes;

        #region Public Methods
        /// <summary>
        /// Pings the specified server.
        /// </summary>
        /// <param name="server">The server address to query.</param>
        /// <param name="port">The port the Minecraft server is running on.</param>
        /// <returns>The elapsed time between sending the ping and receiving the ping, in milliseconds.</returns>
        public static double Ping(string server, ushort port)
        {
            double ping;
            // Attempt to ping, without requesting status (this does not work on some servers)
            try
            {
                using (TcpClient client = InitialiseConnection(server, port, 1))
                using (NetworkStream network = client.GetStream())
                {
                    SendPing(network);
                    ping = ReceivePing(network);
                }
            }
            catch (Exception)
            {
                // If server requires status request first, send that, then ping
                using (TcpClient client = InitialiseConnection(server, port, 1))
                using (NetworkStream network = client.GetStream())
                {
                    SendStatusRequest(network);
                    ReceiveStatusRequest(network);
                    SendPing(network);
                    ping = ReceivePing(network);
                }
            }
            return ping;
        }

        /// <summary>
        /// Queries the specified server.
        /// </summary>
        /// <param name="server">The server address to query.</param>
        /// <param name="port">The port the Minecraft server is running on.</param>
        /// <returns>The query response, in JSON.</returns>
        public static string Status(string server, ushort port)
        {
            string json = string.Empty;
            using (TcpClient client = InitialiseConnection(server, port, 1))
            using (NetworkStream network = client.GetStream())
            {
                SendStatusRequest(network);
                json = ReceiveStatusRequest(network);
            }
            return json;
        }
        #endregion

        /// <summary>
        /// Initialise a connection to the specified server and set up connection by performing a handshake.
        /// </summary>
        /// <param name="server">The server to connect and handshake with.</param>
        /// <param name="port">The port of the Minecraft server.</param>
        /// <param name="state">The next state that should follow after the handshake, usually 1 for status, 2 for login.</param>
        /// <returns>A <see cref="TcpClient"/> is returned which has an initialised connection that is set up.</returns>
        private static TcpClient InitialiseConnection(string server, ushort port, int state)
        {
            TcpClient client = new TcpClient();
            client.Client.Blocking = true;
            client.Connect(server, port);
            Handshake(client.GetStream(), state, server, port);
            return client;
        }

        /// <summary>
        /// Performs a handshake with the server. (https://wiki.vg/Protocol#Handshake)
        /// </summary>
        /// <param name="network">A connected network stream with the server.</param>
        /// <param name="state">The state command that follows after this handshake (usually 1 for status, 2 for login).</param>
        /// <param name="server">The server to handshake with.</param>
        /// <param name="port">The port of the Minecraft server.</param>
        private static void Handshake(NetworkStream network, int state, string server, ushort port)
        {
            using (MemoryStream packet = new MemoryStream())
            using (MemoryStream data = new MemoryStream())
            {
                {
                    // Protocol
                    Utilities.WriteVarInt(data, Protocol);
                    // Address Length
                    data.WriteByte((byte)Encoding.ASCII.GetByteCount(server));
                    // Server address
                    data.Write(Encoding.ASCII.GetBytes(server));
                    // Server port
                    data.Write(BitConverter.GetBytes(port));
                    // State command
                    Utilities.WriteVarInt(data, state);
                }
                // Write packet length (Packet ID + Inner Packet)
                Utilities.WriteVarInt(packet, (int)data.Length + 1);
                // Packet ID (0 = Handshake at this state)
                Utilities.WriteVarInt(packet, 0);
                // Seek to beginning and copy inner packet to actual packet
                data.Seek(0, SeekOrigin.Begin);
                data.CopyTo(packet);
                // Seek to beginning and transmit packet to server
                packet.Seek(0, SeekOrigin.Begin);
                packet.CopyTo(network);
            }
        }

        private static void SendPing(NetworkStream network)
        {
            // Prepare new stopwatch for measuring ping
            stopwatch = new Stopwatch();
            using (MemoryStream packet = new MemoryStream())
            using (MemoryStream data = new MemoryStream())
            {
                {
                    // Get some long to send to the server
                    long ticks = DateTime.UtcNow.Ticks;
                    // Convert that into long bytes
                    pingBytes = BitConverter.GetBytes(ticks);
                    // Write long into data stream
                    data.Write(pingBytes);
                }
                // Write packet length (including packet ID)
                Utilities.WriteVarInt(packet, (int)data.Length + 1);
                // Write packet ID (1 for ping state)
                Utilities.WriteVarInt(packet, 1);
                // Seek to the start and copy over to packet stream
                data.Seek(0, SeekOrigin.Begin);
                data.CopyTo(packet);
                // Seek to the start and copy over to network stream
                packet.Seek(0, SeekOrigin.Begin);
                // Begin stopwatch to measure ping (from when data is being transmitted)
                stopwatch.Start();
                packet.CopyTo(network);
            }
        }

        private static double ReceivePing(NetworkStream network)
        {
            // Store ping for return
            double ping;
            using (MemoryStream packet = new MemoryStream())
            using (MemoryStream data = new MemoryStream())
            {
                // Read packet length from first VarInt
                int packetLength = Utilities.ReadVarInt(network);
                // Read packet ID from next VarInt
                int packetID = Utilities.ReadVarInt(network);
                if (packetID != 1)
                    throw new InvalidDataException($"Expected packet ID 1, got {packetID}.");

                // Parse the received ping bytes
                byte[] pongBytes = new byte[Math.Max(8, packetLength - 1)];
                network.Read(pongBytes);
                // Stop stopwatch, no need to measure from this point onwards
                stopwatch.Stop();
                ping = stopwatch.Elapsed.TotalMilliseconds;
                stopwatch = null;

                // Check if the sent bytes matches the bytes received
                if (!pingBytes.SequenceEqual(pongBytes))
                    throw new InvalidDataException("Sent ping bytes did not match received pong bytes.");
            }
            return ping;
        }

        /// <summary>
        /// Sends a status request to the connected network stream. (https://wiki.vg/Protocol#Status)
        /// </summary>
        /// <param name="network">The network stream that is connected to the Minecraft server.</param>
        private static void SendStatusRequest(NetworkStream network)
        {
            // Initialise packet construction
            using (MemoryStream packet = new MemoryStream())
            using (MemoryStream data = new MemoryStream())
            {
                // No data, only packet ID to indicate status request
                // Packet ID + Packet
                Utilities.WriteVarInt(packet, (int)data.Length + 1);
                // Packet ID
                Utilities.WriteVarInt(packet, 0);
                // Seek to beginning and copy inner packet to actual packet
                data.Seek(0, SeekOrigin.Begin);
                data.CopyTo(packet);
                // Seek to beginning and transmit packet to server
                packet.Seek(0, SeekOrigin.Begin);
                packet.CopyTo(network);
            }
        }

        private static string ReceiveStatusRequest(NetworkStream network)
        {
            string json = string.Empty;
            using (MemoryStream packet = new MemoryStream())
            using (MemoryStream data = new MemoryStream())
            {
                // Get packet length
                int packetLength = Utilities.ReadVarInt(network);
                // Get packet ID
                int packetID = Utilities.ReadVarInt(network);
                // Get response length
                int responseLength = Utilities.ReadVarInt(network);
                // Allocate responseLength bytes amount of memory, or at most 32767 bytes
                byte[] responseBytes = new byte[Math.Min(32767, responseLength)];

                // Solution courtesy of Marc Gravell (https://stackoverflow.com/a/9461017)
                // Read in responseBytes.Length amount of bytes from the network
                int read;
                int offset = 0;
                int count = responseBytes.Length;
                while (count > 0 && (read = network.Read(responseBytes, offset, count)) > 0)
                {
                    offset += read;
                    count -= read;
                }

                json = Encoding.UTF8.GetString(responseBytes);
            }
            return json;
        }
    }
}
