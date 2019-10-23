using ObjectDeliverer.Protocol;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ObjectDeliverer.PacketRule
{
	public abstract class PacketRuleBase
    {
        public enum ECNBufferEndian
        {
            /** Big Endian */
            Big = 0,
	        /** Little Endian */
	        Little
        };

		public abstract int WantSize { get; }

		public abstract PacketRuleBase Clone();

        public abstract void Initialize();


        public abstract Memory<byte> MakeSendPacket(Memory<byte> bodyBuffer);

        public abstract IEnumerable<Memory<byte>> NotifyReceiveData(Memory<byte> dataBuffer);

    }
}
