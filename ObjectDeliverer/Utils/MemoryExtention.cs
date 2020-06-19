// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace ObjectDeliverer.Utils
{
    public static class MemoryExtention
    {
        public static unsafe void Copy(Span<byte> toBuffer, ReadOnlySpan<byte> fromBuffer)
        {
            if (toBuffer.Length != fromBuffer.Length) return;

            fixed (byte* to = toBuffer, from = fromBuffer)
            {
                var pTo = to;
                var pFrom = from;

                var last = pTo + toBuffer.Length;
                while (pTo + 7 < last)
                {
                    *(ulong*)pTo = *(ulong*)pFrom;
                    pTo += 8;
                    pFrom += 8;
                }

                if (pTo + 3 < last)
                {
                    *(uint*)pTo = *(uint*)pFrom;
                    pTo += 4;
                    pFrom += 4;
                }

                while (pTo < last)
                {
                    *pTo = *pFrom;
                    ++pTo;
                    ++pFrom;
                }
            }
        }

        public static unsafe void Clear(Span<byte> targetBuffer)
        {
            fixed (byte* pin = targetBuffer)
            {
                var p = pin;
                var last = p + targetBuffer.Length;
                while (p + 7 < last)
                {
                    *(ulong*)p = 0;
                    p += 8;
                }

                if (p + 3 < last)
                {
                    *(uint*)p = 0;
                    p += 4;
                }

                while (p < last)
                {
                    *p = 0;
                    ++p;
                }
            }
        }
    }
}
