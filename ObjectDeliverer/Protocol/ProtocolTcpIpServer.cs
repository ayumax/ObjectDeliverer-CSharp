// Copyright 2019 ayumax. All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ObjectDeliverer.Utils;

namespace ObjectDeliverer.Protocol
{
    public class ProtocolTcpIpServer : ProtocolTcpIpSocket
    {
        public int ListenPort { get; private set; }

        private TcpListener? tcpListener = null;

        private List<ProtocolTcpIpSocket> ConnectedSockets = new List<ProtocolTcpIpSocket>();

        public void Initialize(int Port)
        {
            ListenPort = Port;
        }

        public override async ValueTask Start()
        {
            await Close();

            tcpListener = new TcpListener(IPAddress.Any, ListenPort);

            List<ValueTask> pollingTasks = new List<ValueTask>();

            while (true)
            {
                var client = await tcpListener.AcceptTcpClientAsync();

                var clientSocket = new ProtocolTcpIpSocket();
                clientSocket.Disconnected += ClientSocket_Disconnected;
                clientSocket.ReceiveData += ClientSocket_ReceiveData;
                clientSocket.SetPacketRule(PacketRule.Clone());

                ConnectedSockets.Add(clientSocket);

                DispatchConnected(clientSocket);

                pollingTasks.Add(clientSocket.StartPollilng(client));
            }

        }

        private void ClientSocket_ReceiveData(ObjectDelivererProtocol delivererProtocol, Memory<byte> receivedBuffer)
        {
            throw new NotImplementedException();
        }

        private void ClientSocket_Disconnected(ObjectDelivererProtocol delivererProtocol)
        {
            throw new NotImplementedException();
        }



        public override async ValueTask Close()
        {
            foreach (var clientSocket in ConnectedSockets)
            {
                clientSocket.Disconnected -= ClientSocket_Disconnected;
                clientSocket.ReceiveData -= ClientSocket_ReceiveData;
                await clientSocket.Close();
            }

            ConnectedSockets.Clear();
        }

        public override async ValueTask Send(Memory<byte> dataBuffer)
        {
            List<Task> sendTasks = new List<Task>();

            foreach(var clientSocket in ConnectedSockets)
            {
                sendTasks.Add(clientSocket.Send(dataBuffer).AsTask());
            }

            await Task.WhenAll(sendTasks);
        }
    }
}



//void DisconnectedClient(const UObjectDelivererProtocol* ClientSocket)
//{
//	auto _clientSocket = (UProtocolTcpIpSocket*)(ClientSocket);
//	if (!IsValid(_clientSocket)) return;

//	auto foundIndex = ConnectedSockets.Find(_clientSocket);
//	if (foundIndex != INDEX_NONE)
//	{
//		_clientSocket->Disconnected.Unbind();
//		_clientSocket->ReceiveData.Unbind();

//		ConnectedSockets.RemoveAt(foundIndex);

//		DispatchDisconnected(ClientSocket);
//	}
//}

//void ReceiveDataFromClient(const UObjectDelivererProtocol* ClientSocket, const TArray<uint8>& Buffer)
//{
//    DispatchReceiveData(ClientSocket, Buffer);
//}
//}