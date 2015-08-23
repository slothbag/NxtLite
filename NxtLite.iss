[Setup]
AppName=NxtLite
AppVersion=0.12
DefaultDirName={pf}\NxtLite
DefaultGroupName=NxtLite
UninstallDisplayIcon={app}\NxtLite.exe
Compression=lzma2
SolidCompression=yes
OutputDir=.\
OutputBaseFilename=build\nxtlite_setup

[Files]
Source: "build\win32\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\NxtLite"; Filename: "{app}\nxtlite.exe"; IconFilename: "{app}\core\nxtlite.ico"; WorkingDir: "{app}"
