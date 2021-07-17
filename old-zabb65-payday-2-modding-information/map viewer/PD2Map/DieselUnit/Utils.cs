using System.Runtime.InteropServices;
using System.Text;

namespace DieselUnit
{
    namespace Utils
    {
        public class Hash64
        {
            [DllImport("hash64.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern ulong Hash(byte[] k, ulong length, ulong level);
            public static ulong HashString(string input, ulong level = 0)
            {
                return Hash(Encoding.UTF8.GetBytes(input), (ulong)Encoding.UTF8.GetByteCount(input), level);
            }
        }
    }
}
