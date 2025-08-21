# PowerShell script to install the FileProcessor as a Windows Service
# Run this script as Administrator

param(
    [string]$ServiceName = "FileProcessorService",
    [string]$DisplayName = "File Processor Service",
    [string]$Description = "Processes files automatically in the background",
    [string]$BinaryPath = $null,
    [string]$StartType = "Automatic"
)

# Get the current directory if BinaryPath is not provided
if (-not $BinaryPath) {
    $currentDir = Get-Location
    $BinaryPath = Join-Path $currentDir "FileProcessor.Service.exe"
}

Write-Host "Installing Windows Service..." -ForegroundColor Green
Write-Host "Service Name: $ServiceName" -ForegroundColor Yellow
Write-Host "Display Name: $DisplayName" -ForegroundColor Yellow
Write-Host "Binary Path: $BinaryPath" -ForegroundColor Yellow
Write-Host "Start Type: $StartType" -ForegroundColor Yellow

try {
    # Check if service already exists
    $existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    
    if ($existingService) {
        Write-Host "Service '$ServiceName' already exists. Stopping and removing..." -ForegroundColor Yellow
        
        # Stop the service if it's running
        if ($existingService.Status -eq 'Running') {
            Stop-Service -Name $ServiceName -Force
            Write-Host "Service stopped." -ForegroundColor Green
        }
        
        # Remove the existing service
        sc.exe delete $ServiceName
        Write-Host "Existing service removed." -ForegroundColor Green
        
        # Wait a moment for the service to be fully removed
        Start-Sleep -Seconds 2
    }
    
    # Create the new service
    New-Service -Name $ServiceName -BinaryPathName $BinaryPath -DisplayName $DisplayName -Description $Description -StartupType $StartType
    Write-Host "Service '$ServiceName' created successfully." -ForegroundColor Green
    
    # Start the service
    Start-Service -Name $ServiceName
    Write-Host "Service '$ServiceName' started successfully." -ForegroundColor Green
    
    # Display service status
    Get-Service -Name $ServiceName | Format-Table -AutoSize
    
    Write-Host "Installation completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "To manage the service, use:" -ForegroundColor Cyan
    Write-Host "  Start:   Start-Service -Name $ServiceName" -ForegroundColor White
    Write-Host "  Stop:    Stop-Service -Name $ServiceName" -ForegroundColor White
    Write-Host "  Status:  Get-Service -Name $ServiceName" -ForegroundColor White
    Write-Host "  Remove:  sc.exe delete $ServiceName" -ForegroundColor White
}
catch {
    Write-Error "Failed to install service: $($_.Exception.Message)"
    exit 1
}