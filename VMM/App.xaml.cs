using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FirstFloor.ModernUI.Windows.Controls;
using VkNet.Exception;

namespace VMM
{
    public partial class App
    {
        private App()
        {
            DispatcherUnhandledException += AppDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var innerException = e.Exception.InnerException;
            if(innerException is AccessTokenInvalidException //if invalid authorization data
               || innerException is OperationCanceledException //if some of operation was canceled
               || innerException is WebException && ((WebException)innerException).Status == WebExceptionStatus.RequestCanceled) //if some of request was canceled
            {
                e.SetObserved();
                return;
            }

            ProcessUnhandledException(e.Exception);
        }

        private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ProcessUnhandledException(e.Exception);
        }

        private void ProcessUnhandledException(Exception e)
        {
            Trace.WriteLine($"Unhandeled exception: {e}");

#if DEBUG
            Dispatcher.Invoke(
                () => { ModernDialog.ShowMessage($"Во время работы произошла непредвиденная ошибка: {e}", "Критическая ошибка", MessageBoxButton.OK); });

            Environment.Exit(0);
#endif
        }
    }
}