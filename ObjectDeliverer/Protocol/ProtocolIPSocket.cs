// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ObjectDeliverer.Protocol.IP;
using ObjectDeliverer.Utils;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ObjectDeliverer.Protocol
{
    public class ProtocolIPSocket : ObjectDelivererProtocol
    {
        private readonly GrowBuffer receiveBuffer = new GrowBuffer();
        private Task? receiveTask = null;
        private CancellationTokenSource? canceler;

        public int ReceiveBufferSize { get; set; } = 8192;

        public int SendBufferSize { get; set; } = 8192;

        protected IPProtocolHelper? IpClient { get; set; } = null;

        protected bool IsSelfClose { get; set; } = false;

        public override ValueTask StartAsync()
        {
            this.receiveBuffer.SetBufferSize(1024);
            return default(ValueTask);
        }

        public override ValueTask CloseAsync()
        {
            if (this.IpClient == null) return default(ValueTask);

            this.IsSelfClose = true;

            this.IpClient.Close();

            this.canceler?.Cancel();

            this.IpClient = null;
            this.canceler = null;

            return default(ValueTask);
        }

        public override ValueTask SendAsync(ReadOnlyMemory<byte> dataBuffer)
        {
            if (this.IpClient == null) return default(ValueTask);

            var sendBuffer = this.PacketRule.MakeSendPacket(dataBuffer);

            return this.IpClient.WriteAsync(sendBuffer);
        }

        public void StartPollingForReceive(IPProtocolHelper connectionSocket)
        {
            this.IpClient = connectionSocket;

            this.receiveBuffer.SetBufferSize(1024);

            this.receiveTask = this.ReceivedDatas();
        }

        private async Task ReceivedDatas()
        {
            if (this.IpClient == null)
            {
                this.DispatchDisconnected(this);
                return;
            }

            this.canceler = new CancellationTokenSource();

            while (this.canceler!.IsCancellationRequested == false)
            {
                if (this.IpClient?.Available > 0)
                {
                    int wantSize = this.PacketRule.WantSize;

                    if (wantSize > 0)
                    {
                        if (this.IpClient.Available < wantSize) continue;
                    }

                    var receiveSize = wantSize == 0 ? this.IpClient.Available : wantSize;

                    this.receiveBuffer.SetBufferSize(receiveSize);

                    if (this.IpClient == null)
                    {
                        this.NotifyDisconnect();
                        return;
                    }

                    if (await this.IpClient.ReadAsync(this.receiveBuffer.MemoryBuffer) <= 0)
                    {
                        this.NotifyDisconnect();
                        return;
                    }

                    foreach (var receivedMemory in this.PacketRule.MakeReceivedPacket(this.receiveBuffer.MemoryBuffer))
                    {
                        this.DispatchReceiveData(new DeliverData()
                        {
                            Sender = this,
                            Buffer = receivedMemory,
                        });
                    }
                }
                else
                {
                    if (this.IpClient?.IsEnable == false)
                    {
                        this.NotifyDisconnect();
                        return;
                    }

                    await Task.Delay(1);
                }
            }
        }

        private void NotifyDisconnect()
        {
            if (!this.IsSelfClose)
            {
                this.IpClient?.Close();
                this.DispatchDisconnected(this);
            }
        }
    }
}
