using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace WebTvTest
{
    public class Util
    {
        public static byte[] ReadStream(Stream stream, int size)
        {
            var buffer = new byte[size];
            for (int l = 0; l < buffer.Length; )
            {
                int s = stream.Read(buffer, l, buffer.Length - l);
                if (s <= 0) { return null; }
                l += s;
            }
            return buffer;
        }
        public static string RemoveStartSpace(string input)
        {
            int Pos = 0;
            while ((Pos < input.Length) && (input[Pos] == ' ')) Pos++;
            return input.Substring(Pos);
        }
    }
}
