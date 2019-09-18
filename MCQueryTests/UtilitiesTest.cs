using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace MCQueryTests
{
    [TestClass]
    public class UtilitiesTest
    {
        [TestMethod]
        public void ReadVarIntTest()
        {
            byte[] samplePacketLength = new byte[] { 0x81, 0x01 };  // VarInt(129) == 0x81, 0x01
            byte[] samplePacketId = new byte[] { 0x00 };            // VarInt(0)   == 0x00
            byte[] sampleResponseLength = new byte[] { 0x7F };      // VarInt(127) == 0x7F

            bool cond1, cond2, cond3;

            using (MemoryStream memory = new MemoryStream(samplePacketLength))
            {
                int result = MCQuery.Utilities.ReadVarInt(memory);
                cond1 = result == 129;
            }

            using (MemoryStream memory = new MemoryStream(samplePacketId))
            {
                int result = MCQuery.Utilities.ReadVarInt(memory);
                cond2 = result == 0;
            }

            using (MemoryStream memory = new MemoryStream(sampleResponseLength))
            {
                int result = MCQuery.Utilities.ReadVarInt(memory);
                cond3 = result == 127;
            }

            Assert.IsTrue(cond1 && cond2 && cond3);
        }
    }
}