using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MCQuery
{
    public class MCServer
    {
        private Stopwatch stopwatch;
        private byte[] pingBytes;

        /// <summary>
        /// Address of the Minecraft server.
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        /// Port of the Minecraft server.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Protocol number for latest, stable Minecraft.
        /// </summary>
        public int Protocol { get; private set; }

        #region Public Methods
        public MCServer(string Address, int Port) : this(Address, Port, -1)
        {

        }

        public MCServer(string Address, int Port, int Protocol)
        {
            if (Port < IPEndPoint.MinPort || Port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("Port is out of range (must be between 0 and 65535)");
            }
            else if (Protocol < -1)
            {
                throw new ArgumentOutOfRangeException("Protocol version cannot be less than -1");
            }

            this.Address = Address;
            this.Port = Port;
            this.Protocol = Protocol;
        }

        /// <summary>
        /// Pings the specified server.
        /// </summary>
        /// <param name="Timeout">Timeout duration for connecting with the host, in milliseconds.</param>
        /// <returns>The elapsed time between sending the ping and receiving the ping, in milliseconds. Default 5000</returns>
        public double Ping(int Timeout = 5000)
        {
            double ping;
            // Attempt to ping, without requesting status (this does not work on some servers)
            try
            {
                using (TcpClient client = InitialiseConnection(1, Timeout))
                using (NetworkStream network = client.GetStream())
                {
                    SendPing(network);
                    ping = ReceivePing(network);
                }
            }
            catch (Exception)
            {
                // If server requires status request first, send that, then ping
                using (TcpClient client = InitialiseConnection(1, Timeout))
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
        /// Performs a standard status request to the Minecraft server but deserialises to type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type to deserialise received JSON response to.</typeparam>
        /// <param name="Timeout">Timeout duration for connecting with the host, in milliseconds. Default 5000</param>
        /// <returns>The query response as <see cref="T"/>.</returns>
        public T Status<T>(int Timeout = 5000)
        {
            string json = string.Empty;
            using (TcpClient client = InitialiseConnection(1, Timeout))
            using (NetworkStream network = client.GetStream())
            {
                SendStatusRequest(network);
                json = ReceiveStatusRequest(network);
            }
            var result = JsonConvert.DeserializeObject<T>(json);
            return result;
        }

        /// <summary>
        /// Queries the specified server.
        /// </summary>
        /// <param name="Timeout">Timeout duration for connecting with the host, in milliseconds. Default 5000</param>
        /// <returns>The query response as <see cref="ServerStatus"/>.</returns>
        public ServerStatus Status(int Timeout = 5000)
        {
            return Status<ServerStatus>(Timeout);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initialise a connection to the specified server and set up connection by performing a handshake.
        /// </summary>
        /// <param name="state">The next state that should follow after the handshake, usually 1 for status, 2 for login.</param>
        /// <param name="timeout">Timeout period of attempting to send and receive data from host.</param>
        /// <returns>A <see cref="TcpClient"/> is returned which has an initialised connection that is set up.</returns>
        private TcpClient InitialiseConnection(int state, int timeout = 5000)
        {
            TcpClient client = new TcpClient();
            client.Client.Blocking = true;
            var result = client.BeginConnect(Address, Port, null, null);
            result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(timeout));
            if (!client.Connected)
            {
                throw new TimeoutException("Connection to host timed out");
            }
            Handshake(client.GetStream(), state);
            return client;
        }

        /// <summary>
        /// Performs a handshake with the server. (https://wiki.vg/Protocol#Handshake)
        /// </summary>
        /// <param name="network">A connected network stream with the server.</param>
        /// <param name="state">The state command that follows after this handshake (usually 1 for status, 2 for login).</param>
        private void Handshake(NetworkStream network, int state)
        {
            if (network == null)
            {
                throw new ArgumentNullException("Cannot perform handshake with a null network stream");
            }
            else if (!network.CanWrite)
            {
                throw new IOException("Cannot write to this network stream");
            }

            using (MemoryStream packet = new MemoryStream())
            using (MemoryStream data = new MemoryStream())
            {
                {
                    // Protocol
                    Utilities.WriteVarInt(data, Protocol);
                    // Address Length
                    data.WriteByte((byte)Encoding.ASCII.GetByteCount(Address));
                    // Server address
                    var addressBytes = Encoding.ASCII.GetBytes(Address);
                    data.Write(addressBytes, 0, addressBytes.Length);
                    // Server port
                    var portBytes = BitConverter.GetBytes((ushort)Port);
                    data.Write(portBytes, 0, portBytes.Length);
                    // State command
                    Utilities.WriteVarInt(data, state);
                }
                // Append data to packet and send to network
                // (Packet ID 0 = Handshake at this state)
                SendConstructedPacket(network, packet, data, 0);
            }
        }

        private void SendPing(NetworkStream network)
        {
            if (network == null)
            {
                throw new ArgumentNullException("Cannot send ping to a null network stream");
            }
            else if (!network.CanWrite)
            {
                throw new IOException("Cannot write to this network stream");
            }

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
                    data.Write(pingBytes, 0, pingBytes.Length);
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

        private double ReceivePing(NetworkStream network)
        {
            if (network == null)
            {
                throw new ArgumentNullException("Cannot receive ping from a null network stream");
            }
            else if (!network.CanRead)
            {
                throw new IOException("Cannot read from this network stream");
            }

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
                {
                    throw new InvalidDataException($"Expected packet ID 1, got {packetID}.");
                }

                // Parse the received ping bytes
                byte[] pongBytes = new byte[Math.Max(8, packetLength - 1)];
                network.Read(pongBytes, 0, pongBytes.Length);
                // Stop stopwatch, no need to measure from this point onwards
                stopwatch.Stop();
                ping = stopwatch.Elapsed.TotalMilliseconds;
                stopwatch = null;

                // Check if the sent bytes matches the bytes received
                if (!pingBytes.SequenceEqual(pongBytes))
                {
                    throw new InvalidDataException("Sent ping bytes did not match received pong bytes.");
                }
            }
            return ping;
        }

        /// <summary>
        /// Sends a status request to the connected network stream. (https://wiki.vg/Protocol#Status)
        /// </summary>
        /// <param name="network">The network stream that is connected to the Minecraft server.</param>
        private static void SendStatusRequest(NetworkStream network)
        {
            if (network == null)
            {
                throw new ArgumentNullException("Cannot send status request to a null network stream");
            }
            else if (!network.CanWrite)
            {
                throw new IOException("Cannot write to this network stream");
            }

            // Initialise packet construction
            using (MemoryStream packet = new MemoryStream())
            using (MemoryStream data = new MemoryStream())
            {
                // No data, only packet ID to indicate status request
                SendConstructedPacket(network, packet, data, 0);
            }
        }

        private static string ReceiveStatusRequest(NetworkStream network)
        {
            if (network == null)
            {
                throw new ArgumentNullException("Cannot receive status request from a null network stream");
            }
            else if (!network.CanRead)
            {
                throw new IOException("Cannot read from this network stream");
            }

            string json = string.Empty;
            using (MemoryStream packet = new MemoryStream())
            using (MemoryStream data = new MemoryStream())
            {
                // Get packet length
                int packetLength = Utilities.ReadVarInt(network);
                // 4 == At least one byte for Packet ID + Response length + minimum JSON "[]"
                if (packetLength <= 4)
                {
                    throw new InvalidDataException($"Packet length is of unusual size ({packetLength} bytes).");
                }

                // Get packet ID
                int packetID = Utilities.ReadVarInt(network);
                if (packetID != 0)
                {
                    throw new InvalidDataException($"Expected packet ID 0, got {packetID}.");
                }

                // Get response length
                int responseLength = Utilities.ReadVarInt(network);
                if (responseLength < 0)
                {
                    throw new InvalidDataException($"Response length size was less than 0.");
                }

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

        private static void SendConstructedPacket(NetworkStream network, MemoryStream packet, MemoryStream data, int packetId)
        {
            // Write packet length (Packet ID + Inner Packet)
            Utilities.WriteVarInt(packet, (int)data.Length + 1);
            // Packet ID
            Utilities.WriteVarInt(packet, packetId);
            // Seek to beginning and copy inner packet to actual packet
            data.Seek(0, SeekOrigin.Begin);
            data.CopyTo(packet);
            // Seek to beginning and transmit packet to server
            packet.Seek(0, SeekOrigin.Begin);
            packet.CopyTo(network);
        }
        #endregion
    }
}
