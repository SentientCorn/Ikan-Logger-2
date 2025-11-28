using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System.Windows.Controls;

namespace IkanLogger2.Services
{
    public class MapService
    {
        public void Configure(GMapControl mapControl, double latitude, double longitude)
        {
            // Required GMap settings
            GMapProvider.UserAgent = "FishingMapApp/1.0";
            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            mapControl.MapProvider = BingMapProvider.Instance;
            mapControl.Position = new PointLatLng(latitude, longitude);

            mapControl.Zoom = 15;
            mapControl.MinZoom = 10;
            mapControl.MaxZoom = 20;
            mapControl.ShowCenter = false;
        }
    }
}
