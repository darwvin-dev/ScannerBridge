using System.IO;
using NTwain;
using NTwain.Data;

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

        public static List<string> Scan(string scannerName)
        {
            try
            {
                return TwainScannerHelper.Scan(scannerName);
            }
            catch
            {
                return WiaScannerHelper.Scan(scannerName);
            }
        }
        
        public static List<string> GetAllScanners()
        {
            var scanners = new List<string>();

            var wia = WiaScannerHelper.GetAvailableScanners();
            if (wia.Count > 0)
                scanners.AddRange(wia);
            else
                scanners.AddRange(TwainScannerHelper.GetAvailableTwainScanners());

            return scanners;
        }

    }
}
