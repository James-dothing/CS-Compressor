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
            // CONTINUME THIS LOL
        }
    }
}