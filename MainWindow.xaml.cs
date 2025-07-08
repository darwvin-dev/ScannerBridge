using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ScannerBridge.Utils;

namespace ScannerBridge
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadScanners();
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
                MessageBox.Show("Please select a scanner first.");
                return;
            }

            string selectedScanner = ScannerComboBox.SelectedItem.ToString();

            ScanButton.IsEnabled = false;
            ScanStatus.Text = "🔄 Scanning...";
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
                            ScanStatus.Text = $"✅ Scanned {images.Count} image(s) successfully.";
                            ScanStatus.Foreground = new SolidColorBrush(Colors.Green);
                        }
                        else
                        {
                            ScanStatus.Text = "⚠ No pages scanned.";
                            ScanStatus.Foreground = new SolidColorBrush(Colors.Orange);
                        }
                    });
                }
                catch (System.Exception ex)
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
