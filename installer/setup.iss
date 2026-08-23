; ============================================================================
;  Instalador do Cantina IBJ (app desktop offline)
;  Compile com: ISCC.exe setup.iss   (Inno Setup 6)
;  Ou use build-installer.ps1 na raiz do CantinaIBJAPI.
; ============================================================================

#define AppName "Cantina IBJ"
#define AppVersion "1.0.0"
#define AppPublisher "Cantina IBJ"
#define AppExe "CantinaIBJ.exe"

; Pasta com o resultado do publish (build-desktop.ps1). Pode ser sobrescrita:
;   ISCC.exe /DSourceDir="C:\caminho\CantinaIBJ-Publish" setup.iss
#ifndef SourceDir
  #define SourceDir "..\..\CantinaIBJ-Publish"
#endif

#define IconFile "..\ProjetoCantinaIBJ\CantinaIBJ.Desktop\app.ico"

[Setup]
AppId={{8F5131CA-A42F-486B-98D1-CANTINAIBJ01}}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#AppExe}
SetupIconFile={#IconFile}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
; App e SQLite nativo sao x64
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=Output
OutputBaseFilename=CantinaIBJ-Setup-{#AppVersion}
PrivilegesRequired=admin

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "Criar um atalho na Área de Trabalho"; GroupDescription: "Atalhos:"

[Files]
; Copia todo o conteudo do publish (exe unico + wwwroot + appsettings)
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"; IconFilename: "{app}\{#AppExe}"
Name: "{group}\Desinstalar {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; IconFilename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "Abrir o {#AppName} agora"; Flags: nowait postinstall skipifsilent

; Observacao: os dados ficam em %LOCALAPPDATA%\CantinaIBJ e NAO sao removidos na
; desinstalacao, para preservar as vendas entre reinstalacoes/atualizacoes.
