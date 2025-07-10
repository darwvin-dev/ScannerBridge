using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ScannerBridge.Utils;
using System.Windows.Forms;
using System.Drawing;
using System;
using System.IO;

namespace ScannerBridge
{
    public partial class MainWindow : Window
    {
        private NotifyIcon _notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTrayIcon();
            LoadScanners();
            this.Hide();
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
            {
                this.Hide();
            }
            base.OnStateChanged(e);
        }

        private void LoadScanners()
        {
            ScannerComboBox.Items.Clear();

            var scanners = WiaScannerHelper.GetAvailableScanners();
            if (scanners.Count == 0)
                scanners = TwainScannerHelper.GetAvailableTwainScanners();

            foreach (var scanner in scanners)
                ScannerComboBox.Items.Add(scanner);

            if (ScannerComboBox.Items.Count > 0)
                ScannerComboBox.SelectedIndex = 0;
        }

        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScannerComboBox.SelectedItem == null)
            {
                System.Windows.MessageBox.Show("Please select a scanner.");
                return;
            }

            string selectedScanner = ScannerComboBox.SelectedItem.ToString();

            ScanButton.IsEnabled = false;
            ScanStatus.Text = "🔄 Scanning in progress...";
            ScanStatus.Foreground = new SolidColorBrush(Colors.DarkOrange);

            Task.Run(() =>
            {
                try
                {
                    List<string> images = ScannerManager.SafeScan(() =>
                        ScannerManager.Scan(selectedScanner)
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
    }
}
