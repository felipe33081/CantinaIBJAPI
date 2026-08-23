<#
    build-installer.ps1 — Gera o INSTALADOR do Cantina IBJ (CantinaIBJ-Setup-x.y.z.exe).

    Faz, em ordem:
      1. (Re)publica o app desktop com o ícone embutido (chama build-desktop.ps1)
      2. Compila o instalador com o Inno Setup (ISCC.exe)

    Uso:
      powershell -ExecutionPolicy Bypass -File build-installer.ps1
      (opcional) -SkipBuild     pula a etapa de publish (usa a pasta CantinaIBJ-Publish atual)

    Saída: CantinaIBJAPI\installer\Output\CantinaIBJ-Setup-1.0.0.exe
#>
param(
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path   # ...\CantinaIBJAPI
$Iss       = Join-Path $ScriptDir "installer\setup.iss"

if (-not $SkipBuild) {
    Write-Host "==> Publicando o app desktop (com ícone)..." -ForegroundColor Cyan
    & (Join-Path $ScriptDir "build-desktop.ps1") -SkipFrontend
}

# Localiza o compilador do Inno Setup
$iscc = @(
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe",
    (Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe")
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $iscc) {
    $cmd = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($cmd) { $iscc = $cmd.Source }
}
if (-not $iscc) {
    throw "Inno Setup (ISCC.exe) não encontrado. Instale com: winget install JRSoftware.InnoSetup"
}

Write-Host "==> Compilando o instalador com o Inno Setup..." -ForegroundColor Cyan
& $iscc $Iss
if ($LASTEXITCODE -ne 0) { throw "Falha ao compilar o instalador (ISCC retornou $LASTEXITCODE)." }

Write-Host ""
Write-Host "Instalador gerado em:" -ForegroundColor Green
Get-ChildItem (Join-Path $ScriptDir "installer\Output") -Filter *.exe | ForEach-Object { Write-Host "  $($_.FullName)" -ForegroundColor Green }
