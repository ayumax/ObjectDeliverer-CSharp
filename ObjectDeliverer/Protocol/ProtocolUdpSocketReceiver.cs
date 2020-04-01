// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ObjectDeliverer.Protocol.IP;
using ObjectDeliverer.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ObjectDeliverer.Protocol
{
    public class ProtocolUdpSocketReceiver : ProtocolIPSocket
    {
        public int BoundPort { get; set; }

        public override async ValueTask StartAsync()
        {
            await base.StartAsync();

            this.IpClient = new UDPProtocolHelper(this.BoundPort, this.ReceiveBufferSize);

            this.StartPollingForReceive(this.IpClient);

            this.DispatchConnected(this);
        }
    }
}
