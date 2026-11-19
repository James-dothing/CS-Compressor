namespace CsCompressor.Core.Models;

public enum VideoCodec
{
    Copy,
    Gif,
    Libx264,
    Libx265,
    LibaomAv1,
    Libvpx,
    LibvpxVp9,
    H264Nvenc,
    HevcNvenc,
    Av1Nvenc,
    H264Amf,
    HevcAmf,
    Av1Amf,
    H264Qsv,
    HevcQsv,
    Av1Qsv
}

public enum AudioCodec
{
    Copy,
    Aac,
    Flac,
    Libmp3lame,
    Libvorbis,
    Libopus,
    Libspeex,
    PcmS16le,
    PcmS24le,
    PcmS32le,
    PcmF32le,
    PcmF64le,
    PcmAlaw,
    Alac
}

public enum ContainerFormat
{
    Mp4,
    Mov,
    Gif,
    Mkv,
    Webm,
    Ogg,
    Aac,
    Wav,
    Flac,
    Mp3
}

public enum RateControlMode
{
    Cbr,
    Vbr,
    Cqp,
    Cq
}

public enum EncoderPreset
{
    Ultrafast,
    Superfast,
    Veryfast,
    Faster,
    Fast,
    Medium,
    Slow,
    Slower,
    Veryslow,
    Placebo,
    Best,
    Good,
    Realtime,
    P1,
    P2,
    P3,
    P4,
    P5,
    P6,
    P7,
    Speed,
    Balanced,
    Quality
}

public enum ScalingAlgorithm
{
    FastBilinear,
    Bilinear,
    Bicubic,
    Neighbor,
    Area,
    Bicublin,
    Gauss,
    Sinc,
    Lanczos,
    Spline
}
