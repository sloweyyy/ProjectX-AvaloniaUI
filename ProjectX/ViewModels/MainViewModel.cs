using ProjectX.Views;

namespace ProjectX.ViewModels;

public class MainViewModel : ViewModelBase
{
    // open login
    public void OpenLogin()
    {
        LoginWindow login = new LoginWindow();
        login.Show();
    }
}
