using MCQuery;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace MCQueryTests
{
    [TestClass]
    public class UtilitiesTest
    {
        [TestMethod]
        public void PingTest()
        {
            string server = "mc.hypixel.net";
            ushort port = 25565;
            double ping = Minecraft.Ping(server, port);
            if (ping <= 0)
            {
                throw new InvalidDataException("Ping cannot be equal to or less than 0.");
            }
        }

        [TestMethod]
        public void WriteVarIntTest()
        {
            int[] testCases = new int[]
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
            byte[][] testCasesResults = new byte[][]
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

            using (MemoryStream memory = new MemoryStream())
            {
                for (int i = 0; i < testCases.Length; ++i)
                {
                    memory.SetLength(0);                                // Reset stream
                    Utilities.WriteVarInt(memory, testCases[i]);        // Write integer (VarInt) to stream
                    byte[] result = memory.ToArray();
                    if (!testCasesResults[i].SequenceEqual(result))
                    {
                        Assert.Fail($"Expected {BitConverter.ToString(testCasesResults[i])}, got {BitConverter.ToString(result)}");
                    }
                }
            }
        }

        [TestMethod]
        public void ReadVarIntTest()
        {
            byte[][] testCases = new byte[][]
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
            int[] testCasesResults = new int[]
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

            using (MemoryStream memory = new MemoryStream())
            {
                for (int i = 0; i < testCases.Length; ++i)
                {
                    int result = ReadVarIntHelper(memory, testCases[i]);
                    if (result != testCasesResults[i])
                    {
                        Assert.Fail($"Expected {testCasesResults[i]}, got {result}.");
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