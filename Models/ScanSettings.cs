using System;

namespace ScannerBridge.Models
{
    public class ScanSettings
    {
        public string DefaultScanner { get; set; } = "";
        public int Dpi { get; set; } = 300;
        public string ColorMode { get; set; } = "Color"; 
        public bool UseAdf { get; set; } = true;
        public bool TwainPreferred { get; set; } = false;
    }
}
