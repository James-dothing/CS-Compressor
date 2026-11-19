using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using CsCompressor.Core.Models;
using CsCompressor.Core.Services;
using CsCompressor.Core.Validation;

namespace VideoProcessor
{
    public partial class VideoProcessingWindow : Window
    {
        private bool isInit = false;
        private string[] EncoderArray = new string[] { "libx264", "libx265", "libaom_av1", "libvpx", "libvpx_vp9", "gif" };
        private string path = "";
        private MediaInfo? currentMediaInfo;
        private CancellationTokenSource? compressionCts;

        public VideoProcessingWindow()
        {
            InitializeComponent();
            isInit = true;
            RateControlHandler();
            if (OutputFormatCB.SelectedIndex < 0)
            {
                OutputFormatCB.SelectedIndex = 0; // Default to MP4
            }
        }

        public void SetPath(string patharg)
        {
            if (string.IsNullOrWhiteSpace(patharg)) return;

            path = patharg;
            pathTB.Text = patharg;

            // Automatically set default output path
            try
            {
                var dir = Path.GetDirectoryName(patharg) ?? "";
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(patharg);
                var currentFormatStr = QOL.GetStringFromComboBox(OutputFormatCB) ?? "MP4";
                var ext = currentFormatStr.ToLowerInvariant();
                OutputPathTB.Text = Path.Combine(dir, $"{fileNameWithoutExt}_compressed.{ext}");
            }
            catch
            {
                // Ignore path calculation errors
            }

            // Asynchronously probe input media info
            _ = LoadMediaInfoAsync(patharg);
        }

        private async Task LoadMediaInfoAsync(string filePath)
        {
            try
            {
                MediaInfoTextBlock.Text = "Probing media info...";
                var info = await MediaProbeService.ProbeAsync(filePath, FFmpegAPI.ffprobePath);
                currentMediaInfo = info;

                if (info != null)
                {
                    var dur = info.Duration > TimeSpan.Zero ? info.FormattedDuration : "Unknown duration";
                    var res = info.Width > 0 && info.Height > 0 ? $"{info.Width}×{info.Height}" : "Unknown resolution";
                    var fps = info.Framerate > 0 ? $" ({info.Framerate:0.##} fps)" : "";
                    var vcodec = !string.IsNullOrEmpty(info.VideoCodec) ? info.VideoCodec : "unknown";
                    var acodec = !string.IsNullOrEmpty(info.AudioCodec) ? info.AudioCodec : "none";

                    MediaInfoTextBlock.Text = $"Source: {dur} | {res}{fps} | Video: {vcodec} | Audio: {acodec} | {info.FormattedFileSize}";

                    // Populate trims end time if duration known
                    if (info.Duration > TimeSpan.Zero && EndHourTB.Text == "--")
                    {
                        EndHourTB.Text = $"{(int)info.Duration.TotalHours:00}";
                        EndMinuteTB.Text = $"{info.Duration.Minutes:00}";
                        EndSecondTB.Text = $"{info.Duration.Seconds:00}";
                    }
                }
                else
                {
                    MediaInfoTextBlock.Text = "Unable to probe media info.";
                }
            }
            catch (Exception ex)
            {
                MediaInfoTextBlock.Text = $"Probe error: {ex.Message}";
            }
        }

        public void UpdateVideoEncoders(string[] finalList)
        {
            EncoderArray = finalList;
            ReplaceOutput();
        }

        public void RateControlHandler()
        {
            if (!isInit) return;

            switch (QOL.GetStringFromComboBox(VideoRateControlCB))
            {
                case "CBR":
                    VideoBitrateTB.IsEnabled = true;
                    CQTB.IsEnabled = false;
                    MinimumVideoBitrateTB.IsEnabled = false;
                    MaximumVideoBitrateTB.IsEnabled = false;
                    BufferSizeTB.IsEnabled = true;
                    break;
                case "VBR":
                    VideoBitrateTB.IsEnabled = true;
                    CQTB.IsEnabled = false;
                    MinimumVideoBitrateTB.IsEnabled = true;
                    MaximumVideoBitrateTB.IsEnabled = true;
                    BufferSizeTB.IsEnabled = true;
                    break;
                case "CQP":
                    VideoBitrateTB.IsEnabled = false;
                    CQTB.IsEnabled = true;
                    MinimumVideoBitrateTB.IsEnabled = false;
                    MaximumVideoBitrateTB.IsEnabled = false;
                    BufferSizeTB.IsEnabled = false;
                    break;
                case "CQ":
                    VideoBitrateTB.IsEnabled = false;
                    CQTB.IsEnabled = true;
                    MinimumVideoBitrateTB.IsEnabled = false;
                    MaximumVideoBitrateTB.IsEnabled = false;
                    BufferSizeTB.IsEnabled = false;
                    break;
                default:
                    VideoRateControlCB.SelectedIndex = 0;
                    RateControlHandler();
                    break;
            }
        }

