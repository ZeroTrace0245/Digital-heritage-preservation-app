#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#ifndef PublishDir
  #define PublishDir "..\artifacts\publish\win-x64"
#endif
[Setup]
AppId={{AB66B167-DA6D-4799-84E1-216E7A1046BD}
AppName=Lorevia
AppVersion={#AppVersion}
AppPublisher=Lorevia
AppPublisherURL=https://github.com/ZeroTrace0245/Digital-heritage-preservation-app
AppSupportURL=https://github.com/ZeroTrace0245/Digital-heritage-preservation-app/issues
AppUpdatesURL=https://github.com/ZeroTrace0245/Digital-heritage-preservation-app/releases
DefaultDirName={localappdata}\Programs\Lorevia
DefaultGroupName=Lorevia
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.19041
OutputDir=..\artifacts\installer
OutputBaseFilename=Lorevia-Setup-{#AppVersion}-x64
SetupIconFile=..\Assets\Lorevia.ico
UninstallDisplayIcon={app}\digital heritage preservation app.exe
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE.txt

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\artifacts\tools\MicrosoftEdgeWebview2Setup.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Icons]
Name: "{autoprograms}\Lorevia"; Filename: "{app}\digital heritage preservation app.exe"
Name: "{autodesktop}\Lorevia"; Filename: "{app}\digital heritage preservation app.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Shortcuts:"

[Run]
Filename: "{tmp}\MicrosoftEdgeWebview2Setup.exe"; Parameters: "/silent /install"; StatusMsg: "Setting up the Microsoft Edge WebView2 browser runtime..."; Flags: waituntilterminated
Filename: "{app}\digital heritage preservation app.exe"; Description: "Open Lorevia"; Flags: nowait postinstall skipifsilent

; Personal data is deliberately not included in UninstallDelete.
