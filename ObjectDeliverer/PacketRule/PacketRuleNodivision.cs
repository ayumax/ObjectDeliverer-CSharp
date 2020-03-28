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

        public override ReadOnlyMemory<byte> MakeSendPacket(ReadOnlyMemory<byte> bodyBuffer)
        {
            return bodyBuffer;
        }

        public override IEnumerable<ReadOnlyMemory<byte>> NotifyReceiveData(ReadOnlyMemory<byte> dataBuffer)
        {
            yield return dataBuffer;
        }

        public override int WantSize => 0;


        public override PacketRuleBase Clone() => PacketRuleFactory.CreatePacketRuleNodivision();
        
    }
}