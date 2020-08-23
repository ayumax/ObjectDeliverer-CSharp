// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace ObjectDeliverer
{
    public class PacketRuleNodivision : IPacketRule
    {
        public int WantSize => 0;

        public void Initialize()
        {
        }

        public ReadOnlyMemory<byte> MakeSendPacket(ReadOnlyMemory<byte> bodyBuffer)
        {
            return bodyBuffer;
        }

        public IEnumerable<ReadOnlyMemory<byte>> MakeReceivedPacket(ReadOnlyMemory<byte> dataBuffer)
        {
            yield return dataBuffer;
        }

        public IPacketRule Clone() => new PacketRuleNodivision();
    }
}
