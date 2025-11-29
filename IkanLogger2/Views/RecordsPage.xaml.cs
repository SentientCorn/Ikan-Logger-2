using IkanLogger2.Core;
using IkanLogger2.Models;
using IkanLogger2.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace IkanLogger2.Views
{
    public partial class RecordsPage : Page
    {
        public RecordsPage()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadAllRecords();
        }

        private async Task LoadAllRecords()
        {
            try
            {
                // Validasi session user
                //if (Session.CurrentUser == null || Session.CurrentUser.Id <= 0)
                //{
                //    MessageBox.Show("Silakan login terlebih dahulu",
                //                  "Session Error",
                //                  MessageBoxButton.OK,
                //                  MessageBoxImage.Warning);
                //    NavigationService?.Navigate(new LoginPage());
                //    return;
                //}

                // Show loading
                LoadingPanel.Visibility = Visibility.Visible;
                RecordsContainer.Children.Clear();

                // Load data
                var logs = await LogService.GetAllLogs(1);//Session.CurrentUser.Id

                // Hide loading
                LoadingPanel.Visibility = Visibility.Collapsed;

                if (logs == null || logs.Count == 0)
                {
                    EmptyStatePanel.Visibility = Visibility.Visible;
                    return;
                }

                EmptyStatePanel.Visibility = Visibility.Collapsed;

                // Render cards dengan styling baru
                foreach (var log in logs)
                {
                    var card = CreateDetailedLogCard(log);
                    RecordsContainer.Children.Add(card);
                }
            }
            catch (Exception ex)
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
                CustomMessageBox.Show($"Error loading records: {ex.Message}",
                              "Error",
                              CustomMessageBox.MessageBoxButton.OK);
            }
        }

        private Border CreateDetailedLogCard(CatchLogDetail log)
        {
            var card = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1F4F6E")),
                BorderThickness = new Thickness(3),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 0, 20)
            };

            var mainStack = new StackPanel
            {
                Margin = new Thickness(0)
            };

            // === HEADER BLUE SECTION ===
            var headerBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1F4F6E")),
                CornerRadius = new CornerRadius(6, 6, 0, 0),
                Padding = new Thickness(16, 12, 16, 12)
            };

            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // DATE (kiri) - seperti pada gambar
            var dateText = new TextBlock
            {
                Text = log.logdate.ToString("dddd, dd MMMM yyyy"),
                FontFamily = new FontFamily("Plus Jakarta Sans"),
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(dateText, 0);
            headerGrid.Children.Add(dateText);


            // Time (right side)
            var timeText = new TextBlock
            {
                Text = log.logdate.ToString("HH:mm"),
                FontFamily = new FontFamily("Plus Jakarta Sans"),
                FontWeight = FontWeights.Bold,
                FontSize = 18,
                VerticalAlignment = VerticalAlignment.Top,
                Foreground = Brushes.White
            };
            Grid.SetColumn(timeText, 1);
            headerGrid.Children.Add(timeText);


            headerBorder.Child = headerGrid;
            mainStack.Children.Add(headerBorder);

            // === CONTENT SECTION ===
            var contentBorder = new Border
            {
                Padding = new Thickness(16, 16, 16, 16)
            };

            var contentStack = new StackPanel();

            // NOTES SECTION (jika ada)
            if (!string.IsNullOrWhiteSpace(log.notes))
            {
                var notesBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1F4F6E")),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(12, 8, 12, 8),
                    Margin = new Thickness(0, 0, 0, 15)
                };

                var notesText = new TextBlock
                {
                    Text = $"Catatan: {log.notes}",
                    FontFamily = new FontFamily("Plus Jakarta Sans"),
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1F4F6E")),
                    FontSize = 13,
                    FontStyle = FontStyles.Italic
                };
                notesBorder.Child = notesText;
                contentStack.Children.Add(notesBorder);
            }

            // === SUMMARY SECTION ===
            var summaryGrid = new Grid
            {
                Margin = new Thickness(0, 0, 0, 15)
            };
            summaryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            summaryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Total Weight
            var weightStack = new StackPanel();
            var weightLabel = new TextBlock
            {
                Text = "Total Berat",
                FontFamily = new FontFamily("Plus Jakarta Sans"),
                FontSize = 12,
                Foreground = Brushes.Gray
            };
            var weightValue = new TextBlock
            {
                Text = $"{log.totalweight:N2} kg",
                FontFamily = new FontFamily("Plus Jakarta Sans"),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1F4F6E")),
            };
            weightStack.Children.Add(weightLabel);
            weightStack.Children.Add(weightValue);
            Grid.SetColumn(weightStack, 0);
            summaryGrid.Children.Add(weightStack);

            // Total Price
            var priceStack = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var priceLabel = new TextBlock
            {
                Text = "Total Pendapatan",
                FontFamily = new FontFamily("Plus Jakarta Sans"),
                FontSize = 12,
                Foreground = Brushes.Gray,
                TextAlignment = TextAlignment.Right
            };
            var priceValue = new TextBlock
            {
                Text = $"Rp {log.totalprice:N0}",
                FontFamily = new FontFamily("Plus Jakarta Sans"),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(34, 139, 34)),
                TextAlignment = TextAlignment.Right
            };
            priceStack.Children.Add(priceLabel);
            priceStack.Children.Add(priceValue);
            Grid.SetColumn(priceStack, 1);
            summaryGrid.Children.Add(priceStack);

            contentStack.Children.Add(summaryGrid);

            // === FISH CATCHES SECTION ===
            if (log.Catches != null && log.Catches.Count > 0)
            {
                // Section Header
                var fishHeader = new TextBlock
                {
                    Text = $"Detail Tangkapan ({log.Catches.Count} Jenis Ikan)",
                    FontFamily = new FontFamily("Plus Jakarta Sans"),
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 14,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1F4F6E")),
                    Margin = new Thickness(0, 0, 0, 10)
                };
                contentStack.Children.Add(fishHeader);

                // Fish List
                var fishListBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1F4F6E")),
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(5),
                    Padding = new Thickness(12, 12, 12, 12)
                };

                var fishListStack = new StackPanel();

                foreach (var fish in log.Catches)
                {
                    var fishItem = CreateFishCatchItem(fish);
                    fishListStack.Children.Add(fishItem);

                    // Add separator between items (except last item)
                    if (fish != log.Catches.Last())
                    {
                        var itemSeparator = new Border
                        {
                            Height = 1,
                            Background = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                            Margin = new Thickness(0, 8, 0, 8)
                        };
                        fishListStack.Children.Add(itemSeparator);
                    }
                }

                fishListBorder.Child = fishListStack;
                contentStack.Children.Add(fishListBorder);
            }

            contentBorder.Child = contentStack;
            mainStack.Children.Add(contentBorder);
            card.Child = mainStack;

            return card;
        }

        private Grid CreateFishCatchItem(FishCatchDetail fish)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });

            // Fish Name
            var nameText = new TextBlock
            {
                Text = fish.fishname,
                FontFamily = new FontFamily("Plus Jakarta Sans"),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1F4F6E")),
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(nameText, 0);
            grid.Children.Add(nameText);

            // Weight
            var weightText = new TextBlock
            {
                Text = $"{fish.weight:N2} kg",
                FontFamily = new FontFamily("Plus Jakarta Sans"),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1F4F6E")),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(weightText, 1);
            grid.Children.Add(weightText);

            // Price
            var priceText = new TextBlock
            {
                Text = $"Rp {fish.saleprice:N0}",
                FontFamily = new FontFamily("Plus Jakarta Sans"),
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(34, 139, 34)),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Right
            };
            Grid.SetColumn(priceText, 2);
            grid.Children.Add(priceText);

            return grid;
        }

        private void BtnCreateNew_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}