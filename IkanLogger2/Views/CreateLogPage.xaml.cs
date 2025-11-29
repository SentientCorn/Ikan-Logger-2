using IkanLogger2.Core;
using IkanLogger2.Models;
using IkanLogger2.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static IkanLogger2.Services.LogService;

namespace IkanLogger2.Views
{
    public partial class CreateLogPage : Page
    {
        private List<Fish> _allFishes = new List<Fish>();
        private Fish _selectedFish = null;
        private List<TempCatchItem> _catchItems = new List<TempCatchItem>();
        private double _lat;
        private double _lng;

        public CreateLogPage(double latitude, double longitude)
        {
            InitializeComponent();
            _lat = latitude;
            _lng = longitude;
            Loaded += async (s, e) => await LoadFishData();
        }

        private async Task LoadFishData()
        {
            try
            {
                // Validasi session
                if (Session.CurrentUser == null)
                {
                    MessageBox.Show("Silakan login terlebih dahulu");
                    NavigationService?.Navigate(new LoginPage());
                    return;
                }

                _allFishes = await FishService.GetAllFishAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading fish data: {ex.Message}");
            }
        }

        // Autocomplete untuk pencarian ikan
        private void TxtFishSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = TxtFishSearch.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                FishSuggestionPopup.IsOpen = false;
                return;
            }

            var filtered = _allFishes
                .Where(f => f.FishName.ToLower().Contains(searchText))
                .ToList();

