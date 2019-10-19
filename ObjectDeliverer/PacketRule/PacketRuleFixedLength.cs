using System;
using System.Collections.Generic;
using ObjectDeliverer.Utils;
using System.Threading.Tasks;

namespace ObjectDeliverer.PacketRule
{
    public class PacketRuleFixedLength : PacketRuleBase
    {
        public int FixedSize { get; set; } = 128;

        private GrowBuffer BufferForSend = new GrowBuffer();

        public override void OnInitialize()
        {
            base.OnInitialize();
            BufferForSend.Reset(0);
        }

        public override async ValueTask MakeSendPacket(Memory<byte> bodyBuffer)
        {
            BufferForSend.Clear();
            BufferForSend.CopyFrom(bodyBuffer.Span.Slice(0, Math.Min(bodyBuffer.Length, FixedSize)));

            await DispatchMadeSendBuffer(BufferForSend.MemoryBuffer);
        }


        public override void NotifyReceiveData(Memory<byte> dataBuffer)
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