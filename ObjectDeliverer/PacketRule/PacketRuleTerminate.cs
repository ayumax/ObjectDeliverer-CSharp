using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ObjectDeliverer.Utils;


namespace ObjectDeliverer.PacketRule
{
    public class PacketRuleTerminate : PacketRuleBase
    {
        public byte[] Terminate { get; set; } = new byte[0];
        private GrowBuffer BufferForSend = new GrowBuffer();
        private GrowBuffer ReceiveTempBuffer = new GrowBuffer();
        private GrowBuffer BufferForReceive = new GrowBuffer();

        public override void Initialize()
        {
            BufferForSend.SetBufferSize(0);
            ReceiveTempBuffer.SetBufferSize(0);
            BufferForReceive.SetBufferSize(0);
        }

        public override ReadOnlyMemory<byte> MakeSendPacket(ReadOnlyMemory<byte> bodyBuffer)
        {
            var SendSize = bodyBuffer.Length + Terminate.Length;
            BufferForSend.SetBufferSize(SendSize);

            BufferForSend.CopyFrom(bodyBuffer.Span, 0);
            BufferForSend.CopyFrom(Terminate, bodyBuffer.Length);

            return BufferForSend.MemoryBuffer;
        }

        public override IEnumerable<ReadOnlyMemory<byte>> NotifyReceiveData(ReadOnlyMemory<byte> dataBuffer)
        {
            ReceiveTempBuffer.Add(dataBuffer.Span);

            int findIndex = -1;

            while (true)
            {
                for (int i = 0; i <= ReceiveTempBuffer.Length - Terminate.Length; ++i)
                {
                    bool notEqual = false;
                    for (int j = 0; j <= Terminate.Length; ++j)
                    {
                        if (ReceiveTempBuffer[i + j] != Terminate[j])
                        {
                            notEqual = true;
                            break;
                        }
                    }

                    if (notEqual == false)
                    {
                        findIndex = i;
                        break;
                    }
                }

                if (findIndex == -1)
                {
                    yield break;
                }

                BufferForReceive.SetBufferSize(findIndex);
                BufferForReceive.CopyFrom(ReceiveTempBuffer.AsSpan(0, findIndex));

                yield return BufferForReceive.MemoryBuffer;

                ReceiveTempBuffer.RemoveRangeFromStart(0, findIndex + Terminate.Length);

                findIndex = -1;
            }
        }

        public override int WantSize => 0;

        public override PacketRuleBase Clone() => PacketRuleFactory.CreatePacketRuleTerminate(Terminate);
    }
}