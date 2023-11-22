using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using IronOcr;
using RestSharp;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Avalonia;
using Avalonia.Threading;
using MongoDB.Bson;
using MongoDB.Driver;
using static RestSharp.Method;
using MongoDB.Driver;

namespace ProjectX;

public partial class TTS : Window
{
    string path = Directory.GetCurrentDirectory();
    string[] arrGiong = { "Nữ miền Nam", "Nữ miền Bắc", "Nam miền Nam", "Nam miền Bắc" };
    int[] arrGiongmini = { 1, 2, 3, 4 };
    int filexong = -1;
    string version = "3.0.1";
    Thread ThreadBackround;
    Thread DocThread, ThreadUpdateUI;
    Process ffmpeg;
    XuLyAmThanh MainXuLy;
    string keylone = "";
    private const string connectionString = "mongodb+srv://slowey:tlvptlvp@projectx.3vv2dfv.mongodb.net/";
    private OpenFileDialog openFileDialog; // Declare openFileDialog at the class level

    public TTS(string username)
    {
        InitializeComponent();
        string apiKey = GetApiKeyByUsername(username); // Sử dụng hàm để lấy apiKey từ cơ sở dữ liệu
        Apikey.Text = apiKey;
        ThreadUpdateUI = new Thread(() => UpdateUI());
        ThreadUpdateUI.IsBackground = true;
        ThreadUpdateUI.Start();
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase("ProjectX");
        var collection = database.GetCollection<BsonDocument>("USERS");
        var filter = Builders<BsonDocument>.Filter.Eq("username", username);
        var userDocument = collection.Find(filter).FirstOrDefault();
        openFileDialog = new OpenFileDialog(); // Initialize openFileDialog in the constructor
        Nguoidoc.SelectedIndex = 0; // Chọn lựa chọn đầu tiên
        Tocdo.SelectedIndex = 0; // Chọn lựa chọn đầu tiên
    }

    private async void OpenFileButton_Click(object sender, RoutedEventArgs e)
    {
        var result = await openFileDialog.ShowAsync(this);

        if (result != null && result.Length > 0)
        {
            string selectedFile = result[0];
            ProcessFile(selectedFile);
        }
    }


    private string GetApiKeyByUsername(string username)
    {
        string apiKey = ""; // Khởi tạo apiKey trống

        MongoClient
            client = new MongoClient(
                "mongodb+srv://slowey:tlvptlvp@projectx.3vv2dfv.mongodb.net/"); // Thay đổi chuỗi kết nối để phù hợp với cài đặt MongoDB của bạn
        IMongoDatabase
            database = client
                .GetDatabase("ProjectX"); // Thay "your-database-name" bằng tên cơ sở dữ liệu MongoDB của bạn
        IMongoCollection<BsonDocument>
            collection =
                database.GetCollection<BsonDocument>("Users"); // Thay "Users" bằng tên bảng/collection MongoDB của bạn

        var filter = Builders<BsonDocument>.Filter.Eq("Username", username);
        var result = collection.Find(filter).FirstOrDefault();

        if (result != null)
        {
            apiKey = result.GetValue("ApiKey").AsString; // Lấy giá trị của trường "apikey" từ kết quả truy vấn
        }

        return apiKey;
    }


    // private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    // {
    //     var move = sender as System.Windows.Controls.Grid;
    //     var win = Window.GetWindow(move);
    //     win.DragMove();
    // }


    private void ProcessFile(string filePath)
    {
        if (filePath.EndsWith(".txt"))
        {
            string extractedText = File.ReadAllText(filePath, Encoding.UTF8);
            Text.Text = extractedText;
        }
        else if (filePath.EndsWith(".jpg") || filePath.EndsWith(".png"))
        {
            string extractedText = PerformOcr(filePath);
            Text.Text = extractedText;
        }
    }

    private string PerformOcr(string imagePath)
    {
        var Ocr = new IronTesseract
        {
            Language = OcrLanguage.Vietnamese
        };
        var result = Ocr.Read(imagePath);
        return result.Text;
    }

    private async void BtnOpenFile_Click(object? sender, RoutedEventArgs e)
    {
        var result = await openFileDialog.ShowAsync(this);

        if (result != null && result.Length > 0)
        {
            string selectedFile = result[0];
            ProcessFile(selectedFile);
        }
    }

