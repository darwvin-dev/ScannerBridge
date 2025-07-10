using System.IO;
using Newtonsoft.Json;

namespace ScannerBridge.Utils
{
    public static class SettingsStorage
    {
        private static string settingsFile = "scanner_settings.json";

        public static void Save(ScannerSettings settings)
        {
            File.WriteAllText(settingsFile, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        public static ScannerSettings Load()
        {
            if (!File.Exists(settingsFile))
                return new ScannerSettings();

            return JsonConvert.DeserializeObject<ScannerSettings>(File.ReadAllText(settingsFile));
        }
    }
}
