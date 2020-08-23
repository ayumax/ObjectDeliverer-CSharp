// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
            return stream.Length - stream.Position;
        }

        public static async ValueTask<int> ReadIntAsync(this FileStream stream)
        {
            if (stream.RemainSize() < sizeof(int)) return 0;

#if SPAN_IS_IMPLEMENTED
            Memory<byte> buffer = new byte[sizeof(int)];
            await stream.ReadAsync(buffer);

            return BitConverter.ToInt32(buffer.Span);
#else
            byte[] buffer = new byte[sizeof(int)];
            await stream.ReadAsync(buffer);
            return BitConverter.ToInt32(buffer, 0);
#endif
        }

        public static async ValueTask<double> ReadDoubleAsync(this FileStream stream)
        {
            if (stream.RemainSize() < sizeof(double)) return 0;

#if SPAN_IS_IMPLEMENTED
            Memory<byte> buffer = new byte[sizeof(double)];
            await stream.ReadAsync(buffer);

            return BitConverter.ToDouble(buffer.Span);
#else
            byte[] buffer = new byte[sizeof(double)];
            await stream.ReadAsync(buffer);
            return BitConverter.ToDouble(buffer, 0);
#endif
        }

        public static async ValueTask WriteIntAsync(this FileStream stream, int intValue)
        {
            Memory<byte> buffer = BitConverter.GetBytes(intValue);
            await stream.WriteAsync(buffer);
        }

        public static async ValueTask WriteDoubleAsync(this FileStream stream, double doubleValue)
        {
            Memory<byte> buffer = BitConverter.GetBytes(doubleValue);
            await stream.WriteAsync(buffer);
        }
    }
}
