using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using IkanLogger2.Services;
using IkanLogger2.Models;
using IkanLogger2.Core;

namespace IkanLogger2.Views
{
    public partial class ProfilePage : Page
    {
        private User _currentUser;
        private List<CatchLogDetail> _allLogs;

        public ProfilePage()
        {
            InitializeComponent();
            LoadCurrentUser();
            InitializeMonthComboBox();
        }

        private async void LoadCurrentUser()
        {
            if (Session.CurrentUser != null)
            {
                _currentUser = Session.CurrentUser;
                UsernameBox.Text = _currentUser.Username;
                SidebarUsername.Text = _currentUser.Username;

                await LoadDashboardData();
            }
            else
            {
                CustomMessageBox.Show("User tidak ditemukan. Silakan login kembali.", "Error",
                    CustomMessageBox.MessageBoxButton.OK);
            }
        }

        private void InitializeMonthComboBox()
        {
            var months = new List<MonthItem>();

            // Ambil 12 bulan terakhir
            for (int i = 0; i < 12; i++)
            {
                var date = DateTime.Now.AddMonths(-i);
                months.Add(new MonthItem
                {
                    Display = date.ToString("MMMM yyyy"),
                    Month = date.Month,
                    Year = date.Year
                });
            }

            MonthComboBox.ItemsSource = months;
            MonthComboBox.DisplayMemberPath = "Display";
            MonthComboBox.SelectedIndex = 0; // Bulan ini
        }

        private async System.Threading.Tasks.Task LoadDashboardData()
        {
            try
            {
                _allLogs = await LogService.GetAllLogs(_currentUser.Id);

                if (_allLogs == null || !_allLogs.Any())
                {
                    SetEmptyDashboard();
                    return;
                }

                // Overall Statistics
                TxtTotalCatch.Text = $"{_allLogs.Count} Tangkapan";
                TxtTotalWeight.Text = $"{_allLogs.Sum(l => l.totalweight):N2} Kg";
                TxtTotalRevenue.Text = $"Rp {_allLogs.Sum(l => l.totalprice):N0}";
                TxtAvgWeight.Text = $"{_allLogs.Average(l => l.totalweight):N2} Kg";
                TxtAvgRevenue.Text = $"Rp {_allLogs.Average(l => l.totalprice):N0}";

                // Load monthly data
                LoadMonthlyData();

                // Load top fish species
                LoadTopFishSpecies();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error loading dashboard: {ex.Message}", "Error",
                    CustomMessageBox.MessageBoxButton.OK);
            }
        }

        private void SetEmptyDashboard()
        {
            TxtTotalCatch.Text = "0 Tangkapan";
            TxtTotalWeight.Text = "0 Kg";
            TxtTotalRevenue.Text = "Rp 0";
            TxtAvgWeight.Text = "0 Kg";
            TxtAvgRevenue.Text = "Rp 0";
            TxtMonthCatch.Text = "0 Tangkapan";
            TxtMonthWeight.Text = "0 Kg";
            TxtMonthRevenue.Text = "Rp 0";
            TopFishListView.ItemsSource = null;
        }

        private void LoadMonthlyData()
        {
            if (MonthComboBox.SelectedItem == null) return;

            var selectedMonth = (MonthItem)MonthComboBox.SelectedItem;

            var monthlyLogs = _allLogs.Where(l =>
                l.logdate.Month == selectedMonth.Month &&
                l.logdate.Year == selectedMonth.Year
            ).ToList();

            if (monthlyLogs.Any())
            {
                TxtMonthCatch.Text = $"{monthlyLogs.Count} Tangkapan";
                TxtMonthWeight.Text = $"{monthlyLogs.Sum(l => l.totalweight):N2} Kg";
                TxtMonthRevenue.Text = $"Rp {monthlyLogs.Sum(l => l.totalprice):N0}";
            }
            else
            {
                TxtMonthCatch.Text = "0 Tangkapan";
                TxtMonthWeight.Text = "0 Kg";
                TxtMonthRevenue.Text = "Rp 0";
            }
        }

        private void LoadTopFishSpecies()
        {
            // Ambil semua fish catch dari semua log
            var allCatches = _allLogs
                .SelectMany(log => log.Catches)
                .ToList();

            if (!allCatches.Any())
            {
                TopFishListView.ItemsSource = null;
                return;
            }

            // Group by fish name dan hitung statistik
            var topFish = allCatches
                .GroupBy(c => c.fishname)
                .Select(g => new TopFishItem
                {
                    FishName = g.Key,
                    Count = g.Count(),
                    TotalWeight = g.Sum(c => c.weight)
                })
                .OrderByDescending(f => f.Count)
                .Take(5)
                .ToList();

            TopFishListView.ItemsSource = topFish;
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            DashboardView.Visibility = Visibility.Visible;
            EditProfileView.Visibility = Visibility.Collapsed;

            BtnDashboard.Tag = "Active";
            BtnEditProfile.Tag = null;
            BtnLogout.Tag = null;
        }

        private void BtnEditProfile_Click(object sender, RoutedEventArgs e)
        {
            DashboardView.Visibility = Visibility.Collapsed;
            EditProfileView.Visibility = Visibility.Visible;

            BtnEditProfile.Tag = "Active";
            BtnDashboard.Tag = null;
            BtnLogout.Tag = null;
        }

        private void MonthComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_allLogs != null && _allLogs.Any())
            {
                LoadMonthlyData();
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                CustomMessageBox.Show("User tidak ditemukan.", "Error",
                    CustomMessageBox.MessageBoxButton.OK);
                return;
            }

            string newUsername = UsernameBox.Text.Trim();
            string newPassword = PasswordBox.Password;

            if (string.IsNullOrEmpty(newUsername))
            {
                CustomMessageBox.Show("Username tidak boleh kosong.", "Validasi",
                    CustomMessageBox.MessageBoxButton.OK);
                return;
            }

            try
            {
                bool success = await UserService.UpdateUserAsync(
                    _currentUser.Id,
                    newUsername,
                    newPassword
                );

                if (success)
                {
                    CustomMessageBox.Show("Profile berhasil diupdate!", "Sukses",
                        CustomMessageBox.MessageBoxButton.OK);

                    _currentUser = new User(_currentUser.Id, newUsername);
                    Session.CurrentUser = _currentUser;
                    SidebarUsername.Text = newUsername;

                    PasswordBox.Clear();
                }
                else
                {
                    CustomMessageBox.Show("Gagal mengupdate profile. Username mungkin sudah digunakan.",
                        "Error", CustomMessageBox.MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Terjadi kesalahan: {ex.Message}", "Error",
                    CustomMessageBox.MessageBoxButton.OK);
            }
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = LogoutMessageBox.Show(
                "Apakah Anda yakin ingin logout?",
                "Konfirmasi Logout",
                LogoutMessageBox.MessageBoxButton.YesNo);

            if (result == LogoutMessageBox.MessageBoxResult.Yes)
            {
                // Clear session
                Session.CurrentUser = null;

                // Redirect ke Login page
                NavigationService?.Navigate(new Uri("Views/LoginPage.xaml", UriKind.Relative));

                LogoutMessageBox.Show("Anda berhasil logout.", "Logout",
                    LogoutMessageBox.MessageBoxButton.OK);
            }
        }
    }

    // Helper classes
    public class MonthItem
    {
        public string Display { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class TopFishItem
    {
        public string FishName { get; set; }
        public int Count { get; set; }
        public double TotalWeight { get; set; }
    }
}