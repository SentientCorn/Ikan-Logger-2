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

namespace IkanLogger2.Views
{
    public partial class DashboardPage : Page
    {
        private readonly MapService _mapController;

        public DashboardPage()
        {
            InitializeComponent();

            _mapController = new MapService();

            // Set default view ke Laut Selatan Yogyakarta
            _mapController.Configure(MapControl, -8.0245, 110.3290, 13);

            Loaded += async (s, e) => await LoadPins();
        }

        private async Task LoadPins()
        {
            try
            {
                var locations = await FishService.GetFishLocationsAsync();

                if (locations == null || locations.Count == 0)
                {
                    MessageBox.Show("Tidak ada data lokasi ikan");
                    return;
                }

                MapControl.Markers.Clear();

                foreach (var loc in locations)
                {
                    // 1. SIAPKAN DATA TOOLTIP (Nama Ikan & Harga)
                    string tooltipContent = $"Lokasi #{loc.IdLocation}\n";

                    if (loc.Fishes != null && loc.Fishes.Count > 0)
                    {
                        foreach (var fish in loc.Fishes)
                        {
                            // Format: Nama Ikan (Rp Harga)
                            tooltipContent += $"- {fish.FishName} : Rp {fish.MarketPrice:N0}\n";
                        }
                    }
                    else
                    {
                        tooltipContent += "Tidak ada data ikan saat ini.";
                    }

                    PointLatLng center = new PointLatLng(loc.Latitude, loc.Longitude);

                    // 2. BUAT RADIUS 3KM (Menggunakan Polygon agar akurat saat zoom)
                    var radiusMarker = CreateCirclePolygon(center, 3.0); // 3.0 KM
                    MapControl.Markers.Add(radiusMarker);

                    // 3. BUAT PIN POINT (Dengan Tooltip)
                    var pinMarker = CreatePinMarker(center, tooltipContent);
                    MapControl.Markers.Add(pinMarker);
                }

                if (locations.Count > 0)
                {
                    var firstLoc = locations[0];
                    MapControl.Position = new PointLatLng(firstLoc.Latitude, firstLoc.Longitude);

                    // Ubah 12 menjadi 15 atau 16 agar tampilan lebih dekat ke titik
                    MapControl.Zoom = 12;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading pins: {ex.Message}");
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
        private GMapMarker CreatePinMarker(PointLatLng position, string tooltipText)
        {
            var marker = new GMapMarker(position)
            {
                ZIndex = 100 // Pin selalu di paling atas
            };

            // Visual Pin
            var pinVisual = new Grid { Width = 30, Height = 40 };

            // Pointer (Segitiga bawah)
            var pinPointer = new Polygon
            {
                Fill = Brushes.Red,
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Points = new PointCollection { new Point(15, 26), new Point(8, 38), new Point(22, 38) }
            };

            // Badan Pin (Lingkaran atas)
            var pinCircle = new Ellipse
            {
                Width = 26,
                Height = 26,
                Fill = Brushes.Red,
                Stroke = Brushes.White,
                StrokeThickness = 3,
                Margin = new Thickness(2, 0, 2, 14)
            };

            // Dot putih di tengah
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

            // SET TOOLTIP DISINI
            // ToolTip standar WPF mendukung string multiline
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
            marker.Offset = new Point(-15, -40); // Offset agar ujung pin pas di koordinat

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
    }
}