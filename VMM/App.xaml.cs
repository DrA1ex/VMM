using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FirstFloor.ModernUI.Windows.Controls;

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
            Trace.WriteLine(String.Format("Unhandeled task exception: {0}", e.Exception));

            ModernDialog.ShowMessage("Во время работы произошла критическая ошибка. Приложение будет закрыто :(", "Критическая ошибка", MessageBoxButton.OK);
        }

        private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Trace.WriteLine(String.Format("Unhandeled exception: {0}", e.Exception));

            ModernDialog.ShowMessage("Во время работы произошла критическая ошибка. Приложение будет закрыто :(", "Критическая ошибка", MessageBoxButton.OK);
        }
    }
}