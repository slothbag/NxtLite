[Setup]
AppName=NxtLite
AppVersion=0.9
DefaultDirName={pf}\NxtLite
DefaultGroupName=NxtLite
UninstallDisplayIcon={app}\NxtLite.exe
Compression=lzma2
SolidCompression=yes
OutputDir=.\
OutputBaseFilename=nxtlite_setup

[Files]
Source: "NxtLite\bin\Release\NxtLite.exe"; DestDir: "{app}"
Source: "NxtLite\bin\Release\Mono.Net.HttpListener.dll"; DestDir: "{app}"
Source: "NxtLite\bin\Release\Mono.Security.dll"; DestDir: "{app}"
Source: "NxtLite\bin\Release\Newtonsoft.Json.dll"; DestDir: "{app}"
Source: "NxtLite\assets\*"; DestDir: "{app}\assets"; Flags: recursesubdirs
Source: "..\Electron\*"; DestDir: "{app}\electron"; Flags: recursesubdirs
Source: "NxtLite\nxtlite.ico"; DestDir: "{app}"
Source: "NxtLite\icon32.png"; DestDir: "{app}"
Source: "gui_app\*"; DestDir: "{app}\gui_app"

[Icons]
Name: "{group}\NxtLite"; Filename: "{app}\electron\electron.exe"; Parameters: "gui_app"; IconFilename: "{app}\nxtlite.ico"; WorkingDir: "{app}"
