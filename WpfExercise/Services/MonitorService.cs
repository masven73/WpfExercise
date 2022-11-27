using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WpfExercise.Models;

namespace WpfExercise.Services;

public class FileUpdatedEventArgs : EventArgs
{ 
    public FileUpdatedEventArgs(List<Product>? products) => Products = products;

    public List<Product>? Products { get; set; }
}

public interface IMonitorService
{
    Task MonitorFile();
    void Cancel();

    event EventHandler<FileUpdatedEventArgs>? FileUpdatedEvent;
}

public class MonitorService : IMonitorService
{
    private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

    private readonly CancellationTokenSource _cts = new();
    private readonly IDialogService _dialogService;

    private string _previousHash = string.Empty;
    
    public MonitorService(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public event EventHandler<FileUpdatedEventArgs>? FileUpdatedEvent;

    public string JsonFileName { get; set; } = "products.json";

    public async Task MonitorFile()
    {
        Logger.Info("Starting Monitor Service");

        if (!File.Exists(JsonFileName))
            throw new FileNotFoundException();

        await Task.Run(() => MonitorJsonFile(_cts.Token));
    }

    public void Cancel()
    {
        _cts.Cancel();
    }

    private async Task MonitorJsonFile(CancellationToken ct)
    {
        try
        {
            GetAndReportProducts();

            var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

            while (await timer.WaitForNextTickAsync(ct))
            {
                var hash = HashCalculator.ComputeHash(JsonFileName);

                //File changed?
                if (hash != _previousHash) 
                    GetAndReportProducts();

                _previousHash = hash;
            }
        }

        catch (OperationCanceledException)
        {
            Logger.Info("File monitoring canceled");
        }

        catch (Exception ex)
        {
            Logger.Error(ex.Message, ex);
            _dialogService.Show(ex.Message);
        }
    }

    private void GetAndReportProducts()
    {
        using var sr = new StreamReader(JsonFileName);
        var jsonString = sr.ReadToEnd();

        var productList = JsonSerializer.Deserialize<List<Product>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (productList != null)
            FileUpdatedEvent?.Invoke(this, new FileUpdatedEventArgs(productList));
    }
}