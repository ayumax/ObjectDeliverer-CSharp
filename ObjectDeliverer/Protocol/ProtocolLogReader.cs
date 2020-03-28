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
    public class ProtocolLogReader : ObjectDelivererProtocol
    {
        private FileStream? streamReader = null;
        private Stopwatch stopwatch = new Stopwatch();
        private bool isFirst = true;
        private string filePath = "";
        private bool cutFirstInterval = true;
        private GrowBuffer receiveBuffer = new GrowBuffer();
        private PollingTask? pollinger = null;
        private long currentLogTime = 0;
        private long startTime = 0;


        public ProtocolLogReader()
        {

        }

        public void Initialize(string filePath, bool cutFirstInterval)
        {
            this.filePath = filePath;
            this.cutFirstInterval = cutFirstInterval;
        }

        public override async ValueTask StartAsync()
        {
            if (streamReader != null)
            {
                await streamReader.DisposeAsync();
            }

            streamReader = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);

            stopwatch.Restart();
            isFirst = true;
            currentLogTime = -1;

            DispatchConnected(this);

            receiveBuffer = new GrowBuffer();
            pollinger = new PollingTask(OnReceive);
        }

        private async ValueTask<bool> OnReceive()
        {
            if (streamReader == null) return false;

            while (streamReader.RemainSize() > 0 || currentLogTime >= 0)
            {
                if (currentLogTime >= 0)
                {
                    long nowTime = (stopwatch.ElapsedMilliseconds - startTime);

                    if (currentLogTime <= nowTime)
                    {
                        int Size = receiveBuffer.Length;
                        int wantSize = PacketRule.WantSize;

                        if (wantSize > 0)
                        {
                            if (Size < wantSize) return true;
                        }

                        int Offset = 0;
                        while (Size > 0)
                        {
                            wantSize = PacketRule.WantSize;
                            int receiveSize = wantSize == 0 ? Size : wantSize;

                            foreach (var receivedMemory in PacketRule.NotifyReceiveData(receiveBuffer.MemoryBuffer.Slice(Offset, receiveSize)))
                            {
                                DispatchReceiveData(this, receivedMemory);
                            }

                            Offset += receiveSize;
                            Size -= receiveSize;
                        }

                        currentLogTime = -1;
                    }
                    else
                    {
                        break;
                    }
                }

                if (streamReader.RemainSize() < sizeof(double)) return false;

                currentLogTime = (long)await streamReader.ReadDoubleAsync();
                if (isFirst && cutFirstInterval)
                {
                    startTime -= currentLogTime;
                }
                isFirst = false;

                if (streamReader.RemainSize() < sizeof(int)) return false;

                int bufferSize = await streamReader.ReadIntAsync();

                if (streamReader.RemainSize() < bufferSize) return false;

                receiveBuffer.SetBufferSize(bufferSize);

                await streamReader.ReadAsync(receiveBuffer.MemoryBuffer);
            }


            return true;
        }

        public override async ValueTask CloseAsync()
        {
            if (pollinger != null)
            {
                await pollinger.Stop();
                pollinger = null;
            }

            if (streamReader != null)
            {
                await streamReader.DisposeAsync();
                streamReader = null;
            }
        }

        public override ValueTask SendAsync(Memory<byte> dataBuffer)
        {
            return new ValueTask();
        }

    }

}