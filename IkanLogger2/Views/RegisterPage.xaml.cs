using System.Windows;
using System.Windows.Controls;
using IkanLogger2.Services;
using IkanLogger2.Models;

namespace IkanLogger2.Views
{
    /// <summary>
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var main = (MainWindow)Application.Current.MainWindow;
            main.MainFrame.Navigate(new LoginPage());
        }

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            try {
                string username = UsernameBox.Text;
                string password = PasswordBox.Password;
                string confirmPassword = ConfirmPasswordBox.Password;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Username and password cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (password != confirmPassword)
                {
                    MessageBox.Show("Passwords do not match.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                bool success = await UserService.RegisterAsync(username, password);

                    if (success)
                    {
                        MessageBox.Show("Registration successful! You can now log in.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Registration failed. Username may already be taken.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
            } catch (System.Exception ex) 
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            var main = (MainWindow)Application.Current.MainWindow;
            main.MainFrame.Navigate(new LoginPage());
        }
    }

}
