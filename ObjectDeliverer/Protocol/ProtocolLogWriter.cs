// Copyright 2019 ayumax. All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Diagnostics;
using ObjectDeliverer.Protocol.IP;
using ObjectDeliverer.Utils;

namespace ObjectDeliverer.Protocol
{
    public class ProtocolLogWriter : ObjectDelivererProtocol
    {
        private string filePath = "";
        private FileStream? streamWriter = null;
        private Stopwatch stopwatch = new Stopwatch();

        public void Initialize(string filePath)
        {
            this.filePath = filePath;
        }

        public override async ValueTask StartAsync()
        {
            if (streamWriter != null)
            {
                await streamWriter.DisposeAsync();
            }

            streamWriter = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

            stopwatch.Restart();

            DispatchConnected(this);
        }

        public override async ValueTask CloseAsync()
        {
            if (streamWriter != null)
            {
                await streamWriter.DisposeAsync();
                streamWriter = null;
            }
        }

        public override ValueTask SendAsync(Memory<byte> dataBuffer)
        {
            return PacketRule.MakeSendPacket(dataBuffer);
        }

        public override async ValueTask RequestSendAsync(Memory<byte> dataBuffer)
        {
            if (streamWriter == null) return;

            await streamWriter.WriteDoubleAsync((double)stopwatch.ElapsedMilliseconds);
            await streamWriter.WriteIntAsync(dataBuffer.Length);
            await streamWriter.WriteAsync(dataBuffer);
        }

    }
}

