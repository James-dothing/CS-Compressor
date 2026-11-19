# CS-Compressor

A modern, reliable, and user-friendly desktop application for video compression powered by FFmpeg and .NET 10.

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)
![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)
![Tests](https://img.shields.io/badge/tests-73%20passed-brightgreen.svg)

---

## Overview

**CS-Compressor** provides an intuitive, reliable graphical interface over FFmpeg, making it effortless to compress large videos, tweak video and audio codecs, scale resolutions, adjust frame rates, and trim video clips.

### Key Features

- **Intuitive Workflow**: Drag and drop video files directly into the application or use the file browser.
- **Auto-Detection**: Automatically detects system-installed FFmpeg/FFprobe binaries with fallback to embedded or local binaries.
- **Rich Codec Support**:
  - **Video**: H.264 (`libx264`), H.265/HEVC (`libx265`), AV1 (`libaom-av1`), VP8 (`libvpx`), VP9 (`libvpx-vp9`), animated GIF, and stream copy.
  - **Hardware Acceleration**: Auto-detects NVIDIA NVENC (`h264_nvenc`, `hevc_nvenc`, `av1_nvenc`), AMD AMF (`h264_amf`, `hevc_amf`, `av1_amf`), and Intel QSV (`h264_qsv`, `hevc_qsv`, `av1_qsv`).
  - **Audio**: AAC, MP3 (`libmp3lame`), Opus, FLAC, Vorbis, Speex, WAV (PCM), ALAC, and stream copy / disable audio.
- **Multiple Rate Control Modes**: Constant Bitrate (CBR), Variable Bitrate (VBR), Constant Quantization Parameter (CQP), and Constant Quality / CRF (CQ).
- **Video Filters**: Resolution scaling with aspect ratio matching, custom framerates, scaling algorithms (Lanczos, Bicubic, Bilinear, etc.), and palette-generated accurate color GIFs.
- **Precise Trimming**: Set start and end timestamps (HH:MM:SS) for clipping segments.
- **Real-Time Progress & Telemetry**: Live progress bar, percentage, current frame, FPS, speed multiplier, bitrate, and estimated time remaining (ETA).
- **Responsive Cancellation**: Safely cancel compression at any time without leaving zombie FFmpeg processes running in the background.
- **Non-Fatal Validation**: Proactive validation of file paths, codecs, and parameters with clear, user-friendly error messages.

---

## Architecture

The project is structured with clean separation between user interface and video processing logic:

```
CS-Compressor/
├── CsCompressor.sln / .slnx             # Solution files (.NET 10)
├── CsCompressor.csproj                  # WPF Presentation Application (net10.0-windows)
│   ├── Startup.xaml / .cs               # Quick launch & drag-and-drop window
│   ├── VideoProcessorWindow.xaml / .cs  # Full compression workspace
│   ├── FFmpegAPI.cs                     # Tool lifecycle & hardware encoder probe
│   └── QOLThings.cs                     # UI helper utilities
├── CsCompressor.Core/                   # Core Compression Engine (net10.0)
│   ├── Models/                          # CompressionOptions, MediaInfo, CompressionProgress, Enums
│   ├── Services/                        # FFmpegLocator, ProcessRunner, ProcessTracker,
│   │                                    # MediaProbeService, FFmpegArgumentBuilder, CodecHelper
│   └── Validation/                      # CompressionOptionsValidator, ValidationResult
└── tests/CsCompressor.Tests/            # Comprehensive automated test suite (xUnit)
    ├── BaselineCodecTests.cs            # Codec parsing & container compatibility tests
    ├── FFmpegLocatorTests.cs            # Binary detection & version parsing tests
    ├── ProcessRunnerTests.cs            # Process execution, pipe I/O & cancellation tests
    ├── ProcessTrackerTests.cs           # Process lifecycle & tree termination tests
    ├── FFmpegArgumentBuilderTests.cs    # CLI argument building & escaping tests
    ├── FFmpegProgressParserTests.cs     # Real-time progress & ETA calculation tests
    ├── CompressionOptionsValidatorTests.cs # Parameter validation tests
    └── FFmpegIntegrationTests.cs        # End-to-end transcoding tests with real FFmpeg
```

---

## Prerequisites

- **.NET SDK 10.0** or later ([Download .NET 10](https://dotnet.microsoft.com/download))
- **Operating System**: Windows 10/11 (for running the WPF GUI)
  - *Note*: `CsCompressor.Core` and the test suite run cross-platform on Windows, Linux, and macOS.
- **FFmpeg**:
  - The application automatically checks your system `PATH` for `ffmpeg` and `ffprobe`.
  - Alternatively, place `ffmpeg.exe` and `ffprobe.exe` in the application directory or `%APPDATA%\CSCompressor`.
  - On Windows, bundled extraction from embedded 7z archives is supported as an automatic fallback.

---

## Building and Running

### Build via .NET CLI

Clone the repository:
```bash
git clone https://github.com/James-dothing/CS-Compressor.git
cd CS-Compressor
```

Build the entire solution in Release configuration:
```bash
dotnet build -c Release CsCompressor.sln
```

### Run Tests

Execute the automated test suite (73 unit and integration tests):
```bash
dotnet test CsCompressor.sln
```

### Publish Single-File Executable (Windows x64)

```bash
dotnet publish CsCompressor.csproj -c Release -r win-x64 --self-contained false
```

The published executable will be located in:
`bin/Release/net10.0-windows/win-x64/publish/CsCompressor.exe`

---

## Application Usage Guide

### 1. Select Input File
- Drag and drop your video file onto the startup window or click **Browse...** to pick a video file.
- The application automatically probes the file and extracts its duration, resolution, video codec, audio codec, and file size.

### 2. Configure Output Destination & Format
- Select your desired output container format (MP4, MKV, WebM, MOV, GIF, MP3, etc.).
- The output file path automatically defaults to `<name>_compressed.<ext>` in the same directory. Click **Browse...** to customize the location.

### 3. Choose Video Codec & Quality
- **Video Encoder**: Choose a software encoder (e.g. `libx264`, `libx265`, `libvpx-vp9`) or hardware encoder (NVENC, AMF, QSV) if available. Choose `No encoding` (copy) to pass video streams through without re-encoding.
- **Rate Control**:
  - **CQ (Constant Quality / CRF)** *(Recommended)*: Quality-based encoding. Lower values yield higher quality and larger files (default: 23 for H.264, 28 for H.265).
  - **CBR (Constant Bitrate)**: Forces a fixed bitrate (e.g. `2M` or `2500k`), useful for streaming or strict bandwidth limits.
  - **VBR (Variable Bitrate)**: Balances quality with average and maximum bitrates.
  - **CQP (Constant Quantization Parameter)**: Fixed QP across frames.
- **Encoding Speed / Preset**: Choose from presets (`ultrafast` through `veryslow`). Slower presets achieve better compression efficiency at the expense of encoding time.

### 4. Adjust Video Filters (Optional)
- **Resolution**: Type a target Width × Height or click **Match Source** to populate the original resolution.
- **Framerate**: Set a target framerate (e.g. `30` or `60`).
- **Scaling Algorithm**: Choose the scaling filter (default: `area` or `lanczos`).
- **Accurate GIF color**: For GIF outputs, enables two-pass palette generation for optimal color reproduction.

### 5. Audio Settings
- Select an audio codec (e.g. `aac`, `libmp3lame`, `libopus`, `flac`) or select `No encoding` to copy audio.
- Set target audio bitrate (e.g. `128k`, `192k`, `320k`) and channel count (1 for mono, 2 for stereo).
- Check **Disable audio** to remove the audio track entirely.

### 6. Trims (Optional)
- Enter Start Time (`HH:MM:SS`) and End Time (`HH:MM:SS`) to trim the video.

### 7. Compress & Monitor
- Click **Compress** to start processing.
- Live progress shows the percentage, current time, frame count, processing speed, and estimated time remaining (ETA).
- To view raw FFmpeg output, check **Show FFmpeg output log**.
- Click **Cancel** at any time to abort compression immediately and cleanly terminate FFmpeg.

---

## Troubleshooting

| Issue | Resolution |
| :--- | :--- |
| **"FFmpeg executable not found"** | Ensure FFmpeg is installed and added to your system `PATH`. Open a terminal and verify with `ffmpeg -version`. Alternatively, place `ffmpeg.exe` and `ffprobe.exe` into the application directory. |
| **Hardware encoder not available** | Ensure your GPU drivers (NVIDIA, AMD, or Intel) are up to date. Hardware encoders appear automatically in the video encoder list if supported by your hardware. |
| **"Resolution contains odd dimensions"** | Most video codecs (such as H.264 using YUV420p) require width and height to be divisible by 2. Ensure both dimensions are even numbers (e.g. 1920×1080 instead of 1920×1081). |
| **Compression cancelled by user** | Partial output files from cancelled operations can be safely deleted or replaced by starting a new compression. |

---

## Contributing

Pull requests, bug reports, and suggestions are welcome!
1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/my-feature`).
3. Ensure all tests pass (`dotnet test CsCompressor.sln`).
4. Commit your changes following Conventional Commits (`git commit -m 'feat: add feature'`).
5. Push to your branch and open a Pull Request.

---

## License

This project is licensed under the [MIT License](LICENSE).
