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
                Console.WriteLine("Try: netsh http add urlacl url=http://localhost:14859/ user=Everyone");
            }
        }

        private void AddCorsHeaders(HttpListenerResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
        }

        private async Task ListenLoop()
        {
            while (_listener.IsListening)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    var path = context.Request.Url.AbsolutePath;

                    if (path == "/scanners")
                    {
                        var scanners = ScannerManager.GetAllScanners();
                        var json = JsonConvert.SerializeObject(scanners);
                        byte[] buffer = Encoding.UTF8.GetBytes(json);

                        context.Response.ContentType = "application/json";
                        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                        context.Response.Close();
                    }
                    else if (context.Request.Url.AbsolutePath == "/scan")
                    {
                        Console.WriteLine("[+] Scan requested.");
                        AddCorsHeaders(context.Response);

                        string scannerName = context.Request.QueryString["scanner"];

                        if (string.IsNullOrEmpty(scannerName))
                        {
                            var all = WiaScannerHelper.GetAvailableScanners();
                            if (all.Count == 0)
                                all = TwainScannerHelper.GetAvailableTwainScanners();

                            if (all.Count == 0)
                                throw new Exception("No scanners found.");

                            scannerName = all.FirstOrDefault();
                        }

                        try
                        {
                            var images = ScannerManager.SafeScan(() =>
                                ScannerManager.Scan(scannerName)
                            );

                            var json = JsonConvert.SerializeObject(images);
                            byte[] buffer = Encoding.UTF8.GetBytes(json);

                            context.Response.ContentType = "application/json";
                            context.Response.ContentEncoding = Encoding.UTF8;
                            context.Response.ContentLength64 = buffer.Length;

                            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                        }
                        catch (Exception ex)
                        {
                            context.Response.StatusCode = 500;
                            byte[] error = Encoding.UTF8.GetBytes("Error: " + ex.Message);
                            await context.Response.OutputStream.WriteAsync(error, 0, error.Length);
                        }
                        finally
                        {
                            context.Response.Close();
                        }
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
