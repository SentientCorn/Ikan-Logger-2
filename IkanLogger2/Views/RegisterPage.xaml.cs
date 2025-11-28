using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IkanLogger2.Views
{
    /// <summary>
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var main = (MainWindow)Application.Current.MainWindow;
            main.MainFrame.Navigate(new LoginPage());
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Validation & Registration Logic

            MessageBox.Show("Account created successfully!");

            var main = (MainWindow)Application.Current.MainWindow;
            main.MainFrame.Navigate(new LoginPage());
        }
    }

}
