using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDeliverer.PacketRule
{
    class GrowBuffer
    {
        private byte[] InnerBuffer;
        private int BufferLength = 0;

        public Span<byte> Buffer => new Span<byte>(InnerBuffer, 0, BufferLength);

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

            BufferLength = newSize;

            return isGrow;
        }

        public void Add(byte[] addBuffer)
        {
            var oldBuffer = InnerBuffer;

            if (Reset(BufferLength + addBuffer.Length))
            {
                System.Buffer.BlockCopy(oldBuffer, 0, InnerBuffer, 0, oldBuffer.Length);
            }

            System.Buffer.BlockCopy(addBuffer, 0, InnerBuffer, BufferLength, addBuffer.Length);
        }

        public void CopyFromArray(byte[] fromBuffer, int fromBufferOffset, int fromBufferSize, int myOffset)
        {
            System.Buffer.BlockCopy(fromBuffer, fromBufferOffset, InnerBuffer, myOffset, fromBufferSize);
        }
    }
}
