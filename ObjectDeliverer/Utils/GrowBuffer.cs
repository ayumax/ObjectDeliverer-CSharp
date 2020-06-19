// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDeliverer.Utils
{
    public class GrowBuffer
    {
        private readonly int packetSize = 1024;
        private byte[] innerBuffer = new byte[0];

        public GrowBuffer(int initialSize = 1024, int packetSize = 1024)
        {
            this.packetSize = packetSize;
            this.SetBufferSize(initialSize);
        }

        public int Length { get; private set; } = 0;

        public Memory<byte> MemoryBuffer => this.innerBuffer.AsMemory(0, this.Length);

        public int InnerBufferSize => this.innerBuffer.Length;

        public ref byte this[int index] => ref this.innerBuffer[index];

        public Span<byte> AsSpan(int position, int length) => this.innerBuffer.AsSpan(position, length);

        public ReadOnlyMemory<byte> AsReadOnlyMemory(int position, int length) => this.innerBuffer.AsMemory(position, length);

        public bool SetBufferSize(int newSize = 0)
        {
            bool isGrow = false;

            if (this.innerBuffer.Length < newSize)
            {
                var oldBuffer = this.innerBuffer;
                this.innerBuffer = new byte[this.packetSize * ((newSize / this.packetSize) + 1)];
                MemoryExtention.Copy(this.AsSpan(0, oldBuffer.Length), oldBuffer);

                isGrow = true;
            }

            this.Length = newSize;

            return isGrow;
        }

        public void Add(ReadOnlySpan<byte> addBuffer)
        {
            this.SetBufferSize(this.Length + addBuffer.Length);

            MemoryExtention.Copy(this.AsSpan(this.Length - addBuffer.Length, addBuffer.Length), addBuffer);
        }

        public void CopyFrom(ReadOnlySpan<byte> fromBuffer, int myOffset = 0)
        {
            var spanBuffer = this.AsSpan(myOffset, Math.Min(fromBuffer.Length, this.Length - myOffset));

            MemoryExtention.Copy(spanBuffer, fromBuffer);
        }

        public void RemoveRangeFromStart(int start, int length)
        {
            var moveLength = this.Length - length;
            var tempBuffer = new byte[moveLength];
            var moveSpan = this.innerBuffer.AsSpan(start + length, moveLength);
            MemoryExtention.Copy(tempBuffer, moveSpan);
            MemoryExtention.Copy(this.innerBuffer.AsSpan(start, moveLength), tempBuffer);

            this.Length = moveLength;
        }

        public void Clear()
        {
            MemoryExtention.Clear(this.innerBuffer);
        }
    }
}