            if (filtered.Count > 0)
            {
                FishSuggestionList.ItemsSource = filtered;
                FishSuggestionPopup.IsOpen = true;
            }
            else
            {
                FishSuggestionPopup.IsOpen = false;
            }
        }

        private void TxtFishSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtFishSearch.Text))
            {
                TxtFishSearch_TextChanged(sender, null);
            }
        }

        private void FishSuggestionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FishSuggestionList.SelectedItem is Fish selectedFish)
            {
                _selectedFish = selectedFish;
                TxtFishSearch.Text = selectedFish.FishName;
                FishSuggestionPopup.IsOpen = false;

                // Tampilkan selected fish display
                TxtSelectedFishName.Text = selectedFish.FishName;
                TxtSelectedFishPrice.Text = $"Harga pasar: Rp {selectedFish.MarketPrice:N0}/kg";
                SelectedFishDisplay.Visibility = Visibility.Visible;

                // Focus ke input berat
                TxtWeight.Focus();

                UpdateAddButtonState();
            }
        }

        private void BtnClearSelectedFish_Click(object sender, RoutedEventArgs e)
        {
            _selectedFish = null;
            TxtFishSearch.Text = "";
            SelectedFishDisplay.Visibility = Visibility.Collapsed;
            UpdateAddButtonState();
        }

        private void UpdateAddButtonState()
        {
            bool isValidWeight = double.TryParse(TxtWeight.Text,
                                        NumberStyles.Any,
                                        CultureInfo.InvariantCulture,
                                        out double weight);
            BtnAddFish.IsEnabled = _selectedFish != null &&
                                   !string.IsNullOrWhiteSpace(TxtWeight.Text) &&
                                   weight > 0;
        }

        private void BtnAddFish_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFish == null || !double.TryParse(TxtWeight.Text, out double weight) || weight <= 0)
            {
                MessageBox.Show("Pilih ikan dan masukkan berat yang valid",
                              "Input Tidak Valid",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            // Tambah ke list
            var catchItem = new TempCatchItem
            {
                FishId = _selectedFish.IdFish,
                FishName = _selectedFish.FishName,
                MarketPrice = _selectedFish.MarketPrice,
                Weight = weight
            };

            _catchItems.Add(catchItem);

            // Refresh display
            RefreshCatchList();
            UpdateSummary();

            // Clear input
            _selectedFish = null;
            TxtFishSearch.Text = "";
            TxtWeight.Text = "";
            SelectedFishDisplay.Visibility = Visibility.Collapsed;
            UpdateAddButtonState();

            // Enable save button
            BtnSave.IsEnabled = _catchItems.Count > 0;
        }

        private void RefreshCatchList()
        {
            CatchListContainer.Children.Clear();

            if (_catchItems.Count == 0)
            {
                EmptyListMessage.Visibility = Visibility.Visible;
                CatchListContainer.Children.Add(EmptyListMessage);
                return;
            }

            EmptyListMessage.Visibility = Visibility.Collapsed;

            foreach (var item in _catchItems)
            {
                var card = CreateCatchItemCard(item);
                CatchListContainer.Children.Add(card);
            }
        }

        private Border CreateCatchItemCard(TempCatchItem item)
        {
            var card = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Row 0 - Fish name and delete button
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nameText = new TextBlock
            {
                Text = item.FishName,
                FontWeight = FontWeights.SemiBold,
                FontSize = 13
            };
            Grid.SetColumn(nameText, 0);
            headerGrid.Children.Add(nameText);

            var deleteBtn = new Button
            {
                Content = "🗑",
                Width = 25,
                Height = 25,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = Brushes.Red,
                Cursor = Cursors.Hand,
                Tag = item
            };
            deleteBtn.Click += DeleteCatchItem_Click;
            Grid.SetColumn(deleteBtn, 1);
            headerGrid.Children.Add(deleteBtn);

            Grid.SetRow(headerGrid, 0);
            grid.Children.Add(headerGrid);

            // Row 1 - Details
            var detailStack = new StackPanel
            {
                Margin = new Thickness(0, 5, 0, 0)
            };

            var weightText = new TextBlock
            {
                Text = $"Berat: {item.Weight:N2} kg",
                FontSize = 12,
                Foreground = Brushes.Gray
            };
            detailStack.Children.Add(weightText);

            var priceText = new TextBlock
            {
                Text = $"Total: Rp {item.TotalPrice:N0}",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(40, 167, 69)),
                Margin = new Thickness(0, 2, 0, 0)
            };
            detailStack.Children.Add(priceText);

            Grid.SetRow(detailStack, 1);
            grid.Children.Add(detailStack);

            card.Child = grid;
            return card;
        }

        private void DeleteCatchItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TempCatchItem item)
            {
                _catchItems.Remove(item);
                RefreshCatchList();
                UpdateSummary();
                BtnSave.IsEnabled = _catchItems.Count > 0;
            }
        }

        private void UpdateSummary()
        {
            double totalWeight = _catchItems.Sum(c => c.Weight);
            double totalPrice = _catchItems.Sum(c => c.TotalPrice);

            TxtTotalWeight.Text = $"{totalWeight:N2} kg";
            TxtTotalPrice.Text = $"Rp {totalPrice:N0}";
        }

        // Replace the BtnSave_Click method in CreateLogPage.xaml.cs:

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_catchItems.Count == 0)
            {
                MessageBox.Show("Tambahkan minimal satu ikan tangkapan",
                              "Data Tidak Lengkap",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Simpan catatan tangkapan dengan {_catchItems.Count} jenis ikan?",
                "Konfirmasi",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            string notes = TxtNotes.Text;
            var catches = _catchItems.Select(c => new FishCatchInput
            {
                FishId = c.FishId,
                Weight = c.Weight,
                SalePrice = c.TotalPrice
            }).ToList();

            try
            {
                BtnSave.IsEnabled = false;
                BtnSave.Content = "⏳ Menyimpan...";

                // FIXED: Call async method directly without Task.Run
                bool success = await LogService.CreateCatchLogAsync(
                    Session.CurrentUser.Id,
                    notes,
                    _lat,
                    _lng,
                    catches
                );

                if (success)
                {
                    MessageBox.Show("Catatan tangkapan berhasil disimpan!",
                                  "Berhasil",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);

                    NavigationService?.GoBack();
                }
            }
            catch (Exception ex)
            {
                var errorMessage = new System.Text.StringBuilder();
                errorMessage.AppendLine("=== ERROR DETAILS ===");
                errorMessage.AppendLine();
                errorMessage.AppendLine($"Message: {ex.Message}");
                errorMessage.AppendLine();
                errorMessage.AppendLine($"Type: {ex.GetType().Name}");
                errorMessage.AppendLine();

                if (ex.InnerException != null)
                {
                    errorMessage.AppendLine("=== INNER EXCEPTION ===");
                    errorMessage.AppendLine($"Message: {ex.InnerException.Message}");
                    errorMessage.AppendLine($"Type: {ex.InnerException.GetType().Name}");
                    errorMessage.AppendLine();
                }

                errorMessage.AppendLine("=== STACK TRACE ===");
                errorMessage.AppendLine(ex.StackTrace);

                if (ex.InnerException != null && ex.InnerException.StackTrace != null)
                {
                    errorMessage.AppendLine();
                    errorMessage.AppendLine("=== INNER STACK TRACE ===");
                    errorMessage.AppendLine(ex.InnerException.StackTrace);
                }

                System.Diagnostics.Debug.WriteLine(errorMessage.ToString());
                MessageBox.Show($"Error: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                BtnSave.IsEnabled = true;
                BtnSave.Content = "💾 Simpan Catatan";
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Batalkan pembuatan catatan? Data yang sudah diisi akan hilang.",
                "Konfirmasi",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                NavigationService?.GoBack();
            }
        }
        private void TxtWeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateAddButtonState();
        }
    }
}