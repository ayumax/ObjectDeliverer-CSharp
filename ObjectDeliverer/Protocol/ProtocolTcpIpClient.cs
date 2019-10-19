using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ObjectDeliverer.Protocol.IP;
using ObjectDeliverer.Utils;

namespace ObjectDeliverer.Protocol
{
    public class ProtocolTcpIpClient : ProtocolIPSocket
    {
        public string IpAddress { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 0;
        public bool AutoConnectAfterDisconnect { get; set; } = false;

        void Initialize(string ipAddress, int port, bool autoConnectAfterDisconnect = false)
        {
            this.IpAddress = ipAddress;
            this.Port = port;
            this.AutoConnectAfterDisconnect = autoConnectAfterDisconnect;
        }

        public override async ValueTask StartAsync()
        {
            await base.StartAsync();

            do
            {
                ipClient = new TCPClient();

                await ipClient.ConnectAsync(IpAddress, Port);

                DispatchConnected(this);

                _ = StartReceiveAsync(ipClient);
            }
            while (AutoConnectAfterDisconnect == true && IsSelfClose == false);

        }
    }
}