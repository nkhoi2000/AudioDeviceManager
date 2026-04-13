using System.Windows;
using System.Windows.Threading;

namespace AudioDeviceManager;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
        base.OnStartup(e);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        ShowException("An unexpected UI error occurred.", e.Exception);
        e.Handled = true;
    }

    private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception ?? new Exception("Unknown application error.");
        ShowException("A fatal application error occurred.", exception);
    }

    private static void ShowException(string message, Exception exception)
    {
        MessageBox.Show(
            $"{message}\n\n{exception.Message}\n\n{exception}",
            "AudioDeviceManager Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}
