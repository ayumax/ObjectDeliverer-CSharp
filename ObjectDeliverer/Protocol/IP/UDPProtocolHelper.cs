using System;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ObjectDeliverer.Protocol.IP
{
    public class UDPProtocolHelper : IPProtocolHelper
    {
        protected UdpClient udpClient;
        private string host = "";
        private int port = 0;

        public UDPProtocolHelper()
        {
            udpClient = new UdpClient();
        }

        public UDPProtocolHelper(int boundPort)
        {
            udpClient = new UdpClient(boundPort);
        }


        public override int Available => udpClient.Available;

        public override bool IsEnable => udpClient != null;

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer)
        {
            var result = await udpClient.ReceiveAsync();
            if (buffer.Length >= result.Buffer.Length)
            {
                ObjectDeliverer.Utils.MemoryExtention.Copy(buffer.Span, result.Buffer);
                return result.Buffer.Length;
            }

            return 0;
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer)
        {
            await udpClient.SendAsync(buffer.ToArray(), buffer.Length);
        }

        public override void Close()
        {
            udpClient.Close();
        }

        public override Task ConnectAsync(string host, int port)
        {
            this.host = host;
            this.port = port;

            return Task.CompletedTask;
        }
    }
}
