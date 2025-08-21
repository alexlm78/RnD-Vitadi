# PowerShell script to uninstall the FileProcessor Windows Service
# Run this script as Administrator

param(
    [string]$ServiceName = "FileProcessorService"
)

Write-Host "Uninstalling Windows Service '$ServiceName'..." -ForegroundColor Yellow

try {
    # Check if service exists
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    
    if (-not $service) {
        Write-Host "Service '$ServiceName' does not exist." -ForegroundColor Yellow
        exit 0
    }
    
    Write-Host "Found service '$ServiceName'. Current status: $($service.Status)" -ForegroundColor Green
    
    # Stop the service if it's running
    if ($service.Status -eq 'Running') {
        Write-Host "Stopping service..." -ForegroundColor Yellow
        Stop-Service -Name $ServiceName -Force
        
        # Wait for the service to stop
        $timeout = 30
        $elapsed = 0
        do {
            Start-Sleep -Seconds 1
            $elapsed++
            $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
        } while ($service.Status -eq 'Running' -and $elapsed -lt $timeout)
        
        if ($service.Status -eq 'Running') {
            Write-Warning "Service did not stop within $timeout seconds. Forcing removal..."
        } else {
            Write-Host "Service stopped successfully." -ForegroundColor Green
        }
    }
    
    # Remove the service
    Write-Host "Removing service..." -ForegroundColor Yellow
    sc.exe delete $ServiceName
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Service '$ServiceName' removed successfully." -ForegroundColor Green
    } else {
        Write-Error "Failed to remove service. Exit code: $LASTEXITCODE"
        exit 1
    }
}
catch {
    Write-Error "Failed to uninstall service: $($_.Exception.Message)"
    exit 1
}

Write-Host "Uninstallation completed successfully!" -ForegroundColor Green