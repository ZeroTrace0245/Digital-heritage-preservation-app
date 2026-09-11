param([string]$Installer = '')
$ErrorActionPreference = 'Stop'
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if (-not $Installer) { $Installer = Join-Path $repo 'artifacts/installer/Lorevia-Setup-1.0.0-x64.exe' }
$registry = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\{AB66B167-DA6D-4799-84E1-216E7A1046BD}_is1'
if (Test-Path $registry) { throw 'Lorevia is already installed for this user. Run this test in Windows Sandbox or another Windows account.' }
$installRoot = [IO.Path]::GetFullPath((Join-Path $repo ('artifacts/install-check-' + [guid]::NewGuid())))
if (-not $installRoot.StartsWith((Join-Path $repo 'artifacts') + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw 'Test install path is outside the workspace artifacts directory.' }
$dataFiles = @((Join-Path $env:LOCALAPPDATA 'DigitalHeritage/travel.json'), (Join-Path $env:LOCALAPPDATA 'DigitalHeritage/archive.json'))
$hashes = @{}
foreach ($file in $dataFiles) { if (Test-Path -LiteralPath $file) { $hashes[$file] = (Get-FileHash -LiteralPath $file).Hash } }
try {
    foreach ($phase in @('install', 'upgrade')) {
        $arguments = @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/NOICONS', '/TASKS=', ('/DIR="' + $installRoot + '"'), ('/LOG="' + (Join-Path $repo "artifacts/$phase.log") + '"'))
        $process = Start-Process -FilePath $Installer -ArgumentList $arguments -WindowStyle Hidden -Wait -PassThru
        if ($process.ExitCode -notin @(0, 3010)) { throw "Installer $phase failed with code $($process.ExitCode)." }
        foreach ($relative in @('digital heritage preservation app.exe', 'Desktop/TourismWindow.xbf', 'App.xbf', 'MainWindow.xbf', 'PRIVACY.md', 'CONTENT-CREDITS.md', 'Assets/Lorevia.ico', 'Assets/Travel/sigiriya.jpg')) {
            if (-not (Test-Path -LiteralPath (Join-Path $installRoot $relative))) { throw "Missing installed file: $relative" }
        }
        Write-Output "PASS: $phase completed with all required content."
    }
    & (Join-Path $repo 'Tests/InspectRelease.ps1') -AppPath (Join-Path $installRoot 'digital heritage preservation app.exe')
} finally {
    $uninstaller = Join-Path $installRoot 'unins000.exe'
    if (Test-Path -LiteralPath $uninstaller) {
        $process = Start-Process -FilePath $uninstaller -ArgumentList '/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART' -WindowStyle Hidden -Wait -PassThru
        if ($process.ExitCode -ne 0) { throw "Uninstall failed: $($process.ExitCode)" }
    }
}
if (Test-Path -LiteralPath (Join-Path $installRoot 'digital heritage preservation app.exe')) { throw 'Uninstall left the application executable behind.' }
foreach ($file in $hashes.Keys) { if (-not (Test-Path -LiteralPath $file) -or (Get-FileHash -LiteralPath $file).Hash -ne $hashes[$file]) { throw 'Existing personal data changed during installer testing.' } }
Write-Output 'PASS: uninstall removed the app and preserved existing personal data.'
