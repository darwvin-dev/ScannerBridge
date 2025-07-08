using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using WIA;
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

        public static List<string> ScanWithWia(string scannerName, int dpi = 300, string colorMode = "Color", bool useAdf = true)
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

            SetItemIntProperty(item, 6147, dpi); // Horizontal Resolution
            SetItemIntProperty(item, 6148, dpi); // Vertical Resolution
            SetItemIntProperty(item, 6146, colorMode switch { "Grayscale" => 2, "BlackWhite" => 4, _ => 1 }); // Color Mode
            SetItemIntProperty(item, 3088, useAdf ? 1 : 0); // Document Handling (ADF)

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

        public static List<string> ScanWithTwain(string scannerName, bool useShowUI = true)
        {
            Console.WriteLine("HERE ISS THE SCAN WITH TWAIN WORKS");
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
                    Console.WriteLine("→ DataTransferred event fired");

                    try
                    {
                        if (e.NativeData == IntPtr.Zero)
                        {
                            Console.WriteLine("⚠ e.NativeData == IntPtr.Zero");
                        }
                        else
                        {
                            Console.WriteLine("✅ e.NativeData != 0 → Trying to read bitmap");

                            IntPtr hBitmap = e.NativeData;

                            if (IsBitmapPointerValid(hBitmap))
                            {
                                Console.WriteLine("✅ hBitmap is valid");

                                using (var bmp = System.Drawing.Image.FromHbitmap(hBitmap))
                                using (var ms = new MemoryStream())
                                {
                                    bmp.Save(ms, ImageFormat.Jpeg);
                                    images.Add(Convert.ToBase64String(ms.ToArray()));
                                    Console.WriteLine($"✅ image added, current count: {images.Count}");
                                }

                                DeleteObject(hBitmap);
                            }
                            else
                            {
                                Console.WriteLine("❌ hBitmap is invalid");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("❌ Exception in DataTransferred: " + ex.Message);
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

        private static bool IsBitmapPointerValid(IntPtr hBitmap)
        {
            try
            {
                using var bmp = System.Drawing.Image.FromHbitmap(hBitmap);
                return bmp.Width > 0 && bmp.Height > 0;
            }
            catch
            {
                return false;
            }
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
