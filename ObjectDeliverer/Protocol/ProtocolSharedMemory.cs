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
        private int nowCounter = 0;

        private MemoryMappedFile? sharedMenmory;
        private MemoryMappedViewStream? sharedMemoryStream;

        private byte[]? receiveBuffer;
        private PollingTask? pollinger;

        public void Initialize(string sharedMemoryName = "SharedMemory", int sharedMemorySize = 1024)
        {
            this.sharedMemoryName = sharedMemoryName;
            this.sharedMemorySize = sharedMemorySize;
        }

        public override async ValueTask StartAsync()
        {
            /*  Create a named mutex for inter-process protection of data */
            string mutexName = sharedMemoryName + "MUTEX";
            sharedMemoryTotalSize = sharedMemorySize + sizeof(byte) + sizeof(int);

            sharedMemoryMutex = new MutexLocker(mutexName);

            if (sharedMemoryMutex == null) return;

            sharedMenmory = MemoryMappedFile.CreateOrOpen(sharedMemoryName, sharedMemoryTotalSize, MemoryMappedFileAccess.ReadWrite);
            sharedMemoryStream = sharedMenmory.CreateViewStream();

            await sharedMemoryMutex.LockAsync(() => sharedMemoryStream.WriteAsync(new byte[sharedMemoryTotalSize]));


            nowCounter = 0;
            receiveBuffer = new byte[sharedMemoryTotalSize];

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

        private void OnReceive()
        {
            int Size = 0;

            sharedMemoryMutex?.Lock(() =>
            {
                sharedMemoryStream.Position = 0;

                byte counter = (byte)sharedMemoryStream.ReadByte();
                if (counter == nowCounter) return;

                nowCounter = counter;

                FMemory::Memcpy(&Size, SharedMemoryData + sizeof(uint8), sizeof(uint32));

                TempBuffer.SetNum(Size, false);

                FMemory::Memcpy(TempBuffer.GetData(), SharedMemoryData + sizeof(uint8) + sizeof(uint32), FMath::Min((uint32)SharedMemorySize, Size));
            });

            if (Size == 0) return true;

            uint32 wantSize = PacketRule->GetWantSize();

            if (wantSize > 0)
            {
                if (Size < wantSize) return true;
            }

            int32 Offset = 0;
            while (Size > 0)
            {
                wantSize = PacketRule->GetWantSize();
                auto receiveSize = wantSize == 0 ? Size : wantSize;

                ReceiveBuffer.SetNum(receiveSize, false);

                int32 Read = 0;
                FMemory::Memcpy(ReceiveBuffer.GetData(), TempBuffer.GetData() + Offset, receiveSize);
                Offset += receiveSize;
                Size -= receiveSize;

                PacketRule->NotifyReceiveData(ReceiveBuffer);
            }
            return true;
        }
    }

}



//void UProtocolSharedMemory::Send(const TArray<uint8>& DataBuffer) const
//{
//#if PLATFORM_WINDOWS
//	if (!SharedMemoryHandle) return;

//	PacketRule->MakeSendPacket(DataBuffer);
//#endif
//}

//void UProtocolSharedMemory::RequestSend(const TArray<uint8>& DataBuffer)
//{
//#if PLATFORM_WINDOWS
//	if (DataBuffer.Num() > SharedMemorySize)
//		return;

//	if (SharedMemoryMutex && SharedMemoryData)
//	{
//		int32 writeSize = DataBuffer.Num();

//		MutexLock::Lock(SharedMemoryMutex, [this, writeSize, &DataBuffer]()
//		{
//			NowCounter++;
//			if (NowCounter == 0)
//			{
//				NowCounter = 1;
//			}

//			FMemory::Memcpy(SharedMemoryData, &NowCounter, sizeof(uint8));

//			FMemory::Memcpy(SharedMemoryData + sizeof(uint8), &writeSize, sizeof(int32));
//			FMemory::Memcpy(SharedMemoryData + sizeof(uint8) + sizeof(int32), DataBuffer.GetData(), writeSize);

//			FlushViewOfFile(SharedMemoryData, sizeof(uint8) + sizeof(int32) + writeSize);
//		});

//	}
//#endif
//}
