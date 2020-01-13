; Script generated by the Inno Script Studio Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{D704EC6D-7913-4796-88E1-A4BFAFB4DF2B}
AppName=Glitched Epistle
AppVersion=2020.2.0.2
;AppVerName=Glitched Epistle 2020.2.0.2
AppPublisher=Glitched Polygons
AppPublisherURL=glitchedpolygons.com
AppSupportURL=glitchedpolygons.com
AppUpdatesURL=glitchedpolygons.com
DefaultDirName={pf}\Glitched Polygons\Glitched Epistle
DefaultGroupName=Glitched Epistle
LicenseFile=LICENSE
OutputDir=bin
OutputBaseFilename=glitched-epistle-2020.2.0.2
SetupIconFile=src\glitched-epistle-icon.ico
Compression=lzma
ArchitecturesAllowed=x64
SolidCompression=yes

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked


; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\Glitched Epistle"; Filename: "{app}\Epistle.exe"
Name: "{commondesktop}\Glitched Epistle"; Filename: "{app}\Epistle.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\Epistle.exe"; Description: "{cm:LaunchProgram,Glitched Epistle}"; Flags: nowait postinstall skipifsilent

[Dirs]
Name: "{app}\de"
Name: "{app}\gsw"
Name: "{app}\it"
Name: "{app}\x64"
Name: "{app}\x86"

[Files]
Source: "src\bin\Release\System.Security.Permissions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\System.Security.Principal.Windows.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\System.ValueTuple.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\System.Windows.Interactivity.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Unity.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Unity.Container.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\XamlAnimatedGif.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\zxing.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\zxing.presentation.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Epistle.application"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Epistle.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\glitched-epistle-icon.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Epistle.exe.manifest"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Epistle.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Plugin.SimpleAudioPlayer.Abstractions.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Plugin.SimpleAudioPlayer.WPF.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Prism.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Prism.Wpf.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Unity.Abstractions.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Unity.Container.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\zxing.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\zxing.presentation.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Restart.bat"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Epistle.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Epistle.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\BCrypt.Net-Next.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\BouncyCastle.Crypto.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\brolib_x64.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\brolib_x86.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Brotli.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\CommonServiceLocator.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Dapper.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\EntityFramework.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\EntityFramework.SqlServer.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\GlitchedPolygons.ExtensionMethods.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\GlitchedPolygons.ExtensionMethods.RSAXmlPemStringConverter.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\GlitchedPolygons.GlitchedEpistle.Client.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\GlitchedPolygons.RepositoryPattern.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\GlitchedPolygons.RepositoryPattern.SQLite.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\GlitchedPolygons.Services.CompressionUtility.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\GlitchedPolygons.Services.Cryptography.Asymmetric.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\GlitchedPolygons.Services.Cryptography.Symmetric.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\GlitchedPolygons.Services.JwtService.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\GlitchedPolygons.Services.MethodQ.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Microsoft.Expression.Interactions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Microsoft.IdentityModel.JsonWebTokens.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Microsoft.IdentityModel.Logging.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Microsoft.IdentityModel.Tokens.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\NAudio.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Plugin.SimpleAudioPlayer.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Plugin.SimpleAudioPlayer.WPF.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Prism.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\Prism.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\RestSharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\System.Configuration.ConfigurationManager.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\System.Data.SQLite.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\System.Data.SQLite.EF6.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\System.Data.SQLite.Linq.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\System.Drawing.Common.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\System.IdentityModel.Tokens.Jwt.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\System.Security.AccessControl.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\de\Epistle.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion
Source: "src\bin\Release\gsw\Epistle.resources.dll"; DestDir: "{app}\gsw"; Flags: ignoreversion
Source: "src\bin\Release\it\Epistle.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion
Source: "src\bin\Release\x64\SQLite.Interop.dll"; DestDir: "{app}\x64"; Flags: ignoreversion
Source: "src\bin\Release\x86\SQLite.Interop.dll"; DestDir: "{app}\x86"; Flags: ignoreversion
