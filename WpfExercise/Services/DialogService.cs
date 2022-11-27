using System.Windows;

namespace WpfExercise.Services;

public interface IDialogService
{
    MessageBoxResult Show(string messageText);
}
public class DialogService : IDialogService
{
    public MessageBoxResult Show(string messageText)
    {
        return MessageBox.Show(messageText);
    }
}