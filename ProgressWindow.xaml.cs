using System.Windows;
using System.ComponentModel;

namespace CsCompressor
{
    public partial class ProgressWindow : Window
    {
        public ProgressWindow()
        {
            InitializeComponent(); //
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            FFmpegAPI.AbortFFmpeg();
        }

        public async Task SetPercentageAsync(double percentage)
        {
            if (percentage == 100.0d)
            {
                AbortButton.Content = "Close";
            }
            await Dispatcher.InvokeAsync(() =>
            {
                PercentageText.Text = $"{percentage:0.00}%";
                Progress.Value = percentage;
            });
        }
        private void Abort(object sender, RoutedEventArgs e)
        {
            FFmpegAPI.AbortFFmpeg();
            this.Close();
        }
    }
}
