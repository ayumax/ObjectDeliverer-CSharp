// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ObjectDeliverer.Protocol.IP
{
    public abstract class IPProtocolHelper : IDisposable
    {
        private bool disposedValue = false;

        public abstract int Available { get; }

        public abstract bool IsEnable { get; }

        public abstract ValueTask WriteAsync(ReadOnlyMemory<byte> buffer);

        public abstract ValueTask<int> ReadAsync(Memory<byte> buffer);

        public abstract ValueTask<(byte[] Buffer, EndPoint? RemoteEndPoint)> ReceiveAsync();

        public abstract Task ConnectAsync(string host, int port);

        public abstract void Close();

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.Close();
                }

                this.disposedValue = true;
            }
        }
    }
}
