using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectDeliverer.PacketRule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectDeliverer.PacketRule.Tests
{
    [TestClass()]
    public class PacketRuleNodivisionTests
    {
        [TestMethod()]
        public void MakeSendPacketTest()
        {
            {
                var packetRule = new PacketRuleNodivision();
                packetRule.Initialize();

                var expected = Enumerable.Range(1, 10).Select(x => (byte)x).ToArray();
                var actual = packetRule.MakeSendPacket(expected);

                Assert.IsTrue(actual.ToArray().SequenceEqual(expected));
            }
        }

        [TestMethod()]
        public void MakeReceivedPacketTest()
        {
            {
                var packetRule = new PacketRuleNodivision();
                packetRule.Initialize();

                var expected = Enumerable.Range(1, 10).Select(x => (byte)x).ToArray();
                var actual = packetRule.MakeReceivedPacket(expected).ToList();

                Assert.AreEqual(actual.Count(), 1);
                Assert.IsTrue(actual.FirstOrDefault().ToArray().SequenceEqual(expected));
            }
        }
    }
}