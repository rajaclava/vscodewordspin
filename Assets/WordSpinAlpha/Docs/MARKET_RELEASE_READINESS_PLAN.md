# WordSpin Alpha - Market Release Readiness Plan

## Amac
Bu dokumanin amaci, mevcut test/editor/live-config altyapisini bozmadan oyunu markete hazir production omurgasina donusturmek icin gerekli kesin adimlari sabitlemektir.

Bu dosya su sorulara cevap verir:

- markete cikacak build nasil davranacak
- test sandbox ve debug katmanlari release'te nasil ayrilacak
- premium, no-ads, hint, energy ve promosyon akislari production'da nasil calisacak
- web panel ve live-config sistemine nasil gecilecek
- oyuncu telefonundaki oyuna degisiklikler nasil guvenli ve hafif sekilde gonderilecek
- marketten once hangi final testler yapilacak

Bu dokuman, asagidaki mevcut referanslarla capraz dusunulmustur:

- `WORDSPIN_MASTER_PLAN.md`
- `UNIFIED_EDITOR_TO_LIVEOPS_PLAN.md`
- `UNIFIED_EDITOR_AND_LIVE_CONFIG_GUIDE.md`
- `ECONOMY_NOW_AND_FUTURE_REPORT.md`
- `TEKNIK_ZIHIN_HARITASI.md`
- `POST_LAUNCH_DEVELOPER_PLATFORM.md`

## Snapshot Tarihi
Bu planin snapshot tarihi:

- `06.04.2026`

Bu tarihte proje su noktadadir:

- tum ana tuning yuzeyleri editorlestirilmis durumda
- tek shell editor kurulmus durumda
- ekonomi sandbox/test katmani aktif
- content/shape/live apply omurgasi aktif
- remote override / telemetry / hotfix yonunde temel omurga mevcut
- production billing, production rewarded ad ve production storefront pricing henuz kurulmus degil

## Temel Hedef
Market surumunde oyuncu:

- premiumsuz baslar
- temiz save ile baslar
- puan ve coin birikimi sifirdan baslar
- aktif test modu gormez
- fake reklam gormez
- preview fiyat gormez
- gercek store urunleri, gercek reklam ve gercek entitlement akisini kullanir

Ayni anda ekip tarafinda:

- mevcut editorler calismaya devam eder
- test sandbox katmanlari editor/internal build tarafinda kalir
- future live-config ve web panel icin veri semalari korunur
- economy, promo, content, UI ve theme metadata uzaktan yonetilebilir hale gelir

## Mevcut Altyapida Zaten Hazir Olanlar

### 1. Test sandbox mimarisi
Hazir olan siniflar:

- `TestPlayerModeManager`
- `TestPlayerModeProfile`
- `EconomyBalanceWindow`

Anlami:

- `Default` gercek runtime referansidir
- `FreePlayer / PremiumPlayer` yalnizca test sandbox'tir
- production build'de bu ayirim korunabilir, ancak oyuncuya yalnizca `Default` davranisi acilir

### 2. Baslangic economy grant cekirdegi
Hazir olan alanlar:

- `EnergyConfigDefinition.startingHints`
- `EnergyConfigDefinition.startingSoftCurrency`
- `EconomyManager` icindeki ilk grant uygulama mantigi
- `EnergyState.currentEnergy` varsayilan tam enerji baslangici

Anlami:

- oyuncuya ilk acilista hediye hint/coin verme altyapisi veri seviyesinde var
- enerji tam baslama davranisi da teknik olarak uyumlu

Eksik olan:

- bu grant'lerin popup/promosyon diliyle gosterilmesi
- kampanya, gunluk hediye, saatlik sinirsiz enerji, onboarding promo gibi daha zengin promo orkestrasyonu

### 3. Store abstraction yonu
Hazir olan siniflar:

- `IPurchaseService`
- `StorePricingManager`
- `PreviewStorePricingProvider`
- `StorePresenter`

Anlami:

- store mantigi tamamen sabit stringlerle yazilmamis
- provider degistirerek production gecis yapmak mumkun

Eksik olan:

- gercek Google Play purchase servisi
- gercek storefront fiyat provider'i
- `StorePresenter` icindeki `MockPurchaseService` bagimliligini production servise gecirmek

### 4. Reklam test omurgasi
Hazir olan sinif:

- `DebugRewardedAdPresenter`

Anlami:

- rewarded continue akisi presenter tabanli ayri bir katman olarak dusunulmus

Eksik olan:

- production `IRewardedAdService` benzeri bir servis
- gerçek rewarded provider implementasyonu

### 5. Remote content ve hotfix cekirdegi
Hazir olan siniflar:

