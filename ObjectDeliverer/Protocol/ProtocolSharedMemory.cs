using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ObjectDeliverer.Protocol.IP;
using ObjectDeliverer.Utils;

namespace ObjectDeliverer.Protocol
{
    public class ProtocolSharedMemory : ObjectDelivererProtocol
    {
        private string sharedMemoryName = "";
        private int sharedMemorySize = 1024;

        private MutexLocker? sharedMemoryMutex;

        private int sharedMemoryTotalSize = 0;
        private byte nowCounter = 0;

        private MemoryMappedFile? sharedMenmory;
        private MemoryMappedViewStream? sharedMemoryStream;

        private Memory<byte> receiveBuffer = new Memory<byte>();
        private PollingTask? pollinger;

        public void Initialize(string sharedMemoryName = "SharedMemory", int sharedMemorySize = 1024)
        {
            this.sharedMemoryName = sharedMemoryName;
            this.sharedMemorySize = sharedMemorySize;
        }

        public override async ValueTask StartAsync()
        {
            string mutexName = sharedMemoryName + "MUTEX";
            sharedMemoryTotalSize = sharedMemorySize + sizeof(byte) + sizeof(int);

            sharedMemoryMutex = new MutexLocker(mutexName);

            if (sharedMemoryMutex == null) return;

            sharedMenmory = MemoryMappedFile.CreateOrOpen(sharedMemoryName, sharedMemoryTotalSize, MemoryMappedFileAccess.ReadWrite);
            sharedMemoryStream = sharedMenmory.CreateViewStream();

            await sharedMemoryMutex.LockAsync(() => sharedMemoryStream.WriteAsync(new byte[sharedMemoryTotalSize]));


            nowCounter = 0;
            receiveBuffer = new byte[sharedMemorySize];

            pollinger = new PollingTask(OnReceive);

            DispatchConnected(this);
        }

        public override async ValueTask CloseAsync()
        {
            sharedMemoryMutex?.LockAsync(async () =>
                {
                    if (pollinger == null) return;
                    await pollinger.Stop();
                });

            sharedMemoryMutex?.Dispose();
            sharedMemoryMutex = null;

            if (sharedMemoryStream != null)
            {
                await sharedMemoryStream.DisposeAsync();
                sharedMemoryStream = null;
            }

            sharedMenmory?.Dispose();
            sharedMenmory = null;
        }

        private async ValueTask<bool> OnReceive()
        {
            if (sharedMemoryMutex == null || sharedMemoryStream == null) return false;

            int Size = 0;

            await sharedMemoryMutex.LockAsync(async () =>
            {
                sharedMemoryStream.Position = 0;

                byte counter = (byte)sharedMemoryStream.ReadByte();
                if (counter == nowCounter) return;

                nowCounter = counter;

                byte[] sizeBuffer = new byte[sizeof(int)];

                sharedMemoryStream.Read(sizeBuffer);

                Size = BitConverter.ToInt32(sizeBuffer);

                if (Size == 0 || Size != receiveBuffer.Length) return;

                await sharedMemoryStream.ReadAsync(receiveBuffer);
            });

            if (Size == 0) return true;

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

                Offset += receiveSize;
                Size -= receiveSize;

                PacketRule.NotifyReceiveData(receiveBuffer.Slice(Offset, receiveSize));
            }

            return true;
        }

        public override ValueTask SendAsync(Memory<byte> dataBuffer)
        {
            return PacketRule.MakeSendPacket(dataBuffer);
        }

        public override ValueTask RequestSendAsync(Memory<byte> dataBuffer)
        {
            if (dataBuffer.Length > sharedMemorySize) return new ValueTask();

            if (sharedMemoryMutex == null || sharedMemoryStream == null) return new ValueTask();

            int writeSize = dataBuffer.Length;

            return sharedMemoryMutex.LockAsync(() =>
            {
                nowCounter++;
                if (nowCounter == 0)
                {
                    nowCounter = 1;
                }

                sharedMemoryStream.Position = 0;
                sharedMemoryStream.WriteByte(nowCounter);

                sharedMemoryStream.Write(BitConverter.GetBytes(dataBuffer.Length));

                return sharedMemoryStream.WriteAsync(dataBuffer);
            });
        }
    }

}


