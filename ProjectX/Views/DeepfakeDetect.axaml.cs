using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace ProjectX.Views
{
    public partial class DeepfakeDetect : Window
    {
        private string apiKey = "oZ4gD7H2zpD5YK6WynunecuXKQSCbxnj";
        private string videoFilePath;
        private string imageFilePath;

        public DeepfakeDetect()
        {
            InitializeComponent();
        }

        private void UploadVideo_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filters.Add(new FileDialogFilter() { Name = "Video", Extensions = { "mp4" } });
            var result = dialog.ShowAsync(this);
            result.ContinueWith(t =>
            {
                videoFilePath = t.Result[0];
                Dispatcher.UIThread.InvokeAsync(() => { VideoFileName.Text = $"Video file: {videoFilePath}"; });
            });
        }

        private void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filters.Add(new FileDialogFilter() { Name = "Images", Extensions = { "jpg", "png", "gif", "bmp" } });
            var result = dialog.ShowAsync(this);
            result.ContinueWith(t =>
            {
                if (t.Result != null && t.Result.Length > 0)
                {
                    imageFilePath = t.Result[0];
                    Dispatcher.UIThread.InvokeAsync(() => { ImageFileName.Text = $"Image file: {imageFilePath}"; });
                }
            });
        }

        private async void StartDetection_Click(object sender, RoutedEventArgs e)
        {
            var result = await DetectDeepfake();
            ResultText.Text = $"Detection result: {result}";
        }

        private async Task<string> DetectDeepfake()
        {
            var url = "https://api.fpt.ai/dmp/checklive/v2";
            using var client = new HttpClient();
            using var content = new MultipartFormDataContent();
            content.Headers.Add("api-key", apiKey);
            content.Add(new StreamContent(File.OpenRead(videoFilePath)), "video", "video.mp4");
            content.Add(new StreamContent(File.OpenRead(imageFilePath)), "cmnd", "face.jpg");
            var response = await client.PostAsync(url, content);
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
    }
}