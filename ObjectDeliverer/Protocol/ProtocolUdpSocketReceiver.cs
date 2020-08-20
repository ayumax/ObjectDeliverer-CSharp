// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ObjectDeliverer.Protocol.IP;
using ObjectDeliverer.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ObjectDeliverer.Protocol
{
    public class ProtocolUdpSocketReceiver : ProtocolIPSocket
    {
        public int BoundPort { get; set; }

        public override async ValueTask StartAsync()
        {
            await base.StartAsync();

            this.IpClient = new UDPProtocolHelper(this.BoundPort, this.ReceiveBufferSize);

            this.StartPollingForReceive(this.IpClient);

            this.DispatchConnected(this);
        }

        protected override async Task ReceivedDatas()
        {
            if (this.IpClient == null)
            {
                this.DispatchDisconnected(this);
                return;
            }

            this.Canceler = new CancellationTokenSource();

            while (this.Canceler?.IsCancellationRequested == false)
            {
                if (this.IpClient?.Available > 0)
                {
                    try
                    {
                        this.ReceiveBuffer.SetBufferSize(this.IpClient.Available);
                        var (recievedBuffer, endPoint) = await this.IpClient.ReceiveAsync();

                        if (this.Canceler == null || this.IpClient == null) return;

                        ReadOnlyMemory<byte> readOnlyMemory = recievedBuffer;

                        int startOffset = 0;
                        int wantSize = 0;
                        int remainSize = recievedBuffer.Length;

                        while (remainSize > 0)
                        {
                            wantSize = this.PacketRule.WantSize;

                            if (wantSize > remainSize) break;

                            if (wantSize > 0)
                            {
                                if (remainSize < wantSize) continue;
                            }

                            wantSize = wantSize == 0 ? remainSize : wantSize;

                            foreach (var receivedMemory in this.PacketRule.MakeReceivedPacket(readOnlyMemory.Slice(startOffset, wantSize)))
                            {
                                this.DispatchReceiveData(new DeliverData()
                                {
                                    Sender = this,
                                    Buffer = receivedMemory,
                                });
                            }

                            startOffset += wantSize;
                            remainSize -= wantSize;
                        }
                    }
                   catch(Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message);
                    }
                }
                else
                {
                    await Task.Delay(1);
                }
            }
        }
    }
}
