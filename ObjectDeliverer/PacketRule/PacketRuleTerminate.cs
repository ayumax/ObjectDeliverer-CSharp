// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ObjectDeliverer.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ObjectDeliverer.PacketRule
{
    public class PacketRuleTerminate : PacketRuleBase
    {
        private readonly GrowBuffer bufferForSend = new GrowBuffer();
        private readonly GrowBuffer receiveTempBuffer = new GrowBuffer();
        private readonly GrowBuffer bufferForReceive = new GrowBuffer();

        public byte[] Terminate { get; set; } = new byte[0];

        public override int WantSize => 0;

        public override void Initialize()
        {
            this.bufferForSend.SetBufferSize(0);
            this.receiveTempBuffer.SetBufferSize(0);
            this.bufferForReceive.SetBufferSize(0);
        }

        public override ReadOnlyMemory<byte> MakeSendPacket(ReadOnlyMemory<byte> bodyBuffer)
        {
            var sendSize = bodyBuffer.Length + this.Terminate.Length;
            this.bufferForSend.SetBufferSize(sendSize);

            this.bufferForSend.CopyFrom(bodyBuffer.Span, 0);
            this.bufferForSend.CopyFrom(this.Terminate, bodyBuffer.Length);

            return this.bufferForSend.MemoryBuffer;
        }

        public override IEnumerable<ReadOnlyMemory<byte>> MakeReceivedPacket(ReadOnlyMemory<byte> dataBuffer)
        {
            if (this.WantSize > 0 && dataBuffer.Length != this.WantSize) yield break;

            this.receiveTempBuffer.Add(dataBuffer.Span);

            int findIndex = -1;

            while (true)
            {
                for (int i = 0; i <= this.receiveTempBuffer.Length - this.Terminate.Length; ++i)
                {
                    bool notEqual = false;
                    for (int j = 0; j < this.Terminate.Length; ++j)
                    {
                        if (this.receiveTempBuffer[i + j] != this.Terminate[j])
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

                this.bufferForReceive.SetBufferSize(findIndex);
                this.bufferForReceive.CopyFrom(this.receiveTempBuffer.AsSpan(0, findIndex));

                yield return this.bufferForReceive.MemoryBuffer;

                this.receiveTempBuffer.RemoveRangeFromStart(0, findIndex + this.Terminate.Length);

                findIndex = -1;
            }
        }

        public override PacketRuleBase Clone() => new PacketRuleTerminate()
        {
            Terminate = this.Terminate,
        };
    }
}
