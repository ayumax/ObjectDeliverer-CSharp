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
        protected TcpClient tcpClient = new TcpClient();
        protected bool IsSelfClose = false;

        protected object lockObj = new object();

        protected GrowBuffer ReceiveBuffer { get; private set; } = new GrowBuffer();

        private Task? ReceiveTask;
        private CancellationTokenSource? Canceler;

        public override async ValueTask Close()
        {
            await CloseSocket();
        }

        protected async ValueTask CloseSocket()
        {
            if (tcpClient == null) return;

            IsSelfClose = true;

            tcpClient.Close();

            lock (lockObj)
            {
                Canceler?.Cancel();
            }

            await ReceiveTask;

            tcpClient = null;
            ReceiveTask = null;
            Canceler = null;
        }

        public override async ValueTask Send(Memory<byte> dataBuffer)
        {
            if (tcpClient == null) return;

            await PacketRule.MakeSendPacket(dataBuffer);
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

        private async ValueTask<bool> ReceivedData()
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

                if (await tcpClient.GetStream().ReadAsync(ReceiveBuffer.MemoryBuffer) <= 0)
                {
                    NotifyDisconnect();
                    return false;
                }

                await PacketRule.NotifyReceiveData(ReceiveBuffer.MemoryBuffer);
            }

            return true;
        }

        public override async ValueTask RequestSend(Memory<byte> dataBuffer)
        {
            await tcpClient.GetStream().WriteAsync(dataBuffer);
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