        public void ReplaceOutput()
        {
            if (!isInit) return;

            string? outputFormatStr = QOL.GetStringFromComboBox(OutputFormatCB);
            if (string.IsNullOrEmpty(outputFormatStr)) return;

            var format = CodecHelper.ParseContainerFormat(outputFormatStr);
            if (!format.HasValue) return;

            bool isAudioOnly = CodecHelper.IsAudioOnlyFormat(format.Value);

            if (!isAudioOnly)
            {
                VideoEncoderCB.IsEnabled = true;
                VideoRateControlCB.IsEnabled = true;
                RateControlHandler();
                WidthTB.IsEnabled = true;
                HeightTB.IsEnabled = true;
                FramerateTB.IsEnabled = true;
                VideoScalingFlagCB.IsEnabled = true;
                VideoEncodingSpeedCB.IsEnabled = true;
                CPUUsedCB.IsEnabled = true;
                AccurateColorGifCB.IsEnabled = (format.Value == ContainerFormat.Gif);

                VideoEncoderCB.Items.Clear();
                foreach (string b in EncoderArray)
                {
                    var codec = CodecHelper.ParseVideoCodec(b);
                    if (codec.HasValue && CodecHelper.IsVideoCompatible(codec.Value, format.Value))
                    {
                        VideoEncoderCB.Items.Add(b);
                    }
                }
                VideoEncoderCB.Items.Add("No encoding");
                if (VideoEncoderCB.Items.Count > 0)
                {
                    VideoEncoderCB.SelectedIndex = 0;
                }

                if (format.Value == ContainerFormat.Gif)
                {
                    AccurateColorGifCB.IsEnabled = true;
                    AccurateColorGifCB.IsChecked = true;
                    DisableAudioCB.IsEnabled = false;
                    DisableAudioCB.IsChecked = true;
                    AudioEncoderCB.IsEnabled = false;
                    AudioBitrateTB.IsEnabled = false;
                    AudioChannelAmountCB.IsEnabled = false;

                    VideoRateControlCB.IsEnabled = false;
                    VideoBitrateTB.IsEnabled = false;
                    CQTB.IsEnabled = false;
                    BufferSizeTB.IsEnabled = false;
                    MinimumVideoBitrateTB.IsEnabled = false;
                    MaximumVideoBitrateTB.IsEnabled = false;
                }
                else
                {
                    AccurateColorGifCB.IsEnabled = false;
                    AccurateColorGifCB.IsChecked = false;
                    DisableAudioCB.IsEnabled = true;
                    AudioEncoderCB.IsEnabled = true;
                    AudioBitrateTB.IsEnabled = true;
                    AudioChannelAmountCB.IsEnabled = true;
                }
            }
            else
            {
                // Audio only format
                VideoEncoderCB.IsEnabled = false;
                VideoRateControlCB.IsEnabled = false;
                VideoBitrateTB.IsEnabled = false;
                CQTB.IsEnabled = false;
                BufferSizeTB.IsEnabled = false;
                MinimumVideoBitrateTB.IsEnabled = false;
                MaximumVideoBitrateTB.IsEnabled = false;
                WidthTB.IsEnabled = false;
                HeightTB.IsEnabled = false;
                FramerateTB.IsEnabled = false;
                VideoScalingFlagCB.IsEnabled = false;
                VideoEncodingSpeedCB.IsEnabled = false;
                CPUUsedCB.IsEnabled = false;
                AccurateColorGifCB.IsEnabled = false;

                DisableAudioCB.IsEnabled = false;
                DisableAudioCB.IsChecked = false;
                AudioEncoderCB.IsEnabled = true;
                AudioBitrateTB.IsEnabled = true;
                AudioChannelAmountCB.IsEnabled = true;
            }

            // Populate Audio Encoders dynamically based on container format
            PopulateAudioEncoders(format.Value);
        }

