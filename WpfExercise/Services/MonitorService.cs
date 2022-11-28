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
    Task MonitorFileAsync();

    void Cancel();

    event EventHandler<FileUpdatedEventArgs>? FileUpdatedEvent;
}

/// <summary>
/// Monitors a json file and report changes
/// </summary>
public class MonitorService : IMonitorService
{
    #region Private Fields    
    
    private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

    private readonly CancellationTokenSource _cts = new();
    private readonly IDialogService _dialogService;

    private string _previousHash = string.Empty;

    #endregion

    #region Constructor        

    public MonitorService(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    #endregion

    #region Events

    public event EventHandler<FileUpdatedEventArgs>? FileUpdatedEvent;

    #endregion

    #region Properties

    public string JsonFileName { get; set; } = "products.json";

    #endregion

    #region Methods

    /// <summary>
    /// Start to monitor json file for changes
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public async Task MonitorFileAsync()
    {
        Logger.Info("Starting Monitor Service");

        if (!File.Exists(JsonFileName))
            throw new FileNotFoundException();

        await Task.Run(() => MonitorJsonFileAsync(_cts.Token));
    }

    /// <summary>
    /// Cancel monitor service
    /// </summary>
    public void Cancel()
    {
        _cts.Cancel();
    }

    /// <summary>
    /// Monitor json file for changes each second.
    /// Raise event if file content is changed. 
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    private async Task MonitorJsonFileAsync(CancellationToken ct)
    {
        try
        {
            _previousHash = HashCalculator.ComputeHash(JsonFileName);
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

    /// <summary>
    /// Read json file and raise FileUpdatedEvent
    /// </summary>
    private void GetAndReportProducts()
    {
        using var sr = new StreamReader(JsonFileName);
        var jsonString = sr.ReadToEnd();

        var productList = JsonSerializer.Deserialize<List<Product>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (productList != null)
            FileUpdatedEvent?.Invoke(this, new FileUpdatedEventArgs(productList));
    }

    #endregion
}