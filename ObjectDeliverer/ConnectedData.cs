// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ObjectDeliverer.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDeliverer
{
    public class ConnectedData
    {
        public ConnectedData()
        {
        }

        public ObjectDelivererProtocol Target { get; set; } = null!;
    }
}
