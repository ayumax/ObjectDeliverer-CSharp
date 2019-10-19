// Copyright 2019 ayumax. All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ObjectDeliverer.Protocol.IP;
using ObjectDeliverer.Utils;

namespace ObjectDeliverer.Protocol
{
    public class ProtocolTcpIpServer : ObjectDelivererProtocol
    {
        public int ListenPort { get; set; }
        public bool IsMultiClient { get; set; }

        private TcpListener? tcpListener = null;

        private List<ProtocolIPSocket> ConnectedSockets = new List<ProtocolIPSocket>();

        private CancellationTokenSource? Canceler;

        public void Initialize(int Port, bool IsMultiClient)
        {
            this.ListenPort = Port;
            this.IsMultiClient = IsMultiClient;
        }

        public override async ValueTask StartAsync()
        {
            await CloseAsync();

            Canceler = new CancellationTokenSource();

            tcpListener = new TcpListener(IPAddress.Any, ListenPort);

            List<Task> pollingTasks = new List<Task>();

            while (Canceler.IsCancellationRequested == false)
            {
                var client = new TCPClient(await tcpListener.AcceptTcpClientAsync());

                var clientSocket = new ProtocolIPSocket();
                clientSocket.Disconnected += ClientSocket_Disconnected;
                clientSocket.ReceiveData += ClientSocket_ReceiveData;
                clientSocket.SetPacketRule(PacketRule.Clone());

                ConnectedSockets.Add(clientSocket);

                DispatchConnected(clientSocket);

                pollingTasks.Add(clientSocket.StartReceiveAsync(client));
            }

            await Task.WhenAll(pollingTasks);

        }

        private void ClientSocket_ReceiveData(ObjectDelivererProtocol delivererProtocol, Memory<byte> receivedBuffer)
        {
            DispatchReceiveData(delivererProtocol, receivedBuffer);
        }

        private void ClientSocket_Disconnected(ObjectDelivererProtocol delivererProtocol)
        {
            if (delivererProtocol == null) return;

            if (delivererProtocol is ProtocolIPSocket protocolTcpIp)
            {
                int foundIndex = ConnectedSockets.IndexOf(protocolTcpIp);
                if (foundIndex >= 0)
                {
                    protocolTcpIp.Disconnected -= ClientSocket_Disconnected;
                    protocolTcpIp.ReceiveData -= ClientSocket_ReceiveData;

                    ConnectedSockets.RemoveAt(foundIndex);

                    DispatchDisconnected(protocolTcpIp);
                }
            }
        }

        public override async ValueTask CloseAsync()
        {
            Canceler?.Cancel();
            tcpListener?.Stop();

            List<Task> closeTasks = new List<Task>();

            foreach (var clientSocket in ConnectedSockets)
            {
                clientSocket.Disconnected -= ClientSocket_Disconnected;
                clientSocket.ReceiveData -= ClientSocket_ReceiveData;
                closeTasks.Add(clientSocket.CloseAsync().AsTask());
            }

            ConnectedSockets.Clear();

            await Task.WhenAll(closeTasks);
        }

        public override async ValueTask SendAsync(Memory<byte> dataBuffer)
        {
            List<Task> sendTasks = new List<Task>();

            foreach(var clientSocket in ConnectedSockets)
            {
                sendTasks.Add(clientSocket.SendAsync(dataBuffer).AsTask());
            }

            await Task.WhenAll(sendTasks);
        }
    }
}


