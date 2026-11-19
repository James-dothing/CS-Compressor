using System.Diagnostics;
using System.IO;
using System.Reflection;

public class Extract
{
    public static string GetPath(string fileName, string realFileName="")
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string frameworksDir = Path.Combine(appData, "CSCompressor");
        string filePath = Path.Combine(frameworksDir, fileName);
        string szPath = Path.Combine(frameworksDir, "7za.exe");
        string realPath = string.IsNullOrEmpty(realFileName) ? "" : Path.Combine(frameworksDir, realFileName);

        // If target unzipped file already exists and is non-empty, return it immediately
        if (!string.IsNullOrEmpty(realFileName) && File.Exists(realPath) && new FileInfo(realPath).Length > 0)
        {
            return realPath;
        }

        // If extracting an archive requires 7za.exe, ensure 7za.exe is extracted first
        if (!string.IsNullOrEmpty(realFileName) && !File.Exists(szPath))
        {
            string extracted7za = GetPath("7za.exe");
            if (string.IsNullOrEmpty(extracted7za) || !File.Exists(szPath))
            {
                MessageBoxAPI.Error("7za.exe could not be extracted");
                return "";
            }
        }

        // Extract resource file from embedded assembly if it doesn't exist or is empty
        if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
        {
            if (realFileName == "ffmpeg.exe")
            {
                _ = MessageBoxAPI.DebugAsync("Extracting... Please wait\n(this won't be taking this long again after this)");
            }
            Directory.CreateDirectory(frameworksDir);

            using Stream? resource = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("CsCompressor.frameworks." + fileName);

            if (resource == null)
            {
                MessageBoxAPI.Error(fileName + " not found inside this file");
                return "";
            }

            using (FileStream file = File.Create(filePath))
            {
                resource.CopyTo(file);
            }
        }

        // If no unzipping required, return the extracted file
        if (string.IsNullOrEmpty(realFileName))
        {
            return filePath;
        }

        if (File.Exists(realPath) && new FileInfo(realPath).Length > 0)
        {
            return realPath;
        }

        // Extract with 7za.exe
        string[] result = ProcessAPI.Start(szPath,
            $"e \"{filePath}\" -y -o\"{frameworksDir}\""
        );

        if (result[1] != "0")
        {
            // If decompression failed (e.g. corrupted archive), delete corrupt archive to allow retry
            if (File.Exists(filePath))
            {
                try { File.Delete(filePath); } catch { }
            }
            MessageBoxAPI.Error($"Something went wrong while decompressing! (exit code: {result[1]})\n{result[0]}\n{result[2]}");
            return "";
        }

        if (File.Exists(realPath) && new FileInfo(realPath).Length > 0)
        {
            // Clean up temporary .7z archive to save disk space
            if (File.Exists(filePath) && filePath != realPath)
            {
                try { File.Delete(filePath); } catch { }
            }
            return realPath;
        }

        MessageBoxAPI.Error("real path doesn't exist");
        return "";
    }
}