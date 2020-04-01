using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectDeliverer.PacketRule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectDeliverer.PacketRule.Tests
{
    [TestClass()]
    public class PacketRuleTerminateTests
    {
        [TestMethod()]
        public void MakeSendPacketTest()
        {
            // terminate is {0xFE, 0xFF}
            {
                var packetRule = new PacketRuleTerminate()
                {
                    Terminate = new byte[] { 0xFE, 0xFF }
                };

                packetRule.Initialize();

                var expected = Enumerable.Range(1, 10).Select(x => (byte)x).ToArray();
                var actual = packetRule.MakeSendPacket(expected);

                Assert.AreEqual(actual.Length, expected.Length + packetRule.Terminate.Length);

                Assert.IsTrue(actual.Slice(0, actual.Length - packetRule.Terminate.Length).ToArray().SequenceEqual(expected));

                Assert.IsTrue(actual.Slice(expected.Length).ToArray().SequenceEqual(packetRule.Terminate));
            }
        }

        [TestMethod()]
        public void MakeReceivedPacketTest()
        {
            // terminate is {0xFE, 0xFF}. just size.
            {
                var packetRule = new PacketRuleTerminate()
                {
                    Terminate = new byte[] { 0xFE, 0xFF }
                };

                packetRule.Initialize();

                var expected = Enumerable.Range(1, 10).Select(x => (byte)x).ToArray();

                var receiveBuffer = new[]
                {
                    expected,
                    packetRule.Terminate
                }
                .SelectMany(x => x.Select(x2 => (byte)x2)).ToArray();

                var actual = packetRule.MakeReceivedPacket(receiveBuffer).ToList();
                Assert.AreEqual(actual.Count, 1);
                Assert.IsTrue(actual.FirstOrDefault().ToArray().SequenceEqual(expected));
            }

            // terminate is {0xFE, 0xFF}. over size.
            {
                var packetRule = new PacketRuleTerminate()
                {
                    Terminate = new byte[] { 0xFE, 0xFF }
                };

                packetRule.Initialize();

                var expected = Enumerable.Range(1, 10).Select(x => (byte)x).ToArray();

                var receiveBuffer = new[]
                {
                    expected,
                    packetRule.Terminate,
                    Enumerable.Range(20, 10).Select(x => (byte)x)
                }
                .SelectMany(x => x.Select(x2 => (byte)x2)).ToArray();

                var actual = packetRule.MakeReceivedPacket(receiveBuffer).ToList();
                Assert.AreEqual(actual.Count, 1);
                Assert.IsTrue(actual.FirstOrDefault().ToArray().SequenceEqual(expected));
            }

            // terminate is {0xFE, 0xFF}. over size.
            {
                var packetRule = new PacketRuleTerminate()
                {
                    Terminate = new byte[] { 0xFE, 0xFF }
                };

                packetRule.Initialize();

                var expected = Enumerable.Range(1, 10).Select(x => (byte)x).ToArray();
                var expected2 = Enumerable.Range(20, 10).Select(x => (byte)x).ToArray();

                var receiveBuffer = new[]
                {
                    expected,
                    packetRule.Terminate,
                    expected2,
                    packetRule.Terminate,
                }
                .SelectMany(x => x.Select(x2 => (byte)x2)).ToArray();

                var actual = packetRule.MakeReceivedPacket(receiveBuffer);

                int count = 0;
                foreach (var packet in actual)
                {
                    switch (count++)
                    {
                        case 0:
                            Assert.IsTrue(packet.ToArray().SequenceEqual(expected));
                            break;

                        case 1:
                            Assert.IsTrue(packet.ToArray().SequenceEqual(expected2));
                            break;
                    }
                }

                Assert.AreEqual(count, 2);     
            }
        }
    }
}