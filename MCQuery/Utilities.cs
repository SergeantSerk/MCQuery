using System;
using System.IO;

namespace MCQuery
{
    public static class Utilities
    {
        public static void WriteVarLong(Stream stream, long value)
        {
            do
            {
                byte temp = (byte)(value & 0b01111111);
                // Note: >>> means that the sign bit is shifted with the rest of the number rather than being left alone
                value >>= 7;
                if (value != 0)
                {
                    temp |= 0b10000000;
                }
                stream.WriteByte(temp);
            } while (value != 0);
        }

        public static void WriteVarInt(Stream stream, uint value)
        {
            do
            {
                byte temp = (byte)(value & 0b01111111);
                // Note: >>> means that the sign bit is shifted with the rest of the number rather than being left alone
                value >>= 7;
                if (value != 0)
                {
                    temp |= 0b10000000;
                }
                stream.WriteByte(temp);
            } while (value != 0);
        }

        public static long ReadVarLong(Stream stream)
        {
            int numRead = 0;
            long result = 0;
            byte read;
            do
            {
                read = (byte)stream.ReadByte();
                int value = read & 0b01111111;
                result |= value << (7 * numRead);

                numRead++;
                if (numRead > 10)
                {
                    throw new FormatException("VarLong is too big");
                }
            } while ((read & 0b10000000) != 0);

            return result;
        }

        public static int ReadVarInt(Stream stream)
        {
            int numRead = 0;
            int result = 0;
            byte read;
            do
            {
                read = (byte)stream.ReadByte();
                int value = read & 0b01111111;
                result |= (value << (7 * numRead));

                numRead++;
                if (numRead > 5)
                {
                    throw new FormatException("VarInt is too big.");
                }
            } while ((read & 0b10000000) != 0);

            return result;
        }
    }
}
