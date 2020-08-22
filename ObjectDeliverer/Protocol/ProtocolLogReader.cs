// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ObjectDeliverer.Protocol.IP;
using ObjectDeliverer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObjectDeliverer.Protocol
{
    public class ProtocolLogReader : ObjectDelivererProtocol
    {
        private FileStream? streamReader = null;
        private Stopwatch stopwatch = new Stopwatch();
        private bool isFirst = true;
        private GrowBuffer receiveBuffer = new GrowBuffer();
        private PollingTask? pollinger = null;
        private long currentLogTime = 0;
        private long startTime = 0;

        public ProtocolLogReader()
        {
        }

        public string FilePath { get; set; } = string.Empty;

        public bool CutFirstInterval { get; set; } = true;

        public override async ValueTask StartAsync()
        {
            this.streamReader = new FileStream(this.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);

            this.isFirst = true;
            this.currentLogTime = -1;

            await Task.Delay(1);

            this.stopwatch.Restart();

            this.DispatchConnected(this);

            this.receiveBuffer = new GrowBuffer();
            this.pollinger = new PollingTask(this.OnReceive);
        }

        public override ValueTask SendAsync(ReadOnlyMemory<byte> dataBuffer)
        {
            return default(ValueTask);
        }

        protected override async ValueTask CloseAsync()
        {
            if (this.pollinger != null)
            {
                await this.pollinger.DisposeAsync();
                this.pollinger = null;
            }

            if (this.streamReader != null)
            {
                await this.streamReader.DisposeAsync();
                this.streamReader = null;
            }
        }

        private async ValueTask<bool> OnReceive()
        {
            if (this.streamReader == null) return false;

            while (this.streamReader.RemainSize() > 0 || this.currentLogTime >= 0)
            {
                if (this.currentLogTime >= 0)
                {
                    long nowTime = this.stopwatch.ElapsedMilliseconds - this.startTime;

                    if (this.currentLogTime <= nowTime)
                    {
                        int size = this.receiveBuffer.Length;
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

                            foreach (var receivedMemory in this.PacketRule.MakeReceivedPacket(this.receiveBuffer.MemoryBuffer.Slice(offset, receiveSize)))
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

                        this.currentLogTime = -1;
                    }
                    else
                    {
                        break;
                    }
                }

                if (this.streamReader.RemainSize() < sizeof(double))
                {
                    this.DispatchDisconnected(this);
                    return false;
                }

                this.currentLogTime = (long)await this.streamReader.ReadDoubleAsync();
                if (this.isFirst && this.CutFirstInterval)
                {
                    this.startTime -= this.currentLogTime;
                }

                this.isFirst = false;

                if (this.streamReader.RemainSize() < sizeof(int))
                {
                    this.DispatchDisconnected(this);
                    return false;
                }

                int bufferSize = await this.streamReader.ReadIntAsync();

                if (this.streamReader.RemainSize() < bufferSize)
                {
                    this.DispatchDisconnected(this);
                    return false;
                }

                this.receiveBuffer.SetBufferSize(bufferSize);

                await this.streamReader.ReadAsync(this.receiveBuffer.MemoryBuffer);
            }

            return true;
        }
    }
}
