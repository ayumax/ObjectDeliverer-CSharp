using System;
using System.Collections.Generic;

namespace ObjectDeliverer.PacketRule
{
    public class PacketRuleTerminate : PacketRuleBase
    {
        private byte[] Terminate = new byte[0];
        private GrowBuffer BufferForSend = new GrowBuffer();
        private GrowBuffer ReceiveTempBuffer = new GrowBuffer();
        private byte[] BufferForReceive = new byte[0];

        public override void Initialize()
        {
            BufferForSend = new GrowBuffer();
            ReceiveTempBuffer = new GrowBuffer();
            BufferForReceive = new byte[1024];
        }

        public override void MakeSendPacket(byte[] bodyBuffer)
        {
            var SendSize = bodyBuffer.Length + Terminate.Length;
            BufferForSend.Reset(SendSize);

            BufferForSend.CopyFromArray(bodyBuffer, 0, bodyBuffer.Length, 0);
            BufferForSend.CopyFromArray(Terminate, bodyBuffer.Length, Terminate.Length, 0);


            Buffer.BlockCopy(bodyBuffer, 0, BufferForSend, 0, bodyBuffer.Length);
            Buffer.BlockCopy(Terminate, 0, BufferForSend, bodyBuffer.Length, Terminate.Length);

            DispatchMadeSendBuffer(BufferForSend.Buffer);
        }

        public override void NotifyReceiveData(byte[] dataBuffer)
        {
            ReceiveTempBuffer += DataBuffer;

            int32 findIndex = -1;

            while (true)
            {
                for (int i = 0; i <= ReceiveTempBuffer.Num() - Terminate.Num(); ++i)
                {
                    bool notEqual = false;
                    for (int j = 0; j <= Terminate.Num() - Terminate.Num(); ++j)
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

                BufferForReceive.SetNum(findIndex, false);
                FMemory::Memcpy(BufferForReceive.GetData(), ReceiveTempBuffer.GetData(), findIndex);
                DispatchMadeReceiveBuffer(BufferForReceive);

                ReceiveTempBuffer.RemoveAt(0, findIndex + Terminate.Num());

                findIndex = -1;
            }
        }

        public override int WantSize => 0;

        public override PacketRuleBase Clone() => PacketRuleFactory.CreatePacketRuleTerminate(Terminate);
    }
}