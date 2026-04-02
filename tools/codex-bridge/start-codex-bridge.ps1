$ErrorActionPreference = 'Stop'

$ws = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$codexExe = Join-Path $ws 'codex-local.exe'
$bridge = Join-Path $PSScriptRoot 'codex-bridge.ps1'

# Stale bridge processlerini temizle
$self = $PID
$old = Get-CimInstance Win32_Process -ErrorAction SilentlyContinue |
  Where-Object {
    $_.ProcessId -ne $self -and
    $_.CommandLine -and
    $_.CommandLine -like '*codex-bridge.ps1*'
  }

foreach ($p in $old) {
  try { Stop-Process -Id $p.ProcessId -Force -ErrorAction Stop } catch {}
}

Write-Host 'Codex bridge baslatiliyor...'
Write-Host ('Workspace: ' + $ws)
Write-Host ('Codex exe: ' + $codexExe)

& $bridge `
  -Port 4317 `
  -WorkspaceRoot $ws `
  -CodexExePath $codexExe `
  -Model 'gpt-5.4'
