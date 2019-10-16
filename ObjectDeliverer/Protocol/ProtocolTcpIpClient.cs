using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ObjectDeliverer.Utils;

namespace ObjectDeliverer.Protocol
{
    public class ProtocolTcpIpClient : ProtocolTcpIpSocket
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

        public override async ValueTask Start()
        {
            await CloseSocket();

            do
            {
                tcpClient = new TcpClient();

                await tcpClient.ConnectAsync(IpAddress, Port);

                DispatchConnected(this);

                await StartPollilng(tcpClient);
            }
            while (AutoConnectAfterDisconnect == true && IsSelfClose == false) ;
 
        }


    }
}