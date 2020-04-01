// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ObjectDeliverer.Protocol;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ObjectDeliverer.DeliveryBox
{
    public abstract class DeliveryBoxBase<T>
    {
        public DeliveryBoxBase()
        {
        }

        public abstract ReadOnlyMemory<byte> MakeSendBuffer(T message);

        public abstract T BufferToMessage(ReadOnlyMemory<byte> buffer);
    }
}
