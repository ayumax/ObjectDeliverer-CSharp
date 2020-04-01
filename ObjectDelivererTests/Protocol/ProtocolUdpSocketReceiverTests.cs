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
        [TestMethod()]
        public async Task InitializeTest()
        {
            CountdownEvent condition0 = new CountdownEvent(2);

            var receiver = new ProtocolUdpSocketReceiver()
            {
                BoundPort = 9013,
            };
            receiver.SetPacketRule(new PacketRuleFixedLength() { FixedSize = 3 });

            var sender = new ProtocolUdpSocketSender()
            {
                DestinationIpAddress = "127.0.0.1",
                DestinationPort = 9013,
            };
            sender.SetPacketRule(new PacketRuleFixedLength() { FixedSize = 3 });

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
                    }

                    if (!condition.Wait(1000))
                    {
                        Assert.Fail();
                    }
                }
            }
        }
    }
}