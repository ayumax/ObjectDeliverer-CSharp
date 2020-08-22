using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectDeliverer.PacketRule;
using ObjectDeliverer.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObjectDeliverer.Protocol.Tests
{
    [TestClass()]
    public class ProtocolSharedMemoryTests
    {
        private async Task TestSharedMemoryAsync(PacketRuleBase packetRule)
        {
            CountdownEvent condition0 = new CountdownEvent(2);

            var sender = new ProtocolSharedMemory()
            {
                SharedMemoryName = "test_shared_memory",
                SharedMemorySize = 10,
            };
            sender.SetPacketRule(packetRule.Clone());

            var receiver = new ProtocolSharedMemory()
            {
                SharedMemoryName = "test_shared_memory",
                SharedMemorySize = 10,
            };
            receiver.SetPacketRule(packetRule.Clone());

            using (sender.Connected.Subscribe(x => condition0.Signal()))
            using (receiver.Connected.Subscribe(x => condition0.Signal()))
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

                using (var condition = new CountdownEvent(1))
                using (receiver.ReceiveData.Subscribe(x =>
                {
                    Assert.IsTrue(x.Buffer.ToArray().SequenceEqual(expected));
                    condition.Signal();
                }))
                {
                    await sender.SendAsync(expected);

                    if (!condition.Wait(1000))
                    {
                        Assert.Fail();
                    }
                }
            }

            {
                var expected = new byte[] { 1, 2, 3 };

                using (var condition = new CountdownEvent(1))
                using (sender.ReceiveData.Subscribe(x =>
                {
                    Assert.IsTrue(x.Buffer.ToArray().SequenceEqual(expected));
                    condition.Signal();
                }))
                {
                    await receiver.SendAsync(expected);

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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                await TestSharedMemoryAsync(new PacketRuleSizeBody());
                await TestSharedMemoryAsync(new PacketRuleFixedLength() { FixedSize = 3 });
                await TestSharedMemoryAsync(new PacketRuleNodivision());
                await TestSharedMemoryAsync(new PacketRuleTerminate() { Terminate = new byte[] { 0xEE, 0xFF } });
            }
        }
    }
}