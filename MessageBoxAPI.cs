using System.Windows;

public class MessageBoxAPI
{
    public static void Error(string text, bool Crash = false)
    {
        if (Application.Current != null)
        {
            MessageBox.Show(text, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            if (Crash)
            {
                Application.Current.Shutdown();
            }
        }
    }

    public static void Debug(string text, bool Crash = false)
    {
        if (Application.Current != null)
        {
            MessageBox.Show(text, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            if (Crash)
            {
                Application.Current.Shutdown();
            }
        }
    }

    public static async Task DebugAsync(string text)
    {
        await Task.Run(() =>
        {
            if (Application.Current != null)
            {
                MessageBox.Show(text, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        });
    }
}
