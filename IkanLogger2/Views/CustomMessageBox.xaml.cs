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
using System.Windows.Shapes;

namespace IkanLogger2.Views
{
    public partial class CustomMessageBox : Window
    {
        public enum MessageBoxButton
        {
            OK,
            OKCancel,
            YesNo,
            YesNoCancel
        }

        public enum MessageBoxResult
        {
            None,
            OK,
            Cancel,
            Yes,
            No
        }

        public MessageBoxResult Result { get; private set; }

        private CustomMessageBox(string message, string title, MessageBoxButton button, Brush headerColor = null, Brush buttonColor = null)
        {
            InitializeComponent();

            TxtMessage.Text = message;
            TxtTitle.Text = title;

            // Set custom colors if provided
            if (headerColor != null)
            {
                HeaderBorder.Background = headerColor;
                MainBorder.BorderBrush = headerColor;
                TxtMessage.Foreground = headerColor;
            }

            if (buttonColor != null)
            {
                ApplyButtonColor(buttonColor);
            }

            ConfigureButtons(button);
        }

        private void ApplyButtonColor(Brush color)
        {
            // Apply to OK button
            var okStyle = new Style(typeof(Button));
            okStyle.Setters.Add(new Setter(Button.BackgroundProperty, color));
            okStyle.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
            okStyle.Setters.Add(new Setter(Button.TemplateProperty, CreateButtonTemplate()));

            var okTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            okTrigger.Setters.Add(new Setter(Button.BackgroundProperty, CreateLighterBrush(color)));
            okTrigger.Setters.Add(new Setter(Button.ForegroundProperty, color));
            okStyle.Triggers.Add(okTrigger);

            BtnOK.Style = okStyle;
            BtnYes.Style = okStyle;

            // Apply to Cancel/No buttons
            var cancelStyle = new Style(typeof(Button));
            cancelStyle.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.White));
            cancelStyle.Setters.Add(new Setter(Button.ForegroundProperty, color));
            cancelStyle.Setters.Add(new Setter(Button.BorderBrushProperty, color));
            cancelStyle.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(2)));
            cancelStyle.Setters.Add(new Setter(Button.TemplateProperty, CreateButtonTemplate(true)));

            var cancelTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            cancelTrigger.Setters.Add(new Setter(Button.BackgroundProperty, CreateLighterBrush(color)));
            cancelStyle.Triggers.Add(cancelTrigger);

            BtnCancel.Style = cancelStyle;
            BtnNo.Style = cancelStyle;
        }

        private ControlTemplate CreateButtonTemplate(bool hasBorder = false)
        {
            var template = new ControlTemplate(typeof(Button));
            var factory = new FrameworkElementFactory(typeof(Border));
            factory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));

            if (hasBorder)
            {
                factory.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
                factory.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));
            }
            else
            {
                factory.SetValue(Border.BorderThicknessProperty, new Thickness(0));
            }

            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            factory.AppendChild(contentPresenter);

            template.VisualTree = factory;
            return template;
        }

        private Brush CreateLighterBrush(Brush brush)
        {
            if (brush is SolidColorBrush solidBrush)
            {
                var color = solidBrush.Color;
                var lighterColor = Color.FromArgb(
                    color.A,
                    (byte)Math.Min(255, color.R + 100),
                    (byte)Math.Min(255, color.G + 100),
                    (byte)Math.Min(255, color.B + 100)
                );
                return new SolidColorBrush(lighterColor);
            }
            return brush;
        }

        private void ConfigureButtons(MessageBoxButton button)
        {
            // Hide all buttons first
            BtnOK.Visibility = Visibility.Collapsed;
            BtnCancel.Visibility = Visibility.Collapsed;
            BtnYes.Visibility = Visibility.Collapsed;
            BtnNo.Visibility = Visibility.Collapsed;

            // Show buttons based on type
            switch (button)
            {
                case MessageBoxButton.OK:
                    BtnOK.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.OKCancel:
                    BtnOK.Visibility = Visibility.Visible;
                    BtnCancel.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNo:
                    BtnYes.Visibility = Visibility.Visible;
                    BtnNo.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNoCancel:
                    BtnYes.Visibility = Visibility.Visible;
                    BtnNo.Visibility = Visibility.Visible;
                    BtnCancel.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            this.Close();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            this.Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            this.Close();
        }

        // Static methods untuk memanggil MessageBox
        public static MessageBoxResult Show(string message)
        {
            return Show(message, "Information", MessageBoxButton.OK);
        }

        public static MessageBoxResult Show(string message, string title)
        {
            return Show(message, title, MessageBoxButton.OK);
        }

        public static MessageBoxResult Show(string message, string title, MessageBoxButton button)
        {
            return Show(message, title, button, null, null);
        }

        public static MessageBoxResult Show(string message, string title, MessageBoxButton button, Brush headerColor, Brush buttonColor)
        {
            var messageBox = new CustomMessageBox(message, title, button, headerColor, buttonColor);
            messageBox.ShowDialog();
            return messageBox.Result;
        }

        // Helper methods untuk warna preset
        public static MessageBoxResult ShowSuccess(string message, string title = "Success")
        {
            var greenColor = new SolidColorBrush(Color.FromRgb(34, 139, 34));
            return Show(message, title, MessageBoxButton.OK, greenColor, greenColor);
        }

        public static MessageBoxResult ShowError(string message, string title = "Error")
        {
            var redColor = new SolidColorBrush(Color.FromRgb(220, 53, 69));
            return Show(message, title, MessageBoxButton.OK, redColor, redColor);
        }

        public static MessageBoxResult ShowWarning(string message, string title = "Warning")
        {
            var orangeColor = new SolidColorBrush(Color.FromRgb(255, 152, 0));
            return Show(message, title, MessageBoxButton.OK, orangeColor, orangeColor);
        }

        public static MessageBoxResult ShowInfo(string message, string title = "Information")
        {
            var blueColor = new SolidColorBrush(Color.FromRgb(31, 79, 110));
            return Show(message, title, MessageBoxButton.OK, blueColor, blueColor);
        }
    }
}