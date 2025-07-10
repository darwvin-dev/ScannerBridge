using System.IO;
using NTwain;
using NTwain.Data;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Linq;

namespace ScannerBridge.Utils
{
    public static class ScannerManager
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private static readonly object scanLock = new();

        public static List<string> SafeScan(Func<List<string>> scanFunc)
        {
            lock (scanLock)
            {
                return scanFunc();
            }
        }

        public static List<string> Scan(string scannerName, ScannerSettings settings)
        {
            if (settings.ScannerType == "TWAIN")
                return TwainScannerHelper.Scan(scannerName, settings);
            else if (settings.ScannerType == "WIA")
                return WiaScannerHelper.Scan(scannerName, settings);
            else
                throw new NotSupportedException("Unsupported scanner type.");
        }


        public static List<ScannerInfo> GetAllScanners()
        {
            var list = new List<ScannerInfo>();

            try
            {
                var twainScanners = TwainScannerHelper.GetAvailableTwainScanners();
                list.AddRange(twainScanners.Select(name => new ScannerInfo
                {
                    Name = name,
                    Type = "TWAIN"
                }));
            }
            catch { }

            try
            {
                var wiaScanners = WiaScannerHelper.GetAvailableScanners();
                list.AddRange(wiaScanners.Select(name => new ScannerInfo
                {
                    Name = name,
                    Type = "WIA"
                }));
            }
            catch { }

            return list;
        }


    }
}
