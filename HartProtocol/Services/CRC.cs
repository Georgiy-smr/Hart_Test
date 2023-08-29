using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HartProtocol.Services
{
    public static class CrcXor
    {
        public static byte Calculate(byte[] buffer)
        {
            return Calculate(buffer, 0, buffer.Length);
        }

        public static byte Calculate(byte[] buffer, int offset, int count)
        {
            byte b = 0;
            for (int i = offset; i < offset + count; i++)
            {
                b = (byte)(b ^ buffer[i]);
            }

            return b;
        }
    }
    public static class Convectors
    {
        public static string ByteToHex(byte[] comByte)
        {
            StringBuilder builder = new StringBuilder(comByte.Length * 3);
            foreach (byte data in comByte)
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').
                    PadRight(3, ' '));
            return builder.ToString().ToUpper();
        }
    }

}
