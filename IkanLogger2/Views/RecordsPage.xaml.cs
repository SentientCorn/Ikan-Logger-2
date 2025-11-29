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
                if (Session.CurrentUser == null || Session.CurrentUser.Id <= 0)
                {
                    MessageBox.Show("Silakan login terlebih dahulu",
                                  "Session Error",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    NavigationService?.Navigate(new LoginPage());
                    return;
                }

                // Show loading
                LoadingPanel.Visibility = Visibility.Visible;
                RecordsContainer.Children.Clear();

                // Load data
                var logs = await LogService.GetAllLogs(Session.CurrentUser.Id);

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
                    var card = CreateStyledLogCard(log);
                    RecordsContainer.Children.Add(card);
                }
            }
            catch (Exception ex)
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
                MessageBox.Show($"Error loading records: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        // Card dengan styling sesuai gambar
        private Border CreateStyledLogCard(CatchLogDetail log)
        {
            // Main Card Border
            var card = new Border
            {
                Width = 280,
                Margin = new Thickness(15, 15, 15, 15), // Fixed Thickness
                Background = Brushes.White,
                CornerRadius = new CornerRadius(12),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 15,
                    ShadowDepth = 0,
                    Opacity = 0.2
                }
            };

            var stackPanel = new StackPanel();

            // Header (Date) - warna biru
            var header = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2A5F7E")),
                CornerRadius = new CornerRadius(12, 12, 0, 0),
                Padding = new Thickness(15, 12, 15, 12) // Fixed Thickness
            };

            var dateText = new TextBlock
            {
                Text = log.logdate.ToString("dddd, dd MMMM yyyy"),
                FontFamily = new FontFamily("Plus Jakarta Sans"),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            };

            header.Child = dateText;
            stackPanel.Children.Add(header);

            // Content - Gunakan Border untuk padding
            var contentBorder = new Border
            {
                Padding = new Thickness(15, 15, 15, 15) // Fixed Thickness
            };

            var contentPanel = new StackPanel();

            // Location/Notes
            var locationText = new TextBlock
            {
                Text = !string.IsNullOrWhiteSpace(log.notes) ? log.notes : "Catatan Tangkapan",
                FontFamily = new FontFamily("Plus Jakarta Sans"),
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1F4F6E")),
                Margin = new Thickness(0, 0, 0, 8),
                TextWrapping = TextWrapping.Wrap
            };
            contentPanel.Children.Add(locationText);

            // Fish Catched Label
            var fishLabel = new TextBlock
            {
                Text = "Fish Catched:",
                FontFamily = new FontFamily("Plus Jakarta Sans"),
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1F4F6E")),
                Margin = new Thickness(0, 0, 0, 5)
            };
            contentPanel.Children.Add(fishLabel);

            // Fish List
            if (log.Catches != null && log.Catches.Count > 0)
            {
                foreach (var fish in log.Catches)
                {
                    var fishItem = new TextBlock
                    {
                        Text = $"• {fish.fishname} : {fish.weight:N2} Kg",
                        FontFamily = new FontFamily("Plus Jakarta Sans"),
                        FontSize = 12,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1F4F6E")),
                        Margin = new Thickness(0, 0, 0, 3)
                    };
                    contentPanel.Children.Add(fishItem);
                }
            }

            contentBorder.Child = contentPanel;
            stackPanel.Children.Add(contentBorder);
            card.Child = stackPanel;

            return card;
        }

        private Border CreateDetailedLogCard(CatchLogDetail log)
        {
            var card = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 0, 20),
                Padding = new Thickness(20, 20, 20, 20) // Fixed Thickness
            };

            // Add shadow effect
            card.Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 10,
                ShadowDepth = 2,
                Opacity = 0.1
            };

            var mainStack = new StackPanel();

            // === HEADER SECTION ===
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Date
            var dateText = new TextBlock
            {
                Text = log.logdate.ToString("dddd, dd MMMM yyyy"),
                FontFamily = new FontFamily("Plus Jakarta Sans"),
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1F4F6E"))
            };
            Grid.SetColumn(dateText, 0);
            headerGrid.Children.Add(dateText);

            // Time
            var timeText = new TextBlock
            {
                Text = log.logdate.ToString("HH:mm"),
                FontFamily = new FontFamily("Plus Jakarta Sans"),
                FontSize = 14,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetColumn(timeText, 1);
            headerGrid.Children.Add(timeText);

            mainStack.Children.Add(headerGrid);

            // Notes (if exists)
            if (!string.IsNullOrWhiteSpace(log.notes))
            {
                var notesText = new TextBlock
                {
                    Text = log.notes,
                    FontFamily = new FontFamily("Plus Jakarta Sans"),
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = Brushes.DarkGray,
                    FontSize = 13,
                    Margin = new Thickness(0, 8, 0, 0),
                    FontStyle = FontStyles.Italic
                };
                mainStack.Children.Add(notesText);
            }

            // Separator 1
            var separator1 = new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                Margin = new Thickness(0, 15, 0, 15)
            };
            mainStack.Children.Add(separator1);

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
                Foreground = Brushes.Black
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

            mainStack.Children.Add(summaryGrid);

            // === FISH CATCHES SECTION ===
            if (log.Catches != null && log.Catches.Count > 0)
            {
                // Section Header
                var fishHeader = new TextBlock
                {
                    Text = $"Detail Tangkapan ({log.Catches.Count} jenis ikan)",
                    FontFamily = new FontFamily("Plus Jakarta Sans"),
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 14,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1F4F6E")),
                    Margin = new Thickness(0, 0, 0, 10)
                };
                mainStack.Children.Add(fishHeader);

                // Fish List
                var fishListBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5),
                    Padding = new Thickness(12, 12, 12, 12) // Fixed Thickness
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
                mainStack.Children.Add(fishListBorder);
            }

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
                FontWeight = FontWeights.SemiBold,
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
                Foreground = Brushes.Gray,
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