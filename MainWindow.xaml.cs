using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using ScannerBridge.Utils;

namespace ScannerBridge
{
    public partial class MainWindow : Window
    {
        private NotifyIcon _notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTrayIcon();
            this.Hide();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadScanners();

            var settings = SettingsStorage.Load();
            if (settings != null)
            {
                ScannerComboBox.SelectedItem = ScannerComboBox.Items
                    .OfType<ScannerInfo>()
                    .FirstOrDefault(i => i.Name == settings.SelectedScanner);

                DpiComboBox.SelectedItem = DpiComboBox.Items
                    .OfType<ComboBoxItem>()
                    .FirstOrDefault(i => i.Content.ToString() == settings.DPI.ToString());

                SourceComboBox.SelectedItem = SourceComboBox.Items
                    .OfType<ComboBoxItem>()
                    .FirstOrDefault(i => i.Content.ToString() == settings.Source);

                ColorModeComboBox.SelectedItem = ColorModeComboBox.Items
                    .OfType<ComboBoxItem>()
                    .FirstOrDefault(i => i.Content.ToString() == settings.ColorMode);
            }
        }

        private void LoadScanners()
        {
            ScannerComboBox.Items.Clear();

            var scanners = ScannerManager.GetAllScanners();
            foreach (var scanner in scanners)
                ScannerComboBox.Items.Add(scanner);

            if (ScannerComboBox.Items.Count > 0)
                ScannerComboBox.SelectedIndex = 0;
        }


        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            var scannerInfo = ScannerComboBox.SelectedItem as ScannerInfo;
            if (scannerInfo == null)
            {
                System.Windows.MessageBox.Show("Please select a scanner.");
                return;
            }

            var selectedScanner = ScannerComboBox.SelectedItem.ToString();
            var dpi = int.Parse(((ComboBoxItem)DpiComboBox.SelectedItem).Content.ToString());
            var source = ((ComboBoxItem)SourceComboBox.SelectedItem).Content.ToString();
            var colorMode = ((ComboBoxItem)ColorModeComboBox.SelectedItem).Content.ToString();

            var settings = new ScannerSettings
            {
                SelectedScanner = selectedScanner,
                DPI = dpi,
                ScannerType = scannerInfo.Type,
                Source = source,
                ColorMode = colorMode
            };

            SettingsStorage.Save(settings);

            ScanButton.IsEnabled = false;
            ScanStatus.Text = "🔄 Scanning in progress...";
            ScanStatus.Foreground = new SolidColorBrush(Colors.DarkOrange);

            Task.Run(() =>
            {
                try
                {
                    List<string> images = ScannerManager.SafeScan(() =>
                        ScannerManager.Scan(settings.SelectedScanner, settings)
                    );

                    Dispatcher.Invoke(() =>
                    {
                        if (images.Count > 0)
                        {
                            ScanStatus.Text = $"✅ Successfully scanned {images.Count} page(s).";
                            ScanStatus.Foreground = new SolidColorBrush(Colors.Green);
                        }
                        else
                        {
                            ScanStatus.Text = "⚠ No pages were scanned.";
                            ScanStatus.Foreground = new SolidColorBrush(Colors.Orange);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ScanStatus.Text = $"❌ Scan failed: {ex.Message}";
                        ScanStatus.Foreground = new SolidColorBrush(Colors.Red);
                    });
                }
                finally
                {
                    Dispatcher.Invoke(() => ScanButton.IsEnabled = true);
                }
            });
        }

        private void InitializeTrayIcon()
        {
            _notifyIcon = new NotifyIcon();

            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
            _notifyIcon.Icon = File.Exists(iconPath) ? new Icon(iconPath) : SystemIcons.Application;

            _notifyIcon.Visible = true;
            _notifyIcon.Text = "ScannerBridge is running in the background";

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Exit", null, (s, e) =>
            {
                _notifyIcon.Visible = false;
                System.Windows.Application.Current.Shutdown();
            });
            _notifyIcon.ContextMenuStrip = contextMenu;

            _notifyIcon.DoubleClick += (s, e) =>
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate();
            };
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }
    }
}
