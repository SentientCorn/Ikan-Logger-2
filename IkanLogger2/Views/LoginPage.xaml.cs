using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using IkanLogger2.Models;
using IkanLogger2.Services;
using IkanLogger2.Core;

namespace IkanLogger2.Views
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text;
            string password = PasswordBox.Password;

            // TODO: Replace this with real authentication logic
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                CustomMessageBox.Show("Please enter both username and password.",
                                "Login Failed",
                                CustomMessageBox.MessageBoxButton.OK);
                return;
            }

            try
            {
                User user = await UserService.LoginAsync(username, password);

                if (user != null && user.Id > 0)
                {
                    CustomMessageBox.Show("Login successful!",
                                    "Success",
                                    CustomMessageBox.MessageBoxButton.OK);
                    Session.CurrentUser = user;

                    NavigationService?.Navigate(new DashboardPage());
                }
                else
                {
                    CustomMessageBox.ShowError("Invalid username or password.",
                                    "Login Failed");
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"An error occurred during login: {ex.Message}",
                                "Login Error",
                                CustomMessageBox.MessageBoxButton.OK);
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegisterPage());
        }
    }
}