using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectDeliverer.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ObjectDeliverer.PacketRule;

namespace ObjectDeliverer.Protocol.Tests
{
    [TestClass()]
    public class ProtocolTcpIpClientTests
    {
        private async Task TestTCPAsync(PacketRuleBase packetRule)
        {
            CountdownEvent condition0 = new CountdownEvent(2);

            var client = new ProtocolTcpIpClient()
            {
                IpAddress = "127.0.0.1",
                Port = 9013,
                AutoConnectAfterDisconnect = true,
            };
            client.SetPacketRule(packetRule.Clone());

            var server = new ProtocolTcpIpServer()
            {
                ListenPort = 9013,
            };
            server.SetPacketRule(packetRule.Clone());

            using (client.Connected.Subscribe(x => condition0.Signal()))
            using (server.Connected.Subscribe(x => condition0.Signal()))
            {
                await server.StartAsync();

                await client.StartAsync();

                if (!condition0.Wait(1000))
                {
                    Assert.Fail();
                }
            }

            {
                var expected = new byte[] { 1, 2, 3 };

                using (var condition = new CountdownEvent(100))
                using (server.ReceiveData.Subscribe(x =>
                {
                    var expected2 = new byte[] { (byte)condition.CurrentCount, 2, 3 };
                    Assert.IsTrue(x.Buffer.ToArray().SequenceEqual(expected2));
                    condition.Signal();
                }))
                {
                    for (byte i = 100; i > 0; --i)
                    {
                        expected[0] = i;
                        await client.SendAsync(expected);
                    }

                    if (!condition.Wait(1000))
                    {
                        Assert.Fail();
                    }
                }
            }

            {
                var expected = new byte[] { 1, 2, 3 };

                using (var condition = new CountdownEvent(100))
                using (client.ReceiveData.Subscribe(x =>
                {
                    var expected2 = new byte[] { (byte)condition.CurrentCount, 2, 3 };
                    Assert.IsTrue(x.Buffer.ToArray().SequenceEqual(expected2));
                    condition.Signal();
                }))
                {
                    for (byte i = 100; i > 0; --i)
                    {
                        expected[0] = i;
                        await server.SendAsync(expected);
                    }

                    if (!condition.Wait(3000))
                    {
                        Assert.Fail();
                    }
                }
            }


            {
                using (var condition = new CountdownEvent(1))
                using (client.Disconnected.Subscribe(x => condition.Signal()))
                {
                    await server.DisposeAsync();

                    if (!condition.Wait(1000))
                    {
                        Assert.Fail();
                    }
                }
            }

            {
                using (var condition = new CountdownEvent(1))
                using (client.Connected.Subscribe(x => condition.Signal()))
                {
                    server = new ProtocolTcpIpServer()
                    {
                        ListenPort = 9013,
                    };
                    server.SetPacketRule(packetRule.Clone());

                    await server.StartAsync();

                    if (!condition.Wait(5000))
                    {
                        Assert.Fail();
                    }
                }

                using (var condition = new CountdownEvent(1))
                using (server.Disconnected.Subscribe(x => condition.Signal()))
                {
                    await client.DisposeAsync();

                    if (!condition.Wait(1000))
                    {
                        Assert.Fail();
                    }
                }
            }

            await client.DisposeAsync();
            await server.DisposeAsync();
        }

        [TestMethod()]
        public async Task ConnectTest()
        {
            await TestTCPAsync(new PacketRuleSizeBody());
            await TestTCPAsync(new PacketRuleFixedLength() { FixedSize = 3 });
            await TestTCPAsync(new PacketRuleTerminate() { Terminate = new byte[] { 0xEE, 0xFF } });
        }
    }
}