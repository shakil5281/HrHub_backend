$ErrorActionPreference = "Stop"

Write-Host "Updating manager database..."
dotnet ef database update -c ApplicationDbContext -s ERPBackend.API -p ERPBackend.Infrastructure

Write-Host "Resetting cashbook database..."
dotnet ef database drop -f -c CashbookDbContext -s ERPBackend.API -p ERPBackend.Infrastructure
dotnet ef database update -c CashbookDbContext -s ERPBackend.API -p ERPBackend.Infrastructure

Write-Host "Resetting production database..."
dotnet ef database drop -f -c ProductionDbContext -s ERPBackend.API -p ERPBackend.Infrastructure
dotnet ef database update -c ProductionDbContext -s ERPBackend.API -p ERPBackend.Infrastructure

Write-Host "Resetting store database..."
dotnet ef database drop -f -c StoreDbContext -s ERPBackend.API -p ERPBackend.Infrastructure
dotnet ef database update -c StoreDbContext -s ERPBackend.API -p ERPBackend.Infrastructure

Write-Host "Resetting merchandising database..."
dotnet ef database drop -f -c MerchandisingDbContext -s ERPBackend.API -p ERPBackend.Infrastructure
dotnet ef database update -c MerchandisingDbContext -s ERPBackend.API -p ERPBackend.Infrastructure

Write-Host "Resetting cutting database..."
dotnet ef database drop -f -c CuttingDbContext -s ERPBackend.API -p ERPBackend.Infrastructure
dotnet ef database update -c CuttingDbContext -s ERPBackend.API -p ERPBackend.Infrastructure

Write-Host "Database reset complete."
