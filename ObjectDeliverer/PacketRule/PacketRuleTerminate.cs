using System;
using System.Collections.Generic;
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
            BufferForSend.Reset(0);
            ReceiveTempBuffer.Reset(0);
            BufferForReceive.Reset(0);
        }

        public override void MakeSendPacket(Memory<byte> bodyBuffer)
        {
            var SendSize = bodyBuffer.Length + Terminate.Length;
            BufferForSend.Reset(SendSize);

            BufferForSend.CopyFrom(bodyBuffer, 0);
            BufferForSend.CopyFrom(Terminate, bodyBuffer.Length);

            DispatchMadeSendBuffer(BufferForSend.SpanBuffer);
        }

        public override void NotifyReceiveData(Memory<byte> dataBuffer)
        {
            ReceiveTempBuffer.Add(dataBuffer);

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
                    return;
                }

                BufferForReceive.Reset(findIndex);
                BufferForReceive.CopyFrom(ReceiveTempBuffer.AsSpan(0, findIndex));
                DispatchMadeReceiveBuffer(BufferForReceive.SpanBuffer);

                ReceiveTempBuffer.RemoveAt(0, findIndex + Terminate.Length);

                findIndex = -1;
            }
        }

        public override int WantSize => 0;

        public override PacketRuleBase Clone() => PacketRuleFactory.CreatePacketRuleTerminate(Terminate);
    }
}