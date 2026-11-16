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
        private void ClearAndAdd(ComboBox target, string[] array)
        {
            if (isInit)
            {
                target.Items.Clear();
                foreach (string a in array)
                {
                    target.Items.Add(a);
                }   
            }
        }

        public void UpdateVideoEncoders(string[] finalList)
        {
            EncoderArray = finalList; //incase some weird thing happen
            ReplaceOutput();
        }
        public void ReplaceOutput()
        {
            if (isInit)
            {
                string outputFormat = OutputFormatCB.SelectedItem?.ToString() ?? "";
                // MessageBoxAPI.Debug(outputFormat.Split(": ")[1]);
                if (outputFormat.Split(": ").Length == 0) {return;}
                if (Enum.TryParse<FFmpegArg.supportedOutputFormats>(outputFormat.Split(": ")[1], out FFmpegArg.supportedOutputFormats a))
                {
                    
                    VideoEncoderCB.Items.Clear();
                    foreach (string b in EncoderArray) //lowk gave up on naming variable ;; uses too much brain power (:
                    {
                        if (Enum.TryParse<FFmpegArg.supportedVideoEncoders>(b, out FFmpegArg.supportedVideoEncoders c))
                        {
                            if (FFmpegArg.CheckVideoCompatibility(c, a) ?? false)
                            {
                                // _ = MessageBoxAPI.DebugAsync(b);
                                VideoEncoderCB.Items.Add(b);
                            }   
                        }
                    }
                    VideoEncoderCB.Items.Add("No encoding");
                    VideoEncoderCB.SelectedIndex = 0;

                    switch (outputFormat.Split(": ")[1])
                    {
                        case ("GIF"):
                            AccurateColorGifCB.IsEnabled = true;
                            break;
                        default:
                            AccurateColorGifCB.IsEnabled = false;
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