param(
    [string]$version = "1.0.0"
)

$solutionDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$releaseDir = Join-Path $solutionDir "releases"
if (!(Test-Path $releaseDir)) { New-Item -ItemType Directory -Path $releaseDir }

$buildDir = Join-Path $solutionDir "Waldhari.Core\bin\Release"
$modDir = Join-Path $solutionDir "Waldhari.Core\Properties\Waldhari.Core"

$zipFile = Join-Path $releaseDir "Waldhari.Core.$version.zip"

# Remove existing zip if present
if (Test-Path $zipFile) { Remove-Item $zipFile }

# Create a temporary folder to gather all files
$tempDir = Join-Path $env:TEMP "WaldhariCoreRelease"
if (Test-Path $tempDir) { Remove-Item $tempDir -Recurse -Force }
New-Item -ItemType Directory -Path $tempDir | Out-Null

# Create the scripts/ subfolder in the temporary folder
$scriptsDir = Join-Path $tempDir "scripts"
New-Item -ItemType Directory -Path $scriptsDir | Out-Null

# Copy the DLL into scripts/
Copy-Item -Path "$buildDir\Waldhari.Core.dll" -Destination $scriptsDir

# Create an empty log file alongside the DLL
New-Item -ItemType File -Path (Join-Path $scriptsDir "Waldhari.Core.log") | Out-Null

# Copy the localization directory under scripts/
$destModDir = Join-Path $scriptsDir "Waldhari.Core"
Copy-Item -Path $modDir -Destination $destModDir -Recurse

# Create the ZIP archive
Compress-Archive -Path "$tempDir\*" -DestinationPath $zipFile

# Cleanup temporary folder
Remove-Item $tempDir -Recurse -Force

Write-Host "Release generated: $zipFile"
