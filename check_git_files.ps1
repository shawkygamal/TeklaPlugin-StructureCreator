# PowerShell script to check what files would be tracked by Git
Write-Host "=== TeklaPlugin Git Tracking Analysis ===" -ForegroundColor Green
Write-Host ""

Write-Host "Files that SHOULD be tracked:" -ForegroundColor Yellow
Write-Host "-----------------------------" -ForegroundColor Yellow
Get-ChildItem -Recurse -File | Where-Object {
    $_.FullName -notmatch '\\(bin|obj)\\' -and
    $_.Name -notmatch '\.(user|suo|tmp|temp|log|bak|backup|orig|swp|swo)$' -and
    $_.Name -notmatch '^Thumbs\.db$|^Desktop\.ini$|^ehthumbs\.db$' -and
    $_.FullName -notmatch '\\\.vs\\' -and
    $_.Name -notmatch '\.(exe|dll|pdb|cache|resources)$' -and
    $_.FullName -notmatch '\\_ReSharper'
} | Select-Object FullName | ForEach-Object {
    $relativePath = $_.FullName -replace [regex]::Escape((Get-Location).Path + "\"), ""
    Write-Host "  ✓ $relativePath" -ForegroundColor Green
}

Write-Host ""
Write-Host "Files that will be IGNORED:" -ForegroundColor Red
Write-Host "---------------------------" -ForegroundColor Red
Get-ChildItem -Recurse -File | Where-Object {
    $_.FullName -match '\\(bin|obj)\\' -or
    $_.Name -match '\.(user|suo|tmp|temp|log|bak|backup|orig|swp|swo)$' -or
    $_.Name -match '^Thumbs\.db$|^Desktop\.ini$|^ehthumbs\.db$' -or
    $_.FullName -match '\\\.vs\\' -or
    $_.Name -match '\.(exe|dll|pdb|cache|resources)$' -or
    $_.FullName -match '\\_ReSharper'
} | Select-Object FullName | ForEach-Object {
    $relativePath = $_.FullName -replace [regex]::Escape((Get-Location).Path + "\"), ""
    Write-Host "  ✗ $relativePath" -ForegroundColor Red
}

Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
$tracked = (Get-ChildItem -Recurse -File | Where-Object {
    $_.FullName -notmatch '\\(bin|obj)\\' -and
    $_.Name -notmatch '\.(user|suo|tmp|temp|log|bak|backup|orig|swp|swo)$' -and
    $_.Name -notmatch '^Thumbs\.db$|^Desktop\.ini$|^ehthumbs\.db$' -and
    $_.FullName -notmatch '\\\.vs\\' -and
    $_.Name -notmatch '\.(exe|dll|pdb|cache|resources)$' -and
    $_.FullName -notmatch '\\_ReSharper'
}).Count

$ignored = (Get-ChildItem -Recurse -File | Where-Object {
    $_.FullName -match '\\(bin|obj)\\' -or
    $_.Name -match '\.(user|suo|tmp|temp|log|bak|backup|orig|swp|swo)$' -or
    $_.Name -match '^Thumbs\.db$|^Desktop\.ini$|^ehthumbs\.db$' -or
    $_.FullName -match '\\\.vs\\' -or
    $_.Name -match '\.(exe|dll|pdb|cache|resources)$' -or
    $_.FullName -match '\\_ReSharper'
}).Count

Write-Host "  Files to track: $tracked" -ForegroundColor Green
Write-Host "  Files to ignore: $ignored" -ForegroundColor Red
Write-Host "  Total files: $($tracked + $ignored)" -ForegroundColor Cyan