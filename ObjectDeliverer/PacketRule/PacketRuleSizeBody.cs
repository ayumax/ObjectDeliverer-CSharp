using System;
using System.Collections.Generic;

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
        private int ReceiveBufferPosition = 9;

        private byte[] BufferForSend = new byte[0];

        public int SizeLength { get; set; } = 4;
        public ECNBufferEndian SizeBufferEndian { get; set; } = ECNBufferEndian.Big;

        public override void Initialize()
        {
            BufferForSend = new byte[1024];
            ReceiveMode = EReceiveMode.Size;
            BodySize = 0;
        }

        public override void MakeSendPacket(byte[] bodyBuffer)
        {
            var BodyBufferNum = bodyBuffer.Length;
            var SendSize = BodyBufferNum + SizeLength;

            if (BufferForSend.Length < SendSize)
            {
                BufferForSend = new byte[1024 * ((SendSize / 1024) + 1)];
            }

            var bufferForSendSpan = new Span<byte>(BufferForSend, 0, SendSize);

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

            Buffer.BlockCopy(bodyBuffer, 0, BufferForSend, SizeLength, BodyBufferNum);

            DispatchMadeSendBuffer(bufferForSendSpan);
        }

        public override void NotifyReceiveData(byte[] dataBuffer)
        {
            if (ReceiveMode == EReceiveMode.Size)
            {
                OnReceivedSize(dataBuffer);
                return;
            }

            OnReceivedBody(dataBuffer);
        }


        public void OnReceivedSize(byte[] dataBuffer)
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
                BodySize |= (uint)(dataBuffer[i] << offset);
            }

            ReceiveMode = EReceiveMode.Body;
        }

        public void OnReceivedBody(byte[] dataBuffer)
        {
            DispatchMadeReceiveBuffer(dataBuffer);

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