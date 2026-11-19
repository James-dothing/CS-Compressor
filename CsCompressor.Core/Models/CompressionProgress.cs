namespace CsCompressor.Core.Models;

public record CompressionProgress(
    long Frame,
    double Fps,
    TimeSpan CurrentTime,
    TimeSpan TotalDuration,
    double Percentage,
    double Speed,
    string Bitrate,
    TimeSpan? EstimatedTimeRemaining,
    bool IsComplete
)
{
    public string FormattedPercentage => $"{Percentage:0.0}%";

    public string FormattedCurrentTime =>
        $"{(int)CurrentTime.TotalHours:00}:{CurrentTime.Minutes:00}:{CurrentTime.Seconds:00}";

    public string FormattedTotalDuration =>
        $"{(int)TotalDuration.TotalHours:00}:{TotalDuration.Minutes:00}:{TotalDuration.Seconds:00}";

    public string FormattedEta
    {
        get
        {
            if (IsComplete) return "Finished";
            if (!EstimatedTimeRemaining.HasValue) return "Calculating...";
            var eta = EstimatedTimeRemaining.Value;
            if (eta.TotalHours >= 1)
            {
                return $"{(int)eta.TotalHours:00}:{eta.Minutes:00}:{eta.Seconds:00}";
            }
            return $"{eta.Minutes:00}:{eta.Seconds:00}";
        }
    }
}