    private void UpdateUI()
    {
        string textPre = "";
        while (true)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                if (Text.Text != textPre)
                {
                    Kytu.Content = "Ký tự đã nhập: " + Text.Text.Length.ToString();
                    textPre = Text.Text;
                }
            });
            Thread.Sleep(200);
        }
    }

    private bool CheckKey(string key)
    {
        if (keylone == key)
        {
            return true;
        }
        else
        {
            Tientrinh.Content = "Đang kiểm tra API KEY";
            Thread.Sleep(500);
            var client = new RestClient("https://api.zalo.ai/v1/tts/synthesize");
            var request = new RestRequest(); // Use Method.POST for a POST request
            request.Method = Method.Post;
            request.AddHeader("apikey", key);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            // Execute the request and get the response
            var response = client.Execute<String>(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) // Use HttpStatusCode for clarity
            {
                Tientrinh.Content = "Chưa khởi động.";
                return false;
            }

            Tientrinh.Content = "Đang xử lý";
            keylone = key;
            return true;
        }
    }


    private void _run_Click(object sender, RoutedEventArgs e)
    {
        if (Apikey.Text != null && CheckKey(Apikey.Text) == false)
        {
            ShowMessage("Lỗi", "API KEY không đúng, bạn vui lòng kiểm tra lại key của mình rồi thử lại nhé!");
        }
        else
        {
            File.WriteAllText("APIKey.txt", Apikey.Text);
            string? text = Text.Text;

            // Declare gender outside of the if block
            int gender = -1; // Default value or a value that indicates 'not set'

            ComboBoxItem selectedItem = Nguoidoc.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                string selectedItemText = selectedItem.Content.ToString();
                gender = arrGiongmini[Array.IndexOf(arrGiong, selectedItemText)];
            }
            else
            {
                // Handle the case where no item is selected or set a default value
            }

            string speed = "";
            ComboBoxItem selectedSpeedItem = Tocdo.SelectedItem as ComboBoxItem;
            if (selectedSpeedItem != null)
            {
                string selectedSpeedText = selectedSpeedItem.Content.ToString();
                speed = StringBetween(selectedSpeedText, "(", ")");
            }
            else
            {
                // Handle the case where no item is selected or set a default value
            }

            string? apikey = Apikey.Text;
            MainXuLy = new XuLyAmThanh(text, gender, speed, apikey);
            MainXuLy.mainRun();
            ThreadBackround = new Thread(Backround)
            {
                IsBackground = true
            };
            ThreadBackround.Start();
        }
    }


    private void _stop_Click(object sender, RoutedEventArgs e)
    {
        if (MainXuLy != null)
        {
            MainXuLy.StopRead();
            MainXuLy.StopDown();
        }
    }


    // private void Grid_PointerPressed(object sender, PointerPressedEventArgs e)
    // {
    //     if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
    //     {
    //         this.BeginMoveDrag(e);
    //     }
    // }


    public static string StringBetween(string text, string start, string end)
    {
        int startIndex = text.IndexOf(start, StringComparison.Ordinal);
        if (startIndex == -1) return string.Empty;

        startIndex += start.Length;
        int endIndex = text.IndexOf(end, startIndex, StringComparison.Ordinal);
        if (endIndex == -1) return string.Empty;

        return text.Substring(startIndex, endIndex - startIndex);
    }


    private void _download_Click(object sender, RoutedEventArgs e)
    {
        if (CheckKey(Apikey.Text) == false)
        {
            ShowMessage("Lỗi", "API KEY không đúng, bạn vui lòng kiểm tra lại key của mình rồi thử lại nhé!");
        }
        else
        {
            File.WriteAllText("APIKey.txt", Apikey.Text);
            string text = Text.Text;

            // Correctly get the gender value from the Nguoidoc ComboBox
            int gender = -1;
            ComboBoxItem selectedGenderItem = Nguoidoc.SelectedItem as ComboBoxItem;
            if (selectedGenderItem != null)
            {
                string selectedItemText = selectedGenderItem.Content.ToString();
                gender = arrGiongmini[Array.IndexOf(arrGiong, selectedItemText)];
            }

            // Correctly get the speed value from the Tocdo ComboBox
            string speed = "";
            ComboBoxItem selectedSpeedItem = Tocdo.SelectedItem as ComboBoxItem;
            if (selectedSpeedItem != null)
            {
                string selectedSpeedText = selectedSpeedItem.Content.ToString();
                speed = StringBetween(selectedSpeedText, "(", ")");
            }

            string apikey = Apikey.Text;
            MainXuLy = new XuLyAmThanh(text, gender, speed, apikey);
            MainXuLy.mainDown();
            ThreadBackround = new Thread(() => Backround());
            ThreadBackround.IsBackground = true;
            ThreadBackround.Start();
        }
    }


    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        //var client = new RestClient("https://raw.githubusercontent.com/sloweyyy/IT008.O12/main/ProjectX/version.txt");
        //var request = new RestRequest(Method.GET);
        //IRestResponse response = client.Execute(request);
        //if (!response.Content.Contains(version))
        //{
        //    MessageBox.Show("Đã có phiên bản mới. Hãy cập nhật nhé!");
        //    System.Diagnostics.Process.Start("https://github.com/sloweyyy/IT008.O12/releases/");
        //    System.Environment.Exit(1);
        //}
        //else
        //{

        //    _apikey.Text = System.IO.File.ReadAllText("APIKey.txt");
        //}
    }

    // private void _back_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    // {
    // }

    // private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    // {
    // }

    // private void Window_MouseDown_1(object sender, MouseButtonEventArgs e)
    // {
    //     if (e.ChangedButton == MouseButton.Left)
    //         this.DragMove();
    // }
    //
    // private void _text_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    // {
    //     if (_kytu != null)
    //     {
    //         _kytu.Content = "Ký tự đã nhập: " + _text.Text.Length.ToString();
    //     }
    // }
    //
    // private void _text_TextChanged_1(object sender, System.Windows.Controls.TextChangedEventArgs e)
    // {
    // }

    private void Backround()
    {
        while (true)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Tientrinh.Content = MainXuLy.getProcessMes();
                Process.Value = MainXuLy.getProcessNow();
            });

            Thread.Sleep(2000);
        }
    }

    public void ShowMessage(string title, string message)
    {
        var messageBox = new CustomMessageBox(title, message);
        messageBox.ShowDialog(this);
    }
}