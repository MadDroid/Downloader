using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Forms;

namespace Downloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InputSavePath.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            InputURL.Text = "https://speed.hetzner.de/100MB.bin";
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    InputSavePath.Text = dialog.SelectedPath;
                }
            }
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(InputSavePath.Text))
            {
                System.Windows.MessageBox.Show("Please specify a valid save location", "Downloader", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (string.IsNullOrEmpty(InputURL.Text))
            {
                System.Windows.MessageBox.Show("Please specify a valid url", "Downloader", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (!Directory.Exists(InputSavePath.Text))
            {
                System.Windows.MessageBox.Show("Please specify a valid save location", "Downloader", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (Uri.TryCreate(InputURL.Text, UriKind.Absolute, out Uri uri))
            {
                string fileName = Path.GetFileName(uri.AbsolutePath);
                string filePath = Path.Combine(InputSavePath.Text, fileName);

                DownloadLabel.Content = "Starting download...";

                using (var client = new WebClient())
                {
                    client.DownloadProgressChanged += (s, a) =>
                    {
                        var progress = (100 * a.BytesReceived) / a.TotalBytesToReceive;
                        Dispatcher.Invoke(() =>
                        {
                            DownloadLabel.Content = $"Download Progress: {progress}%";
                            progressDownload.Value = progress;
                        });
                    };

                    client.DownloadFileCompleted += (s, a) =>
                    {
                        if (File.Exists(filePath))
                        {
                            System.Windows.MessageBox.Show("Download completed", "Downloader", MessageBoxButton.OK);
                        }
                    };

                    try
                    {
                        client.DownloadFileAsync(uri, filePath);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message, "Downloader", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Please specify a valid url", "Downloader", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }


    }
}
