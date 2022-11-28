using System.ComponentModel;
using WpfExercise.ViewModels;

namespace WpfExercise.Views;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }


    /// <summary>
    /// Call cancel method on vm when window is closed
    /// </summary>
    private void MainWindowOnClosing(object? sender, CancelEventArgs e)
    {
        var vm = (MainWindowViewModel)DataContext;
        vm.Cancel();
        e.Cancel = true;
    }
}