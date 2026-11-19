using System.Diagnostics;

public class ProcessAPI
{
    public static string[] Start(string fileName, string argument, bool noWindow=true)
    {
        try
        {
            using Process p = new Process();
            p.StartInfo.FileName = fileName;
            p.StartInfo.Arguments = argument ?? "";
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = noWindow;

            if (!p.Start())
            {
                return new string[] { "", "1", "Failed to start process" };
            }

            Task<string> stdoutTask = p.StandardOutput.ReadToEndAsync();
            Task<string> stderrTask = p.StandardError.ReadToEndAsync();

            Task.WaitAll(stdoutTask, stderrTask);
            p.WaitForExit();

            int exitCode = p.ExitCode;
            return new string[] { stdoutTask.Result, exitCode.ToString(), stderrTask.Result };
        }
        catch (Exception e)
        {
            return new string[] { "", "1", "An error has occur:\n" + e.ToString() };
        }
    }
}