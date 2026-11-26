<#
.SYNOPSIS
    Testlarni ishga tushiradi va o'rnatuvchini yig'adi: dist\TaqvimWidget-Setup.exe
.EXAMPLE
    powershell -ExecutionPolicy Bypass -File .\build.ps1 -Version 2.1.0
#>
param(
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version = '2.0.0',
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$dist = Join-Path $root 'dist'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw ".NET SDK topilmadi (8.0 yoki yangiroq): https://dotnet.microsoft.com/download"
}

function Invoke-Dotnet {
    & dotnet @args
    if ($LASTEXITCODE -ne 0) { throw "dotnet $($args[0]) bajarilmadi" }
}

if (-not $SkipTests) {
    Invoke-Dotnet test (Join-Path $root 'tests\TaqvimWidget.Core.Tests') -c Release --nologo "-p:Version=$Version"
}

Invoke-Dotnet build (Join-Path $root 'src\TaqvimWidget.Setup') -c Release --nologo "-p:Version=$Version"

New-Item -ItemType Directory -Force -Path $dist | Out-Null
$setup = Join-Path $dist 'TaqvimWidget-Setup.exe'
Copy-Item (Join-Path $root 'src\TaqvimWidget.Setup\bin\Release\net48\TaqvimWidget-Setup.exe') $setup -Force

$hash = (Get-FileHash $setup -Algorithm SHA256).Hash.ToLowerInvariant()
Set-Content -Path "$setup.sha256" -Value "$hash  TaqvimWidget-Setup.exe" -Encoding ASCII

Write-Host ''
Write-Host ('Tayyor: {0} ({1:N0} KB)' -f $setup, ((Get-Item $setup).Length / 1KB)) -ForegroundColor Green
