using System.Diagnostics;
using System.IO;
using System.Reflection;

public class Extract
{
    public static string ExtractResourceToFile(string resourceFileName, string destinationFilePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath)!);
        using Stream? resource = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("CsCompressor.frameworks." + resourceFileName);

        if (resource == null)
        {
            return "";
        }

        using FileStream file = File.Create(destinationFilePath);
        resource.CopyTo(file);
        return destinationFilePath;
    }

    public static string GetPath(string fileName, string realFileName = "")
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string frameworksDir = Path.Combine(appData, "CSCompressor");
        string filePath = Path.Combine(frameworksDir, fileName);
        string szPath = Path.Combine(frameworksDir, "7za.exe");
        string realPath = Path.Combine(frameworksDir, realFileName);

        // Ensure 7za.exe is extracted if needed
        if (!string.IsNullOrEmpty(realFileName) && !File.Exists(szPath))
        {
            ExtractResourceToFile("7za.exe", szPath);
        }

        // Check if destination exists
        if (File.Exists(filePath) || (!string.IsNullOrEmpty(realFileName) && File.Exists(realPath)))
        {
            return !string.IsNullOrEmpty(realFileName) && File.Exists(realPath) ? realPath : filePath;
        }

        Directory.CreateDirectory(frameworksDir);
        string extracted = ExtractResourceToFile(fileName, filePath);
        if (string.IsNullOrEmpty(extracted))
        {
            return "";
        }

        if (string.IsNullOrEmpty(realFileName))
        {
            return filePath;
        }

        if (File.Exists(realPath))
        {
            return realPath;
        }

        // Unpack archive with 7za.exe if on Windows
        if (OperatingSystem.IsWindows() && File.Exists(szPath))
        {
            string[] result = ProcessAPI.Start(szPath, $"e \"{filePath}\" -y -o\"{frameworksDir}\"");
            if (result[1] != "0")
            {
                MessageBoxAPI.Error($"Extraction error (code {result[1]}): {result[2]}", Crash: false);
            }
        }

        return File.Exists(realPath) ? realPath : "";
    }
}
