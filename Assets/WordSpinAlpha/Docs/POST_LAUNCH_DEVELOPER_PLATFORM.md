# WordSpin Post-Launch Developer Platform

## Hedef
Oyun istemcisinden ayrik ama onunla uyumlu calisan bir gelistirici platformu kurulur:

- canli telemetry toplar
- AI destekli icgoru uretir
- hotfix config yayinlar
- yeni theme/content paketlerini cloud ustunden oyuna dagitir
- rollout ve rollback yonetir

## Tarih Notu ve 06.04.2026 Durumu

Bu dokuman post-alpha / post-launch hedef mimarisini anlatir.

Tarih bazli durum:

### 03.04.2026

- telemetry, cloud, pricing ve sandbox ihtiyaclari belirgin hale geldi

### 04.04.2026

- icerik ve shape tarafinda editorlesme buyudugu icin, ileride bunlarin web tabanli bir authoring/publish paneline tasinmasi ihtiyaci netlesti

### 06.04.2026

- tek shell editor ve live-config kılavuzu yazildi
- fakat bu dokumanda anlatilan post-launch developer platformu henuz aktif bir runtime/backend sistemi olarak kurulmus degildir

06.04.2026 itibariyla durum:

- bu belge bir `hedef platform` dokumanidir
- mevcut aktif sistem degildir
- teknik uygulama on kosullari ve editor/live-config mimarisi artik `UNIFIED_EDITOR_TO_LIVEOPS_PLAN.md` ve `UNIFIED_EDITOR_AND_LIVE_CONFIG_GUIDE.md` icinde daha net tanimlidir

## Temel Bilesenler
- `Unity Client`
  - local fallback icerik
  - remote override icerik
  - telemetry queue
  - snapshot export
- `Backend`
  - telemetry ingest API
  - aggregation jobs
  - signed manifest service
  - content registry
  - rollout / rollback service
- `Web Developer Panel`
  - canli telemetry ekranlari
  - AI oneri merkezi
  - theme/content yayinlama
  - hotfix yonetimi

## Remote Content Kurallari
- Remote yayin sadece veri ve asset paketleri icindir.
- Kod degistiren tum guncellemeler market release gerektirir.
- Istemci su kataloglari remote override edebilir:
  - levels
  - questions
  - themes
  - info cards
  - campaigns
  - difficulty profiles
  - rhythm profiles
  - store config
  - membership / energy config

## AI Icgoru Akisi
1. Istemci event uretir.
2. Event normalize edilir.
3. Aggregate snapshot olusur.
4. AI bu snapshot ustunden oneri uretir.
5. Gelistirici oneriyi panelden onaylar veya duzenler.
6. Hotfix manifest yayinlanir.

## Guvenlik ve Uyumluluk
- Manifest surumlu ve imzali olmalidir.
- Uyumsuz paketler local fallback'e dusmelidir.
- Insan onayi olmadan production degisikligi uygulanmaz.
