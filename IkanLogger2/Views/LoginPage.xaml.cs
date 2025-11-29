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
                MessageBox.Show("Please enter both username and password.",
                                "Login Failed",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            try
            {
                User user = await UserService.LoginAsync(username, password);

                if (user.Id > 0)
                {
                    MessageBox.Show("Login successful!",
                                    "Success",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                    Session.CurrentUser = user;

                    NavigationService?.Navigate(new DashboardPage());
                }
                else
                {
                    MessageBox.Show("Invalid username or password.",
                                    "Login Failed",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during login: {ex.Message}",
                                "Login Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegisterPage());
        }
    }
}