- `ContentService`
- `RemoteContentProvider`
- `RemoteContentManifestDefinition`
- `RemoteContentHotfixWindow`

Anlami:

- runtime local fallback + remote override mantigini taniyor
- persistent remote content klasoru mantigi var
- belirli kataloglarin uzaktan override edilmesine altyapi uygun

### 6. Telemetry omurgasi
Hazir olan siniflar:

- `TelemetryService`
- `TelemetryPolicyProfile`
- `TelemetryPolicyWindow`
- `DeveloperTelemetryWindow`

Anlami:

- event queue
- snapshot
- policy cap/flush mantigi
kurulmus durumda

Eksik olan:

- production ingest endpoint
- upload/ack/rotation operasyon zinciri
- dashboard/aggregation katmani

## Mevcut Altyapida Production Gecisi Icin Zorunlu Eksikler

### 1. Gercek satin alma servisi
Mevcut durum:

- `StorePresenter` dogrudan `MockPurchaseService.Instance.Purchase(...)` cagiriyor

Market icin gereken:

- `GooglePlayPurchaseService : IPurchaseService`
- `StorePresenter` icinde mock yerine soyut satin alma servisine gecis

Bu katman sunlari kapsamalidir:

- urun satin alma
- sonuc callback'i
- receipt/token alma
- restore purchases
- hata durumlari

### 2. Gercek storefront pricing
Mevcut durum:

- `StorePricingManager` fiilen `PreviewStorePricingProvider` delegasyonu yapiyor

Market icin gereken:

- `GooglePlayStorePricingProvider : IStorePricingProvider`
- `formattedPrice`, `currencyCode`, `availability` gibi alanlarin gercek billing product details'tan gelmesi

### 3. Gercek rewarded ad servisi
Mevcut durum:

- `FailModalPresenter` free test akisinda `DebugRewardedAdPresenter` kullanabiliyor

Market icin gereken:

- `IRewardedAdService`
- production provider implementasyonu
- ad hazir degilse fallback davranisi
- cancel/fail/success ayrimi

### 4. Promo / hediye orkestrasyon katmani
Mevcut durum:

- baslangic hint/coin grant altyapisi veri seviyesinde var
- fakat promo'nun ne zaman, hangi UI ile, hangi kosulla verilecegini yoneten production omurga yok

Bu proje icin kurulmasi gereken yeni production katmani:

- `PromoCatalogDefinition`
- `PromoOfferDefinition`
- `PromoTriggerDefinition`
- `PromoRewardDefinition`
- `PlayerPromoState`
- `PromoService`
- `PromoQueuePresenter`
- `PromoEligibilityEvaluator`
- `PromoGrantRecorder`

Bu katmanin gorevi:

- hediye ve promosyonlari yalnizca grant etmek degil
- bunlari popup/modal/inbox/gunluk odul gibi urun tasarimina uygun yuzeylerle sunmak
- grant’i idempotent sekilde kaydetmek
- ayni promoyu ikinci kez yanlis vermemek

### 5. Build channel / release-safe katman
Production icin zorunlu build kanallari:

- `editor/dev`
- `internal test`
- `alpha demo`
- `production release`

Bu kanallarda farkli davranacak seyler:

- test sandbox gorunurlugu
- fake rewarded acikligi
- mock purchase acikligi
- preview pricing acikligi
- telemetry verbose seviyesi
- remote override kanal secimi

Bu nedenle market oncesi mutlaka:

- `BuildProfile`
- `FeatureFlagRegistry`
- `ReleaseSafePolicy`

benzeri bir katman eklenmelidir.

## Production Hedef Davranislar

### 1. Oyuncu ilk kez oyunu actiginda
Oyuncunun ilk save state'i:

- `premiumMembershipActive = false`
- `noAdsOwned = false`
- `softCurrency = 0` veya promo ile kontrollu ilk grant
- `hints = 0` veya promo ile kontrollu ilk grant
- `energy = full`
- `activeTheme = default`
- `remoteContent = fallback local`

Not:

- coin/hint sifirdan baslama urun kararidir
- promosyon ile sonradan popup uzerinden verilmesi isteniyorsa, grant sunum zamani promo katmani uzerinden yonetilir
- bu zamanlama karari henuz kilitlenmeyecek, ama altyapi her senaryoya uyumlu olacak

### 2. Premium ve store production davranisi
Oyuncu premium veya paket satin aldiginda:

1. store urunu gercek product id ile sorgulanir
2. fiyat store'dan gelir
3. satin alma gercek billing servisiyle baslar
4. sonuc alinir
5. entitlement veya tuketilebilir urun resolve edilir
6. save guncellenir
7. UI ve gameplay eventleri yenilenir

