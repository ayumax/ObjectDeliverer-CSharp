using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ObjectDeliverer.Protocol.IP;
using ObjectDeliverer.Utils;

namespace ObjectDeliverer.Protocol
{
    public class ProtocolUdpSocketReceiver : ProtocolIPSocket
    {
        public int BoundPort { get; set; }
        
        public void Initialize(int boundPort)
        {
            BoundPort = boundPort;
        }

        public override async ValueTask StartAsync()
        {
            await base.StartAsync();

            ipClient = new UDPClient(BoundPort);

            _ = StartReceiveAsync(ipClient);
        }

    }
}

