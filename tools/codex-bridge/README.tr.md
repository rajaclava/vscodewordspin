# Codex Bridge Kurulum (API key olmadan)

Bu bridge, `wordspin-tracker-v2.html` dosyasinin tarayicidan yerel Codex 5.4'e baglanmasini saglar.

## 1) codex exe'yi proje icine kopyala

PowerShell:

```powershell
$src = (Get-Command codex).Source
Copy-Item -LiteralPath $src -Destination "C:\Users\freed\Documents\New project\codex-local.exe" -Force
```

Not: WindowsApps altindan dogrudan calistirma bazen `Access denied` verebilir. Bu yuzden kopya exe kullaniliyor.

## 2) Bridge'i baslat

Proje kokunde:

```powershell
powershell -ExecutionPolicy Bypass -File ".\tools\codex-bridge\codex-bridge.ps1" `
  -Port 4317 `
  -WorkspaceRoot "C:\Users\freed\Documents\New project" `
  -CodexExePath "C:\Users\freed\Documents\New project\codex-local.exe" `
  -Model "gpt-5.4"
```

Kisa yol:

```powershell
powershell -ExecutionPolicy Bypass -File ".\tools\codex-bridge\start-codex-bridge.ps1"
```

Bu script eski bridge sureclerini otomatik kapatip temiz baslatir.

Basariliysa terminalde su satirlar gelir:

- `Codex bridge acildi: http://127.0.0.1:4317/`
- `Model: gpt-5.4`

## 3) HTML panel ayari

1. `wordspin-tracker-v2.html` dosyasini ac.
2. Ayarlar > `AI Saglayici` olarak `Codex Local (API key yok)` sec.
3. Ayarlar > `Codex Bridge URL` alanina `http://127.0.0.1:4317` yaz.
4. `Bridge Test` butonuna bas.

## 4) Konusma linkinden .md dosya yukleme

1. Ayarlar > `Codex Konusma Dosyalari` alanina konusma linkini veya session id'yi yapistir.
   Bos birakirsan bridge son aktif Codex oturumunu otomatik kullanir.
2. `.md Listele` butonuna bas.
3. Listeden dosya secip:
   - `Plan Olarak Yukle`
   - `Gunluk Olarak Yukle`
4. `Oturumdan Ilerleme Sor` ile ayni session'a `resume` promptu gonderilir.

## Sorun Giderme

- `.md Listele` ve `AI analiz` calismiyorsa once `Bridge Test` yap.
- `Bridge URL` mutlaka `http://127.0.0.1:4317` olsun.
- Saglayici `Codex Local (API key yok)` secili olsun.
- Bridge penceresi acik kalmali.
- Port cakismasi/surec takilmasi icin:

```powershell
powershell -ExecutionPolicy Bypass -File ".\tools\codex-bridge\start-codex-bridge.ps1"
```

Bu komut eski bridge surecini kapatip tekrar acar.

## Endpoint ozeti

- `GET /health`
- `POST /api/ai`
- `POST /api/session/md-files`
- `POST /api/session/read-file`
- `POST /api/session/progress`

## Onemli notlar

- Bridge sadece `127.0.0.1` dinler.
- Session dosyalari `~/.codex/sessions` altindan okunur.
- `.md` listesi, session logunda gecen yollar + workspace fallback taramasi ile uretilir.
- `resume` cagrisi session baglamini kullanir; yine de yanit model davranisina gore degisebilir.
