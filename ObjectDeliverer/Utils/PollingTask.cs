using System;
using System.Threading;
using System.Threading.Tasks;

namespace ObjectDeliverer.Utils
{
    public class PollingTask
    {
        private Task? pollingTask = null;

        private CancellationTokenSource? canceler;

        public PollingTask(Func<ValueTask<bool>> action)
        {
            canceler = new CancellationTokenSource();

            pollingTask = Task.Run(async () =>
            {
                while(canceler?.IsCancellationRequested == false)
                {
                    if (await action.Invoke() == false)
                    {
                        break;
                    }
                }
            });
        }

        public async ValueTask Stop()
        {
            if (pollingTask == null) return;
            canceler?.Cancel();
            await pollingTask;
        }
    }
}
