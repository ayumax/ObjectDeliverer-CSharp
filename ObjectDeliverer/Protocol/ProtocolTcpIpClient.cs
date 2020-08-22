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
    public class ProtocolTcpIpClient : ProtocolIPSocket
    {
        private ValueTask connectTask;

        public string IpAddress { get; set; } = "127.0.0.1";

        public int Port { get; set; } = 0;

        public bool AutoConnectAfterDisconnect { get; set; } = false;

        public override async ValueTask StartAsync()
        {
            await base.StartAsync();

            this.StartConnect();
        }

        protected override async ValueTask CloseAsync()
        {
            await base.CloseAsync();

            await this.connectTask;
        }

        protected override void DispatchDisconnected(ObjectDelivererProtocol delivererProtocol)
        {
            base.DispatchDisconnected(delivererProtocol);

            if (this.AutoConnectAfterDisconnect)
            {
                this.StartConnect();
            }
        }

        protected override async Task ReceivedDatas()
        {
            if (this.IpClient == null)
            {
                this.DispatchDisconnected(this);
                return;
            }

            this.Canceler = new CancellationTokenSource();

            while (this.Canceler!.IsCancellationRequested == false)
            {
                if (this.IpClient?.Available > 0)
                {
                    int wantSize = this.PacketRule.WantSize;

                    if (wantSize > 0)
                    {
                        if (this.IpClient.Available < wantSize) continue;
                    }

                    var receiveSize = wantSize == 0 ? this.IpClient.Available : wantSize;

                    this.ReceiveBuffer.SetBufferSize(receiveSize);

                    if (this.IpClient == null)
                    {
                        this.NotifyDisconnect();
                        return;
                    }

                    if (await this.IpClient.ReadAsync(this.ReceiveBuffer.MemoryBuffer) <= 0)
                    {
                        this.NotifyDisconnect();
                        return;
                    }

                    foreach (var receivedMemory in this.PacketRule.MakeReceivedPacket(this.ReceiveBuffer.MemoryBuffer))
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

        private void StartConnect()
        {
            async ValueTask ConnectAsync()
            {
                await this.CloseAsync();
                this.IsSelfClose = false;

                this.IpClient = new TCPProtocolHelper(this.ReceiveBufferSize, this.SendBufferSize);

                while (this.IsSelfClose == false)
                {
                    try
                    {
                        await this.IpClient.ConnectAsync(this.IpAddress, this.Port);

                        this.DispatchConnected(this);

                        this.StartPollingForReceive(this.IpClient);

                        break;
                    }
                    catch (SocketException)
                    {
                        // Wait a minute and then try to reconnect.
                        await Task.Delay(1000);
                    }
                }
            }

            this.connectTask = ConnectAsync();
        }
    }
}