Bu akista:

- `MockPurchaseService` release'te devre disi kalmali
- `StorePresenter` mock'a bagimli kalmamali

### 3. Reklam production davranisi
Fail/continue gibi akislarda:

- fake rewarded presenter release'te kapali olacak
- gercek rewarded servis kullanilacak
- ad sonucu fail/cancel/success ayrilacak

### 4. Remote ayar production davranisi
Oyuncu cihazindaki oyuna gidecek degisiklikler sadece:

- content/config JSON
- metadata
- theme bundle referanslari

olmalidir.

Kod degisikligi uzaktan gonderilmez.

## Promo ve Hediye Sistemi Icin Hazirlanacak Genel Omurga

### Ilke
Promo'nun ne zaman uygulanacagi daha sonra kilitlenecek. Bu yuzden production omurgasi su kararlarin tamamina uyumlu olmalidir:

- ilk giris popup'i
- onboarding hediyesi
- gunluk odul
- saatlik sinirsiz enerji
- level milestone hediyesi
- store/promosyon modal'i
- geri donen oyuncu kampanyasi
- dil/ulke/kanal bazli promo

### Bu nedenle promo sistemi su sekilde kurulmalidir

#### Veri katmani
- `promo_catalog.json`
- `promo_channels.json`
- `promo_locales.json` gerekirse ayri

#### Runtime katmani
- `PromoService`
- `PromoStateResolver`
- `PromoGrantService`
- `PromoQueueService`
- `PromoCooldownResolver`

#### UI katmani
- `PromoModalPresenter`
- `PromoInboxPresenter` gerekirse
- `PromoBannerPresenter` gerekirse

#### Save katmani
- alinmis promo id listesi
- aktif gecici bonuslar
- bonus bitis zamanlari
- son gosterim zamanlari
- tekrar gosterim cooldown bilgisi

### Guvenli tasarim karari
Promo katmani:

- economy manager'i replace etmez
- energy manager'i replace etmez
- mevcut sistemlere kontrollu grant/override komutu verir
- tum gecici bonuslari acik veri modeli ile tutar

Bu yaklasim mekanik kodu bozmadan buyumeyi saglar.

## Web Uygulamasi ve Live Config Icin Production Hazirlik

### Ana ilke
Unity editorleri ile kurulan ayar mantigi, ileride web uygulamasina birebir kod tasimasi olarak degil, **ayni veri semalarinin web authoring paneline tasinmasi** olarak ele alinmalidir.

### Onerilen stack

- Admin panel: `Next.js + TypeScript`
- Auth ve metadata DB: `Supabase`
- Config/payload storage: `Cloudflare R2` veya `Supabase Storage`
- Publish/rollback fonksiyonlari: `Supabase Edge Functions` veya hafif Node backend
- Crash takibi: `Sentry` ve gerekirse `Firebase Crashlytics`

### Web panele tasinacak alanlar

- economy config
- promo config
- theme metadata
- content catalog yayinlama
- locale guncellemeleri
- pricing/product metadata
- feature flags
- remote manifest yayinlama
- rollout/rollback

### Web panele hemen tasinmayacak seyler

- cekirdek gameplay mekanikleri
- scene bazli hizli local tuning editorlugu
- editor-only shape referans gorsel authoring

### Oyuncu cihazina degisiklik gonderme modeli

1. istemci manifest kontrol eder
2. config version farki varsa sadece degisen payloadlari indirir
3. payload `persistentDataPath` altina yazilir
4. `ContentService` local fallback + remote override mantigi ile bunlari birlestirir
5. config tipi neyse uygun safe-point'te apply edilir

### Safe-point uygulama kurali

#### Hemen uygulanabilecekler
- telemetry policy
- belirli UI copy
- belirli store/promosyon gorunurluk ayarlari

#### Menu veya sahne refresh ile uygulanabilecekler
- store metadata
- theme runtime metadata
- bazi UI surface tuning ayarlari

#### Sadece yeni session/level ile uygulanabilecekler
- rhythm
- difficulty
- shape/layout
- level economy
- soru/fail flow
- promo eligibility kararlarinin bir kismi

#### Restart veya tam paket yenilemesi gerekenler
- font setleri
- buyuk theme asset bundle degisiklikleri
- render/resource tabanli buyuk varlik degisiklikleri

Bu kural, sonradan karar verilmemis promo senaryolarina da uyum saglar.

## Market Sürümüne Gecis Icin Somut Teknik Donusum Listesi

### A. Billing
Kurulacak:

- `GooglePlayPurchaseService`
- `PurchaseRestoreService`
- gerekiyorsa `PurchaseVerificationService`

