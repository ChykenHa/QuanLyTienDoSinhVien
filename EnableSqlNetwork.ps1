# EnableSqlNetwork.ps1
# Script to enable TCP/IP for SQL Server Express via Registry
# Bypasses the need for SQL Server Configuration Manager

Write-Host "Dang kiem tra cau hinh mang SQL Server..." -ForegroundColor Cyan

# 1. Tim Instance Name cua SQLEXPRESS
$instanceName = "SQLEXPRESS"
$regPath = "HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL"
$internalName = Get-ItemProperty -Path $regPath -Name $instanceName -ErrorAction SilentlyContinue

if ($null -eq $internalName) {
    Write-Host "Khong tim thay SQL Server instance '$instanceName'. Vui long kiem tra lai ten instance." -ForegroundColor Red
    Exit
}

$sqlDirName = $internalName.$instanceName
Write-Host "Tim thay Internal Name: $sqlDirName" -ForegroundColor Green

# 2. Duong dan Registry den TCP/IP settings
$tcpPath = "HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\$sqlDirName\MSSQLServer\SuperSocketNetLib\Tcp"

if (!(Test-Path $tcpPath)) {
    Write-Host "Khong tim thay duong dan Registry: $tcpPath" -ForegroundColor Red
    Exit
}

# 3. Bat TCP/IP (Enabled = 1)
try {
    Set-ItemProperty -Path $tcpPath -Name "Enabled" -Value 1 -Type DWord
    Write-Host "[OK] Da bat giao thuc TCP/IP." -ForegroundColor Green
} catch {
    Write-Host "[ERROR] Khong the ghi Registry. Hay chay PowerShell duoi quyen Admin!" -ForegroundColor Red
    Exit
}

# 4. Cau hinh Port 1433
$ipAllPath = "$tcpPath\IPAll"
if (Test-Path $ipAllPath) {
    Set-ItemProperty -Path $ipAllPath -Name "TcpPort" -Value "1433"
    Set-ItemProperty -Path $ipAllPath -Name "TcpDynamicPorts" -Value ""
    Write-Host "[OK] Da dat Port co dinh la 1433." -ForegroundColor Green
}

# 5. Cau hinh Firewall (Mo port 1433)
Write-Host "Dang cau hinh Firewall..." -ForegroundColor Cyan
try {
    New-NetFirewallRule -DisplayName "SQL Server LAN Access" -Direction Inbound -LocalPort 1433 -Protocol TCP -Action Allow -ErrorAction SilentlyContinue
    Write-Host "[OK] Da mo Firewall port 1433." -ForegroundColor Green
} catch {
    Write-Host "[WARNING] Khong the cau hinh Firewall tu dong. Ban hay kiem tra thu cong." -ForegroundColor Yellow
}

# 6. Restart SQL Service
$serviceName = "MSSQL`$$instanceName"
Write-Host "Dang khoi dong lai dich vu '$serviceName'..." -ForegroundColor Cyan
try {
    Restart-Service -Name $serviceName -Force
    Write-Host "[SUCCESS] Da khoi dong lai SQL Server. Cau hinh hoan tat!" -ForegroundColor Green
} catch {
    Write-Host "[ERROR] Khong the restart service. Vui long mo 'Services' va restart 'SQL Server ($instanceName)' thu cong." -ForegroundColor Red
}

Write-Host "`nNhan Enter de thoat..."
Read-Host
