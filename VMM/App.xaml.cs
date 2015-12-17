using System;
using System.Diagnostics;
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
            Trace.WriteLine($"Unhandeled task exception: {e.Exception}");

            if(e.Exception.InnerException is AccessTokenInvalidException)
            {
                e.SetObserved();
                return;
            }

            Dispatcher.Invoke(
                () => { ModernDialog.ShowMessage("Во время работы произошла критическая ошибка. Приложение будет закрыто :(", "Критическая ошибка", MessageBoxButton.OK); });

            Environment.Exit(0);
        }

        private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Trace.WriteLine($"Unhandeled exception: {e.Exception}");

            ModernDialog.ShowMessage("Во время работы произошла критическая ошибка. Приложение будет закрыто :(", "Критическая ошибка", MessageBoxButton.OK);

            Environment.Exit(0);
        }
    }
}