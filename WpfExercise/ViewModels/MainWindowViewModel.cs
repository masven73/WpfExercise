using System;
using MvvmHelpers;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using WpfExercise.Models;
using WpfExercise.Services;

namespace WpfExercise.ViewModels;

public class MainWindowViewModel : BindableBase
{
    #region Private Fields        

    private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

    private readonly IMonitorService _monitorService;
    private readonly IDialogService _dialogService;

    private readonly object _lockObject = new();

    #endregion

    #region Constructor

    public MainWindowViewModel(IMonitorService monitorService, IDialogService dialogService)
    {
        _monitorService = monitorService;
        _dialogService = dialogService;

        ClearCommand = new DelegateCommand(OnClear);
        WindowLoadedCommand = new DelegateCommand(OnWindowLoaded);

        _monitorService.FileUpdatedEvent += MonitorServiceOnFileUpdated;

        _products = new ObservableRangeCollection<Product>();
        BindingOperations.EnableCollectionSynchronization(_products, _lockObject);
    }

    #endregion

    #region Properties

    private ObservableRangeCollection<Product> _products;
    public ObservableRangeCollection<Product> Products
    {
        get => _products;
        set => SetProperty(ref _products, value);
    }

    #endregion

    #region Commands
    public DelegateCommand ClearCommand { get; }

    public DelegateCommand WindowLoadedCommand { get; }

    #endregion

    #region Command Actions

    /// <summary>
    /// Called on Window Loaded event
    /// </summary>
    private async void OnWindowLoaded()
    {
        try
        {
            await _monitorService.MonitorFile();

            _monitorService.FileUpdatedEvent -= MonitorServiceOnFileUpdated;

            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message, ex);
            _dialogService.Show(ex.Message);
        }
    }

    /// <summary>
    /// Called when user clicks the clear button
    /// </summary>
    private void OnClear()
    {
        UpdateProducts(null);
    }

    #endregion

    #region Events methods

    /// <summary>
    /// Called on FileUpdatedEvent when the monitor service reports that the json file has changed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MonitorServiceOnFileUpdated(object? sender, FileUpdatedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            UpdateProducts(e.Products);
        });
    }

    #endregion

    #region Methods

    /// <summary>
    /// Cancel the monitor service when the main window is closed
    /// </summary>
    public void Cancel()
    {
        _monitorService.Cancel();
    }

    /// <summary>
    /// Update the list of products in the UI
    /// </summary>
    /// <param name="products"></param>
    private void UpdateProducts(List<Product>? products)
    {
        lock (_lockObject)
        {
            if (products == null)
                Products.Clear();
            else
                Products.ReplaceRange(products);
        }
    }

    #endregion
}