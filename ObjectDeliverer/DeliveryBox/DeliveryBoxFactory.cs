using ObjectDeliverer.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDeliverer.DeliveryBox
{
    public class DeliveryBoxFactory
    {
        public static ObjectDeliveryBoxUsingJson<T> CreateObjectDeliveryBoxUsingJson<T>() => new ObjectDeliveryBoxUsingJson<T>();

        public static Utf8StringDeliveryBox CreateUtf8StringDeliveryBox() => new Utf8StringDeliveryBox();

    }
}
