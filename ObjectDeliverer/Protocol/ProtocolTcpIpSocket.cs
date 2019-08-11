using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ObjectDeliverer.Utils;

namespace ObjectDeliverer.Protocol
{
    public class ProtocolTcpIpSocket : ObjectDelivererProtocol
    {
        protected TcpClient? tcpClient;
        protected bool IsSelfClose = false;

        protected object lockObj = new object();

        protected GrowBuffer ReceiveBuffer { get; private set; } = new GrowBuffer();

        private Task? ReceiveTask;
        private CancellationTokenSource? Canceler;

        public override void Close()
        {
            CloseSocket();
        }


        protected void CloseSocket()
        {
            if (tcpClient == null) return;

            IsSelfClose = true;

            tcpClient.Close();

            lock (lockObj)
            {
                Canceler?.Cancel();
            }

            ReceiveTask?.Wait();

            tcpClient = null;
            ReceiveTask = null;
            Canceler = null;
        }

        public override void Send(Span<byte> dataBuffer)
        {
            if (tcpClient == null) return;

            PacketRule.MakeSendPacket(dataBuffer);
        }

        protected void OnConnected(TcpClient connectionSocket)
        {
            tcpClient = connectionSocket;
            StartPollilng();
        }

        private void StartPollilng()
        {
            Canceler = new CancellationTokenSource();
            
            ReceiveBuffer.Reset(1024);

            ReceiveTask = Task.Run(() =>
            {
                while (true)
                {
                    lock(lockObj)
                    {
                        if (Canceler.IsCancellationRequested == false)
                        {
                            if (ReceivedData() == false)
                            {
                                break;
                            }
                        }  
                    }
                  
                    Thread.Sleep(1);
                }
            }, Canceler.Token);
        }

        private bool ReceivedData()
        {
            if (tcpClient == null)
            {
                DispatchDisconnected(this);
                return false;
            }

      
            while (tcpClient?.Available > 0)
            {
                int wantSize = PacketRule.WantSize;

                if (wantSize > 0)
                {
                    if (tcpClient.Available < wantSize) return true;
                }

                var receiveSize = wantSize == 0 ? tcpClient.Available : wantSize;

                ReceiveBuffer.Reset(receiveSize);

                if (tcpClient == null)
                {
                    NotifyDisconnect();
                    return false;
                }

                if (tcpClient.GetStream() == null)
                {
                    NotifyDisconnect();
                    return false;
                }

                if (tcpClient.GetStream().Read(ReceiveBuffer.SpanBuffer) <= 0)
                {
                    NotifyDisconnect();
                    return false;
                }

                PacketRule.NotifyReceiveData(ReceiveBuffer.SpanBuffer);
            }

            return true;
        }

        public override void RequestSend(Span<byte> dataBuffer)
        {
            tcpClient?.GetStream()?.Write(dataBuffer);
        }

        private void NotifyDisconnect()
        {
            if (!IsSelfClose)
            {
                tcpClient?.Close();
                DispatchDisconnected(this);
            }
        }
    }
}