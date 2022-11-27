using Prism.DryIoc;
using Prism.Ioc;
using System;
using System.Windows;
using System.Windows.Threading;
using WpfExercise.Services;
using WpfExercise.Views;

namespace WpfExercise;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : PrismApplication
{
    private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void InitializeShell(Window shell)
    {
        // Error events
        AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandler;
        Current.DispatcherUnhandledException += GlobalWpfExceptionHandler;

        shell.Show();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<IMonitorService, MonitorService>();
        containerRegistry.RegisterSingleton<IDialogService, DialogService>();
    }

    private void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Logger.Error(ex.Message, ex);
        }
    }

    private void GlobalWpfExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        if (e.Exception is { } ex)
        {
            Logger.Error(ex.Message, ex);
        }
    }
}