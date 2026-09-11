param([int]$AppProcessId, [switch]$Exercise)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class TourismWindowCapture {
    [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
}
'@
if ($AppProcessId -eq 0) {
    $appPath = Join-Path $PSScriptRoot '../bin/x64/Debug/net8.0-windows10.0.19041.0/win-x64/digital heritage preservation app.exe'
    $previousDataPath = $env:SERENDIB_TRAVEL_DATA_PATH
    $smokeDataPath = Join-Path $env:TEMP ('serendib-smoke-' + [guid]::NewGuid() + '.json')
    $env:SERENDIB_TRAVEL_DATA_PATH = $smokeDataPath
    try { $appProcess = Start-Process -FilePath $appPath -WindowStyle Hidden -PassThru }
    finally { $env:SERENDIB_TRAVEL_DATA_PATH = $previousDataPath }
    for ($attempt = 0; $attempt -lt 10; $attempt++) {
        Start-Sleep -Seconds 2
        $appProcess.Refresh()
        if ($appProcess.HasExited) { throw "App exited with code $($appProcess.ExitCode)." }
        if ($appProcess.MainWindowHandle -ne 0) { break }
    }
} else { $appProcess = Get-Process -Id $AppProcessId }
$handle = $appProcess.MainWindowHandle
if ($handle -eq 0) { throw 'The app has no main window.' }
$window = [System.Windows.Automation.AutomationElement]::FromHandle($handle)
function Find-Name([string]$Name) {
    $condition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::NameProperty, $Name)
    $result = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
    if ($null -eq $result) { throw "Control not found: $Name" }
    return $result
}
if ($Exercise) {
    $japan = Find-Name 'Japan'
    $japan.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Toggle()
    Start-Sleep -Milliseconds 700
    [void](Find-Name 'Tokyo')
    [void](Find-Name '3 places')
    Write-Output 'PASS: Japan tab filters the rendered catalog.'
    $settings = Find-Name 'Settings'
    $settings.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    Start-Sleep -Milliseconds 500
    $condition = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::AutomationIdProperty, 'LanguageChoice')
    $language = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
    $language.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Expand()
    Start-Sleep -Milliseconds 300
    (Find-Name '日本語').GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    Start-Sleep -Milliseconds 700
    [void](Find-Name '見つける')
    [void](Find-Name '設定')
    (Find-Name '見つける').GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    Start-Sleep -Milliseconds 500
    [void](Find-Name '3 件')
    [void](Find-Name '自然')
    Write-Output 'PASS: Japanese language updates settings, navigation and destination count.'
    $settingsLabel = '設定'
    foreach ($choice in @(@{Label='한국어';Navigation='둘러보기';Settings='설정';Count='3개 장소'}, @{Label='Русский';Navigation='Открыть мир';Settings='Настройки';Count='Мест: 3'})) {
        (Find-Name $settingsLabel).GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
        Start-Sleep -Milliseconds 300
        $language = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
        $language.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Expand()
        Start-Sleep -Milliseconds 200
        (Find-Name $choice.Label).GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
        Start-Sleep -Milliseconds 400
        (Find-Name $choice.Navigation).GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
        Start-Sleep -Milliseconds 300
        [void](Find-Name $choice.Count)
        $settingsLabel = $choice.Settings
        Write-Output ('PASS: interface language ' + $choice.Label)
    }
    (Find-Name 'Россия').GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Toggle()
    Start-Sleep -Milliseconds 400
    [void](Find-Name 'Moscow')
    $stored = Get-Content $smokeDataPath -Raw | ConvertFrom-Json
    if ($stored.Language -ne 'ru' -or $stored.Location -ne 'Russia') { throw 'UI preferences did not persist.' }
    Write-Output 'PASS: UI location and language preferences persisted to isolated storage.'
}
$elements = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
$names = $elements | ForEach-Object { $_.Current.Name } | Where-Object { $_ } | Select-Object -Unique
$names | Select-Object -First 70
$rect = New-Object TourismWindowCapture+Rect
[void][TourismWindowCapture]::GetWindowRect($handle, [ref]$rect)
$bitmap = New-Object System.Drawing.Bitmap(($rect.Right - $rect.Left), ($rect.Bottom - $rect.Top))
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$hdc = $graphics.GetHdc()
try { [void][TourismWindowCapture]::PrintWindow($handle, $hdc, 2) }
finally { $graphics.ReleaseHdc($hdc); $graphics.Dispose() }
$outputPath = Join-Path $env:TEMP 'serendib-preview.png'
$bitmap.Save($outputPath)
$bitmap.Dispose()
Write-Output $outputPath
if ($AppProcessId -eq 0) { [void]$appProcess.CloseMainWindow() }
