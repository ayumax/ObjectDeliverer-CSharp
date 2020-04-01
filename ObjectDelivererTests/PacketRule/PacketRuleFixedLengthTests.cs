using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectDeliverer.PacketRule;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ObjectDeliverer.PacketRule.Tests
{
    [TestClass()]
    public class PacketRuleFixedLengthTests
    {
        [TestMethod()]
        public void MakeSendPacketTest()
        {
            // If the transmit buffer size and the fixed size are the same
            {
                var packetRule = new PacketRuleFixedLength() { FixedSize = 10 };
                packetRule.Initialize();

                var expected = Enumerable.Range(1, 10).Select(x => (byte)x).ToArray();
                var actual = packetRule.MakeSendPacket(expected);

                Assert.AreEqual(actual.Length, packetRule.FixedSize);
                Assert.IsTrue(actual.ToArray().SequenceEqual(expected));
            }

            // When the transmit buffer size is less than the fixed size
            {
                var packetRule = new PacketRuleFixedLength() { FixedSize = 20 };
                packetRule.Initialize();

                var expected = Enumerable.Range(1, 10).Select(x => (byte)x).ToArray();
                var actual = packetRule.MakeSendPacket(expected);

                Assert.AreEqual(actual.Length, packetRule.FixedSize);
                Assert.IsTrue(actual.Slice(0, 10).ToArray().SequenceEqual(expected));
                Assert.IsTrue(actual.Slice(10, 10).ToArray().SequenceEqual(Enumerable.Repeat(0, 10).Select(x => (byte)x).ToArray()));
            }
        }

        [TestMethod()]
        public void MakeReceivedPacketTest()
        {
            {
                var packetRule = new PacketRuleFixedLength() { FixedSize = 10 };
                packetRule.Initialize();

                var expected = Enumerable.Range(1, 10).Select(x => (byte)x).ToArray();
                var actual = packetRule.MakeReceivedPacket(expected);

                Assert.AreEqual(actual.Count(), 1);
                Assert.AreEqual(actual.FirstOrDefault().Length, packetRule.FixedSize);
                Assert.IsTrue(actual.FirstOrDefault().ToArray().SequenceEqual(expected));
            }

            {
                var packetRule = new PacketRuleFixedLength() { FixedSize = 20 };
                packetRule.Initialize();

                var expected = Enumerable.Range(1, 10).Select(x => (byte)x).ToArray();
                var actual = packetRule.MakeReceivedPacket(expected);

                Assert.AreEqual(actual.Count(), 0);
            }

            {
                var packetRule = new PacketRuleFixedLength() { FixedSize = 10 };
                packetRule.Initialize();

                var expected = Enumerable.Range(1, 20).Select(x => (byte)x).ToArray();
                var actual = packetRule.MakeReceivedPacket(expected);

                Assert.AreEqual(actual.Count(), 0);
            }
        }
    }
}