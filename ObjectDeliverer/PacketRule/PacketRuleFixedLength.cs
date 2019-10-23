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

        public override void Initialize()
        {
            BufferForSend.Reset(0);
        }

        public override Memory<byte> MakeSendPacket(Memory<byte> bodyBuffer)
        {
            BufferForSend.Clear();
            BufferForSend.CopyFrom(bodyBuffer.Span.Slice(0, Math.Min(bodyBuffer.Length, FixedSize)));

            return BufferForSend.MemoryBuffer;
        }


        public override IEnumerable<Memory<byte>> NotifyReceiveData(Memory<byte> dataBuffer)
        {
            yield return dataBuffer.ToArray();
        }

        public override int WantSize => FixedSize;

        public override PacketRuleBase Clone()
        {
            return PacketRuleFactory.CreatePacketRuleFixedLength(FixedSize);
        }

    }
}