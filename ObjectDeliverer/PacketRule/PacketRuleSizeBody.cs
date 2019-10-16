using System;
using System.Collections.Generic;
using ObjectDeliverer.Utils;
using System.Threading.Tasks;

namespace ObjectDeliverer.PacketRule
{
    public class PacketRuleSizeBody : PacketRuleBase
    {
        enum EReceiveMode
        {
            Size,
            Body
        }
        EReceiveMode ReceiveMode = EReceiveMode.Size;

        private uint BodySize = 0;

        private GrowBuffer BufferForSend = new GrowBuffer();

        public int SizeLength { get; set; } = 4;
        public ECNBufferEndian SizeBufferEndian { get; set; } = ECNBufferEndian.Big;

        public override void OnInitialize()
        {
            base.OnInitialize();

            BufferForSend.Reset(1024);
            ReceiveMode = EReceiveMode.Size;
            BodySize = 0;
        }

        public override async ValueTask MakeSendPacket(Memory<byte> bodyBuffer)
        {
            var BodyBufferNum = bodyBuffer.Length;
            var SendSize = BodyBufferNum + SizeLength;

            BufferForSend.Reset(SendSize);

            for (int i = 0; i < SizeLength; ++i)
            {
                int offset = 0;
                if (SizeBufferEndian == ECNBufferEndian.Big)
                {
                    offset = 8 * (SizeLength - i - 1);
                }
                else
                {
                    offset = 8 * i;
                }

                BufferForSend[i] = (byte)((BodyBufferNum >> offset) & 0xFF);
            }

            BufferForSend.CopyFrom(bodyBuffer.Span, SizeLength);

            await DispatchMadeSendBuffer(BufferForSend.MemoryBuffer);
        }

        public override async ValueTask NotifyReceiveData(Memory<byte> dataBuffer)
        {
            if (ReceiveMode == EReceiveMode.Size)
            {
                OnReceivedSize(dataBuffer);
                return;
            }

            await OnReceivedBody(dataBuffer);
        }


        public void OnReceivedSize(Memory<byte> dataBuffer)
        {
            BodySize = 0;
            for (int i = 0; i < SizeLength; ++i)
            {
                int offset = 0;
                if (SizeBufferEndian == ECNBufferEndian.Big)
                {
                    offset = 8 * (SizeLength - i - 1);
                }
                else
                {
                    offset = 8 * i;
                }
                BodySize |= (uint)(dataBuffer.Span[i] << offset);
            }

            ReceiveMode = EReceiveMode.Body;
        }

        public async ValueTask OnReceivedBody(Memory<byte> dataBuffer)
        {
            await DispatchMadeReceiveBuffer(dataBuffer);

            BodySize = 0;

            ReceiveMode = EReceiveMode.Size;
        }

        public override int WantSize
        {
            get
            {
                if (ReceiveMode == EReceiveMode.Size)
                {
                    return SizeLength;
                }

                return (int)BodySize;
            }
        }

        public override PacketRuleBase Clone() => PacketRuleFactory.CreatePacketRuleSizeBody(SizeLength, SizeBufferEndian);
        

    }
}