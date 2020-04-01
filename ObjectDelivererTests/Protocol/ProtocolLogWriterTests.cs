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
        [TestMethod()]
        public async Task InitializeTest()
        {
            var tempFilePath = System.IO.Path.GetTempFileName();

            {
                CountdownEvent condition0 = new CountdownEvent(1);

                var sender = new ProtocolLogWriter()
                {
                    FilePath = tempFilePath,
                };
                sender.SetPacketRule(new PacketRuleFixedLength() { FixedSize = 3 });
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

                await sender.CloseAsync();
            }

            {
                CountdownEvent condition0 = new CountdownEvent(1);
                var reader = new ProtocolLogReader()
                {
                    FilePath = tempFilePath,
                    CutFirstInterval = true,
                };
                reader.SetPacketRule(new PacketRuleFixedLength() { FixedSize = 3 });
                reader.Connected.Subscribe(x => condition0.Signal());

                using (var condition = new CountdownEvent(100))
                using (reader.ReceiveData.Subscribe(x =>
                {
                    var expected2 = new byte[] { (byte)condition.CurrentCount, 2, 3 };
                    Assert.IsTrue(x.Buffer.ToArray().SequenceEqual(expected2));
                    condition.Signal();
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

                    await reader.CloseAsync();
                }
            }
        }
    }
}