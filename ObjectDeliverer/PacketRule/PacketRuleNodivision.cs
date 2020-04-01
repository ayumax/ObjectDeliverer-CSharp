// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ObjectDeliverer.PacketRule
{
    public class PacketRuleNodivision : PacketRuleBase
    {
        public override int WantSize => 0;

        public override void Initialize()
        {
        }

        public override ReadOnlyMemory<byte> MakeSendPacket(ReadOnlyMemory<byte> bodyBuffer)
        {
            return bodyBuffer;
        }

        public override IEnumerable<ReadOnlyMemory<byte>> MakeReceivedPacket(ReadOnlyMemory<byte> dataBuffer)
        {
            yield return dataBuffer;
        }

        public override PacketRuleBase Clone() => new PacketRuleNodivision();
    }
}
