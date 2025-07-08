# Golf League Manager - Development Tenant Launcher (PowerShell)
# Usage: .\run-with-tenant.ps1 [tenant-name]
# Example: .\run-with-tenant.ps1 htlyons

param(
    [string]$Tenant = "southmoore"
)

Write-Host "🏌️  Starting Golf League Manager with tenant: $Tenant" -ForegroundColor Green
Write-Host "📁 Database will be: golfdb_$Tenant" -ForegroundColor Yellow
Write-Host ""

# Set the default tenant via command line argument
dotnet run --DefaultTenant="$Tenant"
