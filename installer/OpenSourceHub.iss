; ============================================================================
;  OpenSourceHub — Inno Setup installer script
;  Build the app first:
;    dotnet publish src/OpenSourceHub.UI/OpenSourceHub.UI.csproj -c Release ^
;      -r win-x64 --self-contained false -o release/output
;  Then compile this script with Inno Setup 6+ (iscc installer/OpenSourceHub.iss).
; ============================================================================

#define AppName        "OpenSourceHub"
#define AppVersion      "1.4.0"
#define AppPublisher   "XinPari Software"
#define AppURL         "https://github.com/X1NPAR1/OpenSourceHub"
#define AppExeName     "OpenSourceHub.exe"
#define SourceDir      "..\release\output"

[Setup]
AppId={{B5F4B6D2-7C3E-4E9A-9F2A-OSH140RELEASE}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}/issues
AppUpdatesURL={#AppURL}/releases
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE
OutputDir=..\release\installer
OutputBaseFilename=OpenSourceHub-{#AppVersion}-Setup
SetupIconFile=..\src\OpenSourceHub.UI\Assets\AppIcon.ico
UninstallDisplayIcon={app}\{#AppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17763
PrivilegesRequiredOverridesAllowed=dialog

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "german";  MessagesFile: "compiler:Languages\German.isl"
Name: "dutch";   MessagesFile: "compiler:Languages\Dutch.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

[Code]
// Warn (but don't block) if the .NET 10 Desktop Runtime is not detected.
function IsDotNetDesktopInstalled(): Boolean;
var
  Found: Cardinal;
begin
  Result :=
    RegQueryDWordValue(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', '10', Found) or
    DirExists(ExpandConstant('{commonpf}\dotnet\shared\Microsoft.WindowsDesktop.App'));
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
  if not IsDotNetDesktopInstalled() then
  begin
    if MsgBox('OpenSourceHub needs the .NET 10 Desktop Runtime (x64).' + #13#10 +
              'It does not appear to be installed.' + #13#10#13#10 +
              'Continue anyway? You can install the runtime from ' +
              'https://dotnet.microsoft.com/download/dotnet/10.0',
              mbConfirmation, MB_YESNO) = IDNO then
      Result := False;
  end;
end;
