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
        protected IPProtocolHelper? ipClient = null;
        protected bool IsSelfClose = false;

        private Task? receiveTask = null;

        protected GrowBuffer ReceiveBuffer { get; private set; } = new GrowBuffer();

        private CancellationTokenSource? Canceler;

        public override async ValueTask StartAsync()
        {
            await CloseAsync();

            ReceiveBuffer.SetBufferSize(1024);
        }

        public override ValueTask CloseAsync()
        {
            if (ipClient == null) return new ValueTask();

            IsSelfClose = true;

            ipClient.Close();

            Canceler?.Cancel();

            ipClient = null;
            Canceler = null;

            return new ValueTask();
        }

        public override ValueTask SendAsync(Memory<byte> dataBuffer)
        {
            if (ipClient == null) return new ValueTask();

            var sendBuffer = PacketRule.MakeSendPacket(dataBuffer);

            return ipClient.WriteAsync(sendBuffer);
        }


        protected void StartPollingForReceive(IPProtocolHelper connectionSocket)
        {
            ipClient = connectionSocket;
            
            ReceiveBuffer.SetBufferSize(1024);

            receiveTask = ReceivedData();
        }

        private async Task ReceivedData()
        {
            if (ipClient == null)
            {
                DispatchDisconnected(this);
                return;
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

                    ReceiveBuffer.SetBufferSize(receiveSize);

                    if (ipClient == null)
                    {
                        NotifyDisconnect();
                        return;
                    }

                    if (ipClient.IsEnable == false)
                    {
                        NotifyDisconnect();
                        return;
                    }

                    if (await ipClient.ReadAsync(ReceiveBuffer.MemoryBuffer) <= 0)
                    {
                        NotifyDisconnect();
                        return;
                    }

                    foreach (var receivedMemory in PacketRule.NotifyReceiveData(ReceiveBuffer.MemoryBuffer))
                    {
                        DispatchReceiveData(this, receivedMemory);
                    }
                }
                else
                {
                    await Task.Delay(1);
                }
            }

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