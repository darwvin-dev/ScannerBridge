using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScannerBridge.Utils;

namespace ScannerBridge
{
    public class LocalHttpServer
    {
        private HttpListener _listener;

        public void Start()
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://localhost:14859/");
                _listener.Start();
                Console.WriteLine("LocalHttpServer started on port 14859.");
                Task.Run(() => ListenLoop());
            }
            catch (HttpListenerException ex)
            {
                Console.WriteLine("HttpListener error: " + ex.Message);
                // ⚠ در ویندوز باید آدرس مجاز با netsh ثبت شود:
                // netsh http add urlacl url=http://localhost:14859/ user=Everyone
            }
        }

        private async Task ListenLoop()
        {
            while (_listener.IsListening)
            {
                try
                {
                    var context = await _listener.GetContextAsync();

                    if (context.Request.Url.AbsolutePath == "/scan")
                    {
                        Console.WriteLine("[+] Scan requested.");

                        var settings = SettingsManager.Load();

                        List<string> images = ScannerManager.SafeScan(() =>
                        {
                            if (settings.TwainPreferred)
                                return ScannerManager.ScanWithTwain(settings.DefaultScanner);
                            else
                                return ScannerManager.ScanWithWia(settings.DefaultScanner, settings.Dpi, settings.ColorMode, settings.UseAdf);
                        });

                        var json = JsonConvert.SerializeObject(images);
                        byte[] buffer = Encoding.UTF8.GetBytes(json);

                        context.Response.ContentType = "application/json";
                        context.Response.ContentEncoding = Encoding.UTF8;
                        context.Response.ContentLength64 = buffer.Length;

                        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                        context.Response.Close();
                    }
                    else
                    {
                        context.Response.StatusCode = 404;
                        context.Response.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in HTTP server: " + ex.Message);
                }
            }
        }
    }
}
