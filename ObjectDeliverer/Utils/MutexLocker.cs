// Copyright (c) 2020 ayuma_x. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace ObjectDeliverer.Utils
{
    public class MutexLocker : IDisposable
    {
        private Mutex mutex;
        private bool disposedValue = false;

        public MutexLocker(string mutexName)
        {
            this.mutex = new Mutex(false, mutexName);
        }

        public void Lock(Action action)
        {
            this.mutex.WaitOne();

            try
            {
                action.Invoke();
            }
            finally
            {
                this.mutex.ReleaseMutex();
            }
        }

        public async ValueTask LockAsync(Func<ValueTask> action)
        {
            this.mutex.WaitOne();

            try
            {
                await action.Invoke();
            }
            finally
            {
                this.mutex.ReleaseMutex();
            }
        }

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
                    this.mutex?.Dispose();
                }

                this.disposedValue = true;
            }
        }
    }
}
