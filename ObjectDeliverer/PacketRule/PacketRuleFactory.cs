using System;
using System.Collections.Generic;

namespace ObjectDeliverer.PacketRule
{
    public class PacketRuleBase
    {
        public static PacketRuleFixedLength CreatePacketRuleFixedLength(int FixedSize) => new PacketRuleFixedLength() { FixedSize = FixedSize };

        public static PacketRuleSizeBody CreatePacketRuleSizeBody(int SizeLength, ECNBufferEndian SizeBufferEndian)
        {
            return new PacketRuleSizeBody()
            {
                SizeLength = SizeLength,
                SizeBufferEndian = SizeBufferEndian
            };

        }

        public static PacketRuleTerminate CreatePacketRuleTerminate(byte[] Terminate) => new PacketRuleTerminate() { Terminate = Terminate };

        public static PacketRuleNodivision CreatePacketRuleNodivision() => new PacketRuleNodivision();
    }
}