using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace VMM.Helper
{
    public static class TaskHelper
    {
        public static Task<T> WithTimeout<T>(this Task<T> task, int duration, bool observeExceptions = true)
        {
            return Task.Factory.StartNew(() =>
            {
                bool b = task.Wait(duration);
                if(b) return task.Result;

                if(observeExceptions)
                {
                    task.ContinueWith((t) =>
                    {
                        t.Exception?.Handle((e) => true);
                    }, TaskContinuationOptions.OnlyOnFaulted);
                }

                throw new TimeoutException();
            });
        }

    }
}
