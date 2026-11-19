using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CsCompressor.Core.Services;

public record ToolValidationResult(bool IsAvailable, string? ExecutablePath, string? Version, string? ErrorMessage);

public static class FFmpegLocator
{
    private static readonly string[] ExecutableExtensions = OperatingSystem.IsWindows()
        ? new[] { ".exe", ".cmd", ".bat", "" }
        : new[] { "", ".sh" };

    public static string? FindExecutable(string binaryName, IEnumerable<string?>? preferredPaths = null)
    {
        var candidates = new List<string>();

        if (preferredPaths != null)
        {
            foreach (var path in preferredPaths)
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    candidates.Add(path);
                }
            }
        }

        // Check application directory
        candidates.Add(AppContext.BaseDirectory);

        // Check user application data directory
        try
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!string.IsNullOrWhiteSpace(appData))
            {
                candidates.Add(Path.Combine(appData, "CSCompressor"));
            }
        }
        catch
        {
            // Ignore environment query exceptions
        }

        // Check system PATH
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrWhiteSpace(pathEnv))
        {
            var pathDirs = pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            candidates.AddRange(pathDirs);
        }

        foreach (var dir in candidates)
        {
            if (string.IsNullOrWhiteSpace(dir)) continue;

            // Direct file path passed
            if (File.Exists(dir))
            {
                var fileName = Path.GetFileNameWithoutExtension(dir);
                if (string.Equals(fileName, binaryName, StringComparison.OrdinalIgnoreCase))
                {
                    return Path.GetFullPath(dir);
                }
            }

            if (!Directory.Exists(dir)) continue;

            foreach (var ext in ExecutableExtensions)
            {
                var testPath = Path.Combine(dir, binaryName + ext);
                if (File.Exists(testPath))
                {
                    return Path.GetFullPath(testPath);
                }
            }
        }

        return null;
    }

    public static ToolValidationResult ValidateBinary(string binaryName, string? explicitPath = null)
    {
        string? resolvedPath = null;

        if (!string.IsNullOrWhiteSpace(explicitPath) && File.Exists(explicitPath))
        {
            resolvedPath = Path.GetFullPath(explicitPath);
        }
        else
        {
            resolvedPath = FindExecutable(binaryName);
        }

        if (resolvedPath == null)
        {
            return new ToolValidationResult(
                IsAvailable: false,
                ExecutablePath: null,
                Version: null,
                ErrorMessage: $"{binaryName} executable was not found on PATH or application directories."
            );
        }

        try
        {
            using var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = resolvedPath,
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            var output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit(3000);

            var version = ExtractVersionString(output);
            return new ToolValidationResult(
                IsAvailable: true,
                ExecutablePath: resolvedPath,
                Version: version ?? "Unknown version",
                ErrorMessage: null
            );
        }
        catch (Exception ex)
        {
            return new ToolValidationResult(
                IsAvailable: false,
                ExecutablePath: resolvedPath,
                Version: null,
                ErrorMessage: $"Failed to execute {binaryName}: {ex.Message}"
            );
        }
    }

    public static string? ExtractVersionString(string versionOutput)
    {
        if (string.IsNullOrWhiteSpace(versionOutput)) return null;

        // Examples: "ffmpeg version 8.1.3", "ffmpeg version n6.1.1-2", "ffprobe version 7.0"
        var match = Regex.Match(versionOutput, @"(?:ffmpeg|ffprobe)\s+version\s+([^\s,]+)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        // Fallback: first line
        var lines = versionOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return lines.Length > 0 ? lines[0].Trim() : null;
    }
}
