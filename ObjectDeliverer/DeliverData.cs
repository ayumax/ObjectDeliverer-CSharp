using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDeliverer
{
    public class DeliverData
    {
        public object Sender { get; set; }
        public ReadOnlyMemory<byte> Data { get; set; }

        public DeliverData()
        {
            Sender = new object();
            Data = new byte[0];
        }
    }
}
