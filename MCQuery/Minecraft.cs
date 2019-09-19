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
        public static double Ping(string server, ushort port = 25565)
        {
            double ping;
            using (TcpClient client = InitialiseConnection(server, port, 1))
            using (NetworkStream network = client.GetStream())
            {
                SendPing(network);
                ping = ReceivePing(network);
            }
            return ping;
        }

        /// <summary>
        /// Queries the specified server.
        /// </summary>
        /// <param name="server">The server address to query.</param>
        /// <param name="port">The port the Minecraft server is running on.</param>
        /// <returns>The query response, in JSON.</returns>
        public static string Status(string server, ushort port = 25565)
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
                    Utilities.WriteVarInt(data, Protocol);                      // Protocol
                    data.WriteByte((byte)Encoding.ASCII.GetByteCount(server));  // Address length
                    data.Write(Encoding.ASCII.GetBytes(server));                // Server address
                    data.Write(BitConverter.GetBytes(port));                    // Server port
                    Utilities.WriteVarInt(data, state);                         // State command
                }
                Utilities.WriteVarInt(packet, (int)data.Length + 1);            // Write packet length (Packet ID + Inner Packet)
                Utilities.WriteVarInt(packet, 0);                               // Packet ID (0 = Handshake at this state)
                data.Seek(0, SeekOrigin.Begin);                                 // Seek to beginning and copy inner packet to actual packet
                data.CopyTo(packet);
                packet.Seek(0, SeekOrigin.Begin);                               // Seek to beginning and transmit packet to server
                packet.CopyTo(network);
            }
        }

        private static void SendPing(NetworkStream network)
        {
            stopwatch = new Stopwatch();
            using (MemoryStream packet = new MemoryStream())
            using (MemoryStream data = new MemoryStream())
            {
                {
                    long ticks = DateTime.UtcNow.Ticks;
                    pingBytes = BitConverter.GetBytes(ticks);
                    data.Write(pingBytes);
                }
                Utilities.WriteVarInt(packet, (int)data.Length + 1);
                Utilities.WriteVarInt(packet, 1);
                data.Seek(0, SeekOrigin.Begin);
                data.CopyTo(packet);
                packet.Seek(0, SeekOrigin.Begin);
                stopwatch.Start();
                packet.CopyTo(network);
                stopwatch.Stop();
            }
        }

        private static double ReceivePing(NetworkStream network)
        {
            stopwatch.Stop();
            double ping;
            using (MemoryStream packet = new MemoryStream())
            using (MemoryStream data = new MemoryStream())
            {
                stopwatch.Start();
                int packetLength = Utilities.ReadVarInt(network);
                int packetID = Utilities.ReadVarInt(network);
                stopwatch.Stop();
                byte[] pongBytes = new byte[Math.Max(8, packetLength - 1)];
                stopwatch.Start();
                network.Read(pongBytes);
                stopwatch.Stop();
                ping = stopwatch.Elapsed.TotalMilliseconds;
                stopwatch = null;
                if (!pingBytes.SequenceEqual(pongBytes)) throw new InvalidDataException("Sent ping bytes did not match received pong bytes.");
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
                Utilities.WriteVarInt(packet, (int)data.Length + 1);    // Packet ID + Packet
                Utilities.WriteVarInt(packet, 0);                       // Packet ID
                data.Seek(0, SeekOrigin.Begin);                         // Seek to beginning and copy inner packet to actual packet
                data.CopyTo(packet);
                packet.Seek(0, SeekOrigin.Begin);                       // Seek to beginning and transmit packet to server
                packet.CopyTo(network);
            }
        }

        private static string ReceiveStatusRequest(NetworkStream network)
        {
            string json = string.Empty;
            using (MemoryStream packet = new MemoryStream())
            using (MemoryStream data = new MemoryStream())
            {
                int packetLength = Utilities.ReadVarInt(network);                   // Packet length
                int packetID = Utilities.ReadVarInt(network);                       // Packet ID
                int responseLength = Utilities.ReadVarInt(network);                 // Response length
                byte[] responseBytes = new byte[Math.Min(32767, responseLength)];   // Response

                // Solution courtesy of Marc Gravell (https://stackoverflow.com/a/9461017)
                int read;
                int offset = 0;
                int count = responseLength;
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
