using IkanLogger2.Models;
using IkanLogger2.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
                    CustomMessageBox.ShowError("Username and password cannot be empty.", "Error");
                    return;
                }

                if (password != confirmPassword)
                {
                    CustomMessageBox.ShowError("Passwords do not match.", "Error");
                    return;
                }

                bool success = await UserService.RegisterAsync(username, password);

                    if (success)
                    {
                    CustomMessageBox.Show("Registration successful! You can now log in.", "Success", CustomMessageBox.MessageBoxButton.OK);
                    }
                    else
                    {
                    CustomMessageBox.ShowError("Registration failed. Username may already be taken.", "Error", CustomMessageBox.MessageBoxButton.OK);
                        return;
                    }
            } catch (System.Exception ex) 
            {
                CustomMessageBox.ShowError($"An error occurred: {ex.Message}", "Error");
                return;
            }
            NavigationService?.Navigate(new LoginPage());
        }
        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Create_Click(sender, e);
            }
        }
    }

}
