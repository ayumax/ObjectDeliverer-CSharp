// Copyright 2019 ayumax. All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ObjectDeliverer.Protocol.IP;
using ObjectDeliverer.Utils;
using ValueTaskSupplement;

namespace ObjectDeliverer.Protocol
{
    public class ProtocolTcpIpServer : ObjectDelivererProtocol
    {
        public int ListenPort { get; set; }

        private TcpListener? tcpListener = null;

        private List<ProtocolIPSocket> ConnectedSockets = new List<ProtocolIPSocket>();

        private CancellationTokenSource? Canceler;
        private Task? waitClientsTask;

        public void Initialize(int Port)
        {
            this.ListenPort = Port;
        }

        public override async ValueTask StartAsync()
        {
            await CloseAsync();

            Canceler = new CancellationTokenSource();

            tcpListener = new TcpListener(IPAddress.Any, ListenPort);
            tcpListener.Start();

            waitClientsTask = Task.Run(async () =>
            {
                while (Canceler.IsCancellationRequested == false)
                {
                    try
                    {
                        var _client = await tcpListener.AcceptTcpClientAsync();
                        var client = new TCPClientProtocol(_client);

                        var clientSocket = new ProtocolIPSocket();
                        clientSocket.Disconnected += ClientSocket_Disconnected;
                        clientSocket.ReceiveData += ClientSocket_ReceiveData;
                        clientSocket.SetPacketRule(PacketRule.Clone());

                        ConnectedSockets.Add(clientSocket);

                        DispatchConnected(clientSocket);

                        _ = clientSocket.StartReceiveAsync(client);
                    }
                    catch (Exception e)
                    {

                    }
                   
                }
            }, Canceler.Token);




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

        public override ValueTask SendAsync(Memory<byte> dataBuffer)
        {
            List<ValueTask> sendTasks = new List<ValueTask>();

            foreach(var clientSocket in ConnectedSockets)
            {
                sendTasks.Add(clientSocket.SendAsync(dataBuffer));
            }

            return ValueTaskEx.WhenAll(sendTasks);
        }
    }
}


