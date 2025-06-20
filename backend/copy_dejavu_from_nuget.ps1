# Copy DejaVu Sans fonts from NuGet global packages to backend/Fonts
$ErrorActionPreference = 'Stop'

$nugetRoot = Join-Path $env:USERPROFILE ".nuget\packages"
$packageName = "dejavusans.ttf"
$fontsTarget = Join-Path $PSScriptRoot "Fonts"

# Find the latest version installed
$packageDir = Get-ChildItem -Path (Join-Path $nugetRoot $packageName) -Directory | Sort-Object Name -Descending | Select-Object -First 1
if (-not $packageDir) {
    Write-Error "Could not find NuGet package $packageName in $nugetRoot."
    exit 1
}

$ttfSourceDir = Join-Path $packageDir.FullName "content"
$ttfs = @("DejaVuSans.ttf", "DejaVuSans-Bold.ttf")

if (-not (Test-Path $fontsTarget)) {
    New-Item -ItemType Directory -Path $fontsTarget | Out-Null
}

foreach ($ttf in $ttfs) {
    $src = Join-Path $ttfSourceDir $ttf
    $dst = Join-Path $fontsTarget $ttf
    if (Test-Path $src) {
        Copy-Item $src $dst -Force
        Write-Host "Copied $ttf to $fontsTarget."
    } else {
        Write-Warning "$ttf not found in $ttfSourceDir."
    }
}
Write-Host "Font copy complete."
