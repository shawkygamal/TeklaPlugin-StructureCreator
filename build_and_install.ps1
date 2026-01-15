# Tekla Structure Creator Plugin - Build and Install Script
# This script builds the plugin and installs it to Tekla Structures

param(
    [switch]$Clean,
    [switch]$Release,
    [switch]$Install
)

$ErrorActionPreference = "Stop"

# Configuration
$projectName = "TeklaPlugin"
$projectFile = "$projectName.csproj"
$buildConfig = if ($Release) { "Release" } else { "Debug" }
$outputDir = "bin\$buildConfig"
$pluginName = "$projectName.dll"

# Get Tekla installation path
function Get-TeklaPath {
    $versions = Get-ChildItem "C:\Program Files\Tekla Structures" -Directory | Where-Object { $_.Name -match '^\d+\.\d+$' } | Sort-Object Name -Descending
    if ($versions.Count -eq 0) {
        throw "No Tekla Structures installation found!"
    }
    return "C:\Program Files\Tekla Structures\$($versions[0].Name)"
}

function Write-Step {
    param([string]$message)
    Write-Host "==> $message" -ForegroundColor Cyan
}

# Clean build artifacts
if ($Clean) {
    Write-Step "Cleaning build artifacts..."
    try {
        if (Test-Path "bin") { Remove-Item "bin" -Recurse -Force -ErrorAction Stop }
        if (Test-Path "obj") { Remove-Item "obj" -Recurse -Force -ErrorAction Stop }
        Write-Host "Clean complete!" -ForegroundColor Green
    } catch {
        Write-Host "Warning: Could not clean all files (some may be locked by running processes)" -ForegroundColor Yellow
        Write-Host "Try closing Tekla Structures and running again." -ForegroundColor Yellow
    }
}

# Setup Tekla version
Write-Step "Setting up Tekla Structures version..."
$teklaPath = Get-TeklaPath
Write-Host "Using Tekla Structures at: $teklaPath" -ForegroundColor Yellow

# Run setup script
powershell -ExecutionPolicy Bypass -File setup_tekla_version.ps1 -AutoDetect

# Build the project
Write-Step "Building $projectName ($buildConfig configuration)..."
try {
    dotnet build $projectFile --configuration $buildConfig --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed!"
    }
} catch {
    Write-Host "Build failed. Make sure Tekla Structures is not running (close the application)." -ForegroundColor Red
    exit 1
}

Write-Host "Build successful!" -ForegroundColor Green

# Install plugin if requested
if ($Install) {
    Write-Step "Installing plugin to Tekla Structures..."

    $pluginDir = "$teklaPath\bin\plugins\Tekla\Model\StructureCreator"
    $sourceDll = "$outputDir\$projectName.exe"

    # Create plugin directory
    if (-not (Test-Path $pluginDir)) {
        New-Item -ItemType Directory -Path $pluginDir -Force | Out-Null
        Write-Host "Created plugin directory: $pluginDir" -ForegroundColor Green
    }

    # Copy plugin DLL (rename .exe to .dll for Tekla)
    Copy-Item $sourceDll "$pluginDir\$pluginName" -Force
    Write-Host "Plugin installed to: $pluginDir\$pluginName" -ForegroundColor Green

    Write-Host @"

Plugin installation complete!

To use the plugin:
1. Start Tekla Structures
2. Go to Applications menu
3. Look for StructureCreator plugin
4. The plugin should appear in the menu

"@ -ForegroundColor Green
}

Write-Host @"

Build Summary:
- Configuration: $buildConfig
- Output: $outputDir\$projectName.exe
- Tekla Version: $($teklaPath.Split('\')[-1])

Usage:
  .\build_and_install.ps1              # Build debug version
  .\build_and_install.ps1 -Release     # Build release version
  .\build_and_install.ps1 -Install     # Build and install
  .\build_and_install.ps1 -Clean       # Clean build artifacts

"@ -ForegroundColor Yellow
