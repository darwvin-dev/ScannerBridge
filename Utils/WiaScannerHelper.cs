using System.Collections.Generic;
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
    }
}
