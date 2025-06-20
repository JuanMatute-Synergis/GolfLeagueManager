# Download DejaVu Sans fonts for PDF generation
$ErrorActionPreference = 'Stop'

$fontDir = Join-Path $PSScriptRoot 'Fonts'
if (-not (Test-Path $fontDir)) {
    New-Item -ItemType Directory -Path $fontDir | Out-Null
}

$fonts = @(
    @{ Name = 'DejaVuSans.ttf'; Url = 'https://github.com/dejavu-fonts/dejavu-fonts/raw/version_2_37/ttf/DejaVuSans.ttf' },
    @{ Name = 'DejaVuSans-Bold.ttf'; Url = 'https://github.com/dejavu-fonts/dejavu-fonts/raw/version_2_37/ttf/DejaVuSans-Bold.ttf' }
)

foreach ($font in $fonts) {
    $dest = Join-Path $fontDir $font.Name
    if (-not (Test-Path $dest)) {
        Write-Host "Downloading $($font.Name)..."
        Invoke-WebRequest -Uri $font.Url -OutFile $dest
    } else {
        Write-Host "$($font.Name) already exists."
    }
}
Write-Host "DejaVu Sans fonts are ready in $fontDir."
