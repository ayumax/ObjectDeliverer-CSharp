using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDeliverer.Utils
{
    public class GrowBuffer
    {
        private byte[] InnerBuffer;

        public int Length { get; private set; } = 0;

        public Span<byte> SpanBuffer => InnerBuffer.AsSpan(0, Length);
        public Memory<byte> MemoryBuffer => InnerBuffer;

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
                Memory.Copy(InnerBuffer.AsSpan(0, oldBuffer.Length), oldBuffer);
            }

            Memory.Copy(InnerBuffer.AsSpan(Length, addBuffer.Length), addBuffer);
        }

        public void CopyFrom(Span<byte> fromBuffer, int myOffset = 0)
        {
            var spanBuffer = InnerBuffer.AsSpan(myOffset, Length - myOffset);

            Memory.Copy(spanBuffer, fromBuffer);
        }

        

        public void RemoveAt(int start, int length)
        {
            var moveLength = Length - length;
            var tempBuffer = new byte[moveLength];
            var moveSpan = InnerBuffer.AsSpan(start + length, moveLength);
            Memory.Copy(tempBuffer, moveSpan);
            Memory.Copy(InnerBuffer.AsSpan(start, moveLength), tempBuffer);
        }

        public void Clear()
        {
            Memory.Clear(InnerBuffer);
        }
    }
}
