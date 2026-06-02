#!/usr/bin/env pwsh
# OpenSourceHub v1.1.5 Build & Publish Script
# Publisher: XinPari Software

param(
    [string]$Version = "1.1.5",
    [string]$OutputDir = ".\release"
)

$ErrorActionPreference = "Stop"
$Root = Split-Path $PSScriptRoot -Parent
$ProjectPath = Join-Path $Root "src\OpenSourceHub.UI\OpenSourceHub.UI.csproj"

Write-Host "OpenSourceHub Build Script v$Version" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan

# Clean
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path $OutputDir) { Remove-Item $OutputDir -Recurse -Force }
New-Item -ItemType Directory -Path $OutputDir | Out-Null

# Run Tests
Write-Host "Running tests..." -ForegroundColor Yellow
dotnet test "$Root\tests\OpenSourceHub.Tests\OpenSourceHub.Tests.csproj" --verbosity minimal
if ($LASTEXITCODE -ne 0) { Write-Error "Tests failed! Aborting build."; exit 1 }
Write-Host "All tests passed!" -ForegroundColor Green

# Publish Portable (self-contained)
Write-Host "Publishing portable build..." -ForegroundColor Yellow
$PortableDir = Join-Path $OutputDir "portable"
dotnet publish $ProjectPath `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:Version=$Version `
    -o $PortableDir

# Create portable ZIP
Write-Host "Creating portable ZIP..." -ForegroundColor Yellow
$ZipPath = Join-Path $OutputDir "OpenSourceHub-v$Version-portable.zip"
Compress-Archive -Path "$PortableDir\*" -DestinationPath $ZipPath
Write-Host "Portable ZIP created: $ZipPath" -ForegroundColor Green

# Publish Framework-Dependent (smaller)
Write-Host "Publishing framework-dependent build..." -ForegroundColor Yellow
$FxDir = Join-Path $OutputDir "framework-dependent"
dotnet publish $ProjectPath `
    -c Release `
    -r win-x64 `
    --self-contained false `
    -p:Version=$Version `
    -o $FxDir

Write-Host ""
Write-Host "Build complete!" -ForegroundColor Green
Write-Host "Output: $OutputDir" -ForegroundColor Cyan
Write-Host ""
Write-Host "Release assets:" -ForegroundColor White
Get-ChildItem $OutputDir -File | ForEach-Object {
    $size = [math]::Round($_.Length / 1MB, 1)
    Write-Host "  $($_.Name) ($size MB)" -ForegroundColor Gray
}
