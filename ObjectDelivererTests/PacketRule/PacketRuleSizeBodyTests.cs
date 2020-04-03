using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectDeliverer.PacketRule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectDeliverer.PacketRule.Tests
{
    [TestClass()]
    public class PacketRuleSizeBodyTests
    {
        [TestMethod()]
        public void MakeSendPacketTest()
        {
            // size is big endian
            {
                var packetRule = new PacketRuleSizeBody()
                {
                    SizeBufferEndian = ECNBufferEndian.Big,
                    SizeLength = 4,
                };

                packetRule.Initialize();

                var expected = Enumerable.Range(1, 10).Select(x => (byte)x).ToArray();
                var actual = packetRule.MakeSendPacket(expected);

                Assert.AreEqual(actual.Length, expected.Length + packetRule.SizeLength);

                int actualSize = actual.Span[0] << 24 | actual.Span[1] << 16 | actual.Span[2] << 8 | actual.Span[3];
                Assert.AreEqual(actualSize, expected.Length);

                Assert.IsTrue(actual.Slice(4).ToArray().SequenceEqual(expected));
            }

            // size is little endian
            {
                var packetRule = new PacketRuleSizeBody()
                {
                    SizeBufferEndian = ECNBufferEndian.Little,
                    SizeLength = 4,
                };

                packetRule.Initialize();

                var expected = Enumerable.Range(1, 10).Select(x => (byte)x).ToArray();
                var actual = packetRule.MakeSendPacket(expected);

                Assert.AreEqual(actual.Length, expected.Length + packetRule.SizeLength);

                int actualSize = actual.Span[3] << 24 | actual.Span[2] << 16 | actual.Span[1] << 8 | actual.Span[0];
                Assert.AreEqual(actualSize, expected.Length);

                Assert.IsTrue(actual.Slice(4).ToArray().SequenceEqual(expected));
            }
        }

        [TestMethod()]
        public void MakeReceivedPacketTest()
        {
            // size is big endian
            {
                var packetrule = new PacketRuleSizeBody()
                {
                    SizeBufferEndian = ECNBufferEndian.Big,
                    SizeLength = 4,
                };
                packetrule.Initialize();

                Assert.AreEqual(packetrule.WantSize, packetrule.SizeLength);

                var expectedSizeBuffer = new byte[] { 0, 0, 0, 10 };
            
                var actual0 = packetrule.MakeReceivedPacket(expectedSizeBuffer);

                Assert.AreEqual(actual0.Count(), 0);
                Assert.AreEqual(packetrule.WantSize, 10);

                var expected = Enumerable.Range(1, 10).Select(x => (byte)x).ToArray();

                var actual = packetrule.MakeReceivedPacket(expected).ToList();

                Assert.AreEqual(actual.Count(), 1);

                Assert.IsTrue(actual.FirstOrDefault().ToArray().SequenceEqual(expected));
            }

            // size is little endian
            {
                var packetrule = new PacketRuleSizeBody()
                {
                    SizeBufferEndian = ECNBufferEndian.Little,
                    SizeLength = 4,
                };
                packetrule.Initialize();

                Assert.AreEqual(packetrule.WantSize, packetrule.SizeLength);

                var expectedSizeBuffer = new byte[] { 10, 0, 0, 0 };

                var actual0 = packetrule.MakeReceivedPacket(expectedSizeBuffer);

                Assert.AreEqual(actual0.Count(), 0);
                Assert.AreEqual(packetrule.WantSize, 10);

                var expected = Enumerable.Range(1, 10).Select(x => (byte)x).ToArray();

                var actual = packetrule.MakeReceivedPacket(expected).ToList();

                Assert.AreEqual(actual.Count(), 1);

                Assert.IsTrue(actual.FirstOrDefault().ToArray().SequenceEqual(expected));
            }
        }
    }
}