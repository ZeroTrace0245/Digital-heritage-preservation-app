param([string]$Version = '1.0.0', [string]$Compiler = '', [switch]$SkipPublish)
$ErrorActionPreference = 'Stop'
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if ($Version -notmatch '^\d+\.\d+\.\d+$') { throw 'Use a numeric major.minor.patch version.' }
if (-not $Compiler) {
    $candidates = @("${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe", "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe")
    $Compiler = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}
if (-not $Compiler) { throw 'Install Inno Setup 6, or pass -Compiler with the path to ISCC.exe.' }
$publish = Join-Path $repo 'artifacts/publish/win-x64'
if (-not $SkipPublish) {
    & dotnet publish (Join-Path $repo 'digital heritage preservation app.csproj') -c Release -p:Platform=x64 -p:Version=$Version -o $publish
    if ($LASTEXITCODE -ne 0) { throw 'Publish failed.' }
}
$toolsDir = Join-Path $repo 'artifacts/tools'
[void][IO.Directory]::CreateDirectory($toolsDir)
$webview = Join-Path $toolsDir 'MicrosoftEdgeWebview2Setup.exe'
if (-not (Test-Path -LiteralPath $webview)) { Invoke-WebRequest 'https://go.microsoft.com/fwlink/p/?LinkId=2124703' -OutFile $webview }
$signature = Get-AuthenticodeSignature -FilePath $webview
if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notmatch 'O=Microsoft Corporation') { throw 'WebView2 bootstrapper signature validation failed.' }
& $Compiler "/DAppVersion=$Version" "/DPublishDir=$publish" (Join-Path $PSScriptRoot 'Lorevia.iss')
if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed.' }
$installer = Join-Path $repo "artifacts/installer/Lorevia-Setup-$Version-x64.exe"
$hash = (Get-FileHash -LiteralPath $installer -Algorithm SHA256).Hash.ToLowerInvariant()
[IO.File]::WriteAllText($installer + '.sha256', "$hash  $([IO.Path]::GetFileName($installer))`n")
Write-Output "Built: $installer"
Write-Output 'Development installer is unsigned. Apply your trusted code-signing certificate before public distribution, then regenerate its SHA256 file.'
