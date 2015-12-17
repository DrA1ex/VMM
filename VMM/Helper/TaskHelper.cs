using System;
using System.Threading;
using System.Threading.Tasks;

namespace VMM.Helper
{
    public static class TaskHelper
    {
        public static Task<T> WithTimeout<T>(this Task<T> task, int duration, CancellationToken ct)
        {
            return Task.Factory.StartNew(() =>
            {
                var b = task.Wait(duration, ct);
                if(b) return task.Result;

                task.ContinueWith(t => { t.Exception?.Handle(e => true); }, TaskContinuationOptions.OnlyOnFaulted);

                throw new TimeoutException();
            }, ct);
        }
    }
}