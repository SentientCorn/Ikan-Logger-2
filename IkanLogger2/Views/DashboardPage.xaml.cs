using System.Windows.Controls;
using GMap.NET.WindowsPresentation;
using IkanLogger2.Services;


namespace IkanLogger2.Views
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        private readonly MapService _mapController;

        public DashboardPage()
        {
            InitializeComponent();

            _mapController = new MapService();

            // Example: Jakarta
            _mapController.Configure(MapControl, -6.200000, 106.816666);

            MapControl.Zoom = 15;
        }
    }
}
