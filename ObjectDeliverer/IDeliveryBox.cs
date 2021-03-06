// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace ObjectDeliverer
{
    public interface IDeliveryBox<T>
    {
        ReadOnlyMemory<byte> MakeSendBuffer(T message);

        T BufferToMessage(ReadOnlyMemory<byte> buffer);
    }
}
