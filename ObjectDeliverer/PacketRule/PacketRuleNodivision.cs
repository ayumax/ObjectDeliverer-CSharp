using System;
using System.Collections.Generic;

namespace ObjectDeliverer.PacketRule
{
    public class PacketRuleNodivision : PacketRuleBase
    {
        public override void Initialize()
        {

        }

        public override void MakeSendPacket(byte[] bodyBuffer)
        {
            DispatchMadeSendBuffer(bodyBuffer);
        }

        public override void NotifyReceiveData(byte[] dataBuffer)
        {
            DispatchMadeReceiveBuffer(dataBuffer);
        }

        public override int WantSize => 0;


        public override PacketRuleBase Clone() => PacketRuleFactory::CreatePacketRuleNodivision();
        
    }
}