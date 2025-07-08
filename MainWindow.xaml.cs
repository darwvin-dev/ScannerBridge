using System.Windows;
using ScannerBridge.Utils;
using ScannerBridge.Models;
using System.Windows.Media;

namespace ScannerBridge
{
    public partial class MainWindow : Window
    {
        private ScanSettings _settings;

        public MainWindow()
        {
            InitializeComponent();

            LoadScanners();
        }

        private void LoadScanners()
        {
            _settings = SettingsManager.Load();

            ScannerComboBox.Items.Clear();

            var scanners = WiaScannerHelper.GetAvailableScanners();
            if (scanners.Count == 0)
            {
                scanners = TwainScannerHelper.GetAvailableTwainScanners();
            }

            foreach (var scanner in scanners)
            {
                ScannerComboBox.Items.Add(scanner);
            }

            ScannerComboBox.SelectedItem = _settings.DefaultScanner;
        }

        private void ScannerComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ScannerComboBox.SelectedItem != null)
            {
                _settings.DefaultScanner = ScannerComboBox.SelectedItem.ToString();
                SettingsManager.Save(_settings);
            }
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            var sw = new SettingsWindow();
            sw.ShowDialog();
            LoadScanners();
        }

        private void ManualSave_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.Save(_settings);
            MessageBox.Show("تنظیمات ذخیره شد", "ذخیره");
        }

        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            ScanButton.IsEnabled = false;
            ScanStatus.Text = "Scanning...";
            ScanStatus.Foreground = new SolidColorBrush(Colors.DarkOrange);

            Task.Run(() =>
            {
                try
                {
                    var settings = SettingsManager.Load();
                    List<string> images = new();
                    Console.WriteLine(settings.TwainPreferred ? "Using TWAIN" : "Using WIA");

                    if (settings.TwainPreferred)
                    {
                        try
                        {
                            images = ScannerManager.ScanWithTwain(settings.DefaultScanner);
                        }
                        catch
                        {
                            images = ScannerManager.ScanWithWia(settings.DefaultScanner, settings.Dpi, settings.ColorMode, settings.UseAdf);
                        }
                    }
                    else
                    {
                        try
                        {
                            images = ScannerManager.ScanWithWia(settings.DefaultScanner, settings.Dpi, settings.ColorMode, settings.UseAdf);
                        }
                        catch
                        {
                            images = ScannerManager.ScanWithTwain(settings.DefaultScanner);
                        }
                    }

                    Dispatcher.Invoke(() =>
                    {
                        ScanStatus.Text = $"✅ Scanned {images.Count} image(s) successfully.";
                        ScanStatus.Foreground = new SolidColorBrush(Colors.Green);
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
