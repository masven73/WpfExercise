using System.Windows;

namespace WpfExercise.Services;

public interface IDialogService
{
    MessageBoxResult Show(string messageText);
}

/// <summary>
/// Service to show a System.Windows.MessageBox fom vm
/// </summary>
public class DialogService : IDialogService
{
    public MessageBoxResult Show(string messageText)
    {
        return MessageBox.Show(messageText);
    }
}