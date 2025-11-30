using GMap.NET;
using GMap.NET.WindowsPresentation;
using IkanLogger2.Core;
using IkanLogger2.Models;
using IkanLogger2.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace IkanLogger2.Views
{
    public partial class DashboardPage : Page
    {
        private readonly MapService _mapController;
        private GMapMarker _tempMarker;
        public double SelectedLongitude { get; private set; }
        public double SelectedLatitude { get; private set; }

        private const string PERMANENT_MARKER_TAG = "PERMANENT_MARKER";
        private const string TEMPORARY_MARKER_TAG = "TEMPORARY_MARKER";
        private const string LOG_MARKER_TAG = "LOG_MARKER";


        private List<FishLocation> _allLocations = new List<FishLocation>();
        public DashboardPage()
        {
            InitializeComponent();

            _mapController = new MapService();
            _mapController.Configure(MapControl, -8.0245, 110.3290, 11);

            Loaded += async (s, e) => await LoadDataAndSetupFilter();
        }

        // Load Data & Siapkan Filter (Dipanggil sekali saat Start)
        private async Task LoadDataAndSetupFilter()
        {
            try
            {
                // Ambil data dari database
                _allLocations = await FishService.GetFishLocationsAsync();

                if (_allLocations == null || _allLocations.Count == 0)
                {
                    CustomMessageBox.Show("Tidak ada data lokasi ikan");
                    return;
                }

                // --- LOGIKA FILTER ---
                var fishNames = _allLocations
                    .SelectMany(loc => loc.Fishes) // Ratakan list ikan
                    .Select(f => f.FishName)       // Ambil namanya saja
                    .Distinct()                    // Hapus duplikat
                    .OrderBy(n => n)               // Urutkan abjad
                    .ToList();

                fishNames.Insert(0, "Semua Ikan");

                FishFilterComboBox.ItemsSource = fishNames;
                FishFilterComboBox.SelectedIndex = 0;

                // Render awal (tampilkan semua)
                RenderMarkers(_allLocations);
                ResetTempMarker();

                await LoadRecentLogs(Session.CurrentUser.Id);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        // Load Recent Logs
        private async Task LoadRecentLogs(int userId)
        {
            try
            {
                var logs = await LogService.GetRecentLog(userId);

                RecentLogsContainer.Children.Clear();

                // Remove all previous log markers ONCE
                var oldLogs = MapControl.Markers
                    .Where(m => m.Tag is string tag && tag == LOG_MARKER_TAG)
                    .ToList();

                foreach (var old in oldLogs)
                    MapControl.Markers.Remove(old);

                if (logs == null || logs.Count == 0)
                {
                    var emptyText = new TextBlock
                    {
                        Text = "Belum ada log tangkapan",
                        Foreground = Brushes.Gray,
                        FontStyle = FontStyles.Italic,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 20, 0, 0)
                    };
                    RecentLogsContainer.Children.Add(emptyText);
                    return;
                }

                foreach (var log in logs)
                {
                    // Skip logs with no coordinates
                    if (log.latitude == 0 && log.longitude == 0)
                        continue;

                    // Create UI card
                    var logCard = CreateLogCard(log);
                    RecentLogsContainer.Children.Add(logCard);

                    // Create and add map marker
                    var position = new PointLatLng(log.latitude, log.longitude);
                    var marker = CreateLogMarker(position, log);
                    MapControl.Markers.Add(marker);
                }


            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error loading logs: {ex.Message}");
            }
        }

        // Membuat Card untuk setiap Log
        private Border CreateLogCard(CatchLog log)
        {
            var card = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1F4F6E")),
                BorderThickness = new Thickness(1.5),
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(12)
            };

            var stackPanel = new StackPanel();

            // Tanggal
            var dateText = new TextBlock
            {
                FontFamily = new FontFamily("Plus Jakarta Sans"),
                Text = log.logdate.ToString("dddd, dd MMMM yyyy"),
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1F4F6E")),
                Margin = new Thickness(0, 0, 0, 8)
            };
            stackPanel.Children.Add(dateText);

            // Notes
            if (!string.IsNullOrWhiteSpace(log.notes))
            {
                var notesText = new TextBlock
                {
                    FontFamily = new FontFamily("Plus Jakarta Sans"),
                    Text = log.notes,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1F4F6E")),
                    FontSize = 12,
                    Margin = new Thickness(0, 0, 0, 8)
                };
                stackPanel.Children.Add(notesText);
            }

            // Separator
            var separator = new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                Margin = new Thickness(0, 5, 0, 8)
            };
            stackPanel.Children.Add(separator);

            // Info Grid (Weight & Price)
            var infoGrid = new Grid();
            infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Total Weight
            var weightStack = new StackPanel();
            var weightLabel = new TextBlock
            {
                FontFamily = new FontFamily("Plus Jakarta Sans"),
                Text = "Total Berat",
                FontSize = 11,
                Foreground = Brushes.Gray
            };
            var weightValue = new TextBlock
            {
                FontFamily = new FontFamily("Plus Jakarta Sans"),
                Text = $"{log.totalweight:N2} kg",
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1F4F6E")),
            };
            weightStack.Children.Add(weightLabel);
            weightStack.Children.Add(weightValue);
            Grid.SetColumn(weightStack, 0);
            infoGrid.Children.Add(weightStack);

            // Total Price
            var priceStack = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var priceLabel = new TextBlock
            {
                FontFamily = new FontFamily("Plus Jakarta Sans"),
                Text = "Total Harga",
                FontSize = 11,
                Foreground = Brushes.Gray,
                TextAlignment = TextAlignment.Right
            };
            var priceValue = new TextBlock
            {
                FontFamily = new FontFamily("Plus Jakarta Sans"),
                Text = $"Rp {log.totalprice:N0}",
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(34, 139, 34)),
                TextAlignment = TextAlignment.Right
            };
            priceStack.Children.Add(priceLabel);
            priceStack.Children.Add(priceValue);
            Grid.SetColumn(priceStack, 1);
            infoGrid.Children.Add(priceStack);

            stackPanel.Children.Add(infoGrid);
            card.Child = stackPanel;

            return card;
        }

        // Event saat Dropdown diganti
        private void FishFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FishFilterComboBox.SelectedItem is string selectedFish)
            {
                List<FishLocation> filteredList;

                if (selectedFish == "Semua Ikan")
                {
                    filteredList = _allLocations;
                }
                else
                {
                    filteredList = _allLocations
                        .Where(loc => loc.Fishes.Any(f => f.FishName == selectedFish))
                        .ToList();
                }
                // Gambar ulang marker sesuai hasil filter
                RenderMarkers(filteredList);
            }
        }

        // Menggambar Marker (Dipisah agar bisa dipanggil ulang)
        private void RenderMarkers(List<FishLocation> locationsToRender)
        {
            // Remove only previous permanent markers (keep temp marker if present)
            var toRemove = MapControl.Markers
                .Where(m => m.Tag is string tag && tag == PERMANENT_MARKER_TAG)
                .ToList();

            foreach (var m in toRemove)
            {
                MapControl.Markers.Remove(m);
            }

            FishInfoPanel.Visibility = Visibility.Collapsed;

            foreach (var loc in locationsToRender)
            {
                string tooltipContent = $"Lokasi #{loc.IdLocation}\nKlik untuk detail.";
                PointLatLng center = new PointLatLng(loc.Latitude, loc.Longitude);

                var radiusMarker = CreateCirclePolygon(center, 3.0);
                // mark polygon (if you want to identify it later, set Tag too)
                radiusMarker.Tag = PERMANENT_MARKER_TAG;
                MapControl.Markers.Add(radiusMarker);

                var pinMarker = CreatePinMarker(center, tooltipContent, loc);
                // mark the pin as permanent so we can remove it later without touching temp marker
                pinMarker.Tag = PERMANENT_MARKER_TAG;
                MapControl.Markers.Add(pinMarker);
            }

            if (locationsToRender.Count > 0)
            {
                var firstLoc = locationsToRender[0];
                MapControl.Position = new PointLatLng(firstLoc.Latitude, firstLoc.Longitude);
                MapControl.Zoom = 12;
            }
        }


        // Membuat Lingkaran Geografis (Polygon)
        private GMapMarker CreateCirclePolygon(PointLatLng center, double radiusKm)
        {
            List<PointLatLng> points = new List<PointLatLng>();
            int segments = 60;

            for (int i = 0; i < segments; i++)
            {
                double angle = (Math.PI * 2 * i) / segments;
                double bearing = angle * 180.0 / Math.PI;
                points.Add(CalculatePointAtDistance(center, radiusKm, bearing));
            }

            var polygon = new GMapPolygon(points);

            Path shape = polygon.Shape as Path;
            if (shape == null)
            {
                shape = new Path();
                polygon.Shape = shape;
            }

            // Sekarang aman untuk mengakses properti shape
            shape.Fill = new SolidColorBrush(Color.FromArgb(40, 0, 120, 215));
            shape.Stroke = new SolidColorBrush(Color.FromArgb(150, 0, 120, 215));
            shape.StrokeThickness = 1.5;

            polygon.ZIndex = 1;

            return polygon;
        }

        // Pin pada peta
        private GMapMarker CreatePinMarker(PointLatLng position, string tooltipText, FishLocation data)
        {
            var marker = new GMapMarker(position)
            {
                ZIndex = 100,
                Tag = data 
            };
            var pinVisual = new Grid
            {
                Width = 30,
                Height = 40,
                
                Tag = data,
                
                Cursor = Cursors.Hand
            };

            // Pasang event handler langsung ke objek visual pin
            pinVisual.MouseLeftButtonDown += PinVisual_MouseLeftButtonDown;

            var pinPointer = new Polygon
            {
                Fill = Brushes.Red,
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Points = new PointCollection { new Point(15, 26), new Point(8, 38), new Point(22, 38) }
            };

            var pinCircle = new Ellipse
            {
                Width = 26,
                Height = 26,
                Fill = Brushes.Red,
                Stroke = Brushes.White,
                StrokeThickness = 3,
                Margin = new Thickness(2, 0, 2, 14)
            };

            var innerDot = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.White,
                Margin = new Thickness(10, 8, 10, 22)
            };

            pinVisual.Children.Add(pinPointer);
            pinVisual.Children.Add(pinCircle);
            pinVisual.Children.Add(innerDot);

            // Tooltip tetap ada
            ToolTip toolTipObj = new ToolTip
            {
                Content = tooltipText,
                Background = Brushes.White,
                Foreground = Brushes.Black,
                Padding = new Thickness(5),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1)
            };

            pinVisual.ToolTip = toolTipObj;
            marker.Shape = pinVisual;
            marker.Offset = new Point(-15, -40);

            return marker;
        }

        private void ResetTempMarker()
        {
            if (_tempMarker != null)
            {
                MapControl.Markers.Remove(_tempMarker);
                _tempMarker = null;
            }

            SelectedLatitude = 0;
            SelectedLongitude = 0;
        }


        private GMapMarker CreateLogMarker(PointLatLng position, CatchLog log)
        {
            var marker = new GMapMarker(position)
            {
                ZIndex = 500,
                Tag = LOG_MARKER_TAG
            };

            // Simple circular log marker
            var visual = new Ellipse
            {
                Width = 18,
                Height = 18,
                Fill = new SolidColorBrush(Color.FromRgb(0, 120, 215)), // Blue
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Cursor = Cursors.Hand
            };

            // Tooltip: show date + notes
            visual.ToolTip = new ToolTip
            {
                Content = $"{log.logdate:dd MMM yyyy}\n{log.notes}",
                Padding = new Thickness(5),
                Background = Brushes.White,
                Foreground = Brushes.Black
            };

            // Event handler when the log marker is clicked
            visual.MouseLeftButtonDown += (s, e) =>
            {
                e.Handled = true;

                TxtLocationName.Text = $"Tanggal: {log.logdate:dd MMM yyyy}\n" + $"Log di {position.Lat:F4}, {position.Lng:F4}";
                TxtFishList.Text =
                    $"Catatan:\n{log.notes}\n\n" +
                    $"Berat Total: {log.totalweight:N2} kg\n" +
                    $"Harga Total: Rp {log.totalprice:N0}";

                FishInfoPanel.Visibility = Visibility.Visible;
            };

            marker.Shape = visual;
            marker.Offset = new Point(-9, -9);

            return marker;
        }


        // Method Helper: Rumus Haversine untuk mencari koordinat baru berdasarkan jarak
        private PointLatLng CalculatePointAtDistance(PointLatLng center, double distanceKm, double bearing)
        {
            double earthRadius = 6371.0;
            double lat1 = center.Lat * (Math.PI / 180.0);
            double lon1 = center.Lng * (Math.PI / 180.0);
            double bearingRad = bearing * (Math.PI / 180.0);
            double angularDistance = distanceKm / earthRadius;

            double lat2 = Math.Asin(
                Math.Sin(lat1) * Math.Cos(angularDistance) +
                Math.Cos(lat1) * Math.Sin(angularDistance) * Math.Cos(bearingRad)
            );

            double lon2 = lon1 + Math.Atan2(
                Math.Sin(bearingRad) * Math.Sin(angularDistance) * Math.Cos(lat1),
                Math.Cos(angularDistance) - Math.Sin(lat1) * Math.Sin(lat2)
            );

            return new PointLatLng(
                lat2 * (180.0 / Math.PI),
                lon2 * (180.0 / Math.PI)
            );
        }

        // Event Handler saat Pin Visual diklik
        private void PinVisual_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Ambil objek visual yang diklik (Grid pinVisual)
            if (sender is Grid pinVisual && pinVisual.Tag is FishLocation loc)
            {
                // Cegah event bubbling (agar map tidak ikut ter-drag/klik saat pin diklik)
                e.Handled = true;

                // 1. Isi Data ke Panel
                TxtLocationName.Text = $"Koordinat: {loc.Latitude:F4}, {loc.Longitude:F4}";

                if (loc.Fishes != null && loc.Fishes.Count > 0)
                {
                    string listIkan = "";
                    foreach (var fish in loc.Fishes)
                    {
                        listIkan += $"• {fish.FishName}:   Rp {fish.MarketPrice:N0}\n";
                    }
                    TxtFishList.Text = listIkan;
                }
                else
                {
                    TxtFishList.Text = "Tidak ada data ikan.";
                }

                // 2. Tampilkan Panel
                FishInfoPanel.Visibility = Visibility.Visible;
            }
        }

        private void MapControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        { 
            var pos = e.GetPosition(MapControl);
            var latLng = MapControl.FromLocalToLatLng((int)pos.X, (int)pos.Y);

            SelectedLatitude = latLng.Lat;
            SelectedLongitude = latLng.Lng;

            if (_tempMarker == null)
            {
                _tempMarker = new GMapMarker(latLng)
                {
                    ZIndex = 9999,               // high z so it sits above regular markers
                    Tag = TEMPORARY_MARKER_TAG,
                    Shape = new Ellipse
                    {
                        Width = 14,
                        Height = 14,
                        Fill = Brushes.Orange,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1
                    },
                    Offset = new Point(-7, -7)
                };
                MapControl.Markers.Add(_tempMarker);

            }

            else
            {
                _tempMarker.Position = latLng;
            }
        }

        // Event Tombol Close pada Panel
        private void CloseInfoPanel_Click(object sender, RoutedEventArgs e)
        {
            FishInfoPanel.Visibility = Visibility.Collapsed;
        }

        private void BtnCreateNewLog_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedLatitude == 0 && SelectedLongitude == 0)
            {
                CustomMessageBox.ShowWarning("Silakan pilih lokasi pada peta dengan klik kanan terlebih dahulu.");
                return;
            }
            NavigationService?.Navigate(new CreateLogPage(SelectedLatitude, SelectedLongitude));
        }

        //private void Records_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService?.Navigate(new RecordsPage());
        //}
    }
}