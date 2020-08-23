// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace ObjectDeliverer
{
    public class ObjectDelivererManager<T> : IAsyncDisposable
    {
        private ObjectDelivererProtocol? currentProtocol;
        private IDeliveryBox<T>? deliveryBox;
        private bool disposedValue = false;

        private Subject<ConnectedData> connected = new Subject<ConnectedData>();
        private Subject<ConnectedData> disconnected = new Subject<ConnectedData>();
        private Subject<DeliverData<T>> receiveData = new Subject<DeliverData<T>>();

        public ObjectDelivererManager()
        {
        }

        public IObservable<ConnectedData> Connected => this.connected;

        public IObservable<ConnectedData> Disconnected => this.disconnected;

        public IObservable<DeliverData<T>> ReceiveData => this.receiveData;

        public bool IsConnected => this.ConnectedList.Count > 0;

        public List<ObjectDelivererProtocol> ConnectedList { get; private set; } = new List<ObjectDelivererProtocol>();

        public static ObjectDelivererManager<T> CreateObjectDelivererManager() => new ObjectDelivererManager<T>();

        public ValueTask StartAsync(ObjectDelivererProtocol protocol, IPacketRule packetRule, IDeliveryBox<T>? deliveryBox = null)
        {
            if (protocol == null || packetRule == null) return default(ValueTask);

            this.currentProtocol = protocol;
            this.currentProtocol.SetPacketRule(packetRule);

            this.deliveryBox = deliveryBox;

            this.currentProtocol.Connected.Subscribe(x =>
            {
                this.ConnectedList.Add(x.Target);
                this.connected.OnNext(x);
            });

            this.currentProtocol.Disconnected.Subscribe(x =>
            {
                this.ConnectedList.Remove(x.Target);
                this.disconnected.OnNext(x);
            });

            this.currentProtocol.ReceiveData.Subscribe(x =>
            {
                var data = new DeliverData<T>()
                {
                    Sender = x.Sender,
                    Buffer = x.Buffer,
                };

                if (deliveryBox != null)
                {
                    data.Message = deliveryBox.BufferToMessage(x.Buffer);
                }

                this.receiveData.OnNext(data);
            });

            this.ConnectedList.Clear();

            return this.currentProtocol.StartAsync();
        }

        public ValueTask SendAsync(ReadOnlyMemory<byte> dataBuffer)
        {
            if (this.currentProtocol == null || this.disposedValue) return default(ValueTask);

            return this.currentProtocol.SendAsync(dataBuffer);
        }

        public ValueTask SendToAsync(ReadOnlyMemory<byte> dataBuffer, ObjectDelivererProtocol? target)
        {
            if (this.currentProtocol == null || this.disposedValue) return default(ValueTask);

            if (target != null)
            {
                return target.SendAsync(dataBuffer);
            }

            return default(ValueTask);
        }

        public ValueTask SendMessageAsync(T message)
        {
            if (this.deliveryBox == null) return default(ValueTask);

            return this.SendAsync(this.deliveryBox.MakeSendBuffer(message));
        }

        public ValueTask SendToMessageAsync(T message, ObjectDelivererProtocol target)
        {
            if (this.deliveryBox == null) return default(ValueTask);

            return this.SendToAsync(this.deliveryBox.MakeSendBuffer(message), target);
        }

        public async ValueTask DisposeAsync()
        {
            if (!this.disposedValue)
            {
                this.disposedValue = true;

                this.connected.Dispose();
                this.disconnected.Dispose();
                this.receiveData.Dispose();

                this.connected = null!;
                this.disconnected = null!;
                this.receiveData = null!;

                if (this.currentProtocol == null) return;

                await this.currentProtocol.DisposeAsync();

                this.currentProtocol = null;
            }
        }
    }
}
