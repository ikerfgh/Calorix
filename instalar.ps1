# Prepara Calorix en una computadora nueva: compila la app y las pruebas
# e instala el navegador que usa el robot de Playwright.
#
# Uso (desde la carpeta del proyecto):
#   powershell -ExecutionPolicy Bypass -File instalar.ps1

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

function Paso($n, $texto) { Write-Host "`n[$n/4] $texto" -ForegroundColor Cyan }
function Fallo($texto) { Write-Host "`nERROR: $texto" -ForegroundColor Red; exit 1 }

# 1. .NET 9 SDK
Paso 1 'Revisando .NET 9 SDK'
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Fallo 'No se encontró dotnet. Instala el .NET 9 SDK desde https://dotnet.microsoft.com/download/dotnet/9.0 y abre una terminal nueva.'
}
$sdks = & dotnet --list-sdks
if (-not ($sdks | Where-Object { $_ -match '^9\.' })) {
    Fallo 'Falta el .NET 9 SDK (no basta el Runtime). Descárgalo de https://dotnet.microsoft.com/download/dotnet/9.0'
}
Write-Host 'OK: .NET 9 SDK instalado'

# 2. Compilar la app
Paso 2 'Compilando la aplicación'
& dotnet build Calorix.csproj -nologo -v q
if ($LASTEXITCODE -ne 0) { Fallo 'No compiló la aplicación.' }

# 3. Compilar las pruebas
Paso 3 'Compilando las pruebas (unitarias y robot)'
& dotnet build tests/Calorix.Tests -nologo -v q
if ($LASTEXITCODE -ne 0) { Fallo 'No compilaron las pruebas unitarias.' }
& dotnet build tests/Calorix.UITests -nologo -v q
if ($LASTEXITCODE -ne 0) { Fallo 'No compiló el robot.' }

# 4. Navegador del robot
Paso 4 'Instalando Chromium para el robot (puede tardar 1-2 minutos)'
$playwright = Join-Path $PSScriptRoot 'tests/Calorix.UITests/bin/Debug/net9.0/playwright.ps1'
& powershell -NoProfile -ExecutionPolicy Bypass -File $playwright install chromium
if ($LASTEXITCODE -ne 0) { Fallo 'No se pudo instalar Chromium. Revisa tu conexión a internet y vuelve a correr el script.' }

Write-Host "`nListo. Todo quedó instalado." -ForegroundColor Green
Write-Host @'

Siguientes pasos:
  Terminal 1 (dejar abierta):  dotnet run --urls http://localhost:5080
  Terminal 2:                  dotnet test tests/Calorix.Tests
                               $env:HEADED=1; dotnet test tests/Calorix.UITests
'@
