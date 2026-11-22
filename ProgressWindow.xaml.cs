using System.Windows;

namespace CsCompressor
{
    public partial class ProgressWindow : Window
    {
        public ProgressWindow()
        {
            InitializeComponent();
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
            FFmpegAPI.Abort();
            this.Close();
        }
    }
}
