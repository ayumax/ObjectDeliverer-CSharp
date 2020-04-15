// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ObjectDeliverer.Protocol;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace ObjectDeliverer.DeliveryBox
{
    public class ObjectDeliveryBoxUsingJson<T> : DeliveryBoxBase<T>
    {
        public ObjectDeliveryBoxUsingJson()
        {
        }

        public override ReadOnlyMemory<byte> MakeSendBuffer(T message) => JsonSerializer.SerializeToUtf8Bytes<T>(message);

        public override T BufferToMessage(ReadOnlyMemory<byte> buffer)
        {
            if (buffer.Span[buffer.Length - 1] == 0x00)
            {
                // Remove the terminal null
                return JsonSerializer.Deserialize<T>(buffer.Slice(0, buffer.Length - 1).Span);
            }

            return JsonSerializer.Deserialize<T>(buffer.Span);
        }
    }
}
