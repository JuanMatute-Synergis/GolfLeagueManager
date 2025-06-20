#!/usr/bin/env pwsh
# Script: generate_week10_pdf.ps1
# Deletes Week10-new.pdf, downloads new PDF from API, and opens it in VS Code

$ErrorActionPreference = 'Stop'

# Change to backend directory
Push-Location $PSScriptRoot

# Remove the old PDF if it exists
$oldPdf = "Week10-new.pdf"
if (Test-Path $oldPdf) {
    Remove-Item $oldPdf -Force
    Write-Host "Deleted $oldPdf"
}

# Download the new PDF using curl
$apiUrl = "http://localhost:5274/api/pdf/scorecard/week/d12fc4ef-d944-42c9-9854-94f52554b8fe"
Write-Host "Downloading new PDF from $apiUrl..."
curl -v -o $oldPdf $apiUrl

# Open the PDF in VS Code
code $oldPdf

Pop-Location
