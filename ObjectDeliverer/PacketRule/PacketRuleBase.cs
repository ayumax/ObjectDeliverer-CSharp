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


        public abstract ReadOnlyMemory<byte> MakeSendPacket(ReadOnlyMemory<byte> bodyBuffer);

        public abstract IEnumerable<ReadOnlyMemory<byte>> NotifyReceiveData(ReadOnlyMemory<byte> dataBuffer);

    }
}