        private void PopulateAudioEncoders(ContainerFormat format)
        {
            AudioEncoderCB.Items.Clear();
            var compatible = CodecHelper.GetCompatibleAudioCodecs(format);
            foreach (var codec in compatible)
            {
                AudioEncoderCB.Items.Add(CodecHelper.ToFFmpegAudioCodecName(codec));
            }

            if (AudioEncoderCB.Items.Count > 0)
            {
                int defaultIndex = 0;
                for (int i = 0; i < AudioEncoderCB.Items.Count; i++)
                {
                    var itemStr = AudioEncoderCB.Items[i]?.ToString();
                    if (itemStr == "aac" || itemStr == "libmp3lame" || itemStr == "libopus")
                    {
                        defaultIndex = i;
                        break;
                    }
                }
                AudioEncoderCB.SelectedIndex = defaultIndex;
            }
        }

        private void OutputFormatChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInit) return;
            ReplaceOutput();

            // Update output path extension
            var currentOutput = OutputPathTB.Text?.Trim();
            var formatStr = QOL.GetStringFromComboBox(OutputFormatCB);
            if (!string.IsNullOrEmpty(currentOutput) && !string.IsNullOrEmpty(formatStr))
            {
                try
                {
                    var ext = formatStr.ToLowerInvariant();
                    var dir = Path.GetDirectoryName(currentOutput) ?? "";
                    var name = Path.GetFileNameWithoutExtension(currentOutput);
                    OutputPathTB.Text = Path.Combine(dir, $"{name}.{ext}");
                }
                catch
                {
                    // Ignore path errors
                }
            }
        }

        private void VideoEncoderChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInit) return;
            string? encStr = QOL.GetStringFromComboBox(VideoEncoderCB);
            if (string.IsNullOrEmpty(encStr)) return;

            if (encStr == "No encoding")
            {
                VideoRateControlCB.IsEnabled = false;
                VideoBitrateTB.IsEnabled = false;
                CQTB.IsEnabled = false;
                MinimumVideoBitrateTB.IsEnabled = false;
                MaximumVideoBitrateTB.IsEnabled = false;
                BufferSizeTB.IsEnabled = false;
                VideoEncodingSpeedCB.IsEnabled = false;
                CPUUsedCB.IsEnabled = false;
                return;
            }

            var codec = CodecHelper.ParseVideoCodec(encStr);
            if (!codec.HasValue) return;

            VideoRateControlCB.IsEnabled = true;
            RateControlHandler();
            VideoEncodingSpeedCB.IsEnabled = true;

            VideoEncodingSpeedCB.Items.Clear();
            var presets = CodecHelper.GetSupportedPresets(codec.Value);
            foreach (var p in presets)
            {
                VideoEncodingSpeedCB.Items.Add(p.ToString().ToLowerInvariant());
            }

            if (VideoEncodingSpeedCB.Items.Count > 0)
            {
                int defIndex = 0;
                for (int i = 0; i < VideoEncodingSpeedCB.Items.Count; i++)
                {
                    var str = VideoEncodingSpeedCB.Items[i]?.ToString();
                    if (str == "medium" || str == "good" || str == "balanced" || str == "p4")
                    {
                        defIndex = i;
                        break;
                    }
                }
                VideoEncodingSpeedCB.SelectedIndex = defIndex;
            }

            // CPU-used is supported for AV1, VP8, VP9
            CPUUsedCB.IsEnabled = codec is VideoCodec.LibaomAv1 or VideoCodec.Libvpx or VideoCodec.LibvpxVp9;
        }

        private void RateControlChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInit) return;
            RateControlHandler();
        }

        private void InputBrowse(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Input Video",
                Filter = "Video Files (*.mp4;*.mkv;*.mov;*.webm;*.avi;*.flv;*.wmv)|*.mp4;*.mkv;*.mov;*.webm;*.avi;*.flv;*.wmv|All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                SetPath(dialog.FileName);
            }
        }

        private void OutputBrowse(object sender, RoutedEventArgs e)
        {
            string? o = QOL.GetStringFromComboBox(OutputFormatCB);
            if (string.IsNullOrEmpty(o))
            {
                MessageBoxAPI.Error("Please select an output format first.", Crash: false);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Select Output Destination",
                Filter = $"{o} files (*.{o.ToLower()})|*.{o.ToLower()}|All files (*.*)|*.*",
                FileName = Path.GetFileName(OutputPathTB.Text)
            };

            if (dialog.ShowDialog() == true)
            {
                OutputPathTB.Text = dialog.FileName;
            }
        }

        private void MatchSourceResolution(object sender, RoutedEventArgs e)
        {
            if (currentMediaInfo != null && currentMediaInfo.Width > 0 && currentMediaInfo.Height > 0)
            {
                WidthTB.Text = currentMediaInfo.Width.ToString();
                HeightTB.Text = currentMediaInfo.Height.ToString();
            }
            else
            {
                MessageBoxAPI.Debug("Source resolution is not available.", Crash: false);
            }
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    SetPath(files[0]);
                }
            }
        }

        public CompressionOptions CollectOptionsFromUI()
        {
            var options = new CompressionOptions
            {
                InputPath = pathTB.Text?.Trim() ?? string.Empty,
                OutputPath = OutputPathTB.Text?.Trim() ?? string.Empty,
                FFprobePath = FFmpegAPI.ffprobePath,
                CopyMetadata = CopyMetaDataCB.IsChecked ?? true,
                DisableAudio = DisableAudioCB.IsChecked ?? false,
                AccurateColorGif = AccurateColorGifCB.IsChecked ?? false,
                EnableProgressReporting = true
            };

            var formatStr = QOL.GetStringFromComboBox(OutputFormatCB);
            if (!string.IsNullOrEmpty(formatStr))
            {
                var fmt = CodecHelper.ParseContainerFormat(formatStr);
                if (fmt.HasValue) options.OutputFormat = fmt.Value;
            }

            var videoEncStr = QOL.GetStringFromComboBox(VideoEncoderCB);
            if (!string.IsNullOrEmpty(videoEncStr) && videoEncStr != "-")
            {
                options.VideoCodec = CodecHelper.ParseVideoCodec(videoEncStr);
            }

            var rateCtrlStr = QOL.GetStringFromComboBox(VideoRateControlCB);
            if (!string.IsNullOrEmpty(rateCtrlStr))
            {
                options.RateControl = CodecHelper.ParseRateControlMode(rateCtrlStr);
            }

            options.VideoBitrate = VideoBitrateTB.Text?.Trim();
            options.CQConstant = CQTB.Text?.Trim();
            options.QPConstant = CQTB.Text?.Trim();
            options.MinVideoBitrate = MinimumVideoBitrateTB.Text?.Trim();
            options.MaxVideoBitrate = MaximumVideoBitrateTB.Text?.Trim();
            options.BufferSize = BufferSizeTB.Text?.Trim();

            var presetStr = QOL.GetStringFromComboBox(VideoEncodingSpeedCB);
            if (!string.IsNullOrEmpty(presetStr))
            {
                options.Preset = CodecHelper.ParsePreset(presetStr);
            }

            var cpuStr = QOL.GetStringFromComboBox(CPUUsedCB);
            if (!string.IsNullOrEmpty(cpuStr))
            {
                if (cpuStr.StartsWith("Level ") && int.TryParse(cpuStr.Substring(6), out var lvl))
                {
                    options.CpuUsed = lvl;
                }
                else if (int.TryParse(cpuStr, out var directLvl))
                {
                    options.CpuUsed = directLvl;
                }
            }

            // Resolution
            if (int.TryParse(WidthTB.Text?.Trim(), out var w)) options.TargetWidth = w;
            if (int.TryParse(HeightTB.Text?.Trim(), out var h)) options.TargetHeight = h;

            if (double.TryParse(FramerateTB.Text?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var fps))
            {
                options.Framerate = fps;
            }

            var scaleStr = QOL.GetStringFromComboBox(VideoScalingFlagCB);
            if (!string.IsNullOrEmpty(scaleStr))
            {
                options.ScalingAlgorithm = CodecHelper.ParseScalingAlgorithm(scaleStr);
            }

            // Audio
            var audioEncStr = QOL.GetStringFromComboBox(AudioEncoderCB);
            if (!string.IsNullOrEmpty(audioEncStr) && audioEncStr != "No encoding")
            {
                options.AudioCodec = CodecHelper.ParseAudioCodec(audioEncStr);
            }

            options.AudioBitrate = AudioBitrateTB.Text?.Trim();

            var chStr = QOL.GetStringFromComboBox(AudioChannelAmountCB);
            if (int.TryParse(chStr, out var ch))
            {
                options.AudioChannels = ch;
            }

            // Trims
            if (int.TryParse(StartHourTB.Text, out var sh) &&
                int.TryParse(StartMinuteTB.Text, out var sm) &&
                int.TryParse(StartSecondTB.Text, out var ss))
            {
                var st = new TimeSpan(sh, sm, ss);
                if (st > TimeSpan.Zero) options.StartTime = st;
            }

            if (int.TryParse(EndHourTB.Text, out var eh) &&
                int.TryParse(EndMinuteTB.Text, out var em) &&
                int.TryParse(EndSecondTB.Text, out var es))
            {
                options.EndTime = new TimeSpan(eh, em, es);
            }

            return options;
        }

        private async void StartCompression(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FFmpegAPI.ffmpegPath) || !File.Exists(FFmpegAPI.ffmpegPath))
            {
                MessageBoxAPI.Error("FFmpeg executable was not found. Please install FFmpeg or place ffmpeg.exe in the application folder.", Crash: false);
                return;
            }

            var options = CollectOptionsFromUI();

            // Run validation
            var validation = CompressionOptionsValidator.Validate(options);
            if (!validation.IsValid)
            {
                MessageBoxAPI.Error($"Cannot start compression due to the following issues:\n\n{validation.GetCombinedErrorMessage()}", Crash: false);
                return;
            }

            // If destination directory doesn't exist, create it
            try
            {
                var outDir = Path.GetDirectoryName(options.OutputPath);
                if (!string.IsNullOrEmpty(outDir) && !Directory.Exists(outDir))
                {
                    Directory.CreateDirectory(outDir);
                }
            }
            catch (Exception ex)
            {
                MessageBoxAPI.Error($"Failed to create output directory:\n{ex.Message}", Crash: false);
                return;
            }

            // Set UI to busy compression state
            Go.IsEnabled = false;
            CancelButton.IsEnabled = true;
            CompressionProgressBar.Value = 0;
            StatusTextBlock.Text = "Preparing FFmpeg compression...";
            LogTextBox.Clear();

            if (EnableLog.IsChecked == true)
            {
                LogExpander.Visibility = Visibility.Visible;
                LogExpander.IsExpanded = true;
            }
            else
            {
                LogExpander.Visibility = Visibility.Collapsed;
            }

            compressionCts = new CancellationTokenSource();

            // Calculate target duration for percentage computation
            TimeSpan duration = TimeSpan.FromSeconds(1);
            if (currentMediaInfo != null && currentMediaInfo.Duration > TimeSpan.Zero)
            {
                duration = currentMediaInfo.Duration;
                if (options.StartTime.HasValue && options.EndTime.HasValue)
                {
                    duration = options.EndTime.Value - options.StartTime.Value;
                }
                else if (options.StartTime.HasValue)
                {
                    duration = currentMediaInfo.Duration - options.StartTime.Value;
                }
                else if (options.EndTime.HasValue)
                {
                    duration = options.EndTime.Value;
                }
            }

            var progressParser = new FFmpegProgressParser(duration);

            try
            {
                var argList = FFmpegArgumentBuilder.BuildArgumentList(options);

                // Run FFmpeg asynchronously
                var result = await ProcessAPI.StartAsync(
                    fileName: FFmpegAPI.ffmpegPath,
                    argumentList: argList,
                    onOutputLine: line =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var progress = progressParser.ProcessLine(line);
                            if (progress != null)
                            {
                                CompressionProgressBar.Value = progress.Percentage;
                                var fpsStr = progress.Fps > 0 ? $" | {progress.Fps:0.#} fps" : "";
                                var speedStr = progress.Speed > 0 ? $" | {progress.Speed:0.##}x" : "";
                                StatusTextBlock.Text = $"Compressing: {progress.FormattedPercentage} ({progress.FormattedCurrentTime} / {progress.FormattedTotalDuration}){fpsStr}{speedStr} | ETA: {progress.FormattedEta}";
                            }

                            if (EnableLog.IsChecked == true && !string.IsNullOrWhiteSpace(line))
                            {
                                if (LogTextBox.LineCount > 100)
                                {
                                    LogTextBox.Text = LogTextBox.Text.Substring(LogTextBox.Text.Length / 2);
                                }
                                LogTextBox.AppendText(line + "\n");
                                LogTextBox.ScrollToEnd();
                            }
                        });
                    },
                    onErrorLine: line =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var progress = progressParser.ProcessLine(line);
                            if (progress != null)
                            {
                                CompressionProgressBar.Value = progress.Percentage;
                                StatusTextBlock.Text = $"Compressing: {progress.FormattedPercentage} | ETA: {progress.FormattedEta}";
                            }

                            if (EnableLog.IsChecked == true && !string.IsNullOrWhiteSpace(line))
                            {
                                if (LogTextBox.LineCount > 100)
                                {
                                    LogTextBox.Text = LogTextBox.Text.Substring(LogTextBox.Text.Length / 2);
                                }
                                LogTextBox.AppendText(line + "\n");
                                LogTextBox.ScrollToEnd();
                            }
                        });
                    },
                    cancellationToken: compressionCts.Token
                );

                if (result.WasCancelled)
                {
                    StatusTextBlock.Text = "Compression was cancelled by user.";
                    CompressionProgressBar.Value = 0;
                    MessageBoxAPI.Debug("Compression cancelled by user.", Crash: false);
                }
                else if (result.IsSuccess)
                {
                    CompressionProgressBar.Value = 100;
                    StatusTextBlock.Text = "Compression completed successfully!";

                    // Compute statistics
                    string sizeSummary = "";
                    try
                    {
                        var originalBytes = new FileInfo(options.InputPath).Length;
                        var compressedBytes = new FileInfo(options.OutputPath).Length;
                        double reduction = (1.0 - ((double)compressedBytes / originalBytes)) * 100.0;
                        sizeSummary = $"\nOriginal size: {FormatBytes(originalBytes)}\nCompressed size: {FormatBytes(compressedBytes)} ({reduction:0.#}% reduction)";
                    }
                    catch
                    {
                        // Ignore stats calculation
                    }

                    MessageBoxAPI.Debug($"Video compressed successfully in {result.Duration.TotalSeconds:0.#}s!{sizeSummary}\n\nSaved to:\n{options.OutputPath}", Crash: false);
                }
                else
                {
                    StatusTextBlock.Text = $"Compression failed (exit code {result.ExitCode}).";
                    MessageBoxAPI.Error($"Compression failed with exit code {result.ExitCode}:\n\n{result.GetFormattedErrorMessage()}", Crash: false);
                }
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = "Error executing compression.";
                MessageBoxAPI.Error($"An unexpected error occurred:\n{ex.Message}", Crash: false);
            }
            finally
            {
                Go.IsEnabled = true;
                CancelButton.IsEnabled = false;
                compressionCts?.Dispose();
                compressionCts = null;
            }
        }

        private void CancelCompression(object sender, RoutedEventArgs e)
        {
            if (compressionCts != null && !compressionCts.IsCancellationRequested)
            {
                StatusTextBlock.Text = "Cancelling compression...";
                CancelButton.IsEnabled = false;
                compressionCts.Cancel();
            }
        }

        private static string FormatBytes(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB" };
            double size = bytes;
            int unit = 0;
            while (size >= 1024 && unit < units.Length - 1)
            {
                size /= 1024;
                unit++;
            }
            return $"{size:0.##} {units[unit]}";
        }
    }
}
