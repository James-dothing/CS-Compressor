using System.IO;
using System.Windows;
using Microsoft.Win32;
using VideoProcessor;

namespace CsCompressor
{
    public partial class Startup : Window
    {
        private readonly VideoProcessingWindow p = new VideoProcessingWindow();

        public Startup()
        {
            FFmpegAPI.Init();
            _ = FFmpegAPI.CheckAllEncoders(p);
            InitializeComponent();
        }

        private void SelectFile(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select a video file",
                Filter = "Video Files (*.mp4;*.mkv;*.mov;*.webm;*.avi;*.flv;*.wmv)|*.mp4;*.mkv;*.mov;*.webm;*.avi;*.flv;*.wmv|All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                pathTB.Text = dialog.FileName;
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
                    pathTB.Text = files[0];
                }
            }
        }

        private void Go(object sender, RoutedEventArgs e)
        {
            string path = pathTB.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBoxAPI.Error("Please select or enter an input video file path.", Crash: false);
                return;
            }

            if (!File.Exists(path))
            {
                MessageBoxAPI.Error($"Input file does not exist:\n{path}", Crash: false);
                return;
            }

            FFmpegAPI.CheckFFmpeg(showDialog: true);

            FFmpegAPI.currentVIdeoProcessingWindow = p;
            Application.Current.MainWindow = p;

            p.SetPath(path);
            p.Show();
            this.Close();
        }
    }
}
