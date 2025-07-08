using System;
using System.Windows;

namespace ScannerBridge
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var server = new LocalHttpServer();
            server.Start();
        }
    }
}
