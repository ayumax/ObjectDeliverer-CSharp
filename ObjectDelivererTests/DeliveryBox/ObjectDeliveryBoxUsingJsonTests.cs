using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectDeliverer.DeliveryBox;
using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDeliverer.DeliveryBox.Tests
{
    public class TestObject
    {
        public int IntProperty { get; set; }
        public string StringProperty { get; set; }
        public float FloatProperty { get; set; }
    }

    [TestClass()]
    public class ObjectDeliveryBoxUsingJsonTests
    {
        [TestMethod()]
        public void ConvertBufferTest()
        {
            {
                var deliveryBox = new ObjectDeliveryBoxUsingJson<TestObject>();

                var testObj = new TestObject()
                {
                    IntProperty = 10,
                    StringProperty = "TEST",
                    FloatProperty = 3.14f
                };

                var buffer = deliveryBox.MakeSendBuffer(testObj);
                var deserializedObj = deliveryBox.BufferToMessage(buffer);

                Assert.AreEqual(10, deserializedObj.IntProperty);
                Assert.AreEqual("TEST", deserializedObj.StringProperty);
                Assert.AreEqual(3.14f, deserializedObj.FloatProperty);
            }
           
        }
    }
}