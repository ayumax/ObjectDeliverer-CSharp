using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDeliverer.Utils
{
    public class GrowBuffer
    {
        private byte[] InnerBuffer;

        public int Length { get; private set; } = 0;
        public Memory<byte> MemoryBuffer => InnerBuffer.AsMemory(0, Length);

        public ref byte this[int index] => ref InnerBuffer[index];

        public Span<byte> AsSpan(int position, int length) => InnerBuffer.AsSpan(position, length);

        public GrowBuffer(int initialSize = 1024)
        {
            InnerBuffer = new byte[initialSize];
        }

        public bool SetBufferSize(int newSize = 0)
        {
            bool isGrow = false;

            if (InnerBuffer.Length < newSize)
            {
                var oldBuffer = InnerBuffer;
                InnerBuffer = new byte[1024 * ((newSize / 1024) + 1)];
                Memory.Copy(AsSpan(0, oldBuffer.Length), oldBuffer);

                isGrow = true;
            }

            Length = newSize;

            return isGrow;
        }

        public void Add(ReadOnlySpan<byte> addBuffer)
        {
            SetBufferSize(Length + addBuffer.Length);

            Memory.Copy(AsSpan(Length, addBuffer.Length), addBuffer);
        }

        public void CopyFrom(ReadOnlySpan<byte> fromBuffer, int myOffset = 0)
        {
            var spanBuffer = AsSpan(myOffset, Length - myOffset);

            Memory.Copy(spanBuffer, fromBuffer);
        }

        
        public void RemoveRangeFromStart(int start, int length)
        {
            var moveLength = Length - length;
            var tempBuffer = new byte[moveLength];
            var moveSpan = InnerBuffer.AsSpan(start + length, moveLength);
            Memory.Copy(tempBuffer, moveSpan);
            Memory.Copy(InnerBuffer.AsSpan(start, moveLength), tempBuffer);

            Length = moveLength;
        }

        public void Clear()
        {
            Memory.Clear(InnerBuffer);
        }
    }
}
