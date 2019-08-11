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
        public bool Retry { get; set; } = false;
        public bool AutoConnectAfterDisconnect { get; set; } = false;

        void Initialize(string ipAddress, int port, bool retry = false, bool autoConnectAfterDisconnect = false)
        {
            this.IpAddress = ipAddress;
            this.Port = port;
            this.Retry = retry;
            this.AutoConnectAfterDisconnect = autoConnectAfterDisconnect;
        }

        public override void Start()
        {
            CloseSocket();

            tcpClient = new TcpClient(IpAddress, Port);

            if (tcpClient == null) return;

            tcpClient.ConnectAsync
            ConnectInnerThread = new FWorkerThread([this] { return TryConnect(); }, 1.0f);
            ConnectThread = FRunnableThread::Create(ConnectInnerThread, TEXT("ObjectDeliverer UProtocolTcpIpClient ConnectThread"));
        }

        bool TryConnect()
        {
            if (InnerSocket->Connect(ConnectEndPoint.ToInternetAddr().Get()))
            {
                DispatchConnected(this);

                OnConnected(InnerSocket);
            }
            else if (RetryConnect)
            {
                return true;
            }

            return false;
        }

        public override void Close()
        {
            base.Close();

            if (!ConnectThread) return;
            ConnectThread->Kill(true);

            delete ConnectThread;
            ConnectThread = nullptr;

            if (!ConnectInnerThread) return;
            delete ConnectInnerThread;
            ConnectInnerThread = nullptr;
        }


        protected override void DispatchDisconnected(ObjectDelivererProtocol delivererProtocol)
        {
            base.DispatchDisconnected(delivererProtocol);

            if (AutoConnectAfterDisconnect)
            {
                Start();
            }
        }
    }
}