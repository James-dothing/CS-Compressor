using System.Collections.Concurrent;
using System.Diagnostics;

namespace CsCompressor.Core.Services;

public static class ProcessTracker
{
    private static readonly ConcurrentDictionary<int, Process> ActiveProcesses = new();
    private static bool _isHooked = false;
    private static readonly object _hookLock = new();

    public static void Initialize()
    {
        lock (_hookLock)
        {
            if (_isHooked) return;

            AppDomain.CurrentDomain.ProcessExit += (_, _) => KillAllActiveProcesses();
            Console.CancelKeyPress += (_, e) =>
            {
                KillAllActiveProcesses();
            };

            _isHooked = true;
        }
    }

    public static void Register(Process process)
    {
        Initialize();
        try
        {
            if (!process.HasExited)
            {
                ActiveProcesses.TryAdd(process.Id, process);
            }
        }
        catch
        {
            // Process may not have started yet or already exited
        }
    }

    public static void Unregister(Process process)
    {
        try
        {
            ActiveProcesses.TryRemove(process.Id, out _);
        }
        catch
        {
            // Ignore
        }
    }

    public static void KillAllActiveProcesses()
    {
        foreach (var kvp in ActiveProcesses)
        {
            try
            {
                var process = kvp.Value;
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // Process may have already terminated
            }
        }
        ActiveProcesses.Clear();
    }

    public static int ActiveProcessCount => ActiveProcesses.Count;
}
