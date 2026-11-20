// using System.IO;
using Microsoft.Win32;
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
        private void OutputBrowse(object sender, RoutedEventArgs e)
        {
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
    }
}