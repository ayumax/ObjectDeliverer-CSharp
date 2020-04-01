using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectDeliverer.DeliveryBox;
using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDeliverer.DeliveryBox.Tests
{
    [TestClass()]
    public class Utf8StringDeliveryBoxTests
    {
        [TestMethod()]
        public void ConvertBufferTest()
        {
            string checkString = "ABCDEFG_012345_@!#$%&\r\nZXCVB";

            var deliveryBox = new Utf8StringDeliveryBox();

            var buffer = deliveryBox.MakeSendBuffer(checkString);

            Assert.AreEqual(deliveryBox.BufferToMessage(buffer), checkString);
        }
    }
}