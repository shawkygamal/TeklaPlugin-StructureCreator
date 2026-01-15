param(
    [string]$TargetVersion = "",
    [switch]$AutoDetect
)

# Function to find installed Tekla Structures versions
function Get-TeklaVersions {
    $teklaPath = "C:\Program Files\Tekla Structures"
    if (Test-Path $teklaPath) {
        return Get-ChildItem $teklaPath -Directory | Where-Object { $_.Name -match '^\d+\.\d+$' } | Sort-Object Name -Descending
    }
    return @()
}

# Function to update project file
function Update-ProjectFile {
    param([string]$version, [string]$projectFile)

    $content = Get-Content $projectFile -Raw

    # Update ReferencePath
    $content = $content -replace 'C:\\Program Files\\Tekla Structures\\[^\\]+\\bin', "C:\Program Files\Tekla Structures\$version\bin"

    # Write back to file
    Set-Content $projectFile $content -Encoding UTF8
    Write-Host "Updated $projectFile to use Tekla Structures $version" -ForegroundColor Green
}

# Function to update batch file
function Update-BatchFile {
    param([string]$version, [string]$batchFile)

    $content = Get-Content $batchFile

    # Update version in paths
    $content = $content -replace 'C:\\Program Files\\Tekla Structures\\[^\\]+\\bin', "C:\Program Files\Tekla Structures\$version\bin"

    Set-Content $batchFile $content -Encoding ASCII
    Write-Host "Updated $batchFile to use Tekla Structures $version" -ForegroundColor Green
}

# Function to update README
function Update-ReadmeFile {
    param([string]$version, [string]$readmeFile)

    $content = Get-Content $readmeFile

    # Update version requirement
    $content = $content -replace 'Tekla Structures \d+\.\d+', "Tekla Structures $version"

    # Update paths in manual installation
    $content = $content -replace 'C:\\Program Files\\Tekla Structures\\[^\\]+\\bin', "C:\Program Files\Tekla Structures\$version\bin"

    Set-Content $readmeFile $content -Encoding UTF8
    Write-Host "Updated $readmeFile to use Tekla Structures $version" -ForegroundColor Green
}

# Main logic
Write-Host "Tekla Structures Version Setup Tool" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan

$versions = Get-TeklaVersions

if ($versions.Count -eq 0) {
    Write-Host "No Tekla Structures installations found in C:\Program Files\Tekla Structures\" -ForegroundColor Red
    exit 1
}

Write-Host "Found Tekla Structures versions:" -ForegroundColor Yellow
$versions | ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor White }

# Determine target version
$targetVersion = $TargetVersion
if ($AutoDetect -or [string]::IsNullOrEmpty($targetVersion)) {
    $targetVersion = $versions[0].Name
    Write-Host "Auto-selected latest version: $targetVersion" -ForegroundColor Green
}

# Validate version exists
if (-not ($versions | Where-Object { $_.Name -eq $targetVersion })) {
    Write-Host "Version $targetVersion not found. Available versions:" -ForegroundColor Red
    $versions | ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor White }
    exit 1
}

# Update files
Write-Host "Updating project files to use Tekla Structures $targetVersion..." -ForegroundColor Cyan

Update-ProjectFile $targetVersion "TeklaPlugin.csproj"
Update-BatchFile $targetVersion "install_plugin.bat"
Update-ReadmeFile $targetVersion "README_Plugin_Installation.md"

Write-Host "Setup complete! Project is now configured for Tekla Structures $targetVersion" -ForegroundColor Green
Write-Host "You can now build the project with: dotnet build" -ForegroundColor Yellow
