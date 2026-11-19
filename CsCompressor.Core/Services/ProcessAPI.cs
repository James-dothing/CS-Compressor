namespace CsCompressor.Core.Services;

public static class ProcessAPI
{
    public static string[] Start(string fileName, string argument, bool noWindow = true)
    {
        try
        {
            var result = ProcessRunner.Execute(fileName, argument);
            return new[]
            {
                result.StandardOutput,
                result.ExitCode.ToString(),
                result.StandardError
            };
        }
        catch (Exception e)
        {
            return new[] { "", "1", "An error has occurred:\n" + e.Message };
        }
    }

    public static async Task<ProcessExecutionResult> StartAsync(
        string fileName,
        string? argument = null,
        IEnumerable<string>? argumentList = null,
        Action<string>? onOutputLine = null,
        Action<string>? onErrorLine = null,
        CancellationToken cancellationToken = default)
    {
        return await ProcessRunner.ExecuteAsync(
            fileName,
            arguments: argument,
            argumentList: argumentList,
            onOutputLine: onOutputLine,
            onErrorLine: onErrorLine,
            cancellationToken: cancellationToken
        );
    }
}
