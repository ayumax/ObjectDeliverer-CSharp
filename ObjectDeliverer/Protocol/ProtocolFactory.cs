using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Diagnostics;
using ObjectDeliverer.Protocol.IP;
using ObjectDeliverer.Utils;

namespace ObjectDeliverer.Protocol
{
    public class ProtocolFactory
    {
        public static ProtocolTcpIpClient CreateProtocolTcpIpClient(string ipAddress, int Port, bool autoConnectAfterDisconnect = false)
        {
            var protocol = new ProtocolTcpIpClient();
            protocol.Initialize(ipAddress, Port, autoConnectAfterDisconnect);
            return protocol;
        }

        public static ProtocolTcpIpServer CreateProtocolTcpIpServer(int port)
        {
            var protocol = new ProtocolTcpIpServer();
            protocol.Initialize(port);
            return protocol;
        }

        public static ProtocolUdpSocketSender CreateProtocolUdpSocketSender(string ipAddress, int port)
        {
            var protocol = new ProtocolUdpSocketSender();
            protocol.Initialize(ipAddress, port);
            return protocol;
        }

        public static ProtocolUdpSocketReceiver CreateProtocolUdpSocketReceiver(int boundPort)
        {
            var protocol = new ProtocolUdpSocketReceiver();
            protocol.Initialize(boundPort);
            return protocol;
        }

        public static ProtocolSharedMemory CreateProtocolSharedMemory(string sharedMemoryName = "SharedMemory", int sharedMemorySize = 1024)
        {
            var protocol = new ProtocolSharedMemory();
            protocol.Initialize(sharedMemoryName, sharedMemorySize);
            return protocol;
        }

        public static ProtocolLogWriter CreateProtocolLogWriter(string filePath)
        {
            var protocol = new ProtocolLogWriter();
            protocol.Initialize(filePath);
            return protocol;
        }

        public static ProtocolLogReader CreateProtocolLogReader(string filePath, bool cutFirstInterval = true)
        {
            var protocol = new ProtocolLogReader();
            protocol.Initialize(filePath, cutFirstInterval);
            return protocol;
        }
    }
}