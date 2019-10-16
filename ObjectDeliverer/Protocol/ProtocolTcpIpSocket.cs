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
        protected TcpClient? tcpClient = null;
        protected bool IsSelfClose = false;

        protected object lockObj = new object();

        protected GrowBuffer ReceiveBuffer { get; private set; } = new GrowBuffer();

        private Task? ReceiveTask;
        private CancellationTokenSource? Canceler;

        public override ValueTask Start()
        {
            return new ValueTask();
        }

        public override async ValueTask Close()
        {
            await CloseSocket();
        }

        protected ValueTask CloseSocket()
        {
            if (tcpClient == null) return new ValueTask();

            IsSelfClose = true;

            tcpClient.Close();

            Canceler?.Cancel();

            tcpClient = null;
            ReceiveTask = null;
            Canceler = null;

            return new ValueTask();
        }

        public override async ValueTask Send(Memory<byte> dataBuffer)
        {
            if (tcpClient == null) return;

            await PacketRule.MakeSendPacket(dataBuffer);
        }


        public async ValueTask StartPollilng(TcpClient connectionSocket)
        {
            tcpClient = connectionSocket;

            Canceler = new CancellationTokenSource();
            
            ReceiveBuffer.Reset(1024);

            await foreach(var buffer in ReceivedData())
            {
                await PacketRule.NotifyReceiveData(ReceiveBuffer.MemoryBuffer);
            }
            
        }

        private async IAsyncEnumerable<Memory<byte>> ReceivedData()
        {
            if (tcpClient == null)
            {
                DispatchDisconnected(this);
                yield break;
            }

            while(Canceler!.IsCancellationRequested == false)
            {
                if (tcpClient.Available > 0)
                {
                    int wantSize = PacketRule.WantSize;

                    if (wantSize > 0)
                    {
                        if (tcpClient.Available < wantSize) continue;
                    }

                    var receiveSize = wantSize == 0 ? tcpClient.Available : wantSize;

                    ReceiveBuffer.Reset(receiveSize);

                    if (tcpClient == null)
                    {
                        NotifyDisconnect();
                        yield break;
                    }

                    if (tcpClient.GetStream() == null)
                    {
                        NotifyDisconnect();
                        yield break;
                    }

                    if (await tcpClient.GetStream().ReadAsync(ReceiveBuffer.MemoryBuffer) <= 0)
                    {
                        NotifyDisconnect();
                        yield break;
                    }

                    yield return ReceiveBuffer.MemoryBuffer;
                }
                else
                {
                    await Task.Delay(1);
                }
            }

        }

        public override async ValueTask RequestSend(Memory<byte> dataBuffer)
        {
            if (tcpClient == null) return;

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