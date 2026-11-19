// Compatibility shim forwarding to CsCompressor.Core.Services.ProcessAPI
public static class ProcessAPI
{
    public static string[] Start(string fileName, string argument, bool noWindow = true)
        => CsCompressor.Core.Services.ProcessAPI.Start(fileName, argument, noWindow);

    public static Task<CsCompressor.Core.Services.ProcessExecutionResult> StartAsync(
        string fileName,
        string? argument = null,
        IEnumerable<string>? argumentList = null,
        Action<string>? onOutputLine = null,
        Action<string>? onErrorLine = null,
        CancellationToken cancellationToken = default)
        => CsCompressor.Core.Services.ProcessAPI.StartAsync(fileName, argument, argumentList, onOutputLine, onErrorLine, cancellationToken);
}
