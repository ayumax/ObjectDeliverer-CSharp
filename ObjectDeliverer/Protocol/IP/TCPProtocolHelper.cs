// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ObjectDeliverer.Protocol.IP
{
    public class TCPProtocolHelper : IPProtocolHelper
    {
        public TCPProtocolHelper(int receiveBufferSize, int sendBufferSize)
        {
            this.TcpClient = new TcpClient();
            this.TcpClient.NoDelay = true;
            this.TcpClient.ReceiveBufferSize = receiveBufferSize;
            this.TcpClient.SendBufferSize = sendBufferSize;
        }

        public TCPProtocolHelper(TcpClient tcpClient, int receiveBufferSize, int sendBufferSize)
        {
            this.TcpClient = tcpClient;
            this.TcpClient.NoDelay = true;
            this.TcpClient.ReceiveBufferSize = receiveBufferSize;
            this.TcpClient.SendBufferSize = sendBufferSize;
        }

        public override int Available => this.TcpClient.Available;

        public override bool IsEnable
        {
            get
            {
                if (this.TcpClient == null) return false;
                if (this.TcpClient.Connected == false) return false;
                if (this.TcpClient.Client.Poll(0, SelectMode.SelectRead) && this.TcpClient.Client.Available == 0) return false;

                return true;
            }
        }

        protected TcpClient TcpClient { get; set; }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer)
        {
            return this.TcpClient.GetStream().ReadAsync(buffer);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer)
        {
            return this.TcpClient.GetStream().WriteAsync(buffer);
        }

        public override void Close()
        {
            this.TcpClient.Close();
        }

        public override Task ConnectAsync(string host, int port)
        {
            return this.TcpClient.ConnectAsync(host, port);
        }
    }
}
