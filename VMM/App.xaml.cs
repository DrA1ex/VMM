using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using FirstFloor.ModernUI.Windows.Controls;

namespace VMM
{
    public partial class App
    {
        App()
        {
            DispatcherUnhandledException += AppDispatcherUnhandledException;
        }

        void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Trace.WriteLine(String.Format("Unhandeled exception: {0}", e.Exception.Message));

            ModernDialog.ShowMessage("Во время работы произошла критическая ошибка. Приложение будет закрыто :(", "Критическая ошибка", MessageBoxButton.OK);
        }

    }
}