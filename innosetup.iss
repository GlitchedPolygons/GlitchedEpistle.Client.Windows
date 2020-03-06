; Script generated by the Inno Script Studio Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{D704EC6D-7913-4796-88E1-A4BFAFB4DF2B}
AppName=Glitched Epistle
AppVersion=3.0.0
;AppVerName=Glitched Epistle 3.0.0
AppPublisher=Glitched Polygons
AppPublisherURL=glitchedpolygons.com
AppSupportURL=glitchedpolygons.com
AppUpdatesURL=glitchedpolygons.com
DefaultDirName={pf}\Glitched Polygons\Glitched Epistle
DefaultGroupName=Glitched Epistle
LicenseFile=LICENSE
OutputDir=bin
OutputBaseFilename=glitched-epistle-3.0.0
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
Name: "{app}\gsw"
Name: "{app}\it"
Name: "{app}\publish"
Name: "{app}\publish\de"
Name: "{app}\publish\gsw"
Name: "{app}\publish\it"
Name: "{app}\publish\runtimes"
Name: "{app}\publish\runtimes\linux-x64"
Name: "{app}\publish\runtimes\linux-x64\native"
Name: "{app}\publish\runtimes\linux-x64\native\netstandard2.0"
Name: "{app}\publish\runtimes\osx-x64"
Name: "{app}\publish\runtimes\osx-x64\native"
Name: "{app}\publish\runtimes\osx-x64\native\netstandard2.0"
Name: "{app}\publish\runtimes\win-x64"
Name: "{app}\publish\runtimes\win-x64\native"
Name: "{app}\publish\runtimes\win-x64\native\netstandard2.0"
Name: "{app}\publish\runtimes\win-x86"
Name: "{app}\publish\runtimes\win-x86\native"
Name: "{app}\publish\runtimes\win-x86\native\netstandard2.0"
Name: "{app}\runtimes"
Name: "{app}\runtimes\linux-x64"
Name: "{app}\runtimes\linux-x64\native"
Name: "{app}\runtimes\linux-x64\native\netstandard2.0"
Name: "{app}\runtimes\osx-x64"
Name: "{app}\runtimes\osx-x64\native"
Name: "{app}\runtimes\osx-x64\native\netstandard2.0"
Name: "{app}\runtimes\win-x64"
Name: "{app}\runtimes\win-x64\native"
Name: "{app}\runtimes\win-x64\native\netstandard2.0"
Name: "{app}\runtimes\win-x86"
Name: "{app}\runtimes\win-x86\native"
Name: "{app}\runtimes\win-x86\native\netstandard2.0"

[Files]
Source: "src\bin\Release\netcoreapp3.1\Epistle.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\Epistle.runtimeconfig.dev.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\Epistle.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\glitched-epistle-icon.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\GlitchedPolygons.ExtensionMethods.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\GlitchedPolygons.GlitchedEpistle.Client.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\GlitchedPolygons.RepositoryPattern.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\GlitchedPolygons.RepositoryPattern.SQLite.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\GlitchedPolygons.Services.CompressionUtility.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\GlitchedPolygons.Services.Cryptography.Asymmetric.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\GlitchedPolygons.Services.Cryptography.Symmetric.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\GlitchedPolygons.Services.MethodQ.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\Microsoft.IdentityModel.JsonWebTokens.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\Microsoft.IdentityModel.Logging.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\Microsoft.IdentityModel.Tokens.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\NAudio.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\Plugin.SimpleAudioPlayer.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\Plugin.SimpleAudioPlayer.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\Prism.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\Prism.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\Restart.bat"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\RestSharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\System.Data.SQLite.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\System.IdentityModel.Tokens.Jwt.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\System.Text.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\System.Windows.Interactivity.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\Unity.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\Unity.Container.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\XamlAnimatedGif.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\zxing.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\BCrypt.Net-Next.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\BouncyCastle.Crypto.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\CommonServiceLocator.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\Dapper.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\Epistle.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\Epistle.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\de\Epistle.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\gsw\Epistle.resources.dll"; DestDir: "{app}\gsw"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\it\Epistle.resources.dll"; DestDir: "{app}\it"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\BCrypt.Net-Next.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\BouncyCastle.Crypto.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\CommonServiceLocator.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\Dapper.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\Epistle.deps.json"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\Epistle.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\Epistle.exe"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\Epistle.runtimeconfig.json"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\glitched-epistle-icon.ico"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\GlitchedPolygons.ExtensionMethods.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\GlitchedPolygons.GlitchedEpistle.Client.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\GlitchedPolygons.RepositoryPattern.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\GlitchedPolygons.RepositoryPattern.SQLite.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\GlitchedPolygons.Services.CompressionUtility.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\GlitchedPolygons.Services.Cryptography.Asymmetric.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\GlitchedPolygons.Services.Cryptography.Symmetric.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\GlitchedPolygons.Services.MethodQ.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\Microsoft.IdentityModel.JsonWebTokens.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\Microsoft.IdentityModel.Logging.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\Microsoft.IdentityModel.Tokens.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\NAudio.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\Newtonsoft.Json.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\Plugin.SimpleAudioPlayer.Abstractions.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\Plugin.SimpleAudioPlayer.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\Prism.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\Prism.Wpf.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\Restart.bat"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\RestSharp.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\System.Data.SQLite.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\System.IdentityModel.Tokens.Jwt.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\System.Text.Json.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\System.Windows.Interactivity.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\Unity.Abstractions.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\Unity.Container.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\XamlAnimatedGif.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\zxing.dll"; DestDir: "{app}\publish"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\de\Epistle.resources.dll"; DestDir: "{app}\publish\de"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\gsw\Epistle.resources.dll"; DestDir: "{app}\publish\gsw"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\it\Epistle.resources.dll"; DestDir: "{app}\publish\it"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\runtimes\linux-x64\native\netstandard2.0\SQLite.Interop.dll"; DestDir: "{app}\publish\runtimes\linux-x64\native\netstandard2.0"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\runtimes\osx-x64\native\netstandard2.0\SQLite.Interop.dll"; DestDir: "{app}\publish\runtimes\osx-x64\native\netstandard2.0"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\runtimes\win-x64\native\netstandard2.0\SQLite.Interop.dll"; DestDir: "{app}\publish\runtimes\win-x64\native\netstandard2.0"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\publish\runtimes\win-x86\native\netstandard2.0\SQLite.Interop.dll"; DestDir: "{app}\publish\runtimes\win-x86\native\netstandard2.0"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\runtimes\linux-x64\native\netstandard2.0\SQLite.Interop.dll"; DestDir: "{app}\runtimes\linux-x64\native\netstandard2.0"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\runtimes\osx-x64\native\netstandard2.0\SQLite.Interop.dll"; DestDir: "{app}\runtimes\osx-x64\native\netstandard2.0"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\runtimes\win-x64\native\netstandard2.0\SQLite.Interop.dll"; DestDir: "{app}\runtimes\win-x64\native\netstandard2.0"; Flags: ignoreversion
Source: "src\bin\Release\netcoreapp3.1\runtimes\win-x86\native\netstandard2.0\SQLite.Interop.dll"; DestDir: "{app}\runtimes\win-x86\native\netstandard2.0"; Flags: ignoreversion
