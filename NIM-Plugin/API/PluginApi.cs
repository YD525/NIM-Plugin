using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading;

namespace NIM_Plugin.Core
{
    public class NIM_Api
    {
        /// <summary>
        /// Displays a specified WPF window on an independent UI thread.
        /// Plugins can call this method to display their own windows without blocking the host application's main UI thread.
        /// </summary>
        public static void ShowWindow(Window Window)
        {
            if (Window == null) return;

            Thread UIThread = new Thread(() =>
            {
                Window.Show();
                Dispatcher.Run();
            });

            UIThread.SetApartmentState(ApartmentState.STA);
            UIThread.IsBackground = true;
            UIThread.Start();
        }

        /// <summary>
        /// Displays a customizable modal prompt with multiple selectable buttons.
        /// Plugins can use this method to display messages and request user input through button selections.
        /// The calling thread will remain blocked until the user selects a button or closes the dialog.
        /// </summary>
        /// <param name="Title">The title displayed at the top of the dialog.</param>
        /// <param name="Message">The message displayed in the dialog.</param>
        /// <param name="Buttons">The selectable button labels.</param>
        /// <returns>The text of the selected button, or null if the dialog is closed without a selection.</returns>
        public static string ShowPrompt(string Title, string Message, params string[] Buttons)
        {
            if (Buttons == null || Buttons.Length == 0) return null;

            Window Dialog = new Window
            {
                Title = Title,
                Width = 440,
                SizeToContent = SizeToContent.Height,
                MinHeight = 180,
                MaxHeight = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
            };

            Border Root = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(32, 32, 32)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(65, 65, 65)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(24)
            };

            StackPanel Layout = new StackPanel();

            // Header
            TextBlock Header = new TextBlock
            {
                Text = Title,
                FontSize = 19,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(247, 241, 186)),
                Margin = new Thickness(0, 0, 0, 18)
            };

            Layout.Children.Add(Header);

            // Message
            TextBlock Content = new TextBlock
            {
                Text = Message,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(210, 210, 210)),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 23,
                Margin = new Thickness(0, 0, 0, 28)
            };

            Layout.Children.Add(Content);

            // Buttons
            WrapPanel ButtonPanel = new WrapPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right
            };

            string Result = null;

            foreach (string ButtonName in Buttons)
            {
                Button Button = new Button
                {
                    Content = ButtonName,
                    MinWidth = 85,
                    Height = 34,
                    Padding = new Thickness(14, 0, 14, 0),
                    Margin = new Thickness(8, 0, 0, 0),
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(32, 32, 32)),
                    Background = new SolidColorBrush(Color.FromRgb(247, 241, 186)),
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                Button.Template = CreateButtonTemplate();

                Button.Click += (S, E) =>
                {
                    Result = ButtonName;
                    Dialog.DialogResult = true;
                };

                ButtonPanel.Children.Add(Button);
            }

            Layout.Children.Add(ButtonPanel);

            Root.Child = Layout;
            Dialog.Content = Root;

            Dialog.ShowDialog();

            return Result;
        }
        private static ControlTemplate CreateButtonTemplate()
        {
            FrameworkElementFactory BorderFactory = new FrameworkElementFactory(typeof(Border));

            BorderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
            BorderFactory.SetValue(Border.BackgroundProperty,
                new TemplateBindingExtension(Button.BackgroundProperty));

            FrameworkElementFactory Content = new FrameworkElementFactory(typeof(ContentPresenter));
            Content.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            Content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            BorderFactory.AppendChild(Content);

            return new ControlTemplate(typeof(Button))
            {
                VisualTree = BorderFactory
            };
        }
    }
}
