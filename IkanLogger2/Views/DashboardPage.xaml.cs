using GMap.NET;
using GMap.NET.WindowsPresentation;
using IkanLogger2.Services;
using IkanLogger.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;

namespace IkanLogger2.Views
{
    public partial class DashboardPage : Page
    {
        private readonly MapService _mapController;
        private List<FishLocation> _allLocations = new List<FishLocation>();
        public DashboardPage()
        {
            InitializeComponent();

            _mapController = new MapService();

            // Set default view ke Laut Selatan Yogyakarta
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
                    MessageBox.Show("Tidak ada data lokasi ikan");
                    return;
                }

                // --- LOGIKA FILTER ---
                // 1. Ambil nama-nama ikan unik dari semua lokasi
                var fishNames = _allLocations
                    .SelectMany(loc => loc.Fishes) // Ratakan list ikan
                    .Select(f => f.FishName)       // Ambil namanya saja
                    .Distinct()                    // Hapus duplikat
                    .OrderBy(n => n)               // Urutkan abjad
                    .ToList();

                // 2. Tambahkan opsi "Semua Ikan" di paling atas
                fishNames.Insert(0, "Semua Ikan");

                // 3. Masukkan ke ComboBox
                FishFilterComboBox.ItemsSource = fishNames;
                FishFilterComboBox.SelectedIndex = 0; // Default pilih "Semua Ikan"

                // Render awal (tampilkan semua)
                RenderMarkers(_allLocations);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        // Method 2: Event saat Dropdown diganti
        private void FishFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FishFilterComboBox.SelectedItem is string selectedFish)
            {
                List<FishLocation> filteredList;

                if (selectedFish == "Semua Ikan")
                {
                    // Jika pilih semua, pakai data master
                    filteredList = _allLocations;
                }
                else
                {
                    // Filter lokasi yang memiliki ikan dengan nama tersebut
                    filteredList = _allLocations
                        .Where(loc => loc.Fishes.Any(f => f.FishName == selectedFish))
                        .ToList();
                }

                // Gambar ulang marker sesuai hasil filter
                RenderMarkers(filteredList);
            }
        }

        // Method 3: Menggambar Marker (Dipisah agar bisa dipanggil ulang)
        private void RenderMarkers(List<FishLocation> locationsToRender)
        {
            MapControl.Markers.Clear();
            // Sembunyikan panel saat render ulang (misal ganti filter)
            FishInfoPanel.Visibility = Visibility.Collapsed;

            foreach (var loc in locationsToRender)
            {
                // Tooltip tetap dibuat untuk hover cepat
                string tooltipContent = $"Lokasi #{loc.IdLocation}\nKlik untuk detail.";

                PointLatLng center = new PointLatLng(loc.Latitude, loc.Longitude);

                var radiusMarker = CreateCirclePolygon(center, 3.0);
                MapControl.Markers.Add(radiusMarker);

                // Pass object 'loc' ke method pembuatan pin
                var pinMarker = CreatePinMarker(center, tooltipContent, loc);
                MapControl.Markers.Add(pinMarker);
            }

            if (locationsToRender.Count > 0)
            {
                var firstLoc = locationsToRender[0];
                MapControl.Position = new PointLatLng(firstLoc.Latitude, firstLoc.Longitude);
                MapControl.Zoom = 15;
            }
        }

        // Method Baru: Membuat Lingkaran Geografis (Polygon)
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

            // --- PERBAIKAN DI SINI ---
            // Cek apakah polygon.Shape null. Jika ya, kita inisialisasi manual.
            Path shape = polygon.Shape as Path;
            if (shape == null)
            {
                shape = new Path();
                polygon.Shape = shape;
            }
            // -------------------------

            // Sekarang aman untuk mengakses properti shape
            shape.Fill = new SolidColorBrush(Color.FromArgb(40, 0, 120, 215));
            shape.Stroke = new SolidColorBrush(Color.FromArgb(150, 0, 120, 215));
            shape.StrokeThickness = 1.5;

            polygon.ZIndex = 1;

            return polygon;
        }
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
                        listIkan += $"• {fish.FishName}\n   Rp {fish.MarketPrice:N0}\n";
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

        // Event Tombol Close pada Panel
        private void CloseInfoPanel_Click(object sender, RoutedEventArgs e)
        {
            FishInfoPanel.Visibility = Visibility.Collapsed;
        }
    }
}