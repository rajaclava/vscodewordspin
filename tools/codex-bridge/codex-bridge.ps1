param(
  [int]$Port = 4317,
  [string]$WorkspaceRoot = (Get-Location).Path,
  [string]$CodexExePath = (Join-Path (Split-Path -Parent $PSScriptRoot) '..\codex-local.exe'),
  [string]$CodexHome = (Join-Path $env:USERPROFILE '.codex'),
  [string]$Model = 'gpt-5.4'
)

$ErrorActionPreference = 'Stop'

function Read-TextFile {
  param([Parameter(Mandatory=$true)][string]$Path)
  if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return '' }
  $bytes = [System.IO.File]::ReadAllBytes($Path)
  if (-not $bytes -or $bytes.Length -eq 0) { return '' }

  $utf8Strict = [System.Text.UTF8Encoding]::new($false, $true)
  try {
    return $utf8Strict.GetString($bytes)
  } catch {
    return [System.Text.Encoding]::Default.GetString($bytes)
  }
}

function Write-JsonResponse {
  param(
    [Parameter(Mandatory=$true)]$Context,
    [int]$Status = 200,
    [Parameter(Mandatory=$true)]$Body
  )
  $json = $Body | ConvertTo-Json -Depth 8 -Compress
  $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
  $resp = $Context.Response
  $resp.StatusCode = $Status
  $resp.ContentType = 'application/json; charset=utf-8'
  $resp.Headers['Access-Control-Allow-Origin'] = '*'
  $resp.Headers['Access-Control-Allow-Headers'] = 'Content-Type'
  $resp.Headers['Access-Control-Allow-Methods'] = 'GET,POST,OPTIONS'
  $resp.ContentLength64 = $bytes.Length
  $resp.OutputStream.Write($bytes, 0, $bytes.Length)
  $resp.OutputStream.Close()
}

function Read-JsonBody {
  param([Parameter(Mandatory=$true)]$Request)
  if (-not $Request.HasEntityBody) { return @{} }
  $reader = [System.IO.StreamReader]::new($Request.InputStream, $Request.ContentEncoding)
  $raw = $reader.ReadToEnd()
  $reader.Close()
  if ([string]::IsNullOrWhiteSpace($raw)) { return @{} }
  try { return ($raw | ConvertFrom-Json) } catch { return @{} }
}

function Get-SessionIdFromText {
  param([string]$Text)
  if ([string]::IsNullOrWhiteSpace($Text)) { return '' }
  $m = [regex]::Match($Text.ToLowerInvariant(), '[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}')
  if ($m.Success) { return $m.Value }
  return ''
}

function Resolve-AllowedPath {
  param([string]$Path)
  if ([string]::IsNullOrWhiteSpace($Path)) { throw 'Bos path' }
  $resolved = (Resolve-Path -LiteralPath $Path -ErrorAction Stop).ProviderPath
  $roots = @($WorkspaceRoot, $CodexHome) | Where-Object { Test-Path -LiteralPath $_ }
  foreach ($r in $roots) {
    $rr = (Resolve-Path -LiteralPath $r).ProviderPath
    if ($resolved.StartsWith($rr, [System.StringComparison]::OrdinalIgnoreCase)) {
      return $resolved
    }
  }
  throw 'Path izinli degil'
}

