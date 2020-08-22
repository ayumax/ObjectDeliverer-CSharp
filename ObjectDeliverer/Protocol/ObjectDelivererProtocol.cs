// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ObjectDeliverer.PacketRule;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace ObjectDeliverer.Protocol
{
    public abstract class ObjectDelivererProtocol : IAsyncDisposable
    {
        private bool disposedValue = false;
        private Subject<ConnectedData> connected = new Subject<ConnectedData>();
        private Subject<ConnectedData> disconnected = new Subject<ConnectedData>();
        private Subject<DeliverData> receiveData = new Subject<DeliverData>();

        public ObjectDelivererProtocol()
        {
        }

        public IObservable<ConnectedData> Connected => this.connected;

        public IObservable<ConnectedData> Disconnected => this.disconnected;

        public IObservable<DeliverData> ReceiveData => this.receiveData;

        protected PacketRuleBase PacketRule { get; set; } = new PacketRuleNodivision();

        public abstract ValueTask StartAsync();

        public abstract ValueTask SendAsync(ReadOnlyMemory<byte> dataBuffer);

        public void SetPacketRule(PacketRuleBase packetRule)
        {
            this.PacketRule = packetRule;
            packetRule.Initialize();
        }

        public async ValueTask DisposeAsync()
        {
            this.connected.Dispose();
            this.disconnected.Dispose();
            this.receiveData.Dispose();

            if (!this.disposedValue)
            {
                await this.CloseAsync();
                this.disposedValue = true;
            }
        }

        protected abstract ValueTask CloseAsync();

        protected virtual void DispatchConnected(ObjectDelivererProtocol delivererProtocol)
        {
            this.connected.OnNext(new ConnectedData() { Target = delivererProtocol });
        }

        protected virtual void DispatchDisconnected(ObjectDelivererProtocol delivererProtocol)
        {
            this.disconnected.OnNext(new ConnectedData() { Target = delivererProtocol });
        }

        protected virtual void DispatchReceiveData(DeliverData deliverData)
        {
            this.receiveData.OnNext(deliverData);
        }
    }
}
