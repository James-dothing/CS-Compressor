using System.Windows;
using CsCompressor.Core.Services;

namespace CsCompressor;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ProcessTracker.Initialize();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        ProcessTracker.KillAllActiveProcesses();
        base.OnExit(e);
    }
}
