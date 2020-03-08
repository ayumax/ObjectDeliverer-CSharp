using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.Linq;
using ObjectDeliverer;
using ObjectDeliverer.Protocol;
using ObjectDeliverer.PacketRule;

namespace ObjectDeliverTest
{
    public class DelivererManagerTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Test1()
        {
            var tcpServer = ObjectDelivererManager.CreateObjectDelivererManager();
            tcpServer.Connected += x => System.Diagnostics.Debug.WriteLine($"tcpServer connected : {x}");
            tcpServer.Disconnected += x => System.Diagnostics.Debug.WriteLine($"tcpServer disconnected : {x}");
            tcpServer.Received += (x, y) => System.Diagnostics.Debug.WriteLine($"tcpServer received : {y.Length}");


            await tcpServer.StartAsync(ProtocolFactory.CreateProtocolTcpIpServer(8025),
                                       PacketRuleFactory.CreatePacketRuleSizeBody(4, PacketRuleBase.ECNBufferEndian.Big),
                                       null);

            var tcpClient = ObjectDelivererManager.CreateObjectDelivererManager();
            tcpClient.Connected += x => System.Diagnostics.Debug.WriteLine($"tcpClient connected : {x}");
            tcpClient.Disconnected += x => System.Diagnostics.Debug.WriteLine($"tcpClient disconnected : {x}");
            tcpClient.Received += (x, y) => System.Diagnostics.Debug.WriteLine($"tcpClient received : {y.Length}");


            await tcpClient.StartAsync(ProtocolFactory.CreateProtocolTcpIpClient("127.0.0.1", 8025),
                                       PacketRuleFactory.CreatePacketRuleSizeBody(4, PacketRuleBase.ECNBufferEndian.Big),
                                       null);


            await tcpServer.SendAsync(Enumerable.Range(1, 10).Select(x => (byte)x).ToArray());

            await Task.Delay(1000);
            await tcpServer.CloseAsync();
        }
    }
}