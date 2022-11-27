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
    private readonly IMonitorService _monitorService;
    private readonly object _lockObject = new();

    public DelegateCommand ClearCommand { get; }

    public DelegateCommand WindowLoadedCommand { get; }

    public MainWindowViewModel(IMonitorService monitorService)
    {
        _monitorService = monitorService;

        ClearCommand = new DelegateCommand(OnClear);
        WindowLoadedCommand = new DelegateCommand(OnWindowLoaded);

        _monitorService.FileUpdatedEvent += MonitorServiceOnFileUpdated;

        _products = new ObservableRangeCollection<Product>();
        BindingOperations.EnableCollectionSynchronization(_products, _lockObject);
    }

    private async void OnWindowLoaded()
    {
        await _monitorService.MonitorFile();

        _monitorService.FileUpdatedEvent -= MonitorServiceOnFileUpdated;

        Application.Current.Shutdown();
    }

    private ObservableRangeCollection<Product> _products;
    public ObservableRangeCollection<Product> Products
    {
        get => _products;
        set => SetProperty(ref _products, value);
    }

    private void MonitorServiceOnFileUpdated(object? sender, FileUpdatedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            UpdateProducts(e.Products);
        });
    }

    public void Cancel()
    {
        _monitorService.Cancel();
    }

    private void OnClear()
    {
        UpdateProducts(null);
    }

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
}