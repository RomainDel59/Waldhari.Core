param(
    [string]$version = "1.0.0"
)

$solutionDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$releaseDir = Join-Path $solutionDir "releases"
if (!(Test-Path $releaseDir)) { New-Item -ItemType Directory -Path $releaseDir }

$buildDir = Join-Path $solutionDir "Waldhari.Core\bin\Release"
$modDir = Join-Path $solutionDir "Waldhari.Core\Properties\Waldhari.Core"

$zipFile = Join-Path $releaseDir "Waldhari.Core.$version.zip"

if (Test-Path $zipFile) { Remove-Item $zipFile }

# Crée un dossier temporaire pour rassembler tout
$tempDir = Join-Path $env:TEMP "WaldhariCoreRelease"
if (Test-Path $tempDir) { Remove-Item $tempDir -Recurse -Force }
New-Item -ItemType Directory -Path $tempDir | Out-Null

# Copie la DLL
Copy-Item -Path "$buildDir\Waldhari.Core.dll" -Destination $tempDir

# Copie le répertoire de localisation avec son arborescence relative
Copy-Item -Path $modDir -Destination $tempDir -Recurse

# Crée le ZIP
Compress-Archive -Path "$tempDir\*" -DestinationPath $zipFile

# Nettoyage
Remove-Item $tempDir -Recurse -Force

Write-Host "Release generated: $zipFile"
