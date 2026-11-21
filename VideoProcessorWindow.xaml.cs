// using System.IO;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace VideoProcessor
{
    public partial class VideoProcessingWindow : Window
    {
        private bool isInit = false;
        private string[] EncoderArray = new string[] {};
        public VideoProcessingWindow()
        {
            InitializeComponent();
            isInit = true;
            RateControlHandler();
        }
        private string path = "";
        public void SetPath(string patharg)
        {
            path = patharg;
            pathTB.Text = patharg;
        }
        public void UpdateVideoEncoders(string[] finalList)
        {
            EncoderArray = finalList; //incase some weird thing happen
            ReplaceOutput();
        }
        public void RateControlHandler()
        {
            if (!isInit) {return;}
            switch (QOL.GetStringFromComboBox(VideoRateControlCB))
            {
                case ("CBR"):
                    VideoBitrateTB.IsEnabled = true;
                    CQTB.IsEnabled = false;
                    MinimumVideoBitrateTB.IsEnabled = false;
                    MaximumVideoBitrateTB.IsEnabled = false;
                    BufferSizeTB.IsEnabled = true;
                    break;
                case ("VBR"):
                    VideoBitrateTB.IsEnabled = true;
                    CQTB.IsEnabled = false;
                    MinimumVideoBitrateTB.IsEnabled = true;
                    MaximumVideoBitrateTB.IsEnabled = true;
                    BufferSizeTB.IsEnabled = true;
                    break;
                case ("CQP"):
                    VideoBitrateTB.IsEnabled = false;
                    CQTB.IsEnabled = true;
                    MinimumVideoBitrateTB.IsEnabled = false;
                    MaximumVideoBitrateTB.IsEnabled = false;
                    BufferSizeTB.IsEnabled = false;
                    break;
                case ("CQ"):
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
            if (!isInit) {return;}
        
            string? outputFormat = QOL.GetStringFromComboBox(OutputFormatCB);
            // _ = MessageBoxAPI.DebugAsync(outputFormat);
            if (outputFormat != null)
            {
                 //lowk gave up on naming variable ;; uses too much brain power (:
                FFmpegArg.supportedOutputFormats? a = FFmpegArg.StringToOutputFormat(outputFormat);
                // _ = MessageBoxAPI.DebugAsync(a.ToString() ?? "NULL");
                if (a!=null)
                {
                    if (FFmpegArg.IsFormatVideo((FFmpegArg.supportedOutputFormats)a))
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
                        AccurateColorGifCB.IsEnabled = false;
                        VideoEncoderCB.Items.Clear();
                        foreach (string b in EncoderArray)
                        {
                            FFmpegArg.supportedVideoEncoders? c = FFmpegArg.StringToVideoEncoder(b);
                            if (c!=null)
                            {
                                if (FFmpegArg.CheckVideoCompatibility((FFmpegArg.supportedVideoEncoders)c, (FFmpegArg.supportedOutputFormats)a) ?? false)
                                {
                                    // _ = MessageBoxAPI.DebugAsync(b);
                                    VideoEncoderCB.Items.Add(b);
                                }
                            }
                        }
                        VideoEncoderCB.Items.Add("No encoding");
                        VideoEncoderCB.SelectedIndex = 0;
                        switch (a.ToString())
                        {
                            case ("GIF"):
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
                                break;
                            default:
                                AccurateColorGifCB.IsEnabled = false;
                                AccurateColorGifCB.IsChecked = false;
                                if (!DisableAudioCB.IsEnabled) {DisableAudioCB.IsChecked = false;}
                                DisableAudioCB.IsEnabled = true;
                                AudioEncoderCB.IsEnabled = true;
                                AudioBitrateTB.IsEnabled = true;
                                AudioChannelAmountCB.IsEnabled = true;
                                VideoRateControlCB.IsEnabled = true;
                                RateControlHandler();
                                break;
                        }
                    } else
                    {
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
                        if (!DisableAudioCB.IsEnabled) {DisableAudioCB.IsChecked = false;}
                        DisableAudioCB.IsEnabled = true;
                        AudioEncoderCB.IsEnabled = true;
                        AudioBitrateTB.IsEnabled = true;
                        AudioChannelAmountCB.IsEnabled = true;
                    }

                    // AUDIO ######################################################

                    if (a == FFmpegArg.supportedOutputFormats.GIF) {return;} // GIFs doesn't have audio lol
                    AudioEncoderCB.Items.Clear();
                    FFmpegArg.supportedAudioEncoders[] allAudioEncoders = FFmpegArg.GetAllCompatibleAudioEncoders((FFmpegArg.supportedOutputFormats)a);
                    foreach (FFmpegArg.supportedAudioEncoders audioEncoder in allAudioEncoders)
                    {
                        AudioEncoderCB.Items.Add(audioEncoder.ToString());
                    }
                    AudioEncoderCB.Items.Add("No encoding");
                    AudioEncoderCB.SelectedIndex = 0;

                }
            }
        
        }
        private void OutputFormatChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInit) {return;}
            ReplaceOutput();
            OutputPathTB.Text = "";
        }
        private void VideoEncoderChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInit) {return;}
            string? a = VideoEncoderCB.SelectedItem?.ToString();
            if (a==null) {return;}
            if (a == "No encoding" || a == "gif")
            {
                VideoRateControlCB.IsEnabled = false;
                VideoBitrateTB.IsEnabled = false;
                CQTB.IsEnabled = false;
                MinimumVideoBitrateTB.IsEnabled = false;
                MaximumVideoBitrateTB.IsEnabled = false;
                BufferSizeTB.IsEnabled = false;
                VideoEncodingSpeedCB.IsEnabled = false;
                CPUUsedCB.IsEnabled = false;
            } else
            {
                VideoRateControlCB.IsEnabled = true;
                RateControlHandler();
                VideoEncodingSpeedCB.IsEnabled = true;
                FFmpegArg.supportedVideoEncoders? b = FFmpegArg.StringToVideoEncoder(a);
                if (b==null) {return;}
                if (b==FFmpegArg.supportedVideoEncoders.libaom_av1)
                {
                    VideoEncodingSpeedCB.IsEnabled = false;
                } else
                {
                    VideoEncodingSpeedCB.IsEnabled = true;
                    VideoEncodingSpeedCB.Items.Clear();
                    foreach (FFmpegArg.encodingPresets c in FFmpegArg.GetSupportedEncoderPresets((FFmpegArg.supportedVideoEncoders)b))
                    {
                        VideoEncodingSpeedCB.Items.Add(c.ToString());
                    }
                }

                
                CPUUsedCB.IsEnabled = FFmpegArg.CheckCpuUsedCompatibility((FFmpegArg.supportedVideoEncoders)b);
            }
        }
        private void RateControlChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInit) {return;}
            RateControlHandler();
        }
        private void AudioEncoderChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInit) {return;}
            string? AudioEncoder = AudioEncoderCB.SelectedItem?.ToString();
            if (AudioEncoder==null) {return;}
            if (AudioEncoder == "No encoding")
            {
                AudioBitrateTB.IsEnabled = false;
                AudioChannelAmountCB.IsEnabled = false;
            } else
            {
                AudioBitrateTB.IsEnabled = true;
                AudioChannelAmountCB.IsEnabled = true;
            }
        }
        private void InputBrowse(object sender, RoutedEventArgs e)
        {
            if (!isInit) {return;}

            OpenFileDialog dialog = new OpenFileDialog{Title = "Select a file",
            Filter = "Supported files (*.mp4;*.mov;*.gif;*.mkv;*.webm;*.ogg;*.aac;*.wav;*.flac;*.mp3;*.m4a;*.opus;*.alac;*.avi;*.flv)|*.mp4;*.mov;*.gif;*.mkv;*.webm;*.ogg;*.aac;*.wav;*.flac;*.mp3;*.m4a;*.opus;*.alac;*.avi;*.flv|All files (*.*)|*.*"};

            if (dialog.ShowDialog() == true)
            {
                pathTB.Text = dialog.FileName;
            }
        }
        private void OutputBrowse(object sender, RoutedEventArgs e)
        {
            if (!isInit) {return;}
            string? o = QOL.GetStringFromComboBox(OutputFormatCB);
            if (o == null)
            {
                MessageBoxAPI.Debug("Please select a video format output first!");
                return;
            }
            SaveFileDialog dialog = new SaveFileDialog{Title = "Output Path", Filter = $"{o} files (*.{o.ToLower()})|*.{o.ToLower()}"};

            if (dialog.ShowDialog() == true)
            {
                OutputPathTB.Text = dialog.FileName;
            }
        }
        private void Done(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(pathTB.Text) || !File.Exists(pathTB.Text)) {MessageBoxAPI.Debug("The input file doesn't exist!");return;}
            if (string.IsNullOrEmpty(OutputPathTB.Text)) {MessageBoxAPI.Debug("The saving file path doesn't exist!");return;}

            if (VideoBitrateTB.IsEnabled && !string.IsNullOrEmpty(VideoBitrateTB.Text) && !QOL.CheckSyntax(VideoBitrateTB)) {MessageBoxAPI.Debug("invalid syntax at VideoBitrate");return;}
            if (MinimumVideoBitrateTB.IsEnabled && !string.IsNullOrEmpty(MinimumVideoBitrateTB.Text) && !QOL.CheckSyntax(MinimumVideoBitrateTB)) {MessageBoxAPI.Debug("invalid syntax at MinimumVideoBitrate");return;}
            if (MaximumVideoBitrateTB.IsEnabled && !string.IsNullOrEmpty(MaximumVideoBitrateTB.Text) && !QOL.CheckSyntax(MaximumVideoBitrateTB)) {MessageBoxAPI.Debug("invalid syntax at MaximumVideoBitrate");return;}
            if (BufferSizeTB.IsEnabled && !string.IsNullOrEmpty(BufferSizeTB.Text) && !QOL.CheckSyntax(BufferSizeTB)) {MessageBoxAPI.Debug("invalid syntax at BufferSize");return;}
            if (AudioBitrateTB.IsEnabled && !string.IsNullOrEmpty(AudioBitrateTB.Text) && !QOL.CheckSyntax(AudioBitrateTB)) {MessageBoxAPI.Debug("invalid syntax at AudioBitrate");return;}
            if (CQTB.IsEnabled && !string.IsNullOrEmpty(CQTB.Text) && !CQTB.Text.All(char.IsDigit)) {MessageBoxAPI.Debug("invalid syntax at CQ/CQP constant");return;}
            if (WidthTB.IsEnabled && !string.IsNullOrEmpty(WidthTB.Text) && !WidthTB.Text.All(char.IsDigit)) {MessageBoxAPI.Debug("invalid syntax at Resolution");return;}
            if (HeightTB.IsEnabled && !string.IsNullOrEmpty(HeightTB.Text) && !HeightTB.Text.All(char.IsDigit)) {MessageBoxAPI.Debug("invalid syntax at Resolution");return;}
            if (FramerateTB.IsEnabled && !string.IsNullOrEmpty(FramerateTB.Text) && !FramerateTB.Text.All(char.IsDigit)) {MessageBoxAPI.Debug("invalid syntax at Framerate");return;}
            if (StartSecondTB.IsEnabled && !string.IsNullOrEmpty(StartSecondTB.Text) && QOL.StringToDouble(StartSecondTB.Text) == null) {MessageBoxAPI.Debug("invalid syntax at Starting time");return;}
            if (StartMinuteTB.IsEnabled && !string.IsNullOrEmpty(StartMinuteTB.Text) && !StartMinuteTB.Text.All(char.IsDigit)) {MessageBoxAPI.Debug("invalid syntax at Starting time");return;}
            if (StartHourTB.IsEnabled && !string.IsNullOrEmpty(StartHourTB.Text) && !StartHourTB.Text.All(char.IsDigit)) {MessageBoxAPI.Debug("invalid syntax at Starting time");return;}
            if (!string.IsNullOrEmpty(EndSecondTB.Text) && EndSecondTB.Text != "--" && QOL.StringToDouble(EndSecondTB.Text) == null) {MessageBoxAPI.Debug("invalid syntax at Ending time");return;}
            if (!string.IsNullOrEmpty(EndMinuteTB.Text) && EndMinuteTB.Text != "--" && !EndMinuteTB.Text.All(char.IsDigit)) {MessageBoxAPI.Debug("invalid syntax at Ending time");return;}
            if (!string.IsNullOrEmpty(EndHourTB.Text) && EndHourTB.Text != "--" && !EndHourTB.Text.All(char.IsDigit)) {MessageBoxAPI.Debug("invalid syntax at Ending time");return;}
            if (QOL.StringToDouble(StartSecondTB.Text) >= 60) {MessageBoxAPI.Debug("invalid syntax at Starting time");return;}
            if (QOL.StringToDouble(StartMinuteTB.Text) >= 60) {MessageBoxAPI.Debug("invalid syntax at Starting time");return;}
            if (QOL.StringToDouble(EndSecondTB.Text) >= 60) {MessageBoxAPI.Debug("invalid syntax at Ending time");return;}
            if (QOL.StringToDouble(EndMinuteTB.Text) >= 60) {MessageBoxAPI.Debug("invalid syntax at Ending time");return;}

            string inputPath = pathTB.Text;
            string outputPath = OutputPathTB.Text;
            FFmpegArg.supportedOutputFormats outputFormat = (FFmpegArg.supportedOutputFormats)FFmpegArg.StringToOutputFormat((string)QOL.GetStringFromComboBox(OutputFormatCB)!)!;
            FFmpegArg.supportedVideoEncoders? videoEncoder = VideoEncoderCB.IsEnabled ? FFmpegArg.StringToVideoEncoder(VideoEncoderCB.SelectedItem?.ToString()!) : null;
            FFmpegArg.videoRateControlTypes? rateControl = VideoRateControlCB.IsEnabled ? FFmpegArg.StringToRateControlType(QOL.GetStringFromComboBox(VideoRateControlCB)!) : null;
            string? bitrate = VideoBitrateTB.IsEnabled ? VideoBitrateTB.Text : null;
            bitrate = string.IsNullOrEmpty(bitrate) ? null : bitrate;
            string? QConstant = CQTB.IsEnabled ? CQTB.Text : null;
            QConstant = string.IsNullOrEmpty(QConstant) ? null : QConstant;
            string? minBR = MinimumVideoBitrateTB.IsEnabled ? MinimumVideoBitrateTB.Text : null;
            minBR = string.IsNullOrEmpty(minBR) ? null : minBR;
            string? maxBR = MaximumVideoBitrateTB.IsEnabled ? MaximumVideoBitrateTB.Text : null;
            maxBR = string.IsNullOrEmpty(maxBR) ? null : maxBR;
            string? bufsize = BufferSizeTB.IsEnabled ? BufferSizeTB.Text : null;
            bufsize = string.IsNullOrEmpty(bufsize) ? null : bufsize;
            FFmpegArg.encodingPresets? encodingPreset = VideoEncodingSpeedCB.IsEnabled ? 
            (VideoEncodingSpeedCB.SelectedItem?.ToString() != null ?
                FFmpegArg.StringToEncoderPreset(VideoEncodingSpeedCB.SelectedItem.ToString()!)
                :
                null
            )
            :
            null;
            string? cpuUsed = CPUUsedCB.IsEnabled ? (CPUUsedCB.SelectedItem?.ToString() == null ? null : QOL.GetStringFromComboBox(CPUUsedCB)) : null;
            cpuUsed = string.IsNullOrEmpty(cpuUsed) ? null : cpuUsed;
            string? width = WidthTB.IsEnabled ? WidthTB.Text : null;
            width = string.IsNullOrEmpty(width) ? null : width;
            string? height = HeightTB.IsEnabled ? HeightTB.Text : null;
            height = string.IsNullOrEmpty(height) ? null : height;
            string[]? resolution = null;
            if (! (width == null && height == null))
            {
                width = width == null ? "-2" : width; // if width is null width = "-2"
                height = height == null ? "-2" : height;
                resolution = [width, height];
            }
            string? framerate = FramerateTB.IsEnabled ? FramerateTB.Text : null;
            framerate = string.IsNullOrEmpty(framerate) ? null : framerate;
            string? scaling = VideoScalingFlagCB.IsEnabled ? QOL.GetStringFromComboBox(VideoScalingFlagCB): null;
            FFmpegArg.scalingFlags? scalingFlag = scaling == null ? null : FFmpegArg.StringToScalingFlag(scaling);

            bool? ac = AccurateColorGifCB.IsEnabled ? AccurateColorGifCB.IsChecked : null;

            FFmpegArg.supportedAudioEncoders? audioEncoder = AudioEncoderCB.IsEnabled ? FFmpegArg.StringToAudioEncoder(AudioEncoderCB.SelectedItem?.ToString()!) : null;

            string? audioBitrate = AudioBitrateTB.IsEnabled ? AudioBitrateTB.Text : null;
            audioBitrate = string.IsNullOrEmpty(audioBitrate) ? null : audioBitrate;

            string? acc = QOL.GetStringFromComboBox(AudioChannelAmountCB);

            bool? disableAudio = DisableAudioCB.IsEnabled ? DisableAudioCB.IsChecked : null;

            string startSecond = String.IsNullOrEmpty(StartSecondTB.Text) ? "00" : (StartSecondTB.Text.Length == 1 ? "0" + StartSecondTB.Text : StartSecondTB.Text);
            string startMinute = String.IsNullOrEmpty(StartMinuteTB.Text) ? "00" : (StartMinuteTB.Text.Length == 1 ? "0" + StartMinuteTB.Text : StartMinuteTB.Text);
            string startHour = String.IsNullOrEmpty(StartHourTB.Text) ? "00" :  StartHourTB.Text;
            string startingTime = startHour + ":" + startMinute + ":" + startSecond;
            string? EndSecond = String.IsNullOrEmpty(EndSecondTB.Text) || EndSecondTB.Text == "--" ? null : (EndSecondTB.Text.Length == 1 ? "0" + EndSecondTB.Text : EndSecondTB.Text);
            string? EndMinute = String.IsNullOrEmpty(EndMinuteTB.Text) || EndMinuteTB.Text == "--" ? null : (EndMinuteTB.Text.Length == 1 ? "0" + EndMinuteTB.Text : EndMinuteTB.Text);
            string? EndHour = String.IsNullOrEmpty(EndHourTB.Text) || EndHourTB.Text == "--" ? null :  EndHourTB.Text;
            string? EndingTime = (EndHour != null || EndMinute != null || EndSecond != null ) ? ((EndHour ?? "00") + ":" + (EndMinute ?? "00") + ":" + (EndSecond ?? "00")) : null;
            //MessageBoxAPI.Debug(EndHour ?? "00" + ":" + EndMinute ?? "00" + ":" + EndSecond ?? "00");
            double duration = -1;
            if (EndingTime == null)
            {
                string[] durationGettingAttmpt = ProcessAPI.Start(FFmpegAPI.ffprobePath!, $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{pathTB.Text}\"");
                if (durationGettingAttmpt[1] != "0")
                {
                    MessageBoxAPI.Error("Something Went Wrong!!!\n" + durationGettingAttmpt[2]);
                    return;
                }
                double realStartingTime = (double)QOL.StringToDouble(startSecond)! + ((double)QOL.StringToDouble(startMinute)! * 60) + ((double)QOL.StringToDouble(startHour)! * 3600);
                duration = (double)QOL.StringToDouble(durationGettingAttmpt[0])! - realStartingTime;
            } else
            {
                double realStartingTime = (double)QOL.StringToDouble(startSecond)! + ((double)QOL.StringToDouble(startMinute)! * 60) + ((double)QOL.StringToDouble(startHour)! * 3600);
                double realEndTime = (double)QOL.StringToDouble(EndSecond ?? "0")! + ((double)QOL.StringToDouble(EndMinute ?? "0")! * 60) + ((double)QOL.StringToDouble(EndHour ?? "0")! * 3600);
                duration = realEndTime - realStartingTime;
            }
            if (duration < 0) {MessageBoxAPI.Debug("the duration is negative!\nmake sure you input starting time and ending time correctly");return;}
            bool copyMetaData = CopyMetaDataCB.IsChecked ?? true;
            bool showLog = EnableLog.IsChecked ?? false;
            FFmpegArg fullArgument = new FFmpegArg()
            {
                ffprobePath=FFmpegAPI.ffprobePath,
                inputPath=inputPath,
                outputPath=outputPath,
                startTime=startingTime,
                endTime=EndingTime,
                loggingMode="error",
                outputFormat=outputFormat,
                videoEncoder=videoEncoder,
                videoRateControl=rateControl,
                videoBitrate=bitrate,
                CQConstant=QConstant,
                QPConstant=QConstant,
                MinimumVideoBitrate=minBR,
                MaximumVideoBitrate=maxBR,
                BufferSize=bufsize,
                videoEncodingSpeed=encodingPreset,
                cpuUsed=cpuUsed,
                videoResolution=resolution,
                videoscalingflag=scalingFlag,
                videoFramerate=framerate,
                accurateColorGif=ac,
                disableAudio=disableAudio,
                audioEncoder=audioEncoder,
                audioBitrate=audioBitrate,
                audioChannelAmount=acc,
                copyMetaData=copyMetaData
            };
            
            _ = FFmpegAPI.Start((string)fullArgument.ToString()!, showLog, duration);
        }
    }
}