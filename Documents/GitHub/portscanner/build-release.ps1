# Build self-contained Windows x64 single-file executable for release.
# Run from repo root. Requires .NET 8 SDK.
# Output: PortScanner/publish/win-x64/PortScanner.exe

$ErrorActionPreference = "Stop"
$ProjectDir = Join-Path $PSScriptRoot "PortScanner"
$OutDir = Join-Path $ProjectDir "publish/win-x64"

Write-Host "Publishing PortScanner for win-x64 (self-contained, single file)..." -ForegroundColor Cyan
dotnet publish (Join-Path $ProjectDir "PortScanner.csproj") `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o $OutDir

$exe = Join-Path $OutDir "PortScanner.exe"
if (Test-Path $exe) {
  Write-Host "Done. Executable: $exe" -ForegroundColor Green
} else {
  Write-Error "PortScanner.exe was not produced."
}
