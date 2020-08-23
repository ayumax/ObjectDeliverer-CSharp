// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ObjectDeliverer.Utils;
using System;
using System.Collections.Generic;

namespace ObjectDeliverer
{
    public class PacketRuleFixedLength : IPacketRule
    {
        private readonly GrowBuffer bufferForSend = new GrowBuffer();

        public int FixedSize { get; set; } = 128;

        public int WantSize => this.FixedSize;

        public void Initialize()
        {
            this.bufferForSend.SetBufferSize(this.FixedSize);
        }

        public ReadOnlyMemory<byte> MakeSendPacket(ReadOnlyMemory<byte> bodyBuffer)
        {
            this.bufferForSend.Clear();

            ReadOnlySpan<byte> sendPacketSpan = bodyBuffer.Slice(0, Math.Min(bodyBuffer.Length, this.FixedSize)).Span;
            this.bufferForSend.CopyFrom(sendPacketSpan);

            return this.bufferForSend.MemoryBuffer;
        }

        public IEnumerable<ReadOnlyMemory<byte>> MakeReceivedPacket(ReadOnlyMemory<byte> dataBuffer)
        {
            if (this.WantSize > 0 && dataBuffer.Length != this.WantSize) yield break;
            yield return dataBuffer;
        }

        public IPacketRule Clone() => new PacketRuleFixedLength()
        {
            FixedSize = this.FixedSize,
        };
    }
}
