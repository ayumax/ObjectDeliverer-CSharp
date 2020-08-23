// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ObjectDeliverer.Utils;
using System;
using System.Collections.Generic;

namespace ObjectDeliverer
{
    public enum ECNBufferEndian
    {
        /** Big Endian */
        Big = 0,
        /** Little Endian */
        Little,
    }

    public class PacketRuleSizeBody : IPacketRule
    {
        private readonly GrowBuffer bufferForSend = new GrowBuffer();
        private EReceiveMode receiveMode = EReceiveMode.Size;
        private uint bodySize = 0;

        private enum EReceiveMode
        {
            Size,
            Body,
        }

        public int SizeLength { get; set; } = 4;

        public ECNBufferEndian SizeBufferEndian { get; set; } = ECNBufferEndian.Big;

        public int WantSize
        {
            get
            {
                if (this.receiveMode == EReceiveMode.Size)
                {
                    return this.SizeLength;
                }

                return (int)this.bodySize;
            }
        }

        public void Initialize()
        {
            this.bufferForSend.SetBufferSize(1024);
            this.receiveMode = EReceiveMode.Size;
            this.bodySize = 0;
        }

        public ReadOnlyMemory<byte> MakeSendPacket(ReadOnlyMemory<byte> bodyBuffer)
        {
            var bodyBufferNum = bodyBuffer.Length;
            var sendSize = bodyBufferNum + this.SizeLength;

            this.bufferForSend.SetBufferSize(sendSize);

            for (int i = 0; i < this.SizeLength; ++i)
            {
                int offset = 0;
                if (this.SizeBufferEndian == ECNBufferEndian.Big)
                {
                    offset = 8 * (this.SizeLength - i - 1);
                }
                else
                {
                    offset = 8 * i;
                }

                this.bufferForSend[i] = (byte)((bodyBufferNum >> offset) & 0xFF);
            }

            this.bufferForSend.CopyFrom(bodyBuffer.Span, this.SizeLength);

            return this.bufferForSend.MemoryBuffer;
        }

        public IEnumerable<ReadOnlyMemory<byte>> MakeReceivedPacket(ReadOnlyMemory<byte> dataBuffer)
        {
            if (this.WantSize > 0 && dataBuffer.Length != this.WantSize) yield break;

            if (this.receiveMode == EReceiveMode.Size)
            {
                this.OnReceivedSize(dataBuffer);
                yield break;
            }

            this.OnReceivedBody(dataBuffer);

            yield return dataBuffer;
        }

        public void OnReceivedSize(ReadOnlyMemory<byte> dataBuffer)
        {
            this.bodySize = 0;
            for (int i = 0; i < this.SizeLength; ++i)
            {
                int offset = 0;
                if (this.SizeBufferEndian == ECNBufferEndian.Big)
                {
                    offset = 8 * (this.SizeLength - i - 1);
                }
                else
                {
                    offset = 8 * i;
                }

                this.bodySize |= (uint)(dataBuffer.Span[i] << offset);
            }

            this.receiveMode = EReceiveMode.Body;
        }

        public void OnReceivedBody(ReadOnlyMemory<byte> dataBuffer)
        {
            this.bodySize = 0;

            this.receiveMode = EReceiveMode.Size;
        }

        public IPacketRule Clone() => new PacketRuleSizeBody()
        {
            SizeLength = this.SizeLength,
            SizeBufferEndian = this.SizeBufferEndian,
        };
    }
}
