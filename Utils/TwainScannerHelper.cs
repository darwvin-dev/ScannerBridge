using NTwain;
using NTwain.Data;
using System.Reflection;

namespace ScannerBridge.Utils
{
    public static class TwainScannerHelper
    {
        public static List<string> GetAvailableTwainScanners()
        {
            var scanners = new List<string>();

            var appId = TWIdentity.CreateFromAssembly(DataGroups.Image, Assembly.GetExecutingAssembly());

            var session = new TwainSession(appId);
            session.Open();

            if (session.State >= 3)
            {
                foreach (var src in session)
                {
                    scanners.Add(src.Name);
                }
            }

            session.Close();
            return scanners;
        }
    }
}
