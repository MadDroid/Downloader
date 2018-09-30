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
        private readonly WebClient client = new WebClient();

        private string filePath;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InputSavePath.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            InputURL.Text = "https://speed.hetzner.de/100MB.bin";
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
                progressDownload.Visibility = Visibility.Hidden;
                DownloadLabel.Visibility = Visibility.Hidden;
                DownloadButton.Content = "Download";
                InputSavePath.IsEnabled = true;
                InputURL.IsEnabled = true;
                BrowseButton.IsEnabled = true;
                if (a.Cancelled)
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                    return;
                }
                else if (a.Error is null)
                {
                    System.Windows.MessageBox.Show("Download completed", "Downloader", MessageBoxButton.OK);
                }
                else
                {
                    if (a.Error is WebException exception)
                    {
                        if (exception.Status == WebExceptionStatus.UnknownError)
                        {
                            System.Windows.MessageBox.Show("Não foi possível baixar o arquivo.\nPor favor tenha certeza que o arquivo que está tentando baixar exista.", "Downloader", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    System.Windows.MessageBox.Show(a.Error.Message, "Downloader", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
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
            if ((string)DownloadButton.Content == "Cancel")
            {
                client.CancelAsync();
                return;
            }

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
                filePath = Path.Combine(InputSavePath.Text, fileName);

                DownloadLabel.Visibility = Visibility.Visible;
                progressDownload.Visibility = Visibility.Visible;
                InputSavePath.IsEnabled = false;
                InputURL.IsEnabled = false;
                BrowseButton.IsEnabled = false;

                DownloadLabel.Content = "Starting download...";

                DownloadButton.Content = "Cancel";
                client.DownloadFileTaskAsync(uri, filePath);
            }
            else
            {
                System.Windows.MessageBox.Show("Please specify a valid url", "Downloader", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            client.Dispose();
        }
    }
}
