using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace ProjectX
{
    public class CustomMessageBox : Window
    {
        private TextBlock _messageText = null!;
        private Button _okButton = null!;

        public CustomMessageBox(string title, string message)
        {
            this.Title = title;
            InitializeComponents(message);
        }

        private void InitializeComponents(string message)
        {
            // Configure window properties
            this.Width = 300; // Set a fixed width
            this.Height = 150; // Set a fixed height
            this.Background = Brushes.White; // Set background color to white
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner; // Center the window on the owner window
            this.CanResize = false; // Disable resizing
            // hide the minimize and maximize buttons
            this.WindowState = WindowState.Normal;
            // Initialize and configure the message text block

            _messageText = new TextBlock
            {
                Text = message,
                Margin = new Avalonia.Thickness(50,50,50,10), // Add some margin around the text
                TextWrapping = Avalonia.Media.TextWrapping.Wrap, // Wrap text if it's too long
                VerticalAlignment = VerticalAlignment.Center, // Center text vertically
                HorizontalAlignment = HorizontalAlignment.Center, // Center text horizontally
                // black text
                Foreground = Brushes.Black,

            };

            // Initialize and configure the OK button
            _okButton = new Button
            {
                Content = "OK",
                Margin = new Avalonia.Thickness(10), // Add some margin around the button
                HorizontalAlignment = HorizontalAlignment.Center, // Center button horizontally
                VerticalAlignment = VerticalAlignment.Bottom, // Center button vertically
                Background = Brushes.LightGray, // Set background color to light gray
                HorizontalContentAlignment = HorizontalAlignment.Center,

            };
            _okButton.PointerEntered += (sender, e) => _okButton.Background = Brushes.Black;
            _okButton.PointerExited += (sender, e) => _okButton.Background = Brushes.LightGray;
            _okButton.PointerPressed += (sender, e) => _okButton.Background = Brushes.DarkGray;
            _okButton.Click += OkButton_Click;

            // Create a layout panel to arrange the controls
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical, // Arrange controls vertically
                Children = { _messageText, _okButton } // Add controls to the panel
            };

            // Set the window's content to the stack panel
            this.Content = stackPanel;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}