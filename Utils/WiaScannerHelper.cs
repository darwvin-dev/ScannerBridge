using System.Collections.Generic;
using System.IO;
using System;
using WIA;

namespace ScannerBridge.Utils
{
    public static class WiaScannerHelper
    {
        public static List<string> GetAvailableScanners()
        {
            List<string> scanners = new List<string>();

            var deviceManager = new DeviceManager();

            for (int i = 1; i <= deviceManager.DeviceInfos.Count; i++)
            {
                var info = deviceManager.DeviceInfos[i];

                if (info.Type == WiaDeviceType.ScannerDeviceType)
                {
                    scanners.Add(info.Properties["Name"].get_Value().ToString());
                }
            }

            return scanners;
        }

        public static List<string> Scan(string scannerName, ScannerSettings settings)
        {
            var images = new List<string>();
            var deviceManager = new DeviceManager();
            DeviceInfo selectedScanner = null;

            for (int i = 1; i <= deviceManager.DeviceInfos.Count; i++)
            {
                var info = deviceManager.DeviceInfos[i];
                if (info.Type == WiaDeviceType.ScannerDeviceType &&
                    info.Properties["Name"].get_Value().ToString() == scannerName)
                {
                    selectedScanner = info;
                    break;
                }
            }

            if (selectedScanner == null)
                throw new Exception("Scanner not found (WIA).");

            var device = selectedScanner.Connect();
            var item = device.Items[1];

            SetItemIntProperty(item, 6147, settings.DPI); // Horizontal Resolution
            SetItemIntProperty(item, 6148, settings.DPI); // Vertical Resolution
            SetItemIntProperty(item, 6146, settings.ColorMode.ToLower() switch { "Grayscale" => 2, "BlackWhite" => 4, _ => 1 }); // Color Mode
            SetItemIntProperty(item, 3088, settings.Source.ToLower() == "adf" ? 1 : 0); // Document Handling (ADF)

            Console.WriteLine("Starting WIA transfer...");

            var imgFile = (ImageFile)item.Transfer(FormatID.wiaFormatJPEG);

            if (imgFile == null || imgFile.FileData == null || imgFile.FileData.get_BinaryData() == null)
                throw new Exception("WIA returned empty image data.");

            var tempPath = Path.GetTempFileName().Replace(".tmp", ".jpg");
            imgFile.SaveFile(tempPath);

            var bytes = File.ReadAllBytes(tempPath);
            string base64 = Convert.ToBase64String(bytes);
            images.Add(base64);

            File.Delete(tempPath);

            return images;
        }
        
        private static void SetItemIntProperty(Item item, int propId, int value)
        {
            try
            {
                foreach (Property prop in item.Properties)
                {
                    if (prop.PropertyID == propId)
                    {
                        prop.set_Value(value);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Failed to set property {propId}: {ex.Message}");
            }
        }
    }
}
