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
using ValueTaskSupplement;

namespace ObjectDeliverer.Protocol
{
    public class ProtocolTcpIpServer : ObjectDelivererProtocol
    {
        private readonly List<ProtocolIPSocket> connectedSockets = new List<ProtocolIPSocket>();

        private TcpListener? tcpListener = null;
        private PollingTask? waitClientsTask;

        public int ReceiveBufferSize { get; set; } = 8192;

        public int SendBufferSize { get; set; } = 8192;

        public int ListenPort { get; set; }

        public override ValueTask StartAsync()
        {
            this.tcpListener = new TcpListener(IPAddress.Any, this.ListenPort);
            this.tcpListener.Start();

            this.waitClientsTask = new PollingTask(this.OnAcceptTcpClientAsync);

            return default(ValueTask);
        }

        public override ValueTask SendAsync(ReadOnlyMemory<byte> dataBuffer)
        {
            List<ValueTask> sendTasks = new List<ValueTask>();

            foreach (var clientSocket in this.connectedSockets)
            {
                sendTasks.Add(clientSocket.SendAsync(dataBuffer));
            }

            return ValueTaskEx.WhenAll(sendTasks);
        }

        protected override ValueTask CloseAsync()
        {
            this.tcpListener?.Stop();
            this.tcpListener = null;

            List<ValueTask> closeTasks = new List<ValueTask>();

            if (this.waitClientsTask != null)
            {
                closeTasks.Add(this.waitClientsTask.DisposeAsync());
            }

            foreach (var clientSocket in this.connectedSockets)
            {
                closeTasks.Add(clientSocket.DisposeAsync());
            }

            this.connectedSockets.Clear();

            return ValueTaskEx.WhenAll(closeTasks);
        }

        private async ValueTask<bool> OnAcceptTcpClientAsync()
        {
            if (this.tcpListener == null) return false;

            try
            {
                var acceptedTcpClient = await this.tcpListener.AcceptTcpClientAsync();
                var acceptedTcpClientWrapper = new TCPProtocolHelper(acceptedTcpClient, this.ReceiveBufferSize, this.SendBufferSize);

                var clientSocket = new ProtocolTcpIpClient();
                clientSocket.Disconnected.Subscribe(x => this.ClientSocket_Disconnected(x.Target));
                clientSocket.ReceiveData.Subscribe(x => this.DispatchReceiveData(x));
                clientSocket.SetPacketRule(this.PacketRule.Clone());

                this.connectedSockets.Add(clientSocket);

                this.DispatchConnected(clientSocket);

                clientSocket.StartPollingForReceive(acceptedTcpClientWrapper);
            }
            catch (SocketException)
            {
                return false;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }

            return true;
        }

        private async void ClientSocket_Disconnected(ObjectDelivererProtocol delivererProtocol)
        {
            if (delivererProtocol == null) return;

            if (delivererProtocol is ProtocolIPSocket protocolTcpIp)
            {
                int foundIndex = this.connectedSockets.IndexOf(protocolTcpIp);
                if (foundIndex >= 0)
                {
                    this.connectedSockets.RemoveAt(foundIndex);

                    this.DispatchDisconnected(protocolTcpIp);

                    await protocolTcpIp.DisposeAsync();
                }
            }
        }
    }
}
