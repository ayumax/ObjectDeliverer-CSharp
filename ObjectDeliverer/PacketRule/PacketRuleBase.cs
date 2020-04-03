// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ObjectDeliverer.Protocol;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ObjectDeliverer.PacketRule
{
    public abstract class PacketRuleBase
    {
        public abstract int WantSize { get; }

        public abstract PacketRuleBase Clone();

        public abstract void Initialize();

        public abstract ReadOnlyMemory<byte> MakeSendPacket(ReadOnlyMemory<byte> bodyBuffer);

        public abstract IEnumerable<ReadOnlyMemory<byte>> MakeReceivedPacket(ReadOnlyMemory<byte> dataBuffer);
    }
}
