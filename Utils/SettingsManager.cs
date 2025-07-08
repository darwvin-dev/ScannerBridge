using System;
using System.IO;
using Newtonsoft.Json;
using ScannerBridge.Models;

namespace ScannerBridge.Utils
{
    public static class SettingsManager
    {
        private static string settingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        public static ScanSettings Load()
        {
            try
            {
                if (!File.Exists(settingsFile))
                    Save(new ScanSettings()); 

                var json = File.ReadAllText(settingsFile);
                return JsonConvert.DeserializeObject<ScanSettings>(json);
            }
            catch
            {
                return new ScanSettings(); 
            }
        }

        public static void Save(ScanSettings settings)
        {
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(settingsFile, json);
        }
    }
}
