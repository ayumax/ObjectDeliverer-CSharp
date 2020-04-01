// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ObjectDeliverer.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ObjectDeliverer.DeliveryBox
{
    public class Utf8StringDeliveryBox : DeliveryBoxBase<string>
    {
        public override ReadOnlyMemory<byte> MakeSendBuffer(string message) => UTF8Encoding.UTF8.GetBytes(message);

        public override string BufferToMessage(ReadOnlyMemory<byte> buffer) => UTF8Encoding.UTF8.GetString(buffer.Span);
    }
}
