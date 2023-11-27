using Avalonia.Controls;
using Avalonia.Interactivity;
using MongoDB.Driver;

namespace ProjectX.Views
{
    public partial class LoginWindow : Window
    {
        private readonly IMongoCollection<User> _usersCollection;

        public LoginWindow()
        {
            InitializeComponent();
            _usersCollection = GetMongoCollection(); // Initialize the MongoDB collection
        }

        private IMongoCollection<User> GetMongoCollection()
        {
            // Set your MongoDB connection string and database name
            string connectionString =
                "mongodb+srv://slowey:tlvptlvp@projectx.3vv2dfv.mongodb.net/"; // Update with your MongoDB server details
            string databaseName = "ProjectX"; // Update with your database name

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);

            return database.GetCollection<User>("users");
        }

        private void CheckLogin_Click(object sender, RoutedEventArgs e)
        {
            var selectedUsername = UsernameTextBox.Text;
            var enteredPassword = PasswordBox.Text;

            if (selectedUsername != null && !string.IsNullOrEmpty(enteredPassword))
            {
                var user = GetUser(selectedUsername);

                if (user == null)
                {
                    ShowMessage("Lỗi", "Sai tên tài khoản hoặc mật khẩu.");
                    return;
                }

                if (VerifyPassword(enteredPassword, user.password))
                {
                    // Password is valid, grant access

                    // You can open the main window or perform other actions here.
                    // Open the main window
                    TexttoSpeak mainWindow = new TexttoSpeak(selectedUsername);
                    mainWindow.Show();

                    // Close the login window
                    this.Close();
                }
                else
                {
                    // Password is invalid
                    ShowMessage("Lỗi", "Sai tên tài khoản hoặc mật khẩu.");
                }
            }
            else
            {
                ShowMessage("Lỗi", "Vui lòng nhập đầy đủ thông tin.");
            }
        }

        private User GetUser(string username)
        {
            var filter = Builders<User>.Filter.Eq(u => u.username, username);
            return _usersCollection.Find(filter).FirstOrDefault();
        }

        private bool VerifyPassword(string enteredPassword, string storedPasswordHash)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedPasswordHash);
        }


        private void OpenRegisterForm_Click(object sender, RoutedEventArgs e)
        {
            // Open the register window
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();

            // Close the login window
            this.Close();
        }

        public void ShowMessage(string title, string message)
        {
            var messageBox = new CustomMessageBox(title, message);
            messageBox.ShowDialog(this);
        }
    }
}