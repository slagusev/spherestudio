; Sphere Studio Setup Script for Inno Setup 5.5+
; Builds a Windows installer distribution of Sphere Studio.
; Copyright (c) 2015 Spherical

#define AppName "Sphere Studio"
#define AppPublisher "Spherical"
#define AppVersion "1.2.0"

[Setup]
OutputBaseFilename=SphereStudio-{#AppVersion}
SetupIconFile=Sphere Studio\Sphere Studio.ico
OutputDir=.
ArchitecturesInstallIn64BitMode=x64 ia64
Compression=lzma
LZMAUseSeparateProcess=yes
SolidCompression=yes

UninstallDisplayIcon={app}\Sphere Studio.ico,0
AppId={{F40892B0-C96E-48B7-B1E9-8C2BFB6C167D}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL=http://www.spheredev.org/
AppSupportURL=http://forums.spheredev.org/
AppUpdatesURL=http://forums.spheredev.org/index.php/topic,24.0.html
LicenseFile=LICENSE.txt
DefaultDirName={pf}\{#AppName}
DisableProgramGroupPage=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Types]
Name: "full"; Description: "Full Installation"
Name: "minimal"; Description: "Minimal Installation"
Name: "custom"; Description: "Custom Installation"; Flags: iscustom

[Components]
Name: "spherestudio"; Description: "{#AppName} {#AppVersion}"; Types: full minimal custom; Flags: fixed
Name: "engines"; Description: "Engine Support"
Name: "engines/sphere1x"; Description: "Sphere 1.x Support"; Types: full minimal

[Tasks]
Name: "desktop"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "Sphere Studio\bin\Release\Sphere Studio.exe"; DestDir: "{app}"; Flags: ignoreversion; Components: spherestudio
Source: "Sphere Studio\Sphere Studio.ico"; DestDir: "{app}"; Flags: ignoreversion; Components: spherestudio
Source: "Sphere Studio\bin\Release\Sphere Studio.exe.config"; DestDir: "{app}"; Flags: ignoreversion; Components: spherestudio
Source: "Sphere Studio\bin\Release\Sphere Studio.exe.manifest"; DestDir: "{app}"; Flags: ignoreversion; Components: spherestudio
Source: "Sphere Studio\bin\Release\*.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: spherestudio
Source: "Sphere Studio\bin\Release\SphereLexer.xml"; DestDir: "{app}"; Flags: ignoreversion; Components: spherestudio
Source: "Sphere Studio\bin\Release\Docs\*"; DestDir: "{app}\Docs"; Flags: ignoreversion; Components: spherestudio
Source: "Sphere Studio\bin\Release\Plugins\FontEditPlugin.dll"; DestDir: "{app}\Plugins"; Flags: ignoreversion; Components: spherestudio
Source: "Sphere Studio\bin\Release\Plugins\ImageEditPlugin.dll"; DestDir: "{app}\Plugins"; Flags: ignoreversion; Components: spherestudio
Source: "Sphere Studio\bin\Release\Plugins\MapEditPlugin.dll"; DestDir: "{app}\Plugins"; Flags: ignoreversion; Components: spherestudio
Source: "Sphere Studio\bin\Release\Plugins\ScriptEditPlugin.dll"; DestDir: "{app}\Plugins"; Flags: ignoreversion; Components: spherestudio
Source: "Sphere Studio\bin\Release\Plugins\SoundTestPlugin.dll"; DestDir: "{app}\Plugins"; Flags: ignoreversion; Components: spherestudio
Source: "Sphere Studio\bin\Release\Plugins\SpritesetEditPlugin.dll"; DestDir: "{app}\Plugins"; Flags: ignoreversion; Components: spherestudio
Source: "Sphere Studio\bin\Release\Plugins\TaskListPlugin.dll"; DestDir: "{app}\Plugins"; Flags: ignoreversion; Components: spherestudio
Source: "Sphere Studio\bin\Release\Plugins\WindowstyleEditPlugin.dll"; DestDir: "{app}\Plugins"; Flags: ignoreversion; Components: spherestudio
Source: "Sphere Studio\bin\Release\Plugins\SphereClassicPlugin.dll"; DestDir: "{app}\Plugins"; Flags: ignoreversion; Components: engines/sphere1x

[Icons]
Name: "{commonprograms}\{#AppName}"; Filename: "{app}\Sphere Studio.exe"
Name: "{commondesktop}\{#AppName}"; Filename: "{app}\Sphere Studio.exe"; Tasks: desktop

[Run]
Filename: "{app}\Sphere Studio.exe"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent