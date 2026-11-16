using System.IO;
using System.Diagnostics;

public class ProcessAPI()
{
    public static string[] Start(string fileName, string argument, bool noWindow=true)
    {
        try
        {
            Process? p = new Process();
            p.StartInfo.FileName = fileName;
            p.StartInfo.Arguments = " " + argument; // " " + because incase idk lool
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = noWindow;
            p.Start();
            if (p==null) {return new string[] {"", "1", "process is null"};} // {output, exit status, error message}
            string err = p.StandardError.ReadToEnd();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            int exitCode = p.ExitCode;
            return new string[] {output, exitCode.ToString(), err};
        } catch (Exception e)
        {
            return new string[] {"", "1", "An error has occur:\n" + e.ToString()};
        }
    }
}