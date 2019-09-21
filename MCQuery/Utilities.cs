using System;
using System.IO;

namespace MCQuery
{
    public static class Utilities
    {
        #region VarInt (https://wiki.vg/Protocol#VarInt_and_VarLong)
        public static void WriteVarInt(Stream stream, int value)
        {
            // Converting int to uint is necessary to preserve the sign bit
            // when performing bit shifting
            uint actual = (uint)value;
            do
            {
                byte temp = (byte)(actual & 0b01111111);
                // Note: >>> means that the sign bit is shifted with the
                // rest of the number rather than being left alone
                actual >>= 7;
                if (actual != 0)
                {
                    temp |= 0b10000000;
                }
                stream.WriteByte(temp);
            } while (actual != 0);
        }

        public static int ReadVarInt(Stream stream)
        {
            int numRead = 0;
            int result = 0;
            byte read;
            do
            {
                int r = stream.ReadByte();
                if (r == -1)
                {
                    break;
                }

                read = (byte)r;
                int value = read & 0b01111111;
                result |= (value << (7 * numRead));

                numRead++;
                if (numRead > 5)
                {
                    throw new FormatException("VarInt is too big.");
                }
            } while ((read & 0b10000000) != 0);

            if (numRead == 0)
            {
                throw new InvalidOperationException("Expected a VarInt, none was read.");
            }
            return result;
        }
        #endregion
    }
}
