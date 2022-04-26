using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Suborner.Module
{
    public static class Utility
    {
        public static byte[] StringToByteArray(string s)
        {
            return Enumerable.Range(0, s.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(s.Substring(x, 2), 16))
                .ToArray();
        }

        public static string ConvertRIDToHexString(int rid) 
        {
            return rid.ToString("X8");
        }

        public static int FieldOffset<T>(string fieldName)
        {
            return Marshal.OffsetOf(typeof(T), fieldName).ToInt32();
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }

}
