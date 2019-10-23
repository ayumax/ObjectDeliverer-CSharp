using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ObjectDeliverer.Utils
{
    public static class FileStreamExtension
    {
        public static long RemainSize(this FileStream stream)
        {
            return (stream.Length - stream.Position);
        }

        public static async ValueTask<int> ReadIntAsync(this FileStream stream)
        {
            if (stream.RemainSize() < sizeof(int)) return 0;

            Memory<byte> buffer = new byte[sizeof(int)];
            await stream.ReadAsync(buffer);

            return BitConverter.ToInt32(buffer.Span);
        }

        public static async ValueTask<double> ReadDoubleAsync(this FileStream stream)
        {
            if (stream.RemainSize() < sizeof(double)) return 0;

            Memory<byte> buffer = new byte[sizeof(double)];
            await stream.ReadAsync(buffer);

            return BitConverter.ToDouble(buffer.Span);
        }
    }
}
