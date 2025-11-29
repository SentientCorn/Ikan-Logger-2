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
            NavigationService?.Navigate(new LoginPage());
        }

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            try {
                string username = UsernameBox.Text;
                string password = PasswordBox.Password;
                string confirmPassword = ConfirmPasswordBox.Password;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    CustomMessageBox.Show("Username and password cannot be empty.", "Error", CustomMessageBox.MessageBoxButton.OK);
                    return;
                }

                if (password != confirmPassword)
                {
                    CustomMessageBox.Show("Passwords do not match.", "Error", CustomMessageBox.MessageBoxButton.OK);
                    return;
                }

                bool success = await UserService.RegisterAsync(username, password);

                    if (success)
                    {
                    CustomMessageBox.Show("Registration successful! You can now log in.", "Success", CustomMessageBox.MessageBoxButton.OK);
                    }
                    else
                    {
                    CustomMessageBox.Show("Registration failed. Username may already be taken.", "Error", CustomMessageBox.MessageBoxButton.OK);
                        return;
                    }
            } catch (System.Exception ex) 
            {
                CustomMessageBox.Show($"An error occurred: {ex.Message}", "Error", CustomMessageBox.MessageBoxButton.OK);
                return;
            }


            NavigationService?.Navigate(new LoginPage());
        }
    }

}
