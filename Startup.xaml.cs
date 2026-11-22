
using Microsoft.Win32;
using System.Windows;
using System.IO;
// using System.Drawing.Text;

namespace CsCompressor
{
    public partial class Startup : Window
    {
        VideoProcessingWindow p = new VideoProcessingWindow();
        public Startup()
        {
            FFmpegAPI.Init();
            _ = FFmpegAPI.CheckAllEncoders(p);
            InitializeComponent();
        }

        private void SelectFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog{Title = "Select a file",
            Filter = "Supported files (*.mp4;*.mov;*.gif;*.mkv;*.webm;*.ogg;*.aac;*.wav;*.flac;*.mp3;*.m4a;*.opus;*.alac;*.avi;*.flv)|*.mp4;*.mov;*.gif;*.mkv;*.webm;*.ogg;*.aac;*.wav;*.flac;*.mp3;*.m4a;*.opus;*.alac;*.avi;*.flv|All files (*.*)|*.*"};

            if (dialog.ShowDialog() == true)
            {
                pathTB.Text = dialog.FileName;
            }
        }

        private void Go(object sender, RoutedEventArgs e)
        {
            FFmpegAPI.CheckFFmpeg();
            string path = pathTB.Text;
            if (string.IsNullOrWhiteSpace(path)) {return;}
            if (!File.Exists(path)){return;}

            FFmpegAPI.currentVIdeoProcessingWindow = p;
            Application.Current.MainWindow = p;
            
            p.SetPath(path);
            
            p.Show();
            this.Close();
        }
    }
}
