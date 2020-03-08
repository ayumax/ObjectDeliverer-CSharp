using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ObjectDeliverer.Protocol.IP;
using ObjectDeliverer.Utils;

namespace ObjectDeliverer.Protocol
{
    public class ProtocolUdpSocketSender : ProtocolIPSocket
    {
        public string DestinationIpAddress { get; set; } = "127.0.0.1";
        public int DestinationPort { get; set; } = 0;

        public void Initialize(string ipAddress, int port)
        {
            DestinationIpAddress = ipAddress;
            DestinationPort = port;
        }

        public override ValueTask StartAsync()
        {
            ipClient = new UDPClientProtocol();

            ipClient.ConnectAsync(DestinationIpAddress, DestinationPort);

            DispatchConnected(this);

            return new ValueTask();
        }
    }
}
