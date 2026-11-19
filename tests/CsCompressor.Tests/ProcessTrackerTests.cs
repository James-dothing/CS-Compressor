using System.Diagnostics;
using CsCompressor.Core.Services;
using Xunit;

namespace CsCompressor.Tests;

public class ProcessTrackerTests
{
    [Fact]
    public void ProcessTracker_RegisterAndUnregister_TracksCountCorrectly()
    {
        ProcessTracker.Initialize();

        var psi = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "sleep",
            Arguments = OperatingSystem.IsWindows() ? "/c ping 127.0.0.1 -n 3" : "2",
            CreateNoWindow = true,
            UseShellExecute = false
        };

        using var proc = Process.Start(psi);
        Assert.NotNull(proc);

        ProcessTracker.Register(proc);
        Assert.True(ProcessTracker.ActiveProcessCount >= 1);

        ProcessTracker.Unregister(proc);

        try
        {
            if (!proc.HasExited) proc.Kill();
        }
        catch { }
    }

    [Fact]
    public void ProcessTracker_KillAllActiveProcesses_TerminatesTrackedProcesses()
    {
        ProcessTracker.Initialize();

        var psi = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "sleep",
            Arguments = OperatingSystem.IsWindows() ? "/c ping 127.0.0.1 -n 10" : "10",
            CreateNoWindow = true,
            UseShellExecute = false
        };

        using var proc = Process.Start(psi);
        Assert.NotNull(proc);

        ProcessTracker.Register(proc);

        ProcessTracker.KillAllActiveProcesses();

        Assert.Equal(0, ProcessTracker.ActiveProcessCount);
        Assert.True(proc.HasExited);
    }
}
