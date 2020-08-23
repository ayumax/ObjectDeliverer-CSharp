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
        private Task? receiveTask = null;

        public int ReceiveBufferSize { get; set; } = 8192;

        public int SendBufferSize { get; set; } = 8192;

        protected GrowBuffer ReceiveBuffer { get; set; } = new GrowBuffer();

        protected CancellationTokenSource? Canceler { get; set; }

        protected IPProtocolHelper? IpClient { get; set; } = null;

        protected bool IsSelfClose { get; set; } = false;

        public override ValueTask StartAsync()
        {
            this.ReceiveBuffer.SetBufferSize(1024);
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

            this.ReceiveBuffer.SetBufferSize(1024);

            this.receiveTask = this.ReceivedDatas();
        }

        protected override ValueTask CloseAsync()
        {
            if (this.IpClient == null) return default(ValueTask);

            this.IsSelfClose = true;

            this.IpClient.Close();

            this.Canceler?.Cancel();

            this.IpClient = null;
            this.Canceler = null;

            return default(ValueTask);
        }

        protected virtual Task ReceivedDatas() => Task.CompletedTask;
    }
}
