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
            BufferForSend.SetBufferSize(FixedSize);
        }

        public override ReadOnlyMemory<byte> MakeSendPacket(ReadOnlyMemory<byte> bodyBuffer)
        {
            BufferForSend.Clear();

            ReadOnlySpan<byte> sendPacketSpan = bodyBuffer.Slice(0, Math.Min(bodyBuffer.Length, FixedSize)).Span;
            BufferForSend.CopyFrom(sendPacketSpan);

            return BufferForSend.MemoryBuffer;
        }


        public override IEnumerable<ReadOnlyMemory<byte>> NotifyReceiveData(ReadOnlyMemory<byte> dataBuffer)
        {
            yield return dataBuffer;
        }

        public override int WantSize => FixedSize;

        public override PacketRuleBase Clone()
        {
            return PacketRuleFactory.CreatePacketRuleFixedLength(FixedSize);
        }

    }
}