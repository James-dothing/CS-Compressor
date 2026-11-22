using System.Diagnostics;
using System.IO;
using System.Reflection;

public class Extract
{
    public static string GetPath(String fileName, string realFileName="")
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string frameworksDir = Path.Combine(appData, "CSCompressor");
        string filePath = Path.Combine(frameworksDir, fileName);

        string szPath = Path.Combine(frameworksDir, "7za.exe");
        string realPath = Path.Combine(frameworksDir, realFileName);
        if (realFileName != "" && !File.Exists(szPath)) {MessageBoxAPI.Error("7za.exe doesn't exist");return"";}

        if (! (File.Exists(filePath) || (string.IsNullOrEmpty(realFileName) ? false : File.Exists(realPath))))
        {
            if (realFileName=="ffmpeg.exe")
            {
                _ = MessageBoxAPI.DebugAsync("Extracting... Please wait\n(this won't be taking this long again after this)");
            }
            Directory.CreateDirectory(frameworksDir);

            Stream? resource = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("CsCompressor.frameworks."+fileName);

            if (resource == null) {MessageBoxAPI.Error(fileName+" not found inside this file");return"";}

            FileStream file = File.Create(filePath);
            resource.CopyTo(file);

            file.Dispose();
            resource.Dispose(); //
        }

        if (string.IsNullOrEmpty(realFileName)) {return filePath;} //if needed to be unzip then proceed the following code:
        if (File.Exists(realPath)) {return realPath;}
        
        string[] result = ProcessAPI.Start(szPath,
            $"e \"{filePath}\" -y -o\"{frameworksDir}\"" // "%appdata%\CSCompressor\7za.exe" e "%appdata%\CSCompressor\ffmpeg.7z" -y -o"%appdata%\CSCompressor"
        );
        if (result[1] != "0")
        {
            MessageBoxAPI.Error($"Something went wrong! (exit code: {result[1]})\n{result[0]}\n{result[2]}");
        }

        if (File.Exists(realPath)) {return realPath;}


        MessageBoxAPI.Error("real path doesn't exist");
        return "";
    }
}