using System;
using System.IO;
using System.Windows;
using IWshRuntimeLibrary;

namespace ScannerBridge
{
    public partial class App : Application
    {
        private LocalHttpServer _server;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainWindow = new MainWindow();
            mainWindow.Hide(); 

            AddToStartup();
            StartHttpServer();
        }

        private void StartHttpServer()
        {
            _server = new LocalHttpServer();
            _server.Start();
        }

        private void AddToStartup()
        {
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string shortcutPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                "ScannerBridge.lnk");

            var shell = new IWshRuntimeLibrary.WshShell();
            var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.Description = "Document Scanner Bridge";
            shortcut.TargetPath = exePath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
            shortcut.Save();
        }
    }
}
