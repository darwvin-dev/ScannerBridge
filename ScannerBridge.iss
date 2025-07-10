[Setup]
AppName=ScannerBridge
AppVersion=1.1
DefaultDirName={autopf}\ScannerBridge
DefaultGroupName=ScannerBridge
OutputDir=Output
OutputBaseFilename=Setup_ScannerBridge
Compression=lzma
SolidCompression=yes
SetupIconFile=icon.ico
PrivilegesRequired=lowest

[Files]
Source: "D:\Desktop\Projects\ScannerBridge\ScannerBridge\bin\Release\net48\ScannerBridge.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\Desktop\Projects\ScannerBridge\ScannerBridge\bin\Release\net48\*.dll"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: "icon.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\ScannerBridge"; Filename: "{app}\ScannerBridge.exe"; WorkingDir: "{app}"; IconFilename: "{app}\icon.ico"
Name: "{userstartup}\ScannerBridge"; Filename: "{app}\ScannerBridge.exe"; WorkingDir: "{app}"; IconFilename: "{app}\icon.ico"

[Run]
Filename: "{app}\ScannerBridge.exe"; Description: "Launch ScannerBridge"; Flags: nowait postinstall skipifsilent
