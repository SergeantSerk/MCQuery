using MCQuery;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace MCQueryTests
{
    [TestClass]
    public class UtilitiesTest
    {
        private readonly int[] intTestCases = new[]
            {
                0,
                1,
                2,
                127,
                128,
                255,
                2147483647,
                -1,
                -2147483648
            };

        private readonly byte[][] byteArrayTestCases = new byte[][]
{
                new byte[] { 0x00 },                            // VarInt(0)
                new byte[] { 0x01 },                            // VarInt(1)
                new byte[] { 0x02 },                            // VarInt(2)
                new byte[] { 0x7F },                            // VarInt(127)
                new byte[] { 0x80, 0x01 },                      // VarInt(128)
                new byte[] { 0xFF, 0x01 },                      // VarInt(255)
                new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x07 },    // VarInt(2147483647)
                new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x0F, },   // VarInt(-1)
                new byte[] { 0x80, 0x80, 0x80, 0x80, 0x08 }     // VarInt(-2147483648)
};

        [TestMethod]
        public void PingTest()
        {
            // Note: make a better test with TcpListener and some fake handshake to test this method
            var address = "hub.mc-complex.com";
            var port = 25565;
            var server = new MCServer(address, port);
            var ping = server.Ping();
            if (ping <= 0)
            {
                throw new InvalidDataException("Ping cannot be equal to or less than 0.");
            }
        }

        [TestMethod]
        public void StatusTest()
        {
            // Note: make a better test with TcpListener and some fake handshake to test this method
            var address = "hub.mc-complex.com";
            var port = 25565;
            var server = new MCServer(address, port);
            var response = server.Status();
            if (response == null)
            {
                throw new InvalidDataException("Returned server status was empty or null.");
            }
            // Don't catch JsonException, test will detect this throw
            // Parse to expected minimal server status
        }

        [TestMethod]
        public void WriteVarIntTest()
        {
            using (MemoryStream memory = new MemoryStream())
            {
                for (int i = 0; i < intTestCases.Length; ++i)
                {
                    memory.SetLength(0);                                // Reset stream
                    Utilities.WriteVarInt(memory, intTestCases[i]);     // Write integer (VarInt) to stream
                    byte[] result = memory.ToArray();
                    if (!byteArrayTestCases[i].SequenceEqual(result))
                    {
                        Assert.Fail($"Expected {BitConverter.ToString(byteArrayTestCases[i])}, got {BitConverter.ToString(result)}");
                    }
                }
            }
        }

        [TestMethod]
        public void ReadVarIntTest()
        {
            using (MemoryStream memory = new MemoryStream())
            {
                for (int i = 0; i < byteArrayTestCases.Length; ++i)
                {
                    var result = ReadVarIntHelper(memory, byteArrayTestCases[i]);
                    if (result != intTestCases[i])
                    {
                        Assert.Fail($"Expected {intTestCases[i]}, got {result}.");
                    }
                }
            }
        }

        private static int ReadVarIntHelper(Stream stream, byte[] data)
        {
            StreamHelper(stream, data);             // Setup stream for testing
            return Utilities.ReadVarInt(stream);    // Get VarInt from stream
        }

        private static void StreamHelper(Stream stream, byte[] data)
        {
            stream.SetLength(0);                // Reset stream
            stream.Write(data);                 // Write test bytes
            stream.Seek(0, SeekOrigin.Begin);   // Go back to the start for reading
        }
    }
}