Guncellenecek:

- `StorePresenter`
- `IPurchaseService` kullanim akisi

### B. Pricing
Kurulacak:

- `GooglePlayStorePricingProvider`

Guncellenecek:

- `StorePricingManager`

Kapatilacak:

- release'te `PreviewStorePricingProvider`

### C. Rewarded Ads
Kurulacak:

- `IRewardedAdService`
- production rewarded provider

Kapatilacak:

- release'te `DebugRewardedAdPresenter`

### D. Promo/Gift katmani
Kurulacak:

- `PromoCatalogDefinition`
- `PromoService`
- `PromoQueuePresenter`
- `PlayerPromoState`

Baglanacak:

- `EconomyManager`
- `EnergyManager`
- store/modal UI

### E. Build profile ve feature flag
Kurulacak:

- `BuildProfileDefinition`
- `ReleaseSafePolicy`
- `FeatureFlagRegistry`

Bu katman sunlari kontrol eder:

- sandbox visibility
- mock purchase
- fake ad
- preview pricing
- remote config channel
- debug overlay

### F. Remote publish / live config
Gelistirilecek:

- mevcut `RemoteContentHotfixWindow` mantiginin web publish hattina tasinmasi
- manifest versioning
- signed publish / rollback

## Final Market Hazirlik Asamalari

### Asama 1 - Editor ve bug sweep tamamlanir
Kosul:

- parity saglanmis olur
- tum kritik editor apply zinciri stabil olur
- bug sweep ve hedefli testler tamamlanir

### Asama 2 - Production provider gecisleri yapilir

- billing
- pricing
- rewarded ads
- restore/entitlement

### Asama 3 - Promo/gift production katmani kurulur

- baslangic hediyeleri
- kampanyalar
- enerji promosyonlari
- modal/popup akislari

### Asama 4 - Build profile / release-safe paket hazirlanir

- test katmanlari gizlenir
- debug pencereleri oyuncu build'inden ayrilir
- preview pricing ve fake ad kapatilir
- `Default` runtime zorunlu hale getirilir

### Asama 5 - Web panel / live config pilotu acilir

- once internal/alpha kanalda
- sonra production rollout icin

### Asama 6 - Final market test paketi
Marketten once zorunlu final testler:

#### 1. Save ve baslangic state
- ilk acilis state
- promosyon grant state
- save sil / tekrar yukle
- update sonrasi save uyumu

#### 2. Economy ve promo
- baslangic hint/coin/energy davranisi
- promo grant tek seferliligi
- sureli bonus baslangic ve bitis davranisi
- popup / modal / queue akisi

#### 3. Billing
- premium satin alma
- no ads satin alma
- hint/energy pack satin alma
- cancel/fail/success
- restore purchase
- entitlement refresh

#### 4. Rewarded ads
- ad hazir
- ad hazir degil
- ad fail
- ad success
- continue sonrasi doğru gameplay restore

#### 5. Remote config / hotfix
- local fallback
- remote override
- manifest degisimi
- rollback
- bozuk payload fallback'i

#### 6. Performance ve optimizasyon
- APK boyutu
- startup suresi
- scene acilis suresi
- memory allocation
- telemetry queue buyume davranisi
- remote payload disk etkisi
- dusuk cihaz davranisi

#### 7. UI / locale / device
- 4 dil
- safe area
- farkli cihaz oranlari
- store / fail / result / info card / main menu / gameplay

#### 8. Theme paketleri
- base theme fallback
- eksik asset fallback
- yeni theme metadata ekleme
- remote theme paket referansi

### Bu final test asamasi zorunludur
Bu proje icin marketten once tek bir buyuk final pass yapilacaktir. Bu pass:

- performance
- optimizasyon
- durum senaryolari
- odeme testleri
- reklam testleri
- remote config testleri
- rollback testleri
- locale/device testleri

tamamini kapsar.

Bu testler bitmeden production release cikilmamali.

## 06.04.2026 Itibariyla En Dogru Sonuc

Mevcut altyapi:

- market-ready yone dogru kurulmus durumda
- editor ve live-config omurgasi acisindan dogru yonde
- production provider taraflari eksik
- promo/gift production orkestrasyonu eksik
- build profile / release-safe mode eksik

Dolayisiyla bir sonraki buyuk hedef:

- editorleri yeniden yazmak degil
- bug sweep sonrasi production release katmanlarini tamamlamak

Bu dokumanin rolu:

- test/editor altyapisini production omurgasina cevirirken neyin korunacagini
- neyin production-safe hale getirilecegini
- neyin sifirdan eklenecegini

tek yerde sabitlemektir.
