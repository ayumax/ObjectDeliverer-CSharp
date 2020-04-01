// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ObjectDeliverer.Protocol.IP;
using ObjectDeliverer.Utils;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ObjectDeliverer.Protocol
{
    public class ProtocolSharedMemory : ObjectDelivererProtocol
    {
        private MutexLocker? sharedMemoryMutex;

        private int sharedMemoryTotalSize = 0;
        private byte nowCounter = 0;

        private MemoryMappedFile? sharedMenmory;
        private MemoryMappedViewStream? sharedMemoryStream;

        private Memory<byte> receiveBuffer = default(Memory<byte>);
        private PollingTask? pollinger;

        public string SharedMemoryName { get; set; } = "SharedMemory";

        public int SharedMemorySize { get; set; } = 1024;

        public override async ValueTask StartAsync()
        {
            string mutexName = this.SharedMemoryName + "MUTEX";
            this.sharedMemoryTotalSize = this.SharedMemorySize + sizeof(byte) + sizeof(int);

            this.sharedMemoryMutex = new MutexLocker(mutexName);

            if (this.sharedMemoryMutex == null) return;

            this.sharedMenmory = MemoryMappedFile.CreateOrOpen(this.SharedMemoryName, this.sharedMemoryTotalSize, MemoryMappedFileAccess.ReadWrite);
            this.sharedMemoryStream = this.sharedMenmory.CreateViewStream();

            await this.sharedMemoryMutex.LockAsync(() => this.sharedMemoryStream.WriteAsync(new byte[this.sharedMemoryTotalSize]));

            this.nowCounter = 0;
            this.receiveBuffer = new byte[this.SharedMemorySize];

            this.pollinger = new PollingTask(this.OnReceive);

            this.DispatchConnected(this);
        }

        public override async ValueTask CloseAsync()
        {
            this.sharedMemoryMutex?.LockAsync(async () =>
                {
                    if (this.pollinger == null) return;
                    await this.pollinger.DisposeAsync();
                });

            this.sharedMemoryMutex?.Dispose();
            this.sharedMemoryMutex = null;

            if (this.sharedMemoryStream != null)
            {
                await this.sharedMemoryStream.DisposeAsync();
                this.sharedMemoryStream = null;
            }

            this.sharedMenmory?.Dispose();
            this.sharedMenmory = null;
        }

        public override ValueTask SendAsync(ReadOnlyMemory<byte> dataBuffer)
        {
            var sendBuffer = this.PacketRule.MakeSendPacket(dataBuffer);

            if (sendBuffer.Length > this.SharedMemorySize) return default(ValueTask);

            if (this.sharedMemoryMutex == null || this.sharedMemoryStream == null) return default(ValueTask);

            int writeSize = sendBuffer.Length;

            return this.sharedMemoryMutex.LockAsync(() =>
            {
                this.nowCounter++;
                if (this.nowCounter == 0)
                {
                    this.nowCounter = 1;
                }

                this.sharedMemoryStream.Position = 0;
                this.sharedMemoryStream.WriteByte(this.nowCounter);

                this.sharedMemoryStream.Write(BitConverter.GetBytes(sendBuffer.Length));

                return this.sharedMemoryStream.WriteAsync(sendBuffer);
            });
        }

        private async ValueTask<bool> OnReceive()
        {
            if (this.sharedMemoryMutex == null || this.sharedMemoryStream == null) return false;

            int size = 0;

            await this.sharedMemoryMutex.LockAsync(async () =>
            {
                this.sharedMemoryStream.Position = 0;

                byte counter = (byte)this.sharedMemoryStream.ReadByte();
                if (counter == this.nowCounter) return;

                this.nowCounter = counter;

                byte[] sizeBuffer = new byte[sizeof(int)];

                this.sharedMemoryStream.Read(sizeBuffer);

                size = BitConverter.ToInt32(sizeBuffer);

                if (size == 0 || size > this.receiveBuffer.Length) return;

                await this.sharedMemoryStream.ReadAsync(this.receiveBuffer);
            });

            if (size == 0) return true;

            int wantSize = this.PacketRule.WantSize;

            if (wantSize > 0)
            {
                if (size < wantSize) return true;
            }

            int offset = 0;
            while (size > 0)
            {
                wantSize = this.PacketRule.WantSize;
                int receiveSize = wantSize == 0 ? size : wantSize;

                foreach (var receivedMemory in this.PacketRule.MakeReceivedPacket(this.receiveBuffer.Slice(offset, receiveSize)))
                {
                    this.DispatchReceiveData(new DeliverData()
                    {
                        Sender = this,
                        Buffer = receivedMemory,
                    });
                }

                offset += receiveSize;
                size -= receiveSize;
            }

            return true;
        }
    }
}
