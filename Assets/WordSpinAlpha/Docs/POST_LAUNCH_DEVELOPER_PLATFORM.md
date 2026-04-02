# WordSpin Post-Launch Developer Platform

## Hedef
Oyun istemcisinden ayrı ama onunla uyumlu çalışan bir geliştirici platformu kurulur:

- canlı telemetry toplar
- AI destekli içgörü üretir
- hotfix config yayınlar
- yeni theme/content paketlerini cloud üstünden oyuna dağıtır
- rollout ve rollback yönetir

## Temel Bileşenler
- `Unity Client`
  - local fallback içerik
  - remote override içerik
  - telemetry queue
  - snapshot export
- `Backend`
  - telemetry ingest API
  - aggregation jobs
  - signed manifest service
  - content registry
  - rollout / rollback service
- `Web Developer Panel`
  - canlı telemetry ekranları
  - AI öneri merkezi
  - theme/content yayınlama
  - hotfix yönetimi

## Remote Content Kuralları
- Remote yayın sadece veri ve asset paketleri içindir.
- Kod değiştiren tüm güncellemeler market release gerektirir.
- İstemci şu katalogları remote override edebilir:
  - levels
  - questions
  - themes
  - info cards
  - campaigns
  - difficulty profiles
  - rhythm profiles
  - store config
  - membership / energy config

## AI İçgörü Akışı
1. İstemci event üretir.
2. Event normalize edilir.
3. Aggregate snapshot oluşur.
4. AI bu snapshot üstünden öneri üretir.
5. Geliştirici öneriyi panelden onaylar veya düzenler.
6. Hotfix manifest yayınlanır.

## Güvenlik ve Uyumluluk
- Manifest sürümlü ve imzalı olmalıdır.
- Uyumsuz paketler local fallback'e düşmelidir.
- İnsan onayı olmadan production değişikliği uygulanmaz.
