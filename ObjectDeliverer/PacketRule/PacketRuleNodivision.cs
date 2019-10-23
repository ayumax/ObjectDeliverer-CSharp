using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ObjectDeliverer.PacketRule
{
    public class PacketRuleNodivision : PacketRuleBase
    {
        public override void Initialize()
        {
            
        }

        public override Memory<byte> MakeSendPacket(Memory<byte> bodyBuffer)
        {
            return bodyBuffer;
        }

        public override IEnumerable<Memory<byte>> NotifyReceiveData(Memory<byte> dataBuffer)
        {
            yield return dataBuffer.ToArray();
        }

        public override int WantSize => 0;


        public override PacketRuleBase Clone() => PacketRuleFactory.CreatePacketRuleNodivision();
        
    }
}