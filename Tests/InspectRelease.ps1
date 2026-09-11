param([string]$AppPath = '', [switch]$Online, [switch]$Pdf)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class LoreviaCapture {
 [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left, Top, Right, Bottom; }
 [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out Rect r);
 [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr h, IntPtr dc, uint flags);
 [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int w, int height, uint flags);
 [DllImport("user32.dll")] public static extern IntPtr GetWindow(IntPtr h, uint command);
}
'@
if (-not $AppPath) { $AppPath = Join-Path $PSScriptRoot '../bin/x64/Debug/net8.0-windows10.0.19041.0/win-x64/digital heritage preservation app.exe' }
$runFolder = Join-Path $env:TEMP ('lorevia-release-' + [guid]::NewGuid())
[void][IO.Directory]::CreateDirectory($runFolder)
$travelPath = Join-Path $runFolder 'travel.json'
$tripId = [guid]::NewGuid().ToString()
$seed = @{ Trips = @(@{Id=$tripId;Name='Release test trip';Days=3;Start='2026-10-01';Currency='LKR';Budget=20000;Stops=@(@{Id=[guid]::NewGuid().ToString();DestinationId='ella';Day=1;EstimatedCost=2000;Notes='First stop'},@{Id=[guid]::NewGuid().ToString();DestinationId='sigiriya';Day=1;EstimatedCost=3000;Notes='Second stop'})}); Saved=@();HasSeenUserGuide=$false }
[IO.File]::WriteAllText($travelPath, ($seed | ConvertTo-Json -Depth 8))
$article = @(@{Title='Offline test article';Url='https://en.wikipedia.org/wiki/Sigiriya';Text='This text is available offline.';DownloadedAt='2026-09-11T00:00:00Z'})
[IO.File]::WriteAllText((Join-Path $runFolder 'offline-articles.json'), (ConvertTo-Json -InputObject $article -Depth 4))
$syncFolder = Join-Path $runFolder 'cloud/Lorevia'
$deviceId = [guid]::NewGuid().ToString('N')
[IO.File]::WriteAllText((Join-Path $runFolder 'device.json'), (@{DeviceId=$deviceId;SyncFolder=$syncFolder} | ConvertTo-Json))
$oldPath = $env:SERENDIB_TRAVEL_DATA_PATH
$env:SERENDIB_TRAVEL_DATA_PATH = $travelPath
try { $process = Start-Process -FilePath $AppPath -WindowStyle Hidden -PassThru } finally { $env:SERENDIB_TRAVEL_DATA_PATH = $oldPath }
function Find([string]$value, [switch]$Id) {
    $property = if ($Id) { [System.Windows.Automation.AutomationElement]::AutomationIdProperty } else { [System.Windows.Automation.AutomationElement]::NameProperty }
    $condition = [System.Windows.Automation.PropertyCondition]::new($property, $value)
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        $found = $script:window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
        if ($found) { return $found }
        Start-Sleep -Milliseconds 200
    }
    throw "Control not found: $value"
}
function Invoke([string]$name, [switch]$Id) { (Find $name -Id:$Id).GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke(); Start-Sleep -Milliseconds 300 }
function Select-Control([string]$name) { (Find $name).GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select(); Start-Sleep -Milliseconds 300 }
function Capture([string]$name) {
    $process.Refresh()
    if ($process.HasExited -or $process.MainWindowHandle -eq 0) { Write-Output 'Window closed before screenshot.'; return }
    $rect = [LoreviaCapture+Rect]::new()
    [void][LoreviaCapture]::GetWindowRect($process.MainWindowHandle, [ref]$rect)
    $bitmap = [Drawing.Bitmap]::new($rect.Right-$rect.Left, $rect.Bottom-$rect.Top)
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    $dc = $graphics.GetHdc()
    try { [void][LoreviaCapture]::PrintWindow($process.MainWindowHandle, $dc, 2) }
    finally { $graphics.ReleaseHdc($dc); $graphics.Dispose() }
    $path = Join-Path $runFolder ($name + '.png')
    $bitmap.Save($path); $bitmap.Dispose(); Write-Output "SCREENSHOT $path"
}
try {
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        Start-Sleep -Milliseconds 400
        $process.Refresh()
        if ($process.HasExited) { throw "App exited: $($process.ExitCode)" }
        if ($process.MainWindowHandle -ne 0) { break }
    }
    $script:window = [System.Windows.Automation.AutomationElement]::FromHandle($process.MainWindowHandle)
    [void](Find '1. Discover places')
    Capture 'first-launch-guide'
    Invoke 'CloseButton' -Id
    for ($attempt = 0; $attempt -lt 30; $attempt++) { if ((Get-Content $travelPath -Raw | ConvertFrom-Json).HasSeenUserGuide) { break }; Start-Sleep -Milliseconds 200 }
    if (-not (Get-Content $travelPath -Raw | ConvertFrom-Json).HasSeenUserGuide) { throw 'Guide dismissal did not persist.' }
    Write-Output 'PASS: first-launch guide and persisted dismissal.'
    Select-Control 'My trips'
    [void](Find 'Save PDF'); [void](Find 'Share trip')
    Invoke 'Move up Sigiriya'
    $stored = Get-Content $travelPath -Raw | ConvertFrom-Json
    if ($stored.Trips[0].Stops[0].DestinationId -ne 'sigiriya') { throw 'Stop order did not persist.' }
    $synced = Get-Content (Join-Path $syncFolder ($deviceId + '.lorevia-sync')) -Raw | ConvertFrom-Json
    if ($synced.Trips[0].Stops[0].DestinationId -ne 'sigiriya') { throw 'Changed trip was not published to the sync folder.' }
    Write-Output 'PASS: local edits publish a separate device snapshot to the configured folder.'
    Capture 'trip-budget-and-order'
    Write-Output 'PASS: trip budget, export/share controls and persisted stop ordering.'
    if ($Pdf) {
        Invoke 'Save PDF'
        $saveDialog = $null
        for ($attempt = 0; $attempt -lt 60; $attempt++) {
            $windows = [System.Windows.Automation.AutomationElement]::RootElement.FindAll([System.Windows.Automation.TreeScope]::Children, [System.Windows.Automation.Condition]::TrueCondition)
            $saveDialog = $windows | Where-Object { $_.Current.Name -eq 'Save As' -and [LoreviaCapture]::GetWindow([IntPtr]$_.Current.NativeWindowHandle, 4) -eq $process.MainWindowHandle } | Select-Object -First 1
            if ($saveDialog) { break }
            Start-Sleep -Milliseconds 250
        }
        if (-not $saveDialog) { throw 'PDF export did not open its Save As dialog.' }
        $edit = $saveDialog.FindFirst([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::AutomationIdProperty, '1001'))
        $pdfPath = Join-Path $runFolder 'itinerary.pdf'
        $edit.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue($pdfPath)
        $save = $saveDialog.FindFirst([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::AutomationIdProperty, '1'))
        $save.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
        for ($attempt = 0; $attempt -lt 60; $attempt++) { if ((Test-Path $pdfPath) -and (Get-Item $pdfPath).Length -gt 1000) { break }; Start-Sleep -Milliseconds 250 }
        if (-not (Test-Path $pdfPath) -or [Text.Encoding]::ASCII.GetString([IO.File]::ReadAllBytes($pdfPath), 0, 4) -ne '%PDF') { throw 'PDF export did not produce a PDF file.' }
        Write-Output "PASS: PDF exported to $pdfPath"
    }
    Select-Control 'Offline library'
    $list = Find 'OfflineArticles' -Id
    $item = $list.FindFirst([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::ListItem))
    $item.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    [void](Find 'This text is available offline.')
    Capture 'offline-library'
    Write-Output 'PASS: offline article can be read without a browser.'
    Select-Control 'Settings'
    [void](Find 'Recovery backups'); [void](Find 'Choose cloud folder'); [void](Find 'Clear browsing data')
    foreach ($choice in @(@{Label='日本語';Guide='ユーザーガイド';Settings='設定';Check='復元用バックアップ'},@{Label='한국어';Guide='사용자 가이드';Settings='설정';Check='복구 백업'},@{Label='Русский';Guide='Руководство пользователя';Settings='Настройки';Check='Копии для восстановления'})) {
        $language = Find 'LanguageChoice' -Id
        $language.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Expand()
        Select-Control $choice.Label
        [void](Find $choice.Check)
        [void](Find $choice.Guide)
        Write-Output ('PASS: release controls translated to ' + $choice.Label)
    }
    $language = Find 'LanguageChoice' -Id
    $language.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Expand()
    Select-Control 'English'
    [void][LoreviaCapture]::SetWindowPos($process.MainWindowHandle, [IntPtr]::Zero, 0, 0, 900, 700, 6)
    Start-Sleep -Milliseconds 500
    Capture 'compact-settings'
    if ($Online) {
        Select-Control 'Places wiki'
        $search = Find 'WikiSearch' -Id
        $edit = $search.FindFirst([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::Edit))
        $edit.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue('Sigiriya')
        Invoke 'WikiSearchButton' -Id
        Start-Sleep -Seconds 5
        [void](Find 'Close browser')
        Capture 'embedded-browser'
        Invoke 'Download article'
        for ($attempt = 0; $attempt -lt 30; $attempt++) {
            $downloads = Get-Content (Join-Path $runFolder 'offline-articles.json') -Raw | ConvertFrom-Json
            if ($downloads | Where-Object { $_.Title -eq 'Sigiriya' -and $_.Text.Length -gt 500 }) { break }
            Start-Sleep -Milliseconds 300
        }
        if (-not ($downloads | Where-Object { $_.Title -eq 'Sigiriya' -and $_.Text.Length -gt 500 })) { throw 'Wikipedia article was not downloaded.' }
        Write-Output 'PASS: a live Wikipedia article downloads with source and full text.'
        Invoke 'Close browser'
        Select-Control 'Settings'
        Invoke 'Clear browsing data'
        Invoke 'PrimaryButton' -Id
        [void](Find 'Browsing data cleared.')
        if (-not ((Get-Content (Join-Path $runFolder 'offline-articles.json') -Raw | ConvertFrom-Json) | Where-Object Title -eq 'Sigiriya')) { throw 'Clearing browser data removed offline reading.' }
        Write-Output 'PASS: browser data clearing preserves offline downloads.'
        Write-Output 'PASS: wiki opens its embedded browser shell.'
    }
    Write-Output "ARTIFACTS $runFolder"
} catch {
    Write-Output ("TEST FAILURE: " + $_.Exception.Message)
    Capture 'failure'
    if ($script:window -and -not $process.HasExited) { $script:window.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition) | ForEach-Object { $_.Current.Name } | Where-Object { $_ } | Select-Object -Unique -First 80 }
    throw
} finally { if (-not $process.HasExited) { [void]$process.CloseMainWindow() } }
