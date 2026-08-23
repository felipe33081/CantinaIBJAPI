<#
    build-desktop.ps1 — Gera o aplicativo desktop offline "Cantina IBJ" (.exe unico).

    O que faz, em ordem:
      1. Compila o frontend React (npm run build)
      2. Copia o build para o wwwroot da API (servido na mesma origem)
      3. Publica o app desktop como .exe self-contained single-file (win-x64)

    Uso:
      powershell -ExecutionPolicy Bypass -File build-desktop.ps1
      (opcional) -SkipFrontend  para pular a etapa do React se nada mudou no front

    Saida: pasta CantinaIBJ-Publish (ao lado dos repositorios), com CantinaIBJ.exe
#>
param(
    [switch]$SkipFrontend
)

$ErrorActionPreference = "Stop"

# Caminhos (os dois repositorios sao irmaos dentro de ...\source)
$ScriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Path        # ...\CantinaIBJAPI
$SourceRoot  = Split-Path -Parent $ScriptDir                          # ...\source
$Frontend    = Join-Path $SourceRoot "NovoSistemaCantinaIBJ"
$WebApi      = Join-Path $ScriptDir  "ProjetoCantinaIBJ\CantinaIBJ.WebApi"
$Desktop     = Join-Path $ScriptDir  "ProjetoCantinaIBJ\CantinaIBJ.Desktop\CantinaIBJ.Desktop.csproj"
$WwwRoot     = Join-Path $WebApi "wwwroot"
$Publish     = Join-Path $SourceRoot "CantinaIBJ-Publish"

if (-not $SkipFrontend) {
    Write-Host "==> [1/3] Compilando o frontend React..." -ForegroundColor Cyan
    Push-Location $Frontend
    if (-not (Test-Path (Join-Path $Frontend "node_modules"))) {
        npm install --no-audit --no-fund
    }
    $env:CI = "false"
    npm run build
    Pop-Location

    Write-Host "==> [2/3] Copiando o build para o wwwroot da API..." -ForegroundColor Cyan
    if (Test-Path $WwwRoot) { Remove-Item $WwwRoot -Recurse -Force }
    New-Item -ItemType Directory -Path $WwwRoot | Out-Null
    Copy-Item -Path (Join-Path $Frontend "build\*") -Destination $WwwRoot -Recurse -Force
} else {
    Write-Host "==> [1-2/3] Frontend pulado (-SkipFrontend). Usando wwwroot atual." -ForegroundColor Yellow
}

Write-Host "==> [3/3] Publicando o app desktop (.exe self-contained single-file)..." -ForegroundColor Cyan
if (Test-Path $Publish) { Remove-Item $Publish -Recurse -Force }
dotnet publish $Desktop -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $Publish

Write-Host ""
Write-Host "Pronto! Aplicativo gerado em:" -ForegroundColor Green
Write-Host "  $Publish\CantinaIBJ.exe" -ForegroundColor Green
Write-Host "Basta copiar a pasta para o computador da cantina e dar duplo-clique no .exe." -ForegroundColor Green
