using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NTwain;
using NTwain.Data;
using System.Reflection;
using System.Linq;

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

        public static List<string> Scan(string scannerName, ScannerSettings settings)
        {
            var useShowUI = true;
            var images = new List<string>();
            var waitHandle = new AutoResetEvent(false);

            var appId = TWIdentity.CreateFromAssembly(DataGroups.Image, typeof(ScannerManager).Assembly);
            var session = new TwainSession(appId);

            try
            {
                session.Open();

                if (session.State < 3)
                    throw new Exception("TWAIN session failed to open.");

                var source = session.GetSources()?.FirstOrDefault(x => x.Name == scannerName);
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

                // if (source.Capabilities.ICapXResolution.CanSet)
                //     source.Capabilities.ICapXResolution.SetValue(settings.DPI);

                // if (source.Capabilities.ICapYResolution.CanSet)
                //     source.Capabilities.ICapYResolution.SetValue(settings.DPI);

                // var pixelType = settings.ColorMode.ToLower() switch
                // {
                //     "grayscale" => PixelType.Gray,
                //     "blackwhite" => PixelType.BlackWhite,
                //     _ => PixelType.RGB
                // };

                // if (source.Capabilities.ICapPixelType.CanSet &&
                //     source.Capabilities.ICapPixelType.GetValues().Contains(pixelType))
                // {
                //     source.Capabilities.ICapPixelType.SetValue(pixelType);
                // }

                // bool useAdf = settings.Source.ToLower() == "adf";

                // source.Capabilities.CapFeederEnabled.SetValue(useAdf ? BoolType.True : BoolType.False);
                // if (useAdf)
                //     source.Capabilities.CapAutoFeed.SetValue(BoolType.True);

                var mode = useShowUI ? SourceEnableMode.ShowUI : SourceEnableMode.NoUI;
                source.Enable(mode, false, IntPtr.Zero);

                if (!waitHandle.WaitOne(TimeSpan.FromSeconds(30)))
                    throw new TimeoutException("TWAIN scan timed out.");

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
