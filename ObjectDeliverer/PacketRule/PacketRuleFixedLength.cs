using System;
using System.Collections.Generic;

namespace ObjectDeliverer.PacketRule
{
    public class PacketRuleFixedLength : PacketRuleBase
    {
        public int FixedSize { get; set; } = 128;

        private GrowBuffer BufferForSend = new GrowBuffer();

        public override void Initialize()
        {
            BufferForSend = new GrowBuffer();
        }

        public override void MakeSendPacket(Span<byte> bodyBuffer)
        {
            BufferForSend.Clear();
            BufferForSend.CopyFrom(bodyBuffer.Slice(0, Math.Min(bodyBuffer.Length, FixedSize)));

            DispatchMadeSendBuffer(BufferForSend.SpanBuffer);
        }


        public override void NotifyReceiveData(Span<byte> dataBuffer)
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