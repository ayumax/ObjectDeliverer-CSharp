using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ObjectDeliverer.PacketRule
{
    public class PacketRuleNodivision : PacketRuleBase
    {
        public override async ValueTask MakeSendPacket(Memory<byte> bodyBuffer)
        {
            await DispatchMadeSendBuffer(bodyBuffer);
        }

        public override async ValueTask NotifyReceiveData(Memory<byte> dataBuffer)
        {
            await DispatchMadeReceiveBuffer(dataBuffer);
        }

        public override int WantSize => 0;


        public override PacketRuleBase Clone() => PacketRuleFactory.CreatePacketRuleNodivision();
        
    }
}