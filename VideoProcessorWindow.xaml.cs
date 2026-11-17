// using System.IO;
using System.Drawing.Imaging;
using System.Net.Http.Headers;
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
        public void ReplaceOutput()
        {
            if (!isInit) {return;}
        
            string outputFormat = OutputFormatCB.SelectedItem?.ToString() ?? "";
            // _ = MessageBoxAPI.DebugAsync(outputFormat);
            // MessageBoxAPI.Debug(outputFormat.Split(": ")[1]);
            if (outputFormat.Split(": ").Length != 0)
            {
                 //lowk gave up on naming variable ;; uses too much brain power (:
                FFmpegArg.supportedOutputFormats? a = FFmpegArg.StringToOutputFormat(outputFormat.Split(": ")[1]);
                // _ = MessageBoxAPI.DebugAsync(a.ToString() ?? "NULL");
                if (a!=null)
                {
                    switch (a.ToString()) //using swtich so it's more readable
                    {
                        case not ("AAC" or "WAV" or "OGG" or "MP3" or "FLAC"):

                            VideoEncoderCB.IsEnabled = true;
                            VideoRateControlCB.IsEnabled = true;
                            VideoBitrateTB.IsEnabled = true;
                            CQTB.IsEnabled = true;
                            MinimumVideoBitrateTB.IsEnabled = true;
                            MaximumVideoBitrateTB.IsEnabled = true;
                            WidthTB.IsEnabled = true;
                            HeightTB.IsEnabled = true;
                            FramerateTB.IsEnabled = true;
                            VideoScalingFlagCB.IsEnabled = true;
                            BufferSizeTB.IsEnabled = true;
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
                                    break;
                                default:
                                    AccurateColorGifCB.IsEnabled = false;
                                    AccurateColorGifCB.IsChecked = false;
                                    if (!DisableAudioCB.IsEnabled) {DisableAudioCB.IsChecked = false;}
                                    DisableAudioCB.IsEnabled = true;
                                    AudioEncoderCB.IsEnabled = true;
                                    AudioBitrateTB.IsEnabled = true;
                                    AudioChannelAmountCB.IsEnabled = true;
                                    break;
                            }
                            break;
                        default:
                            VideoEncoderCB.IsEnabled = false;
                            VideoRateControlCB.IsEnabled = false;
                            VideoBitrateTB.IsEnabled = false;
                            CQTB.IsEnabled = false;
                            MinimumVideoBitrateTB.IsEnabled = false;
                            MaximumVideoBitrateTB.IsEnabled = false;
                            WidthTB.IsEnabled = false;
                            HeightTB.IsEnabled = false;
                            FramerateTB.IsEnabled = false;
                            VideoScalingFlagCB.IsEnabled = false;
                            BufferSizeTB.IsEnabled = false;
                            VideoEncodingSpeedCB.IsEnabled = false;
                            CPUUsedCB.IsEnabled = false;
                            AccurateColorGifCB.IsEnabled = false;

                            if (!DisableAudioCB.IsEnabled) {DisableAudioCB.IsChecked = false;}
                            DisableAudioCB.IsEnabled = true;
                            AudioEncoderCB.IsEnabled = true;
                            AudioBitrateTB.IsEnabled = true;
                            AudioChannelAmountCB.IsEnabled = true;
                            break;
                    }
                }
            }
        
        }
        private void OutputFormatChanged(object sender, SelectionChangedEventArgs e)
        {
            ReplaceOutput();
        }
        private void VideoEncoderChanged(object sender, SelectionChangedEventArgs e)
        {
            // if (isInit)
            // {
            //     FFmpegAPI.CheckFFmpeg();
            //     ComboBox VE = (ComboBox)sender;
            //     String? item = (VE.SelectedItem as ComboBoxItem)?.Content.ToString();
            //     if (String.IsNullOrEmpty(item)){return;}
            //     else if (item == "No encoding")
            //     {
            //         VideoBitrateTB.IsReadOnly = true;
            //         VideoBitrateTB.Opacity = 0.4d;
            //         VideoBitrateTitle.Opacity = 0.4d;
            //         //VideoQualityTitle.Opacity = 0.4d;
            //         //VideoQualityCB.IsEnabled = false;
            //     }
            // }
            
        }
        
    }
}