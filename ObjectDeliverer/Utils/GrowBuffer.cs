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

        public readonly int PacketSize = 1024;

        public GrowBuffer(int initialSize = 1024, int packetSize = 1024)
        {
            InnerBuffer = new byte[initialSize];
            this.PacketSize = packetSize;
        }

        public bool SetBufferSize(int newSize = 0)
        {
            bool isGrow = false;

            if (InnerBuffer.Length < newSize)
            {
                var oldBuffer = InnerBuffer;
                InnerBuffer = new byte[PacketSize * ((newSize / PacketSize) + 1)];
                MemoryExtention.Copy(AsSpan(0, oldBuffer.Length), oldBuffer);

                isGrow = true;
            }

            Length = newSize;

            return isGrow;
        }

        public void Add(ReadOnlySpan<byte> addBuffer)
        {
            SetBufferSize(Length + addBuffer.Length);

            MemoryExtention.Copy(AsSpan(Length, addBuffer.Length), addBuffer);
        }

        public void CopyFrom(ReadOnlySpan<byte> fromBuffer, int myOffset = 0)
        {
            var spanBuffer = AsSpan(myOffset, Length - myOffset);

            MemoryExtention.Copy(spanBuffer, fromBuffer);
        }

        
        public void RemoveRangeFromStart(int start, int length)
        {
            var moveLength = Length - length;
            var tempBuffer = new byte[moveLength];
            var moveSpan = InnerBuffer.AsSpan(start + length, moveLength);
            MemoryExtention.Copy(tempBuffer, moveSpan);
            MemoryExtention.Copy(InnerBuffer.AsSpan(start, moveLength), tempBuffer);

            Length = moveLength;
        }

        public void Clear()
        {
            MemoryExtention.Clear(InnerBuffer);
        }
    }
}
