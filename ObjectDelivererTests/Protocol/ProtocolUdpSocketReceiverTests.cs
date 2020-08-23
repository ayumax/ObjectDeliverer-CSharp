using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectDeliverer.PacketRule;
using ObjectDeliverer.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObjectDeliverer.Protocol.Tests
{
    [TestClass()]
    public class ProtocolUdpSocketReceiverTests
    {
        private async Task TestUDPAsync(IPacketRule packetRule)
        {
            CountdownEvent condition0 = new CountdownEvent(2);

            var receiver = new ProtocolUdpSocketReceiver()
            {
                BoundPort = 9013,
            };
            receiver.SetPacketRule(packetRule.Clone());

            var sender = new ProtocolUdpSocketSender()
            {
                DestinationIpAddress = "127.0.0.1",
                DestinationPort = 9013,
            };
            sender.SetPacketRule(packetRule.Clone());

            using (receiver.Connected.Subscribe(x => condition0.Signal()))
            using (sender.Connected.Subscribe(x => condition0.Signal()))
            {
                await sender.StartAsync();

                await receiver.StartAsync();

                if (!condition0.Wait(1000))
                {
                    Assert.Fail();
                }
            }

            {
                var expected = new byte[] { 1, 2, 3 };

                using (var condition = new CountdownEvent(100))
                using (receiver.ReceiveData.Subscribe(x =>
                {
                    var expected2 = new byte[] { (byte)condition.CurrentCount, 2, 3 };
                    Assert.IsTrue(x.Buffer.ToArray().SequenceEqual(expected2));
                    condition.Signal();
                }))
                {
                    for (byte i = 100; i > 0; --i)
                    {
                        expected[0] = i;
                        await sender.SendAsync(expected);

                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        await Task.Run(async () =>
                        {
                            while(condition.CurrentCount != i - 1 && sw.ElapsedMilliseconds < 1000)
                            {
                                await Task.Delay(1);
                            }

                            if (condition.CurrentCount != i - 1)
                            {
                                Assert.Fail();
                            }
                        });
                    }

                    if (!condition.Wait(1000))
                    {
                        Assert.Fail();
                    }
                }
            }

            await sender.DisposeAsync();
            await receiver.DisposeAsync();
        }

        [TestMethod()]
        public async Task InitializeTest()
        {
            await TestUDPAsync(new PacketRuleSizeBody());
            await TestUDPAsync(new PacketRuleFixedLength() { FixedSize = 3 });
            await TestUDPAsync(new PacketRuleNodivision());
            await TestUDPAsync(new PacketRuleTerminate() { Terminate = new byte[] { 0xEE, 0xFF } });
        }       
    }
}