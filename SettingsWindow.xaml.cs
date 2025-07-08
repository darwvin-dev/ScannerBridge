using ScannerBridge.Models;
using ScannerBridge.Utils;
using System.Windows;
using System.Windows.Controls;

namespace ScannerBridge
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            var settings = SettingsManager.Load();

            DpiTextBox.Text = settings.Dpi.ToString();
            AdfCheckBox.IsChecked = settings.UseAdf;

            foreach (ComboBoxItem item in ColorModeComboBox.Items)
            {
                if (item.Content.ToString() == settings.ColorMode)
                    item.IsSelected = true;
            }

            TwainRadio.IsChecked = settings.TwainPreferred;
            WiaRadio.IsChecked = !settings.TwainPreferred;

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

            ScannerComboBox.SelectedItem = settings.DefaultScanner;

        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            var newSettings = new ScanSettings
            {
                DefaultScanner = ScannerComboBox.SelectedItem?.ToString() ?? "",
                Dpi = int.TryParse(DpiTextBox.Text, out int dpi) ? dpi : 300,
                ColorMode = ((ComboBoxItem)ColorModeComboBox.SelectedItem).Content.ToString(),
                UseAdf = AdfCheckBox.IsChecked == true,
                TwainPreferred = TwainRadio.IsChecked == true
            };

            SettingsManager.Save(newSettings);

            MessageBox.Show("تنظیمات ذخیره شد", "OK");
            this.Close();
        }
    }
}
