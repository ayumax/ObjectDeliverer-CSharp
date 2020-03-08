using System;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ObjectDeliverer.Protocol.IP
{
    public class TCPClientProtocol : IPClientProtocol
    {
        protected TcpClient tcpClient;

        public TCPClientProtocol()
        {
            tcpClient = new TcpClient();
        }

        public TCPClientProtocol(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
        }

        public override int Available => tcpClient.Available;

        public override bool IsEnable => tcpClient.GetStream() != null;

        public override ValueTask<int> ReadAsync(Memory<byte> buffer)
        {
            return tcpClient.GetStream().ReadAsync(buffer);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer)
        {
            return tcpClient.GetStream().WriteAsync(buffer);
        }

        public override void Close()
        {
            tcpClient.Close();
        }

        public override Task ConnectAsync(string host, int port)
        {
            return tcpClient.ConnectAsync(host, port);
        }
    }
}
