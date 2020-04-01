using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectDeliverer.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDeliverer.Utils.Tests
{
    [TestClass()]
    public class GrowBufferTests
    {
        [TestMethod()]
        public void SetBufferSizeTest()
        {
            const int packetSize = 1024;

            {
                var buffer = new GrowBuffer(100);

                Assert.AreEqual(buffer.Length, 100);
                Assert.AreEqual(buffer.InnerBufferSize, packetSize);

                buffer.SetBufferSize(2000);

                Assert.AreEqual(buffer.Length, 2000);
                Assert.AreEqual(buffer.InnerBufferSize, packetSize * 2);

                buffer.Add(new byte[] { 1, 2, 3 });

                Assert.AreEqual(buffer.Length, 2003);
                Assert.AreEqual(buffer.InnerBufferSize, packetSize * 2);
                Assert.AreEqual(buffer[2000], 1);
                Assert.AreEqual(buffer[2001], 2);
                Assert.AreEqual(buffer[2002], 3);

                buffer.RemoveRangeFromStart(0, 2000);
                Assert.AreEqual(buffer.Length, 3);
                Assert.AreEqual(buffer.InnerBufferSize, packetSize * 2);
                Assert.AreEqual(buffer[0], 1);
                Assert.AreEqual(buffer[1], 2);
                Assert.AreEqual(buffer[2], 3);

                buffer.CopyFrom(new byte[] { 0xEE, 0xFF }, 1);
                Assert.AreEqual(buffer.Length, 3);
                Assert.AreEqual(buffer.InnerBufferSize, packetSize * 2);
                Assert.AreEqual(buffer[0], 1);
                Assert.AreEqual(buffer[1], 0xEE);
                Assert.AreEqual(buffer[2], 0xFF);
            }
        }
    }
}