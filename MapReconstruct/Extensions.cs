using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;

namespace MapReconstruct
{
    public static class Extensions
    {
        public static T Read<T>(this BinaryReader reader) where T : struct
        {
            byte[] result = reader.ReadBytes(Unsafe.SizeOf<T>());

            return Unsafe.ReadUnaligned<T>(ref result[0]);
        }
    }
}
