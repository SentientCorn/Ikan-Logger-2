using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IkanLogger2.Services;
using IkanLogger2.Models;
using IkanLogger2.Core;

namespace IkanLogger2.Views
{
    /// <summary>
    /// Interaction logic for ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        private User _currentUser;

        public ProfilePage()
        {
            InitializeComponent();
            LoadCurrentUser();
        }

        private async void LoadCurrentUser()
        {
            // Asumsikan Anda menyimpan user yang sedang login di Session atau App
            // Sesuaikan dengan cara Anda menyimpan session
            if (Session.CurrentUser != null)
            {
                _currentUser = Session.CurrentUser;
                UsernameBox.Text = _currentUser.Username;
            }
            else
            {
                MessageBox.Show("User tidak ditemukan. Silakan login kembali.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                // Navigate ke login page
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("User tidak ditemukan.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validasi input
            string newUsername = UsernameBox.Text.Trim();
            string newPassword = PasswordBox.Password;

            if (string.IsNullOrEmpty(newUsername))
            {
                MessageBox.Show("Username tidak boleh kosong.", "Validasi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Update ke database
                bool success = await UserService.UpdateUserAsync(
                    _currentUser.Id,
                    newUsername,
                    newPassword
                );

                if (success)
                {
                    MessageBox.Show("Profile berhasil diupdate!", "Sukses",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Update session user
                    _currentUser = new User(_currentUser.Id, newUsername);
                    Session.CurrentUser = _currentUser;

                    // Clear password field
                    PasswordBox.Clear();
                }
                else
                {
                    MessageBox.Show("Gagal mengupdate profile. Username mungkin sudah digunakan.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Terjadi kesalahan: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}