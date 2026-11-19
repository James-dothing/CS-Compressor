namespace CsCompressor.Core.Models;

public record MediaInfo(
    string FilePath,
    TimeSpan Duration,
    string? VideoCodec,
    string? AudioCodec,
    int Width,
    int Height,
    double Framerate,
    long FileSizeBytes
)
{
    public string FormattedDuration =>
        $"{(int)Duration.TotalHours:00}:{Duration.Minutes:00}:{Duration.Seconds:00}";

    public string FormattedResolution =>
        Width > 0 && Height > 0 ? $"{Width}×{Height}" : "N/A";

    public string FormattedFileSize
    {
        get
        {
            if (FileSizeBytes <= 0) return "0 B";
            string[] units = { "B", "KB", "MB", "GB" };
            double size = FileSizeBytes;
            int unitIndex = 0;
            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }
            return $"{size:0.##} {units[unitIndex]}";
        }
    }
}
