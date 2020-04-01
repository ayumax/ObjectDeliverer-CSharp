﻿// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ObjectDeliverer.Protocol.IP
{
    public class UDPProtocolHelper : IPProtocolHelper
    {
        private string host = string.Empty;
        private int port = 0;

        public UDPProtocolHelper(int sendBufferSize)
        {
            this.UdpClient = new UdpClient();
            this.UdpClient.Client.SendBufferSize = sendBufferSize;
        }

        public UDPProtocolHelper(int boundPort, int receiveBufferSize)
        {
            this.UdpClient = new UdpClient(boundPort);
            this.UdpClient.Client.ReceiveBufferSize = receiveBufferSize;
        }

        public override int Available => this.UdpClient.Available;

        public override bool IsEnable => this.UdpClient != null;

        protected UdpClient UdpClient { get; set; }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer)
        {
            var result = await this.UdpClient.ReceiveAsync();
            if (buffer.Length >= result.Buffer.Length)
            {
                ObjectDeliverer.Utils.MemoryExtention.Copy(buffer.Span, result.Buffer);
                return result.Buffer.Length;
            }

            return 0;
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer)
        {
            await this.UdpClient.SendAsync(buffer.ToArray(), buffer.Length, this.host, this.port);
        }

        public override void Close()
        {
            this.UdpClient.Close();
        }

        public override Task ConnectAsync(string host, int port)
        {
            this.host = host;
            this.port = port;

            return Task.CompletedTask;
        }
    }
}
