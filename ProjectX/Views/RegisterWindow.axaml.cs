using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MongoDB.Driver;
using MongoDB.Bson;
using BCrypt.Net;
using RestSharp;

namespace ProjectX.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly IMongoDatabase _database;

        public RegisterWindow()
        {
            InitializeComponent();
            _database = GetMongoDatabase(); // Initialize the MongoDB database connection
        }

        private IMongoDatabase GetMongoDatabase()
        {
            // Set your MongoDB connection string and database name
            string connectionString =
                "mongodb+srv://slowey:tlvptlvp@projectx.3vv2dfv.mongodb.net/"; // Update with your MongoDB server details
            string databaseName = "ProjectX"; // Update with your database name

            var client = new MongoClient(connectionString);
            return client.GetDatabase(databaseName);
        }

        // This method checks if the username is already taken.
        // It returns true if the username is already taken, otherwise it returns false.
        private bool CheckUsername(string username)
        {
            var usersCollection = _database.GetCollection<BsonDocument>("Users");

            var filter = Builders<BsonDocument>.Filter.Eq("Username", username);
            var count = usersCollection.CountDocuments(filter);

            return count > 0; // Username exists if count > 0
        }

        // This method registers a new user.
        // It returns true if the registration is successful, otherwise it returns false.
        private bool Register(string username, string apiKey, string password)
        {
            var usersCollection = _database.GetCollection<BsonDocument>("Users");

            // Hash the password using bcrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var document = new BsonDocument
            {
                { "Username", username },
                { "ApiKey", apiKey },
                { "Password", hashedPassword }
            };

            try
            {
                usersCollection.InsertOne(document);
                return true; // Registration successful
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during registration
                Console.WriteLine(ex.Message);
                return false; // Registration failed
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string apiKey = ApiKeyTextBox.Text;
            string password = PasswordBox.Text;

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(password))
            {
                if (CheckUsername(username))
                {
                    ShowMessage("Lỗi", "Tên tài khoản đã tồn tại.");
                }
                else
                {
                    bool isKeyValid = await CheckKeyAsync(apiKey);
                    if (!isKeyValid)
                    {
                        ShowMessage("Lỗi", "API Key không hợp lệ.");
                        return;
                    }

                    if (Register(username, apiKey, password))
                    {
                        ShowMessage("Thông báo", "Đăng ký thành công.");
                        LoginWindow loginWindow = new LoginWindow();
                        loginWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        ShowMessage("Lỗi", "Đăng ký thất bại.");
                    }
                }
            }
            else
            {
                ShowMessage("Lỗi", "Vui lòng nhập đầy đủ thông tin.");
            }
        }


        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the current window and open the login window
            this.Close();
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
        }

        private async Task<bool> CheckKeyAsync(string key)
        {
            var client = new RestClient("https://api.zalo.ai/v1/tts/synthesize");
            var request = new RestRequest();
            request.Method = Method.Post;
            request.AddHeader("apikey", key);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            var response = await client.ExecuteAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return false;
            }

            return true;
        }


        public void ShowMessage(string title, string message)
        {
            var messageBox = new CustomMessageBox(title, message);
            messageBox.ShowDialog(this);
        }
    }
}