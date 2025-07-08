using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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

        public static List<string> Scan(string scannerName, bool useShowUI = true)
        {
            var images = new List<string>();
            var waitHandle = new AutoResetEvent(false);

            var appId = TWIdentity.CreateFromAssembly(DataGroups.Image, typeof(ScannerManager).Assembly);
            var session = new TwainSession(appId);

            try
            {
                session.Open();
                if (session.State < 3)
                    throw new Exception("TWAIN session failed to open.");

                var source = session.FirstOrDefault(x => x.Name == scannerName);
                if (source == null)
                    throw new Exception("TWAIN scanner not found.");

                session.DataTransferred += (s, e) =>
                {
                    try
                    {
                        using (Stream stream = e.GetNativeImageStream())
                        {
                            if (stream == null)
                            {
                                Console.WriteLine("❌ Stream is null");
                                return;
                            }

                            using (var ms = new MemoryStream())
                            {
                                stream.CopyTo(ms);
                                images.Add(Convert.ToBase64String(ms.ToArray()));
                                Console.WriteLine($"✅ Image stream added, total: {images.Count}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("❌ Error in stream transfer: " + ex.Message);
                    }
                    finally
                    {
                        waitHandle.Set();
                    }
                };

                session.TransferError += (s, e) =>
                {
                    Console.WriteLine("❌ Transfer error during TWAIN scanning.");
                    waitHandle.Set();
                };

                source.Open();

                var mode = useShowUI ? SourceEnableMode.ShowUI : SourceEnableMode.NoUI;
                source.Enable(mode, false, IntPtr.Zero);

                if (!waitHandle.WaitOne(TimeSpan.FromSeconds(30)))
                    throw new TimeoutException("TWAIN scan timed out.");

                source.Close();
            }
            finally
            {
                session.Close();
            }

            return images;
        }
    }
}
