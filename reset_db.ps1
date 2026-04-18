$contexts = @(
    "ApplicationDbContext",
    "CashbookDbContext",
    "CuttingDbContext",
    "MerchandisingDbContext",
    "ProductionDbContext",
    "StoreDbContext"
)

foreach ($context in $contexts) {
    Write-Host "Dropping database for context $context..." -ForegroundColor Yellow
    dotnet ef database drop --force --context $context --project ERPBackend.Infrastructure --startup-project ERPBackend.API
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Database dropped successfully for $context." -ForegroundColor Green
    } else {
        Write-Host "Failed to drop database for $context. It might not exist yet." -ForegroundColor DarkYellow
    }

    Write-Host "Updating database for context $context..." -ForegroundColor Yellow
    dotnet ef database update --context $context --project ERPBackend.Infrastructure --startup-project ERPBackend.API
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Database updated successfully for $context." -ForegroundColor Green
    } else {
        Write-Host "Failed to update database for $context." -ForegroundColor Red
    }
}

Write-Host "Database reset complete." -ForegroundColor Cyan
