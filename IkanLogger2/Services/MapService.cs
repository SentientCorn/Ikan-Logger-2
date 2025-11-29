using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System.Windows.Controls;

namespace IkanLogger2.Services
{
    public class MapService
    {
        public void Configure(GMapControl mapControl, double latitude, double longitude, int zoomLevel = 12)
        {
            // Required GMap settings
            GMapProvider.UserAgent = "FishingMapApp/1.0";
            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            // Gunakan OpenStreetMap sebagai alternatif yang tidak perlu API key
            mapControl.MapProvider = GMapProviders.OpenStreetMap;

            // Set posisi peta
            mapControl.Position = new PointLatLng(latitude, longitude);

            // Atur zoom level
            mapControl.Zoom = zoomLevel;
            mapControl.MinZoom = 2;
            mapControl.MaxZoom = 20;

            // Pengaturan tambahan
            mapControl.ShowCenter = false;
            mapControl.DragButton = System.Windows.Input.MouseButton.Left;
            mapControl.CanDragMap = true;
            mapControl.MouseWheelZoomEnabled = true;

            mapControl.IgnoreMarkerOnMouseWheel = true;
        }

        public void AddMarker(GMapControl mapControl, double lat, double lng) { }


    }
}