function Resolve-MaybeRelative {
  param(
    [string]$Candidate,
    [string]$Workspace
  )
  if ([string]::IsNullOrWhiteSpace($Candidate)) { return $null }
  $c = $Candidate.Trim().Trim('"').Trim("'")
  if ($c -match '^https?://') { return $null }
  if ($c -notmatch '\.md$') { return $null }

  $full = $null
  if ($c -match '^[A-Za-z]:\\') {
    $full = $c
  } else {
    if ([string]::IsNullOrWhiteSpace($Workspace)) { return $null }
    $full = Join-Path $Workspace ($c -replace '/', '\')
  }

  if (-not (Test-Path -LiteralPath $full -PathType Leaf)) { return $null }
  return (Resolve-Path -LiteralPath $full).ProviderPath
}

function Find-SessionFile {
  param([string]$SessionId)
  $root = Join-Path $CodexHome 'sessions'
  if (-not (Test-Path -LiteralPath $root -PathType Container)) { return $null }
  $f = Get-ChildItem -LiteralPath $root -Recurse -File -Filter "*$SessionId*.jsonl" -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
  return $f
}

function Find-LatestSessionFile {
  $root = Join-Path $CodexHome 'sessions'
  if (-not (Test-Path -LiteralPath $root -PathType Container)) { return $null }
  $f = Get-ChildItem -LiteralPath $root -Recurse -File -Filter "rollout-*.jsonl" -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
  return $f
}

function Get-SessionMeta {
  param([string]$SessionFile)
  $meta = [pscustomobject]@{
    sessionId = ''
    cwd = ''
  }
  $lines = Get-Content -LiteralPath $SessionFile -Encoding UTF8 -TotalCount 50 -ErrorAction SilentlyContinue
  foreach ($ln in $lines) {
    try {
      $j = $ln | ConvertFrom-Json
      if ($j.type -eq 'session_meta') {
        if ($j.payload.id) { $meta.sessionId = [string]$j.payload.id }
        if ($j.payload.cwd) { $meta.cwd = [string]$j.payload.cwd }
        break
      }
    } catch {}
  }
  return $meta
}

function Get-MdFilesFromSession {
  param(
    [string]$SessionFile,
    [string]$Workspace
  )
  $set = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
  $absRx = [regex]'[A-Za-z]:\\[^"''\r\n]*?\.md'
  $relRx = [regex]'(?<![A-Za-z]:)(?:\.{0,2}[\\/])?[A-Za-z0-9_\-./\\]+\.md'
  $patchRx = [regex]'\*\*\* (?:Add|Update) File: ([^\r\n*]+\.md)'

  foreach ($ln in (Get-Content -LiteralPath $SessionFile -Encoding UTF8 -ErrorAction SilentlyContinue)) {
    if ($ln -notmatch '\.md') { continue }

    foreach ($m in $absRx.Matches($ln)) {
      $p = Resolve-MaybeRelative -Candidate $m.Value -Workspace $Workspace
      if ($p) { [void]$set.Add($p) }
    }
    foreach ($m in $relRx.Matches($ln)) {
      $p = Resolve-MaybeRelative -Candidate $m.Value -Workspace $Workspace
      if ($p) { [void]$set.Add($p) }
    }
    foreach ($m in $patchRx.Matches($ln)) {
      $p = Resolve-MaybeRelative -Candidate $m.Groups[1].Value -Workspace $Workspace
      if ($p) { [void]$set.Add($p) }
    }
  }

  $items = @()
  foreach ($p in $set) {
    try {
      $fi = Get-Item -LiteralPath $p -ErrorAction Stop
      $items += [pscustomobject]@{
        path = $p
        name = $fi.Name
        size = [int64]$fi.Length
        mtime = $fi.LastWriteTime.ToString('yyyy-MM-dd HH:mm:ss')
      }
    } catch {}
  }

  if (-not $items -and (Test-Path -LiteralPath $Workspace -PathType Container)) {
    $fallback = Get-ChildItem -LiteralPath $Workspace -Recurse -File -Filter *.md -ErrorAction SilentlyContinue |
      Sort-Object LastWriteTime -Descending | Select-Object -First 50
    foreach ($fi in $fallback) {
      $items += [pscustomobject]@{
        path = $fi.FullName
        name = $fi.Name
        size = [int64]$fi.Length
        mtime = $fi.LastWriteTime.ToString('yyyy-MM-dd HH:mm:ss')
      }
    }
  }

  return $items | Sort-Object mtime -Descending
}

function Invoke-Codex {
  param(
    [Parameter(Mandatory=$true)][string]$Prompt,
    [string]$SessionId = ''
  )

  if (-not (Test-Path -LiteralPath $CodexExePath -PathType Leaf)) {
    throw "codex exe bulunamadi: $CodexExePath"
  }
  if ([string]::IsNullOrWhiteSpace($Prompt)) {
    throw 'Prompt bos olamaz'
  }

  $outFile = Join-Path $env:TEMP ("codex_bridge_" + [guid]::NewGuid().ToString('N') + ".txt")
  function Quote-Arg {
    param([string]$s)
    $val = if ($null -eq $s) { '' } else { [string]$s }
    return '"' + ($val -replace '"', '\"') + '"'
  }

  $argLine = ''

  if ([string]::IsNullOrWhiteSpace($SessionId)) {
    $argLine = 'exec -m ' + (Quote-Arg $Model) +
      ' --skip-git-repo-check -C ' + (Quote-Arg $WorkspaceRoot) +
      ' -o ' + (Quote-Arg $outFile) + ' ' + (Quote-Arg $Prompt)
  } else {
    $argLine = 'exec resume ' + (Quote-Arg $SessionId) +
      ' -m ' + (Quote-Arg $Model) +
      ' --skip-git-repo-check' +
      ' -o ' + (Quote-Arg $outFile) + ' ' + (Quote-Arg $Prompt)
  }

  $tmpStdOut = Join-Path $env:TEMP ("codex_bridge_stdout_" + [guid]::NewGuid().ToString('N') + ".log")
  $tmpStdErr = Join-Path $env:TEMP ("codex_bridge_stderr_" + [guid]::NewGuid().ToString('N') + ".log")
  $p = Start-Process -FilePath $CodexExePath -ArgumentList $argLine -PassThru -Wait -NoNewWindow `
    -RedirectStandardOutput $tmpStdOut -RedirectStandardError $tmpStdErr
  $code = $p.ExitCode
  if ($code -ne 0) {
    $errText = ''
    if (Test-Path -LiteralPath $tmpStdErr) {
      $errText = (Read-TextFile -Path $tmpStdErr).Trim()
    }
    if (Test-Path -LiteralPath $tmpStdOut) { Remove-Item -LiteralPath $tmpStdOut -Force -ErrorAction SilentlyContinue }
    if (Test-Path -LiteralPath $tmpStdErr) { Remove-Item -LiteralPath $tmpStdErr -Force -ErrorAction SilentlyContinue }
    if (Test-Path -LiteralPath $outFile) { Remove-Item -LiteralPath $outFile -Force -ErrorAction SilentlyContinue }
    if ($errText) {
      $first = ($errText -split "`r?`n" | Select-Object -First 1)
      throw "codex komutu basarisiz (exit=$code): $first"
    }
    throw "codex komutu basarisiz (exit=$code)"
  }
  if (Test-Path -LiteralPath $tmpStdOut) { Remove-Item -LiteralPath $tmpStdOut -Force -ErrorAction SilentlyContinue }
  if (Test-Path -LiteralPath $tmpStdErr) { Remove-Item -LiteralPath $tmpStdErr -Force -ErrorAction SilentlyContinue }

  if (-not (Test-Path -LiteralPath $outFile -PathType Leaf)) {
    throw 'codex cikti dosyasi olusmadi'
  }

  $txt = (Read-TextFile -Path $outFile).Trim()
  Remove-Item -LiteralPath $outFile -Force -ErrorAction SilentlyContinue
  if ([string]::IsNullOrWhiteSpace($txt)) { throw 'codex bos cikti dondu' }
  return $txt
}

$listener = [System.Net.HttpListener]::new()
$prefix = "http://127.0.0.1:$Port/"
$listener.Prefixes.Add($prefix)
$listener.Start()

Write-Host "Codex bridge acildi: $prefix"
Write-Host "Workspace: $WorkspaceRoot"
Write-Host "Codex exe: $CodexExePath"
Write-Host "Model: $Model"

try {
  while ($listener.IsListening) {
    $ctx = $listener.GetContext()
    $req = $ctx.Request
    $path = $req.Url.AbsolutePath.ToLowerInvariant()

    try {
      if ($req.HttpMethod -eq 'OPTIONS') {
        Write-JsonResponse -Context $ctx -Status 200 -Body @{ok=$true}
        continue
      }

      if ($path -eq '/health' -and $req.HttpMethod -eq 'GET') {
        Write-JsonResponse -Context $ctx -Body @{
          ok = $true
          hasCodex = (Test-Path -LiteralPath $CodexExePath -PathType Leaf)
          codexPath = $CodexExePath
          model = $Model
          workspaceRoot = $WorkspaceRoot
        }
        continue
      }

      if ($path -eq '/api/ai' -and $req.HttpMethod -eq 'POST') {
        $b = Read-JsonBody -Request $req
        $prompt = [string]$b.prompt
        $sessionId = ''
        if ($b.resumeSession -eq $true) {
          if ($b.sessionId) { $sessionId = [string]$b.sessionId }
          if (-not $sessionId -and $b.sessionLink) { $sessionId = Get-SessionIdFromText -Text ([string]$b.sessionLink) }
        }
        $txt = Invoke-Codex -Prompt $prompt -SessionId $sessionId
        Write-JsonResponse -Context $ctx -Body @{ok=$true; text=$txt; sessionId=$sessionId}
        continue
      }

      if ($path -eq '/api/session/md-files' -and $req.HttpMethod -eq 'POST') {
        $b = Read-JsonBody -Request $req
        $sessionId = ''
        if ($b.sessionId) { $sessionId = [string]$b.sessionId }
        if (-not $sessionId -and $b.sessionLink) { $sessionId = Get-SessionIdFromText -Text ([string]$b.sessionLink) }
        $sf = $null
        if ($sessionId) {
          $sf = Find-SessionFile -SessionId $sessionId
          if (-not $sf) { throw "Session dosyasi bulunamadi: $sessionId" }
        } else {
          $sf = Find-LatestSessionFile
          if (-not $sf) { throw 'Session dosyasi bulunamadi (son oturum da yok)' }
        }
        $meta = Get-SessionMeta -SessionFile $sf.FullName
        if (-not $sessionId) {
          if ($meta.sessionId) {
            $sessionId = [string]$meta.sessionId
          } elseif ($sf.Name -match '^rollout-(.+)\.jsonl$') {
            $sessionId = [string]$Matches[1]
          }
        }
        $workspace = if ($meta.cwd) { $meta.cwd } else { $WorkspaceRoot }
        $files = Get-MdFilesFromSession -SessionFile $sf.FullName -Workspace $workspace

        Write-JsonResponse -Context $ctx -Body @{
          ok = $true
          sessionId = $sessionId
          sessionFile = $sf.FullName
          workspace = $workspace
          files = $files
        }
        continue
      }

      if ($path -eq '/api/session/read-file' -and $req.HttpMethod -eq 'POST') {
        $b = Read-JsonBody -Request $req
        $p = Resolve-AllowedPath -Path ([string]$b.path)
        $txt = Read-TextFile -Path $p
        Write-JsonResponse -Context $ctx -Body @{ok=$true; path=$p; text=$txt}
        continue
      }

      if ($path -eq '/api/session/progress' -and $req.HttpMethod -eq 'POST') {
        $b = Read-JsonBody -Request $req
        $sessionId = ''
        if ($b.sessionId) { $sessionId = [string]$b.sessionId }
        if (-not $sessionId -and $b.sessionLink) { $sessionId = Get-SessionIdFromText -Text ([string]$b.sessionLink) }
        if (-not $sessionId) {
          $sf = Find-LatestSessionFile
          if ($sf) {
            $meta = Get-SessionMeta -SessionFile $sf.FullName
            if ($meta.sessionId) {
              $sessionId = [string]$meta.sessionId
            } elseif ($sf.Name -match '^rollout-(.+)\.jsonl$') {
              $sessionId = [string]$Matches[1]
            }
          }
        }
        if (-not $sessionId) { throw 'Session id bulunamadi ve son oturum tespit edilemedi' }

        $prompt = [string]$b.prompt
        if ([string]::IsNullOrWhiteSpace($prompt)) {
          $prompt = 'Bu oturuma gore mevcut ilerlemeyi 3 maddede ozetle ve sonraki 3 adimi yaz.'
        }
        $txt = Invoke-Codex -Prompt $prompt -SessionId $sessionId
        Write-JsonResponse -Context $ctx -Body @{ok=$true; sessionId=$sessionId; text=$txt}
        continue
      }

      Write-JsonResponse -Context $ctx -Status 404 -Body @{ok=$false; error='Route bulunamadi'}
    } catch {
      Write-JsonResponse -Context $ctx -Status 500 -Body @{ok=$false; error=$_.Exception.Message}
    }
  }
} finally {
  if ($listener.IsListening) { $listener.Stop() }
  $listener.Close()
}
