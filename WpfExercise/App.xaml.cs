using Prism.DryIoc;
using Prism.Ioc;
using System;
using System.Windows;
using System.Windows.Threading;
using WpfExercise.Services;
using WpfExercise.Views;

namespace WpfExercise;

public partial class App : PrismApplication
{
    #region Private Fields        

    private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

    #endregion

    #region Methods

    /// <summary>
    /// Creates the shell or main window of the application.
    /// </summary>
    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    /// <summary>
    /// Initializes the shell.
    /// </summary>
    protected override void InitializeShell(Window shell)
    {
        // Error events
        AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandler;
        Current.DispatcherUnhandledException += GlobalWpfExceptionHandler;

        shell.Show();
    }

    /// <summary>
    /// Register types with DryIoc
    /// </summary>
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<IMonitorService, MonitorService>();
        containerRegistry.RegisterSingleton<IDialogService, DialogService>();
    }


    /// <summary>
    /// Handles CurrentDomain.UnhandledException
    /// </summary>
    private void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Logger.Error(ex.Message, ex);
        }
    }

    /// <summary>
    /// Handles DispatcherUnhandledException
    /// </summary>
    private void GlobalWpfExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        if (e.Exception is { } ex)
        {
            Logger.Error(ex.Message, ex);
        }
    }

    #endregion
}