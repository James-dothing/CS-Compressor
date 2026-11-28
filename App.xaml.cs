using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace CsCompressor;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void ApplicationExit(object sender, ExitEventArgs e)
    {
        FFmpegAPI.AbortFFmpeg(); //
    }
}

