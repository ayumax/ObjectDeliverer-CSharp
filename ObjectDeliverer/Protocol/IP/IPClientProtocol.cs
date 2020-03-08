using System;
using System.Threading;
using System.Threading.Tasks;

namespace ObjectDeliverer.Protocol.IP
{
    public abstract class IPClientProtocol : IDisposable
    {
        public abstract ValueTask WriteAsync(ReadOnlyMemory<byte> buffer);
        public abstract ValueTask<int> ReadAsync(Memory<byte> buffer);

        public abstract int Available { get; }
        public abstract bool IsEnable { get; }

        public abstract Task ConnectAsync(string host, int port);
        public abstract void Close();


        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Close();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
