// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using ObjectDeliverer.Protocol;
using ObjectDeliverer.Protocol.IP;
using System.Threading.Tasks;

namespace ObjectDeliverer
{
    public class ProtocolUdpSocketSender : ProtocolIPSocket
    {
        public string DestinationIpAddress { get; set; } = "127.0.0.1";

        public int DestinationPort { get; set; } = 0;

        public override async ValueTask StartAsync()
        {
            this.IpClient = new UDPProtocolHelper(this.SendBufferSize);

            await this.IpClient.ConnectAsync(this.DestinationIpAddress, this.DestinationPort);

            this.DispatchConnected(this);
        }
    }
}
