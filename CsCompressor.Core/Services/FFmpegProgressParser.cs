using System.Globalization;
using System.Text.RegularExpressions;
using CsCompressor.Core.Models;

namespace CsCompressor.Core.Services;

public class FFmpegProgressParser
{
    private readonly TimeSpan _totalDuration;
    private long _currentFrame = 0;
    private double _currentFps = 0;
    private TimeSpan _currentTime = TimeSpan.Zero;
    private double _currentSpeed = 1.0;
    private string _currentBitrate = "N/A";
    private bool _isComplete = false;

    private static readonly Regex StderrProgressRegex = new(
        @"frame=\s*(\d+)\s+fps=\s*([\d\.]+)\s+.*?time=(\d+:\d+:\d+(?:\.\d+)?)\s+bitrate=\s*([\d\.]+\s*\S+)\s+speed=\s*([\d\.]+)x",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public FFmpegProgressParser(TimeSpan totalDuration)
    {
        _totalDuration = totalDuration > TimeSpan.Zero ? totalDuration : TimeSpan.FromSeconds(1);
    }

    public CompressionProgress? ProcessLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return null;

        line = line.Trim();

        // 1. Process key=value lines from -progress pipe:1
        int eqIndex = line.IndexOf('=');
        if (eqIndex > 0)
        {
            string key = line.Substring(0, eqIndex).Trim();
            string val = line.Substring(eqIndex + 1).Trim();

            switch (key)
            {
                case "frame":
                    long.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out _currentFrame);
                    break;
                case "fps":
                    double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out _currentFps);
                    break;
                case "out_time_us":
                    if (long.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out var us))
                    {
                        _currentTime = TimeSpan.FromMicroseconds(us);
                    }
                    break;
                case "out_time":
                    if (TimeSpan.TryParse(val, CultureInfo.InvariantCulture, out var parsedTime))
                    {
                        _currentTime = parsedTime;
                    }
                    break;
                case "bitrate":
                    _currentBitrate = val;
                    break;
                case "speed":
                    string speedClean = val.TrimEnd('x', 'X');
                    if (double.TryParse(speedClean, NumberStyles.Float, CultureInfo.InvariantCulture, out var spd))
                    {
                        _currentSpeed = spd > 0 ? spd : 1.0;
                    }
                    break;
                case "progress":
                    if (val.Equals("end", StringComparison.OrdinalIgnoreCase))
                    {
                        _isComplete = true;
                        _currentTime = _totalDuration;
                    }
                    return CreateSnapshot();
            }
        }

        // 2. Fallback: Parse stderr line format
        var match = StderrProgressRegex.Match(line);
        if (match.Success)
        {
            long.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _currentFrame);
            double.TryParse(match.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out _currentFps);
            if (TimeSpan.TryParse(match.Groups[3].Value, CultureInfo.InvariantCulture, out var t))
            {
                _currentTime = t;
            }
            _currentBitrate = match.Groups[4].Value;
            if (double.TryParse(match.Groups[5].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var spd))
            {
                _currentSpeed = spd > 0 ? spd : 1.0;
            }
            return CreateSnapshot();
        }

        return null;
    }

    public CompressionProgress CreateSnapshot()
    {
        double percentage = 0.0;
        if (_totalDuration.TotalSeconds > 0)
        {
            percentage = Math.Clamp((_currentTime.TotalSeconds / _totalDuration.TotalSeconds) * 100.0, 0.0, 100.0);
        }

        TimeSpan? eta = null;
        if (!_isComplete && _totalDuration > _currentTime && _currentSpeed > 0)
        {
            double remainingSeconds = (_totalDuration - _currentTime).TotalSeconds / _currentSpeed;
            if (remainingSeconds >= 0 && remainingSeconds < 86400 * 30)
            {
                eta = TimeSpan.FromSeconds(remainingSeconds);
            }
        }

        return new CompressionProgress(
            Frame: _currentFrame,
            Fps: _currentFps,
            CurrentTime: _currentTime,
            TotalDuration: _totalDuration,
            Percentage: _isComplete ? 100.0 : percentage,
            Speed: _currentSpeed,
            Bitrate: _currentBitrate,
            EstimatedTimeRemaining: _isComplete ? TimeSpan.Zero : eta,
            IsComplete: _isComplete
        );
    }
}
