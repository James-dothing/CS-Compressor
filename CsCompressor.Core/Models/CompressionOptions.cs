namespace CsCompressor.Core.Models;

public class CompressionOptions
{
    public string InputPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public ContainerFormat OutputFormat { get; set; } = ContainerFormat.Mp4;
    public string? FFprobePath { get; set; }

    // Video settings
    public VideoCodec? VideoCodec { get; set; }
    public RateControlMode? RateControl { get; set; }
    public string? VideoBitrate { get; set; }
    public string? CQConstant { get; set; }
    public string? QPConstant { get; set; }
    public string? MinVideoBitrate { get; set; }
    public string? MaxVideoBitrate { get; set; }
    public string? BufferSize { get; set; }
    public EncoderPreset? Preset { get; set; }
    public int? CpuUsed { get; set; }

    // Video filters
    public int? TargetWidth { get; set; }
    public int? TargetHeight { get; set; }
    public double? Framerate { get; set; }
    public ScalingAlgorithm? ScalingAlgorithm { get; set; }
    public bool AccurateColorGif { get; set; }

    // Audio settings
    public bool DisableAudio { get; set; }
    public AudioCodec? AudioCodec { get; set; }
    public string? AudioBitrate { get; set; }
    public int? AudioChannels { get; set; }

    // Trimming
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }

    // Metadata
    public bool CopyMetadata { get; set; } = true;

    // Execution flags
    public bool IsTestFile { get; set; }
    public string? LoggingMode { get; set; }
    public bool EnableProgressReporting { get; set; } = true;
}
