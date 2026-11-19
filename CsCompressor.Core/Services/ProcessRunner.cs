using System.Diagnostics;
using System.Text;

namespace CsCompressor.Core.Services;

public record ProcessExecutionResult(
    int ExitCode,
    string StandardOutput,
    string StandardError,
    bool WasCancelled,
    TimeSpan Duration
)
{
    public bool IsSuccess => ExitCode == 0 && !WasCancelled;

    public string GetFormattedErrorMessage()
    {
        if (WasCancelled) return "Process was cancelled by the user.";
        if (IsSuccess) return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine($"Process exited with non-zero code {ExitCode}.");
        if (!string.IsNullOrWhiteSpace(StandardError))
        {
            sb.AppendLine("Error details:");
            sb.AppendLine(StandardError.Trim());
        }
        else if (!string.IsNullOrWhiteSpace(StandardOutput))
        {
            sb.AppendLine("Output details:");
            sb.AppendLine(StandardOutput.Trim());
        }
        return sb.ToString().Trim();
    }
}

public static class ProcessRunner
{
    public static async Task<ProcessExecutionResult> ExecuteAsync(
        string fileName,
        string? arguments = null,
        IEnumerable<string>? argumentList = null,
        string? workingDirectory = null,
        Action<string>? onOutputLine = null,
        Action<string>? onErrorLine = null,
        CancellationToken cancellationToken = default)
    {
        var startTime = Stopwatch.StartNew();
        var stdoutBuilder = new StringBuilder();
        var stderrBuilder = new StringBuilder();

        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (!string.IsNullOrWhiteSpace(workingDirectory) && Directory.Exists(workingDirectory))
        {
            psi.WorkingDirectory = workingDirectory;
        }

        if (argumentList != null)
        {
            foreach (var arg in argumentList)
            {
                psi.ArgumentList.Add(arg);
            }
        }
        else if (!string.IsNullOrWhiteSpace(arguments))
        {
            psi.Arguments = arguments;
        }

        using var process = new Process { StartInfo = psi };

        var outputCloseEvent = new TaskCompletionSource<bool>();
        var errorCloseEvent = new TaskCompletionSource<bool>();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data == null)
            {
                outputCloseEvent.TrySetResult(true);
            }
            else
            {
                lock (stdoutBuilder)
                {
                    stdoutBuilder.AppendLine(e.Data);
                }
                onOutputLine?.Invoke(e.Data);
            }
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data == null)
            {
                errorCloseEvent.TrySetResult(true);
            }
            else
            {
                lock (stderrBuilder)
                {
                    stderrBuilder.AppendLine(e.Data);
                }
                onErrorLine?.Invoke(e.Data);
            }
        };

        try
        {
            process.Start();
            ProcessTracker.Register(process);
        }
        catch (Exception ex)
        {
            return new ProcessExecutionResult(
                ExitCode: -1,
                StandardOutput: string.Empty,
                StandardError: $"Failed to start process '{fileName}': {ex.Message}",
                WasCancelled: false,
                Duration: startTime.Elapsed
            );
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        bool wasCancelled = false;

        using var registration = cancellationToken.CanBeCanceled
            ? cancellationToken.Register(() =>
            {
                wasCancelled = true;
                GracefulKillProcess(process);
            })
            : default;

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            wasCancelled = true;
            GracefulKillProcess(process);
        }

        // Wait for asynchronous standard I/O streams to finish flushing
        try
        {
            await Task.WhenAll(outputCloseEvent.Task, errorCloseEvent.Task).WaitAsync(TimeSpan.FromSeconds(2));
        }
        catch
        {
            // Ignore stream flush timeout on abort
        }

        int exitCode = wasCancelled ? -1 : process.ExitCode;
        ProcessTracker.Unregister(process);

        return new ProcessExecutionResult(
            ExitCode: exitCode,
            StandardOutput: stdoutBuilder.ToString(),
            StandardError: stderrBuilder.ToString(),
            WasCancelled: wasCancelled,
            Duration: startTime.Elapsed
        );
    }

    public static ProcessExecutionResult Execute(
        string fileName,
        string? arguments = null,
        IEnumerable<string>? argumentList = null,
        string? workingDirectory = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => ExecuteAsync(
            fileName,
            arguments,
            argumentList,
            workingDirectory,
            null,
            null,
            cancellationToken)).GetAwaiter().GetResult();
    }

    private static void GracefulKillProcess(Process process)
    {
        try
        {
            if (process.HasExited) return;

            // Attempt to send FFmpeg graceful quit command via stdin
            try
            {
                if (process.StartInfo.RedirectStandardInput && !process.HasExited)
                {
                    process.StandardInput.WriteLine("q");
                    process.StandardInput.Flush();
                    if (process.WaitForExit(800))
                    {
                        return;
                    }
                }
            }
            catch
            {
                // Fall through to process kill
            }

            // Terminate entire process tree
            process.Kill(entireProcessTree: true);
        }
        catch
        {
            // Ignore exceptions if already terminated
        }
    }
}
