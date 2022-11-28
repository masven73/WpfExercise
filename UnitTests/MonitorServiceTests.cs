using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using WpfExercise.Models;
using WpfExercise.Services;
using Xunit;

namespace UnitTests;

public class MonitorServiceTests
{
    private readonly List<Product>? _testProducts = new()
    {
        new()
        {
            Id = 1,
            Title = "iPhone 12",
            Description = "Old model from Apple",
            Price = 549,
            DiscountPercentage = 12.96,
            Rating = 4.69,
            Stock = 94,
            Brand = "Apple",
            Category = "smartphones"
        },
        new()
        {
            Id = 2,
            Title = "iPhone 13",
            Description = "New model from Apple",
            Price = 899,
            DiscountPercentage = 17.94,
            Rating = 4.44,
            Stock = 34,
            Brand = "Apple",
            Category = "smartphones"
        }
    };

    [Fact]
    public async void TestMonitorService()
    {
        List<Product>? updatedProducts = new List<Product>();
        var mockDialogService = new Mock<IDialogService>();
        
        var monitorService = new MonitorService(mockDialogService.Object)
        {
            JsonFileName = "TestFile.json"
        };

        await File.WriteAllTextAsync(monitorService.JsonFileName, JsonConvert.SerializeObject(_testProducts));

        var resetEvent = new AutoResetEvent(false);

        monitorService.FileUpdatedEvent += (sender, args) =>
        {
            updatedProducts = args.Products;
            resetEvent.Set();
        };

        var monitorFileTask = monitorService.MonitorFileAsync();

        var eventReceived = resetEvent.WaitOne(TimeSpan.FromSeconds(1));
        Assert.True(eventReceived);
        Assert.Equal(_testProducts, updatedProducts);

        resetEvent.Reset();

        //Update product
        var iPhone12 = _testProducts?.FirstOrDefault(x => x.Id == 1 && x.Title == "iPhone 12");
        if (iPhone12 != null)
        {
            iPhone12.Title = "iPhone 11";
            await File.WriteAllTextAsync(monitorService.JsonFileName, JsonConvert.SerializeObject(_testProducts));
        }

        eventReceived = resetEvent.WaitOne(TimeSpan.FromSeconds(1));
        Assert.True(eventReceived);

        var updatedIphone = updatedProducts?.FirstOrDefault(x => x.Id == 1);
        Assert.Equal("iPhone 11", updatedIphone?.Title);

        monitorService.Cancel();
        await monitorFileTask;
    }
}