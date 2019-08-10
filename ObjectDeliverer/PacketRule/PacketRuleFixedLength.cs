using System;
using System.Collections.Generic;

namespace ObjectDeliverer.PacketRule
{
    public class PacketRuleFixedLength : PacketRuleBase
    {
        public int FixedSize { get; set; } = 128;

        private byte[] BufferForSend = new byte[0];

        public override void Initialize()
        {
            BufferForSend = new byte[FixedSize];
        }

        public override void MakeSendPacket(byte[] bodyBuffer)
        {
            Array.Clear(BufferForSend, 0, BufferForSend.Length);
            Buffer.BlockCopy(bodyBuffer, 0, BufferForSend, 0, Math.Min(bodyBuffer.Length, FixedSize));

            DispatchMadeSendBuffer(BufferForSend);
        }


        public override void NotifyReceiveData(byte[] dataBuffer)
        {
            DispatchMadeReceiveBuffer(dataBuffer);
        }

        public override int WantSize => FixedSize;

        public override PacketRuleBase Clone()
        {
            return PacketRuleFactory.CreatePacketRuleFixedLength(FixedSize);
        }

    }
}