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
    public class ProtocolLogWriterTests
    {
        private async Task TestLogFileAsync(IPacketRule packetRule)
        {
            var tempFilePath = System.IO.Path.GetTempFileName();

            {
                CountdownEvent condition0 = new CountdownEvent(1);

                var sender = new ProtocolLogWriter()
                {
                    FilePath = tempFilePath,
                };
                sender.SetPacketRule(packetRule.Clone());
                sender.Connected.Subscribe(x => condition0.Signal());

                await sender.StartAsync();

                if (!condition0.Wait(1000))
                {
                    Assert.Fail();
                }

                var expected = new byte[] { 1, 2, 3 };

                for (byte i = 100; i > 0; --i)
                {
                    expected[0] = i;
                    await sender.SendAsync(expected);
                }

                await sender.DisposeAsync();
            }

            {
                CountdownEvent condition0 = new CountdownEvent(1);
                var reader = new ProtocolLogReader()
                {
                    FilePath = tempFilePath,
                    CutFirstInterval = true,
                };
                reader.SetPacketRule(packetRule.Clone());
                reader.Connected.Subscribe(x => condition0.Signal());

                using (var condition = new CountdownEvent(100))
                using (reader.ReceiveData.Subscribe(x =>
                {
                    var expected2 = new byte[] { (byte)condition.CurrentCount, 2, 3 };
                    Assert.IsTrue(x.Buffer.ToArray().SequenceEqual(expected2));
                    condition.Signal();
                    System.Diagnostics.Debug.WriteLine(condition.CurrentCount);
                }))
                {
                    await reader.StartAsync();

                    if (!condition0.Wait(1000))
                    {
                        Assert.Fail();
                    }

                    if (!condition.Wait(1000))
                    {
                        Assert.Fail();
                    }   
                }

                await reader.DisposeAsync();
            }
        }

        [TestMethod()]
        public async Task InitializeTest()
        {
            await TestLogFileAsync(new PacketRuleSizeBody());
            await TestLogFileAsync(new PacketRuleFixedLength() { FixedSize = 3 });
            await TestLogFileAsync(new PacketRuleNodivision());
            await TestLogFileAsync(new PacketRuleTerminate() { Terminate = new byte[] { 0xEE, 0xFF } });
        }
    }
}