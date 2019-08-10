using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDeliverer.PacketRule
{
    class GrowBuffer
    {
        private byte[] InnerBuffer;

        public int Length { get; private set; } = 0;

        public Span<byte> SpanBuffer => new Span<byte>(InnerBuffer, 0, Length);

        public ref byte this[int index] => ref SpanBuffer[index];

        public Span<byte> AsSpan(int position, int length) => InnerBuffer.AsSpan(position, length);

        public GrowBuffer(int initialSize = 1024)
        {
            InnerBuffer = new byte[initialSize];
        }

        public bool Reset(int newSize = 0)
        {
            bool isGrow = false;

            if (InnerBuffer.Length < newSize)
            {
                InnerBuffer = new byte[1024 * ((newSize / 1024) + 1)];
                isGrow = true;
            }

            Length = newSize;

            return isGrow;
        }

        public void Add(Span<byte> addBuffer)
        {
            var oldBuffer = InnerBuffer;

            if (Reset(Length + addBuffer.Length))
            {
                Copy(InnerBuffer.AsSpan(0, oldBuffer.Length), oldBuffer);
            }

            Copy(InnerBuffer.AsSpan(Length, addBuffer.Length), addBuffer);
        }

        public void CopyFrom(Span<byte> fromBuffer, int myOffset = 0)
        {
            var spanBuffer = InnerBuffer.AsSpan(myOffset, Length - myOffset);

            Copy(spanBuffer, fromBuffer);
        }

        private static void Copy(Span<byte> toBuffer, Span<byte> fromBuffer)
        {
            unsafe
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
        }

        public void Clear()
        {
            unsafe
            {
                var spanBuffer = SpanBuffer;

                fixed (byte* pin = spanBuffer)
                {
                    var p = pin;
                    var last = p + spanBuffer.Length;
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

        public void RemoveAt(int start, int length)
        {
            var moveLength = Length - length;
            var tempBuffer = new byte[moveLength];
            var moveSpan = InnerBuffer.AsSpan(start + length, moveLength);
            Copy(tempBuffer, moveSpan);
            Copy(InnerBuffer.AsSpan(start, moveLength), tempBuffer);
        }
    }
}
