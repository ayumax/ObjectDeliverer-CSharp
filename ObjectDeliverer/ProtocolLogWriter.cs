// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using ObjectDeliverer.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ObjectDeliverer
{
    public class ProtocolLogWriter : ObjectDelivererProtocol
    {
        private FileStream? streamWriter = null;
        private Stopwatch stopwatch = new Stopwatch();

        public string FilePath { get; set; } = string.Empty;

        public override async ValueTask StartAsync()
        {
            this.streamWriter = new FileStream(this.FilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

            await Task.Delay(1);

            this.stopwatch.Restart();

            this.DispatchConnected(this);
        }

        public override async ValueTask SendAsync(ReadOnlyMemory<byte> dataBuffer)
        {
            if (this.streamWriter == null) return;

            var sendBuffer = this.PacketRule.MakeSendPacket(dataBuffer);

            await this.streamWriter.WriteDoubleAsync((double)this.stopwatch.ElapsedMilliseconds);
            await this.streamWriter.WriteIntAsync(sendBuffer.Length);
            await this.streamWriter.WriteAsync(sendBuffer);
        }

        protected override async ValueTask CloseAsync()
        {
            if (this.streamWriter != null)
            {
                await this.streamWriter.DisposeAsync();
                this.streamWriter = null;
            }
        }
    }
}
