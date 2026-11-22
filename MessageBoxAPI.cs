using System.Windows;

public class MessageBoxAPI
{
    public static void Error(string text, bool Crash=true)
    {
        MessageBox.Show("FATAL ERROR: " + text + "??", "FATAL ERROR", MessageBoxButton.OK, MessageBoxImage.Error);

        if (Crash){Application.Current.Shutdown();}
    }

    public static void Debug(string text, bool Crash=false)
    {
        MessageBox.Show(text, "debug message:", MessageBoxButton.OK);
        if (Crash){Application.Current.Shutdown();}
    }
    
    async public static Task DebugAsync(string text)
    {
        await Task.Run(() =>
        {
            MessageBox.Show(text, "debug message:", MessageBoxButton.OK);
        });
    }
    public static bool YesNo(string text)
    {
        MessageBoxResult result = MessageBox.Show(
        text,
        "CSCompressor",
        MessageBoxButton.YesNo,
        MessageBoxImage.Question
        );

        return result == MessageBoxResult.Yes ? true : false;
    }
}