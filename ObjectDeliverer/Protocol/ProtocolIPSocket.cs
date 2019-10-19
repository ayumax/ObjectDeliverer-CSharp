using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ObjectDeliverer.Protocol.IP;
using ObjectDeliverer.Utils;

namespace ObjectDeliverer.Protocol
{
    public class ProtocolIPSocket : ObjectDelivererProtocol
    {
        protected IPClient? ipClient = null;
        protected bool IsSelfClose = false;

        private Task? receiveTask = null;

        protected GrowBuffer ReceiveBuffer { get; private set; } = new GrowBuffer();

        private CancellationTokenSource? Canceler;

        public override async ValueTask StartAsync()
        {
            await CloseSocket();

            ReceiveBuffer.Reset(1024);
        }

        public override async ValueTask CloseAsync()
        {
            await CloseSocket();
        }

        protected ValueTask CloseSocket()
        {
            if (ipClient == null) return new ValueTask();

            IsSelfClose = true;

            ipClient.Close();

            Canceler?.Cancel();

            ipClient = null;
            Canceler = null;

            return new ValueTask();
        }

        public override async ValueTask SendAsync(Memory<byte> dataBuffer)
        {
            if (ipClient == null) return;

            await PacketRule.MakeSendPacket(dataBuffer);
        }


        public Task StartReceiveAsync(IPClient connectionSocket)
        {
            ipClient = connectionSocket;

            Canceler = new CancellationTokenSource();
            
            ReceiveBuffer.Reset(1024);

            receiveTask = Task.Run(async () =>
            {
                await foreach (var buffer in ReceivedData())
                {
                    PacketRule.NotifyReceiveData(ReceiveBuffer.MemoryBuffer);
                }
            });

            return receiveTask;
        }

        private async IAsyncEnumerable<Memory<byte>> ReceivedData()
        {
            if (ipClient == null)
            {
                DispatchDisconnected(this);
                yield break;
            }

            while(Canceler!.IsCancellationRequested == false)
            {
                if (ipClient.Available > 0)
                {
                    int wantSize = PacketRule.WantSize;

                    if (wantSize > 0)
                    {
                        if (ipClient.Available < wantSize) continue;
                    }

                    var receiveSize = wantSize == 0 ? ipClient.Available : wantSize;

                    ReceiveBuffer.Reset(receiveSize);

                    if (ipClient == null)
                    {
                        NotifyDisconnect();
                        yield break;
                    }

                    if (ipClient.IsEnable == false)
                    {
                        NotifyDisconnect();
                        yield break;
                    }

                    if (await ipClient.ReadAsync(ReceiveBuffer.MemoryBuffer) <= 0)
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

        public override async ValueTask RequestSendAsync(Memory<byte> dataBuffer)
        {
            if (ipClient == null) return;

            await ipClient.WriteAsync(dataBuffer);
        }

        private void NotifyDisconnect()
        {
            if (!IsSelfClose)
            {
                ipClient?.Close();
                DispatchDisconnected(this);
            }
        }
    }
}