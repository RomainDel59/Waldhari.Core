param(
    [string]$version = "1.0"
)

$solutionDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$releaseDir = Join-Path $solutionDir "releases"
if (!(Test-Path $releaseDir)) { New-Item -ItemType Directory -Path $releaseDir }

$buildDir = Join-Path $solutionDir "Waldhari.Core\bin\Release"
$modDir = Join-Path $solutionDir "Waldhari.Core\Properties\Core"

$zipFile = Join-Path $releaseDir "Waldhari.Core.$version.zip"

# Remove existing zip if present
if (Test-Path $zipFile) { Remove-Item $zipFile }

# Create a temporary folder to gather all files
$tempDir = Join-Path $env:TEMP "WaldhariCoreRelease"
if (Test-Path $tempDir) { Remove-Item $tempDir -Recurse -Force }
New-Item -ItemType Directory -Path $tempDir | Out-Null

# Create scripts/Waldhari subfolder
$waldhariDir = Join-Path $tempDir "scripts\Waldhari"
New-Item -ItemType Directory -Path $waldhariDir -Force | Out-Null

# Copy the DLL into scripts/Waldhari and rename to Core.dll
Copy-Item -Path "$buildDir\Waldhari.Core.dll" -Destination (Join-Path $waldhariDir "Core.dll")

# Create an empty log file alongside Core.dll
New-Item -ItemType File -Path (Join-Path $waldhariDir "Core.log") | Out-Null

# Copy the localization directory under scripts/Waldhari/Core
$destModDir = Join-Path $waldhariDir "Core"
Copy-Item -Path $modDir -Destination $destModDir -Recurse

# Create the ZIP archive
Compress-Archive -Path "$tempDir\*" -DestinationPath $zipFile

# Cleanup temporary folder
Remove-Item $tempDir -Recurse -Force

Write-Host "Release generated: $zipFile"
