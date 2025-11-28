using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace IkanLogger2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var box = sender as PasswordBox;
            if (box == null) return;

            var template = box.Template;
            var watermark = template.FindName("Watermark", box) as UIElement;

            if (watermark == null) return;

            watermark.Visibility = string.IsNullOrEmpty(box.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }

}
