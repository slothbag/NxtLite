[Setup]
AppName=NxtLite
AppVersion=0.6
DefaultDirName={pf}\NxtLite
DefaultGroupName=NxtLite
UninstallDisplayIcon={app}\NxtLite.exe
Compression=lzma2
SolidCompression=yes
OutputDir=.\
OutputBaseFilename=nxtlite_setup

[Files]
Source: "NxtLite\bin\Release\NxtLite.exe"; DestDir: "{app}"
Source: "NxtLite\bin\Release\Eto.dll"; DestDir: "{app}"
Source: "NxtLite\bin\Release\Eto.Mac.dll"; DestDir: "{app}"
Source: "NxtLite\bin\Release\Eto.WinForms.dll"; DestDir: "{app}"
Source: "NxtLite\bin\Release\Mono.Net.HttpListener.dll"; DestDir: "{app}"
Source: "NxtLite\bin\Release\Mono.Security.dll"; DestDir: "{app}"
Source: "NxtLite\bin\Release\MonoMac.dll"; DestDir: "{app}"
Source: "NxtLite\bin\Release\Newtonsoft.Json.dll"; DestDir: "{app}"
Source: "NxtLite\assets\*"; DestDir: "{app}\assets"; Flags: recursesubdirs


[Icons]
Name: "{group}\NxtLite"; Filename: "{app}\NxtLite.exe"
