using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ProjectX.ViewModels;
using ProjectX.Views;

namespace ProjectX
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Create and show the LoginWindow
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                // TexttoSpeak mainWindow = new TexttoSpeak("hi");
                // mainWindow.Show();


                // Set the main window's DataContext, but don't show it yet
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = new MainViewModel()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}