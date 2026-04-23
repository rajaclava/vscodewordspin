# WordSpin Alpha Toplu Editor ve Live Config Kılavuzu

## Dokuman Rolu ve Iliskili Referanslar

Bu dosya bir `uygulama ve mimari kilavuzu`dur. Su sorulara cevap verir:

- editor veriyi nereden aliyor
- hangi runtime/script/config yuzeyini degistiriyor
- ayni ayar diger editorlere nasil yansiyor
- yeni bir ayar nasil guvenli sekilde eklenir
- bu sistem daha sonra web panel ve live-config tarafina nasil tasinir

Bu dosya, yol haritasi dokumani degildir. Is sirasini ve fazlamayi belirleyen ana plan su dosyadir:

- `Assets/WordSpinAlpha/Docs/UNIFIED_EDITOR_TO_LIVEOPS_PLAN.md`

Rol ayrimi:

- `UNIFIED_EDITOR_AND_LIVE_CONFIG_GUIDE.md`
  - mevcut teknik durum
  - editor mimarisi
  - veri sahipligi
  - kullanim kilavuzu
  - cloud/live-config uygulama modeli
- `UNIFIED_EDITOR_TO_LIVEOPS_PLAN.md`
  - hangi islerin hangi sirayla yapilacagi
  - alpha oncesi ve sonrasi fazlar
  - risklerin ne zaman ele alinacagi

Bu iki dokuman birlikte okunmalidir. Bir yazilimci veya AI modeli:

1. once plan dokumanindan hangi fazda oldugunu anlamali
2. sonra bu kılavuzdan mevcut editor mimarisine gore degisikligi yapmalidir

## Snapshot Tarihi ve Mimari Durum

Bu kılavuzun anlattigi teknik durumun ana snapshot tarihi:

- `06.04.2026`
- `08.04.2026` ek durum notlari

Tarih bazli kisa ozet:

### 03.04.2026

- ekonomi sandbox ve pricing abstraction katmanlari kuruldu
- ekonomi/store/test mode editor omurgasi netlesti

### 04.04.2026

- icerik, level ve shape authoring editorleri kuruldu
- referans gorselden shape uretimi ve canli apply akisi eklendi

### 06.04.2026

- eksik tuning editorleri tamamlandi
- tum editorler `Toplu Tek Editor` kabugu altinda birlestirildi
- editorler arasi commit-sonrasi sync katmani eklendi
- bu kılavuz, o mimarinin referans dokumani haline getirildi

### 08.04.2026

- bug sweep'in ilk manuel turu baslatildi
- save/session/restore ailesindeki ilk kritik maddeler sahada kontrol edildi
- `BUG-007`, `BUG-009`, `BUG-010`, `BUG-011`, `BUG-013` icin orijinal hipotez kapsami icinde bozuk davranis cogu ltulemedi
- buna karsin ayni restore zincirinde puan state'inin save'e hic girmedigi bulundu
- score persistence sorunu kapatildi:
  - `SessionSnapshot` score state alanlariyla genisletildi
  - `ScoreManager` snapshot doldurma ve restore akisina baglandi
  - `GameManager` restore zinciri gameplay + score state'i birlikte kurar hale getirildi
- `BUG-014` de ayni turda kapatildi:
  - fail/continue modal ve info card acikken gameplay input kapanmiyordu
  - `InputManager`, `KeyboardPresenter` ve `GameManager` uzerinden input gating merkezi hale getirildi
- `BUG-029` de ayni turda kapatildi:
  - progress save modeli dil bazli ilerleme destekleyecek sekilde genisletildi
  - menu summary, `Play` akisi ve level complete update yollarÄ± secili dilin kendi progress state'ini okur hale getirildi
  - level reward/first-clear coin takibi global birakildi
- `BUG-027` manuel olarak denendi ve ilk turda sorun vermedi
- ayni gun sonraki kontrolde:
  - `BUG-024` su anki progress/session ayristirma mimarisi nedeniyle aktif bug adayi olmaktan cikti
  - `BUG-025` ana hipotezi cogu ltulemedi
  - fakat ayni testte continue sonrasi menu/store donusunde gameplay input'un kilitli kalmasi bulundu
  - bu sorun `GameManager` restore akisinda `pending result` yokken gereksiz input kapatilmasi duzeltilerek kapatildi
  - `BUG-026`, `BUG-034`, `BUG-068` manuel cogu ltma turunda sorun vermedi
  - `BUG-037` ve `BUG-040` bu asamada acik runtime blocker olarak ele alinmadi; final live-config / web panel / release-safe build gecisine yakin tekrar acilacak

### 06.04.2026 itibariyla teknik durum

- tek shell editor vardir
- tekli editorler parity icin halen mevcuttur
- veri sahipligi, refresh ve sync katmanlari artik ayrik olarak tanimlanmistir
- web panel ve live-config gecisi henuz uygulanmamis, fakat bu kılavuzdaki prensiplere gore planlanmis durumdadir

### 08.04.2026 itibariyla teknik durum

- tek shell editor ve tekli editor parity modeli korunuyor
- bug sweep aktif olarak baslamis durumda
- save/session restore omurgasi ilk manuel turdan gecirildi
- score state'in restore edilmemesi seklindeki gercek regression kapatildi
- `BUG-014` ve `BUG-029` kapatildi
- `BUG-027` ilk turda cogu ltulemedi
- `BUG-024` ve `BUG-025` ilk halleriyle acik bug olarak kalmadi
- `BUG-025` taramasinda bulunan continue-sonrasi menu/store donus klavye kilidi sorunu kapatildi
- `BUG-026`, `BUG-034`, `BUG-068` acik bug olarak kalmadi
- `BUG-037` ve `BUG-040` ertelendi cunku bunlar su an oyuncu akisindan cok editor/live-config/release gecisi tarafina ait risklerdir
- bu noktadan sonra ayni ailede yeni degisiklik yapilirsa session snapshot ve restore zinciri birlikte ele alinmalidir

08.04.2026 sonu teknik yorum:

- su an alpha demo icin acil runtime blocker kalmamistir
- bug sweep sonrasi acik kalan maddelerin agirlik merkezi artik gameplay cekirdegi degil, su katmanlardadir:
  - remote override ve local precedence
  - test sandbox izolasyonu
  - telemetry queue / policy
  - release-safe build ayrimi
  - production provider gecisi

Bu nedenle editor ve live-config mimarisi acisindan uygulanacak kural:

1. bu maddeler simdi gameplay hotfix backlog'u gibi ele alinmayacak
2. `live config`, `web panel`, `release-safe build`, `market release` asamalarinda yeniden acilacak
3. o ana kadar mevcut editor mimarisine yeni risk eklenmemeli; yalnizca veri sahipligi ve refresh/sync kurallari korunmalidir

Hemen sonraki teknik adim:

- yeni editor yazmaktan cikilip
- parity/stabilizasyon ve bug sweep giris fazina gecilmistir

## Amac
Bu dokuman, projede kurulan tum editorlerin:

- veriyi nereden aldigini
- hangi sahne objelerini, scriptleri veya config dosyalarini etkiledigini
- hangi ayarin neyi degistirdigini
- gelecekte nasil guvenli sekilde genisletilecegini
- ileride web uygulamasi ve cloud live-config katmanina nasil tasinacagini

tek yerde aciklar.

Bu dosya, daha sonra projeye dahil olacak bir yazilimciya veya baska bir yapay zeka modeline dogrudan verilebilir. Hedef, yeni ayarlar eklenirken cekirdek oyun koduna ve mekaniklere gereksiz mudahale edilmemesidir.

Guncel durum notu:

- tek shell editor kurulmustur
- tekli editorler parity dogrulamasi icin halen korunmaktadir
- editorler arasi commit-sonrasi sync katmani aktiftir
- bu nedenle bu kılavuz, hem tekli editorleri hem de toplu editoru birlikte aciklar

## Kapsam
Bu kılavuz su editor katmanlarini kapsar:

- `Toplu Tek Editor`
- `Icerik ve Level Editoru`
- `Ekonomi Denge Editoru`
- `Klavye Yerlesim Editoru`
- `Gameplay Scene Tuner`
- `Slot ve Hit Ayarlari`
- `Pin ve Input Ayarlari`
- `Ritmik Donus ve Zorluk Ayarlari`
- `Soru ve Fail Akisi Ayarlari`
- `UI Yuzey Ayarlari`
- `Hissiyat ve Gorsel Ayarlari`
- `Tema ve Magaza Config`
- `Tema Paketi Onizleme`
- `Mobil Runtime ve Cihaz Ayarlari`
- `Ambiyans ve Pulse Ayarlari`
- `Telemetry Politikasi`
- `Gelistirici Telemetry Paneli`
- `Uzak Icerik ve Hotfix Ayarlari`
- `Global Validation ve Referans Taramasi`
- `Save ve Session Paneli`
- `Android Build Araclari`

## Temel Mimari Prensip
Bu editor sistemi uc temel katman uzerine kuruludur:

1. `Veri sahibi`
   - Bir ayarin asil kaynagi neresidir sorusunun cevabidir.
   - Bu kaynak su tiplerden biri olur:
     - sahne component verisi
     - `ScriptableObject` asset
     - JSON runtime config
     - JSON content dosyasi
     - save/session/telemetry runtime durumu

2. `Editor apply/refesh katmani`
   - Editorler dogrudan rastgele objeleri degistirmez.
   - Mümkün oldugunca:
     - config repository
     - hedef componentin editor-safe apply metodu
     - ortak refresh utility
     uzerinden ilerler.

3. `Commit-tabanli editor sync`
   - Bir editor `Kaydet` veya `Uygula` dediginde, ayni veriyi kullanan diger editorler stale kalmasin diye sync bildirimi yayilir.
   - Bu sync yarim edit durumunu degil, yalnizca commit edilmis durumu tasir.

Bu yapinin amacı:

- editorler arasi cakismayi azaltmak
- ayni verinin farkli pencerelerde tutarsiz gorunmesini onlemek
- runtime mekanikleri editor yuzunden kirilmasin diye veri sahipligini net tutmak

Bu mimarinin plan tarafindaki karsiligi:

- once editorlerin parity ile tek shelle toplanmasi
- sonra bug sweep
- sonra visual/alpha lock
- sonra telemetry/hotfix/web panel

Bu siralama `UNIFIED_EDITOR_TO_LIVEOPS_PLAN.md` dosyasinda tutulur.

## Ortak Destek Katmanlari

### 1. Toplu Editor Kabuğu
- Dosya: `Assets/WordSpinAlpha/Scripts/Editor/WordSpinAlphaUnifiedEditorWindow.cs`
- Gorevi:
  - tum editorleri tek shell icinde toplar
  - sol menude moduller arasi gecis saglar
  - eski tekli editorleri hemen silmeden, host ederek parity korur
- Onemli not:
  - bu dosya editor-only kabuktur
  - runtime mekanik barindirmez

### 2. Runtime Config Repository
- Dosya: `Assets/WordSpinAlpha/Scripts/Editor/WordSpinAlphaRuntimeConfigRepository.cs`
- Gorevi:
  - JSON tabanli runtime config dosyalarini tek API altinda yuklemek ve kaydetmek
- Su veri setlerini yukler/kaydeder:
  - `themes.json`
  - `store_catalog.json`
  - `membership_profile.json`
  - `energy_config.json`
  - `remote_manifest_template.json`
  - `difficulty_profiles.json`
  - `difficulty_tiers.json`
  - `rhythm_profiles.json`
  - `shape_layouts.json`
- Tasarim karari:
  - editorler JSON path’leri dogrudan daginik sekilde yazmaz
  - ortak repository uzerinden gider

### 3. Runtime Refresh Utility
- Dosya: `Assets/WordSpinAlpha/Scripts/Editor/WordSpinAlphaEditorRuntimeRefreshUtility.cs`
- Gorevi:
  - play mode icinde sahne ve runtime tarafina guvenli apply saglamak
- Kritik metotlar:
  - `ApplyContentAndConfigRefresh(bool reloadCurrentSession)`
  - `RefreshRotatorPresentation()`
  - `RefreshThemePresentation()`
  - `RefreshCurrentTargetState()`
- Tasarim karari:
  - editorler, sahnede etkili degisiklikleri kendi kafasina `Find` zinciriyle daginik uygulamak yerine bu utility uzerinden tetikler

### 4. Editor Sync Utility
- Dosya: `Assets/WordSpinAlpha/Scripts/Editor/WordSpinAlphaEditorSyncUtility.cs`
- Gorevi:
  - editorler arasi commit-sonrasi sync saglar
- Sync tipleri:
  - `Scene`
  - `RuntimeConfig`
  - `Content`
  - `ScriptableAssets`
  - `Telemetry`
- Calisma sekli:
  - bir editor `NotifyChanged(...)` cagirir
  - diger editor pencereleri `ConsumeChanges(...)` ile yeni stamp’i gorurse veriyi tekrar okur
- Onemli not:
  - bu sync bilerek yarim-edit state tasimaz
  - sadece `Kaydet/Uygula` sonrasi calisir

### 5. UI Tuning Utility
- Dosya: `Assets/WordSpinAlpha/Scripts/Editor/WordSpinAlphaEditorUiTuningUtility.cs`
- Gorevi:
  - sahnedeki UI `RectTransform`, `TMP_Text`, `Image`, `Button` gibi yuzeylerin editor-safe sekilde duzenlenmesini standart hale getirmek
- Ayrica:
  - sahne tabanli UI tuning degisikliginden sonra `Scene` sync bildirimi yollar

## Veri Kaynagi Tipleri

### A. Sahne tabanli tuning
Bu editorler sahnedeki component instance’larini degistirir:

- `GameplaySceneTuner`
- `GameplayHudPresenter`
- `FailModalPresenter`
- `InfoCardPresenter`
- `ResultPresenter`
- `StorePresenter`
- `MembershipPresenter`
- `ThemeRuntimeController`
- `RotatorPlaquePresenter`
- `Slot`
- `Pin`
- `PinLauncher`
- `TargetRotator`
- `QuestionLifeManager`
- `MobileRuntimeController`

### B. ScriptableObject tabanli tuning
Bu editorler asset duzeyinde veri tutar:

- `EconomyBalanceProfile`
- `EconomyBalanceProfile_Free`
- `EconomyBalanceProfile_Premium`
- `TestPlayerModeProfile`
- `KeyboardLayoutTuningProfile`
- `ScoreTuningProfile`
- `TelemetryPolicyProfile`

### C. JSON runtime config tabanli tuning
Bu editorler runtime JSON dosyalarini yazar:

- `themes.json`
- `store_catalog.json`
- `membership_profile.json`
- `energy_config.json`
- `difficulty_profiles.json`
- `difficulty_tiers.json`
- `rhythm_profiles.json`
- `shape_layouts.json`
- `remote_manifest_template.json`

### D. JSON content tabanli tuning
Bu editorler level ve lokalizasyon verisini yazar:

- `levels.json`
- `questions.json`
- `info_cards.json`
- lokalizasyon klasorleri altindaki dil bazli content dosyalari

### E. Save/session/telemetry tabanli editorler
Bu editorler oyun verisini debug etmek veya resetlemek icin runtime/save alanina dokunur:

- `Save ve Session Paneli`
- `Gelistirici Telemetry Paneli`

## Editor Modulleri: Genel Harita

| Modul | Asil Veri Kaynagi | Runtime/Sahne Etki Yuzeyi | Uygulama Bicimi |
|---|---|---|---|
| Toplu Tek Editor | diger editorlerin tamami | dogrudan veri sahibi degil | host/shell |
| Icerik ve Level | content JSON + shape JSON | `ContentService`, `GameManager`, `LevelFlowController` | save + content refresh |
| Ekonomi ve Sandbox | economy SO + test mode SO | `EconomyManager`, `EnergyManager`, store/level economy | asset save + refresh |
| Klavye | keyboard SO/profile | keyboard layout runtime/presenter | asset save + scene refresh |
| Gameplay Layout | sahne `GameplaySceneTuner` | gameplay hud ve placement | scene apply |
| Slot ve Hit | sahne `Slot` / plaque presenter | slot plaque gorunumu ve hit araliklari | scene apply + rotator refresh |
| Pin ve Input | sahne `Pin`, `PinLauncher`, `InputManager`, `FireGate` | pin davranisi ve input pacing | scene apply |
| Ritim ve Donus | JSON rhythm/difficulty + `TargetRotator` | active level pacing | config save + content refresh |
| Soru ve Fail | sahne `QuestionLifeManager`, `FailModalPresenter` | soru cani ve fail akisi | scene apply |
| UI Yuzeyleri | sahne UI componentleri | HUD, fail, result, info, store, membership, menu | scene apply |
| Hissiyat ve Gorsel | score SO + impact profile + theme runtime | score, flash, haptic, impact, color pressure | save/apply |
| Tema ve Magaza Config | JSON theme/store/membership/energy | `ThemeRuntimeController`, `StorePresenter`, `EconomyManager`, `EnergyManager` | config save + refresh |
| Tema Paketi Onizleme | JSON theme catalog | theme preview/runtime | config read + preview apply |
| Mobil Runtime | sahne/mobile config | `MobileRuntimeController` | scene apply |
| Ambiyans ve Pulse | sahne/theme runtime | glow, pulse, ambience | scene apply |
| Telemetry Politikasi | SO policy | `TelemetryService` | asset save + telemetry sync |
| Gelistirici Telemetry | telemetry snapshot/state | `TelemetryService` | runtime inspect |
| Uzak Icerik ve Hotfix | remote manifest JSON + persistent override klasoru | `ContentService` remote override | publish/refresh |
| Validation | tum katalog ve referanslar | dogrudan runtime degistirmez | audit |
| Save ve Session | save/runtime state | `SaveManager`, `GameManager`, economy/session | debug reset |
| Android Araclari | build pipeline | editor/distribution | tool |

## Editorler: Tek Tek Kullanim Kilavuzu

### 1. Toplu Tek Editor
- Dosya:
  - `Assets/WordSpinAlpha/Scripts/Editor/WordSpinAlphaUnifiedEditorWindow.cs`
- Menu:
  - `Tools > WordSpin Alpha > Toplu Tek Editor`
- Veri kaynagi:
  - kendisi veri sahibi degildir
  - alt modulleri host eder
- Neyi degistirir:
  - hicbir runtime veriyi tek basina degistirmez
- Kullanım:
  - sol menuden modulu sec
  - ilgili editorun kendi `Kaydet/Uygula` davranisi devam eder
- Not:
  - eski tekli editorler halen mevcuttur
  - parity tam dogrulaninca kademeli kapatilmalidir

### 2. Icerik ve Level Editoru
- Dosyalar:
  - `WordSpinAlphaContentEditorWindow.cs`
  - `WordSpinAlphaContentEditorData.cs`
- Veri kaynagi:
  - `levels.json`
  - `questions.json`
  - `info_cards.json`
  - lokalizasyon dosyalari
  - `shape_layouts.json`
- Degistirdigi yuzey:
  - level tanimlari
  - soru/cevap
  - bilgi karti
  - 4 dil content
  - shape atama
- Ayar gruplari:
  - `Leveller`
    - level id, tema, difficulty, rhythm, shape, obstacle vb.
  - `Dil Icerikleri`
    - TR/EN/ES/DE soru
    - cevap
    - bilgi karti baslik/govde
  - `Sekil Kutuphanesi`
    - custom shape
    - referans gorselden shape uretme
    - manuel nokta ve plaque acisi duzenleme
    - gameplay fit ve adaptif plaque ayarlari
- Play mode etkisi:
  - `Kaydet ve Canli Uygula` ile aktif content cache yenilenebilir
- Dikkat:
  - bu editor mekanik mantigi degistirmez
  - yalnizca content ve shape tanimlarini degistirir

### 3. Ekonomi Denge Editoru
- Dosya:
  - `EconomyBalanceWindow.cs`
- Veri kaynagi:
  - `EconomyBalanceProfile.asset`
  - `EconomyBalanceProfile_Free.asset`
  - `EconomyBalanceProfile_Premium.asset`
  - `TestPlayerModeProfile.asset`
- Degistirdigi yuzey:
  - level coin/star kurallari
  - preview fiyatlar
  - test sandbox davranisi
- Ayar gruplari:
  - `Default / Free / Premium` profil secimi
  - reward/stars
  - theme offer
  - preview pricing
  - test player mode
  - reset/debug butonlari
- Play mode etkisi:
  - economy ve sandbox refresh zinciri ile etkisini gorebilir
- Dikkat:
  - `Default` ana runtime davranistir
  - `Free/Premium` test sandbox’idir

### 4. Klavye Yerlesim Editoru
- Dosya:
  - `KeyboardLayoutTuningWindow.cs`
- Veri kaynagi:
  - `KeyboardLayoutTuningProfile.asset`
- Degistirdigi yuzey:
  - klavye dock
  - grid boyutu
  - tus boyutlari
  - spacing/padding
  - dil bazli yerlesim
- Ayar gruplari:
  - dil secimi
  - keyboard frame
  - grid
  - menu/store button anchorlari
  - swipe hint alanlari
- Play mode etkisi:
  - scene apply sonrasi gameplay UI’de gorunur

### 5. Gameplay Layout
- Dosya:
  - `GameplaySceneTunerEditor.cs`
- Veri kaynagi:
  - sahnedeki `GameplaySceneTuner`
- Degistirdigi yuzey:
  - camera
  - rotator konumu
  - launcher konumu
  - lane
  - top/bottom bar
  - keyboard frame/grid anchorlari
- Ayar gruplari:
  - gameplay alan yerlesimi
  - hud placement
  - sahne dekor katmanlari
- Play mode etkisi:
  - sahne yerlesimini anlik degistirir

### 6. Slot ve Hit Ayarlari
- Dosya:
  - `SlotHitTuningWindow.cs`
- Veri kaynagi:
  - sahnedeki `Slot` componentleri
  - `RotatorPlaquePresenter`
- Degistirdigi yuzey:
  - plaque size
  - active scale
  - perfect/good/hata feedback renkleri
  - hit band/padding
  - idle/active plaque renkleri
- Ayar gruplari:
  - `Gorsel`
  - `Hit toleranslari`
  - `Feedback`
- Play mode etkisi:
  - rotator presentation refresh ile anlik gorunur
- Onemli not:
  - mekanik hit zone tarafinda degisiklikler bu editor uzerinden kontrollu yapilir
  - degisiklikler sadece scene ve presenter apply hattindan gitmelidir

### 7. Pin ve Input Ayarlari
- Dosya:
  - `PinInputTuningWindow.cs`
- Veri kaynagi:
  - `Pin`
  - `PinLauncher`
  - `InputManager`
  - `InputBuffer`
  - `FireGate`
- Degistirdigi yuzey:
  - pin boyutu
  - pin hiz/ucus davranisi
  - swipe esigi
  - cooldown
  - input buffer davranisi
- Ayar gruplari:
  - `Pin gorseli`
  - `Launch davranisi`
  - `Input pacing`
- Play mode etkisi:
  - aktif sahnede anlik apply olur

### 8. Ritmik Donus ve Zorluk Ayarlari
- Dosya:
  - `RotatorRhythmTuningWindow.cs`
- Veri kaynagi:
  - `rhythm_profiles.json`
  - `difficulty_profiles.json`
  - `difficulty_tiers.json`
  - aktif `TargetRotator`
- Degistirdigi yuzey:
  - level pacing
  - base rotation
  - rhythm assist etkisi
  - difficulty profile degerleri
- Ayar gruplari:
  - `Rhythm profile`
  - `Difficulty profile`
  - `Difficulty tier`
  - `Baz donus preview`
- Play mode etkisi:
  - config refresh + aktif session/content refresh ile gorunur
- Onemli not:
  - bu editor yalnizca pacing verisini degistirmelidir
  - hit mekanigi veya slot geometri kodunu degistirmemelidir

### 9. Soru ve Fail Akisi Ayarlari
- Dosya:
  - `QuestionFailFlowTuningWindow.cs`
- Veri kaynagi:
  - `QuestionLifeManager`
  - `FailModalPresenter`
- Degistirdigi yuzey:
  - varsayilan soru cani
  - fail modal alanlari
  - continue/retry copy ve duzeni
- Ayar gruplari:
  - `Question life`
  - `Fail panel layout`
  - `CTA alanlari`

### 10. UI Yuzey Ayarlari
- Dosya:
  - `UiSurfaceTuningWindow.cs`
- Veri kaynagi:
  - sahnedeki UI presenter componentleri
  - `GameplayHudPresenter`
  - `FailModalPresenter`
  - `InfoCardPresenter`
  - `ResultPresenter`
  - `StorePresenter`
  - `MembershipPresenter`
  - `MainMenuPresenter`
- Degistirdigi yuzey:
  - panel rect’leri
  - metin boyutu/rengi/alignment
  - cevap kutusu renkleri
  - button placement
  - HUD spacing
  - result/info/store/membership/menu yuzeyleri
- Ayar gruplari:
  - `Gameplay HUD`
  - `Fail`
  - `Info Card`
  - `Result`
  - `Store`
  - `Membership`
  - `Main Menu`
- Play mode etkisi:
  - scene apply ile anlik gorunur
- Dikkat:
  - bu editor mevcut presenter’larin field’larini duzenler
  - layout hesaplayan ana gameplay mantigini degistirmemelidir

### 11. Hissiyat ve Gorsel Ayarlari
- Dosya:
  - `FeelVisualTuningWindow.cs`
- Veri kaynagi:
  - `ScoreTuningProfile`
  - impact feel profile
  - `ThemeRuntimeController`
- Degistirdigi yuzey:
  - puanlama parametreleri
  - impact/haptic/flash/shake parametreleri
  - theme runtime pressure etkileri
- Ayar gruplari:
  - `Skor`
  - `Impact`
  - `Tema runtime`

### 12. Tema ve Magaza Config
- Dosya:
  - `ThemeStoreConfigWindow.cs`
- Veri kaynagi:
  - `themes.json`
  - `store_catalog.json`
  - `membership_profile.json`
  - `energy_config.json`
- Degistirdigi yuzey:
  - tema metadata
  - store urunleri
  - membership urun tanimi
  - enerji config
- Ayar gruplari:
  - `Temalar`
  - `Store catalog`
  - `Membership`
  - `Energy`
- Play mode etkisi:
  - config refresh ile store ve runtime theme tarafi yenilenir

### 13. Tema Paketi Onizleme
- Dosya:
  - `ThemePackagePreviewWindow.cs`
- Veri kaynagi:
  - `themes.json`
- Degistirdigi yuzey:
  - kalici veri yazmaktan cok onizleme saglar
  - tema set butunlugunu kontrol eder
- Ayar gruplari:
  - secili tema onizleme
  - varlik/yol denetimi

### 14. Mobil Runtime ve Cihaz Ayarlari
- Dosya:
  - `MobileRuntimeTuningWindow.cs`
- Veri kaynagi:
  - `MobileRuntimeController`
- Degistirdigi yuzey:
  - safe area marjinleri
  - mobil runtime davranisi
  - cihaz odakli layout toleranslari

### 15. Ambiyans ve Pulse Ayarlari
- Dosya:
  - `AmbientPulseTuningWindow.cs`
- Veri kaynagi:
  - `ThemeRuntimeController`
  - `PulseSprite`
- Degistirdigi yuzey:
  - glow konum/olcek
  - ambience yerlesimi
  - pulse hiz ve amplitude
- Onemli not:
  - bu mikro-gorsel tuning’dir
  - gameplay mekanigine dokunmamali

### 16. Telemetry Politikasi
- Dosya:
  - `TelemetryPolicyWindow.cs`
- Veri kaynagi:
  - `TelemetryPolicyProfile.asset`
- Degistirdigi yuzey:
  - telemetry queue cap
  - flush davranisi
  - snapshot sayisi
  - pending count save davranisi
- Onemli not:
  - buradaki ayarlar performans ve disk kullanimi icindir
  - gameplay mantigi icin degildir

### 17. Gelistirici Telemetry Paneli
- Dosya:
  - `DeveloperTelemetryWindow.cs`
- Veri kaynagi:
  - `TelemetryService`
  - telemetry snapshot/state
- Degistirdigi yuzey:
  - kalici tuning degil
  - daha cok inspect/debug

### 18. Uzak Icerik ve Hotfix Ayarlari
- Dosya:
  - `RemoteContentHotfixWindow.cs`
- Veri kaynagi:
  - `remote_manifest_template.json`
  - remote override klasoru
- Degistirdigi yuzey:
  - remote manifest uretimi
  - override publish
  - runtime remote refresh
- Ayar gruplari:
  - manifest template
  - publish secenekleri
  - remote klasor temizligi
- Onemli not:
  - bu katman gelecekte cloud/liveops’a gecisin cekirdegi olacaktir

### 19. Global Validation ve Referans Taramasi
- Dosya:
  - `ValidationAuditWindow.cs`
- Veri kaynagi:
  - tum config ve content kataloglari
  - referans path’leri
- Degistirdigi yuzey:
  - runtime veri degistirmez
  - audit raporu uretir
- Kontrol ettigi alanlar:
  - duplicate id
  - missing locale/content
  - broken theme/store path
  - shape/answer uyumsuzlugu
  - referans kirliligi

### 20. Save ve Session Paneli
- Dosya:
  - `SaveSessionDebugWindow.cs`
- Veri kaynagi:
  - `SaveManager`
  - `GameManager`
  - economy/session state
- Degistirdigi yuzey:
  - debug reset
  - session temizleme
  - save inspect
- Dikkat:
  - bu panel debug aracidir
  - shipping runtime davranisini degistiren ana tuning paneli degildir

### 21. Android Build Araclari
- Dosya:
  - `AndroidDeviceBuildTools.cs`
- Veri kaynagi:
  - build pipeline
  - adb
- Gorevi:
  - APK build
  - device install/run
  - adb device check
- Not:
  - tuning editoru degil
  - fakat alpha surecinde zorunlu operasyon aracidir

## Editorler Arasi Uyumluluk Kurali
Bir ayar birden fazla editor tarafindan goruluyorsa su kurallar gecerlidir:

1. Veri sahibi tek olmalidir.
   - Ornek:
     - rhythm ayari JSON’daysa tek dogru kaynak JSON’dur
     - sahne preview yalnizca bunu uygular

2. `Kaydet/Uygula` eden editor `WordSpinAlphaEditorSyncUtility.NotifyChanged(...)` cagirir.

3. Ayni veriyi kullanan editorler `ConsumeChanges(...)` ile stale state’i temizler.

4. Editorler birbirinin yarim durumunu degil, commit edilmis state’ini okumalıdır.

5. Sahne apply zinciri gerekiyorsa `WordSpinAlphaEditorRuntimeRefreshUtility` uzerinden gitmelidir.

## Gelecekte Yeni Ayar Ekleme Calisma Prensibi
Bu proje icin en onemli kural:

**Yeni tuning eklenirken mekanik koda direkt dalinmaz. Once veri sahipligi secilir, sonra editor bu veri sahibine baglanir.**

### Yeni ayar eklemek icin zorunlu adimlar
1. Ayarin veri sahibini belirle.
   - sahne mi
   - `ScriptableObject` mi
   - JSON config mi
   - content mi
   - save/debug state mi

2. Ayarin uygulanma zamanini belirle.
   - hemen apply olabilir mi
   - sadece level basinda mi
   - sadece menu gecisinde mi
   - restart gerektirir mi

3. Cekirdek mekanige girmeden once presentation/config tarafinda cozum olup olmadigini kontrol et.

4. JSON kaynakliysa `WordSpinAlphaRuntimeConfigRepository` icine yukle/kaydet API’si ekle.

5. Sahne kaynakliysa hedef component icine editor-safe apply veya refresh noktasi ekle.
   - ornek:
     - `ApplyEditorTuning`
     - `RefreshForEditor`
     - `ApplyPreviewState`

6. Gerekli runtime refresh zincirini `WordSpinAlphaEditorRuntimeRefreshUtility` icine ekle.

7. Editorler arasi stale goruntu riski varsa `WordSpinAlphaEditorSyncUtility` icine uygun sync tipiyle bagla.

8. Ayar, tekli editor ve toplu editor icine ayni veri sahipligiyle eklenmelidir.

9. Yeni ayar, validation kuralina muhtacsa `ValidationAuditWindow` tarafina kontrol eklenmelidir.

10. Editor-only asset veya referans gorsel gerekiyorsa build’e girmeyecek klasorde tutulmalidir.

### Kesinlikle yapilmamasi gerekenler
- tuning icin cekirdek gameplay akisini editor penceresi icinden yeniden yazmak
- ayni veriyi iki farkli dosyada master kaynak yapmak
- play mode apply icin sahnede rastgele daginik `Find` zincirleri kurmak
- editor draft asset’lerini `Resources` altina atmak
- yarim edit state’i baska editorlere zorla yansitmak

## Basit Guvenli Genisletme Kalibi
Yeni bir ayar eklenecekse takip edilmesi gereken en guvenli sira:

1. veri modeli
2. repository veya asset baglantisi
3. hedef runtime apply noktasi
4. refresh utility baglantisi
5. sync bildirimi
6. tekli editor alani
7. toplu editor modulu
8. validation kuralı
9. play-mode smoke test

## Web Uygulamasi ve Cloud Live Config Gecis Plani

### Ana ilke
Mevcut Unity editorleri dogrudan web’e tasinmaz. Web panel, **ayni veri semalarini** kullanan baska bir authoring katmani olur.

Yani:

- Unity editor = lokal authoring ve hizli tuning araci
- Web panel = publish, rol bazli duzenleme, live config ve operasyon araci

### Hedef Mimari

#### 1. Admin web uygulamasi
Onerilen stack:

- `Next.js`
- `TypeScript`
- `React`
- `Tailwind` veya sade bir admin UI library

Gorevi:

- ayni JSON ve config semalarini form tabanli duzenlemek
- publish/rollback yapmak
- audit trail tutmak
- kim hangi ayari ne zaman degistirdi kaydetmek

#### 2. Kimlik ve veri tabani
Onerilen servis:

- `Supabase`

Kullanimi:

- admin auth
- rol yonetimi
- config metadata
- publish loglari
- revision history
- theme paket metadata

#### 3. Dosya/payload depolama
Onerilen servis:

- `Cloudflare R2`
veya
- `Supabase Storage`

Burada tutulacaklar:

- versioned manifest dosyalari
- kucuk JSON config payloadlari
- gerekirse theme paket bundle metadata
- remote localization/config paketleri

#### 4. Sunucu/edge publish katmani
Onerilen:

- Supabase Edge Functions
veya
- hafif bir Node publish service

Gorevi:

- editor/web panelden gelen degisikligi validate etmek
- yeni manifest versiyonu uretmek
- config payloadlarini versiyonlu klasore yazmak
- rollback icin onceki versiyonlari saklamak

#### 5. Hata ve saglik izleme
Onerilen:

- `Sentry`
- mobil crash takibi icin gerekirse `Firebase Crashlytics`

### Canli Oyuna Degisiklik Gonderme Modeli
Canli oyuna hicbir zaman “kod” gonderilmez. Gonderilen seyler:

- hafif config JSON’lari
- content JSON’lari
- theme metadata
- gerektiginde bundle referanslari

### Oyuncu cihazinda uygulanma sekli
Mevcut mimari korunarak su model kullanilmalidir:

1. Oyun acilista kucuk bir manifest ceker.
2. Manifest:
   - hangi kanal aktif
   - hangi config versiyonu aktif
   - hangi payload’lar guncellenmis
   bilgisini verir.
3. Sadece degisen kucuk payload’lar indirilir.
4. Bunlar `persistentDataPath` altindaki remote override alanina yazilir.
5. Mevcut `ContentService` / remote override hattı bu dosyalari local fallback ustune uygular.

### Guvenli apply anlari
Her config ayni anda apply edilmemelidir.

#### Aninda uygulanabilecekler
- telemetry policy
- store copy
- bilgi karti / text duzeltmeleri
- menu/store gibi kritik olmayan UI copy renkleri

#### Sahne yenilemesiyle uygulanabilecekler
- menu/store layout tuning
- theme runtime renk basıncı
- HUD metin ayarlari

#### Sadece yeni level/session ile uygulanmasi gerekenler
- rhythm ve difficulty
- shape atama
- question/fail davranis ayarlari
- economy dengesini etkileyen level parametreleri

#### Restart veya tam paket yenilemesi gerektirenler
- font paketleri
- buyuk theme asset bundle degisiklikleri
- temel rendering/resource seti degisiklikleri

### APK’yi sisirmeden live config
Su kurallar zorunludur:

1. Editor kodu build’e girmemeli.
2. Referans shape gorselleri build’e girmemeli.
3. Remote config yalnizca JSON ve kucuk metadata olmali.
4. Theme paketleri gerekiyorsa metadata + remote bundle modeliyle tasinmali.
5. Oyuna gereksiz telemetry backlog birikmemeli.
6. Remote payload’lar versiyonlanmali ve trim edilmelidir.

### Tema paketleri icin ozel strateji
Tema paketleri sonradan da eklenebilecegi icin su modele gecilmelidir:

- `theme metadata`
  - id
  - display name
  - palette
  - asset path veya bundle key
  - music/sfx referansi
  - UI variant referansi
- `theme payload`
  - buyuk assetler gerekiyorsa ayri bundle

Bu sayede:

- alpha demo bittikten sonra yeni tema eklemek kolay olur
- APK her yeni tema ile tekrar sismez
- web panel uzerinden sadece manifest/payload referansi eklenir

### Tavsiye edilen rollout modeli
- kanal tabanli:
  - `alpha`
  - `internal`
  - `live`
- her kanal icin:
  - ayri manifest
  - ayri rollback noktasi
- oyuncu istemcisi:
  - once local fallback
  - sonra remote override
  - sonra safe-point apply

## Oyun Kodlarina ve Mekaniklere Dokunmadan Web Panele Tasima Kurali
Web panel asamasinda yapilacak sey:

- editor UI’sini degil
- editorlerin baglandigi veri semalarini ve apply kurallarini tasimaktir

Yani web panelde ayni sey yeniden uygulanir:

1. dogru veri sahibi secilir
2. ayni schema kullanilir
3. ayni validation calisir
4. publish edilen sey JSON/config olur
5. runtime, bunu mevcut remote override akisindan okur

Boylece:

- Unity icindeki yerel editorler bozulmaz
- web panel ayni sistemin uzaktan authoring katmani olur
- cekirdek gameplay koduna web panel bagimliligi sokulmaz

## Alpha ve Market Oncesi Operasyon Kurallari

### Alpha surecinde
- mevcut editorler yerel authoring icin kullanilir
- web panel sadece veri modeli ve publish akisi acisindan tasarlanir
- remote config, minimum gerekli kapsamla tutulur

### Alpha bitince
- tek editor ve live-config akisi sabitlenir
- telemetry ve hotfix guvenlik kurallari sikilastirilir
- versiyonlama ve rollback kurallari kesinlestirilir

### Markete cikmadan once
- `Resources` altindaki gereksiz draft/editor bagimliliklari temizlenir
- remote override fallback testleri yapilir
- queue cap / trim politikasi gercek cihazda test edilir
- sadece guvenli apply kategorileri canliya acilir

## Baska Bir Yazilimci veya AI icin Handover Kurali
Bu projede editor veya live-config tarafina eklenecek her yeni sey icin su sorulari cevaplamadan kod degistirilmemelidir:

1. Bu ayarin veri sahibi kim?
2. Bu ayar scene mi, asset mi, JSON mu?
3. Bu ayar hangi runtime component’i etkiliyor?
4. Play mode’da hangi refresh zinciriyle apply olacak?
5. Baska hangi editor bu veriyi de goruyor?
6. Hangi sync tipiyle diger editorlere duyurulacak?
7. Bu ayar validation gerektiriyor mu?
8. Bu ayar safe-point disinda canli apply edilebilir mi?
9. Bu ayarin build boyutu veya telemetry maliyeti var mi?
10. Bu ayar web panele tasinacaksa payload tipi ne olacak?

Bu 10 soru cevaplanmadan yeni tuning eklemek, orta vadede teknik borc dogurur.

## Dokuman Bakim Kurali
Bu kılavuz her editor veya live-config katmani degisikliginde guncellenmelidir.

Zorunlu guncelleme durumlari:

- yeni bir tuning editoru eklendiginde
- bir editor yeni veri sahibine baglandiginda
- refresh veya sync davranisi degistiginde
- yeni JSON config dosyasi eklendiginde
- web panel/live-config publish akisinda veri modeli degistiginde

Bu dosya guncellenirken:

- eski kararlar silinmemeli
- degisen kararlarin ustune `guncel durum` veya `yeni kural` olarak ek yapilmali
- eger faz sirasi etkileniyorsa ayni degisiklik `UNIFIED_EDITOR_TO_LIVEOPS_PLAN.md` dosyasina da yansitilmalidir

Bu iki dokuman arasinda drift olusmasi, ileride hem editor parity hem de web panel gecisinde hata dogurur.

## Sonuc
Mevcut editor sistemi, asagidaki stratejiyle guvenli kalir:

- veri sahipligi tek tutulur
- editorler ortak repository/refresh/sync katmanlari uzerinden calisir
- mekanik mantik ile tuning yuzeyleri ayrilir
- web panel asamasinda editor UI degil, veri semalari ve publish akisi tasinir
- remote degisiklikler sadece guvenli anlarda uygulanir

Bu dokumanin bundan sonraki rolü:

- tek editor mimarisinin referansi olmak
- yeni tuning eklemelerinde kurallari sabitlemek
- web panel ve cloud live-config tasarimina temel olmak
- yeni bir gelistirici veya AI’nin projeye zarar vermeden ekleme yapabilmesini saglamak
---

## 08.04.2026 - Hub / Pause / Resume Mimari Guncellemesi

08.04.2026 tarihinde oyun ici navigasyon omurgasi guncellendi. Bu karar, bundan sonra hem Unity icindeki local editor islerinde hem de ileride kurulacak web panel / live-config tarafinda referans kabul edilmelidir.

### Mevcut davranis

- `MainMenuPresenter`
  - artik giris ekrani rolundedir
  - dil secimi ve `Oyna` akisina hizmet eder
- `HubPresenter`
  - seviye secim yolunu
  - alt menulu sayfa gecislerini
  - secili level kartini
  - resume / restart popup mantigini
  yonetir
- `GameplayPausePresenter`
  - gameplay ustunde pause popup acilmasini
  - oyun donmasini
  - `Devam Et` ve `Ana Merkeze Don` davranisini
  yonetir
- `SceneNavigator`
  - `MainMenu` ve `Hub` rollerini artik yeni akisa gore ayirir

### Veri ve state semantigi

- aktif level session'i gameplay kapanmadan once `SessionManager.TakeSnapshot()` ile alinabilir durumda kalmali
- hub'a donus oncesi `SaveManager.FlushNow()` ile disk yazimi zorlanir
- ayni levele geri donuste `HubPresenter` session varligini kontrol eder
- ayni level icin:
  - resume varsa `Devam Et / Bastan Basla / Vazgec` popup'i acilir
  - resume yoksa normal giris yapilir

### Bu yapida neye dokunurken dikkat edilmeli

Bu akisi degistirecek herkes su kurallara uymali:

1. `pause` davranisini direkt scene jump'e geri cevirmemeli
2. `resume` davranisini enerji/can maliyetine baglamamali
3. `restart` davranisini `resume` ile ayni semantic kategoriye koymamali
4. hub'a donus oncesi snapshot/flush zincirini kaldirmamali
5. info card, result ve fail modal input kilitleri ile pause popup mantigini birbirine karistirmamali

### Editor ve web panel etkisi

Bu yeni akista ileride editor veya web panelden degistirilebilecek seyler:

- popup metinleri
- popup renkleri / stil tokenlari
- hub alt menu sekmeleri
- hub icindeki UX teklif kartlari
- seviye karti copy ve buton etiketleri

Ama web panelden rastgele degistirilmemesi gereken seyler:

- pause -> hub -> resume cekirdek karari
- resume maliyetsiz / restart maliyetli semantigi
- save snapshot ve flush zorunlulugu

### Handoff notu

Yeni bir yazilimci veya model bu yapiya ekleme yaparken once su dosyalari birlikte okumali:

- `SceneNavigator.cs`
- `MainMenuPresenter.cs`
- `HubPresenter.cs`
- `GameplayPausePresenter.cs`
- `GameSceneNavigationButtons.cs`
- `GameManager.cs`
- `SaveManager.cs`

Bu dosyalar birlikte degerlendirilmeden hub/gameplay/session akisina mudahale edilmemelidir.

---

## 08.04.2026 - Tasarim Kompozisyonundan Unity Asset Uretim Kurali

08.04.2026 itibariyla Stitch benzeri bir tasarim aracindan gelen tam ekran kompozisyonlar icin su kural kabul edilmistir:

### Ana ilke

Unity'ye:

- ne tek parca final ekran PNG'si yuklenecek
- ne de sahnedeki her kucuk parca kontrolsuz sekilde tek tek export edilecektir

Dogru model:

- kompozisyon once tam ekran olarak tasarlanir
- sonra teknik olarak anlamli katmanlara bolunur
- Unity'de mevcut scene/prefab iskeletine skin olarak baglanir

### Teknik ayrim

#### A. Buyuk statik gorseller

Bunlar buyuk katmanlar halinde alinabilir:

- gokyuzu
- sehir / ada / yol arka plani
- buyuk dekoratif foreground/backdrop katmanlari

Bu tip asset'ler:

- genelde `PNG`
- cogunlukla interaktif olmayan
- sahnenin atmosferini tasiyan
katmanlardir.

#### B. Ayrik UI skin asset'leri

Bunlar ayri alinmalidir:

- alt menu butonlari
- top bar skinleri
- kart arka planlari
- teklif butonlari
- level badge skinleri
- popup panelleri
- ikonlar

Bu katmanlar tekrar kullanilir ve prefab'a baglanir.

#### C. Unity'de uretilmesi gereken seyler

Bunlar asla gorselin icine gomulmemelidir:

- dinamik metinler
- localization metinleri
- sayaclar
- level numaralari
- bildirim badge adetleri
- countdown / sureler
- coin / enerji / ipucu degerleri

### Pratik teslim modeli

Bu projede tasarim teslim akisi su sekildedir:

1. tasarimci once tek tam ekran kompozisyonu yollar
2. gelistirici Unity sahnesi ile bunu karsilastirir
3. gelistirici teknik export listesi uretir
4. tasarimci sadece istenen katmanlari ayri export eder
5. bu katmanlar Unity sahnesine prefab ve sprite skin olarak yerlestirilir

### Gelistiricinin verecegi export listesi formati

Beklenen teknik liste su tipte olur:

- dosya adi
- kullanim amaci
- seffaf arka plan gerekli mi
- `9-slice` uygun mu
- Unity'de hangi objeye baglanacak
- text veya ikon mu
- tek buyuk layer mi yoksa tekrar kullanilan skin mi

Ornek:

- `BG_RoadBase.png`
- `BTN_BottomNav_Journey.png`
- `BTN_BottomNav_Store.png`
- `CARD_LevelActive.png`
- `POPUP_PausePanel.png`
- `ICON_Coin.png`

### Unity klasorleme kurali

Bu tip asset'ler ileride karismasin diye kategori bazli tutulmalidir:

- `Assets/WordSpinAlpha/Art/UI/Hub/Backgrounds`
- `Assets/WordSpinAlpha/Art/UI/Hub/Buttons`
- `Assets/WordSpinAlpha/Art/UI/Hub/Cards`
- `Assets/WordSpinAlpha/Art/UI/Hub/Icons`
- `Assets/WordSpinAlpha/Art/UI/Common/Popups`

Bu klasorleme daha sonra:

- atlaslama
- tema ayirma
- build sizinti kontrolu

icin de fayda saglar.

### Hangi alanlar Unity prefab olarak kalmali

Asagidaki yuzeyler tasarim skin'i alsa bile Unity prefab ve component yapisinda kalmalidir:

- hub bottom navigation
- level node sistemleri
- pause popup
- resume / restart popup
- store teklif kartlari
- gorev kartlari
- bildirim badge'leri

### Web panel ve live-config etkisi

Bu karar gelecekteki web panel acisindan da dogrudur. Cunku:

- metinler ve dinamik degerler Unity component'lerinde kalir
- skin asset'leri ile veri birbirinden ayrik olur
- uzaktan gelen copy/offer/layout tuning'i gorselin icine gomulu kalmaz

### Handoff kuralı

Yeni bir yazilimci veya model, tasarim aktarma isinde once su sorulari cevaplamalidir:

1. Bu alan statik arka plan mi?
2. Bu alan tekrar kullanilan UI skin'i mi?
3. Bu alan runtime text veya data mi?
4. Bu alan `9-slice` panel olmali mi?
5. Bu alan prefab olarak mi kalmali?
6. Bu alan localization aliyor mu?
7. Bu alan sonradan web panel veya live-config ile degisebilir mi?

Bu sorular cevaplanmadan tasarim asset'i import edilmemelidir.

---

## 19.04.2026 - HubPreview Ray Editor ve Alpha Demo Son Faz Kurali

19 Nisan 2026 itibariyla `HubPreview` yalnizca PNG tasarim deneme sahnesi degil, level hub yol/ray ayari icin de resmi preview sahnesidir.

### Yeni editor katmani

Level hub yolu icin editor destekli ray sistemi eklendi:

- `Assets/WordSpinAlpha/Scripts/Presentation/LevelHubPreviewController.cs`
- `Assets/WordSpinAlpha/Scripts/Editor/LevelHubPreviewControllerEditor.cs`

Bu editorun amaci:

- arka plan gorseli sahnede gorunurken ray noktalarini manuel yerlestirmek
- level kutularinin bu ray uzerinde akmasini test etmek
- yeni arka plan tasarimi geldiginde kod yazmadan yol hizalamasi yapmak
- onaylanan sahne/prefab ana Hub akisana tasindiginda ayarlari kaybetmemek

### Neden veri controller uzerinde tutuluyor

Ray verisi ayri bir gecici editor dosyasinda tutulmaz. `LevelHubPreviewController` uzerinde serialize edilir.

Sebep:

- `HubPreview` sahnesi kopyalaninca ray ayarlari da kopyalanir
- ana `Hub` sahnesine aktarimda hangi profile bakilacagi belirsiz kalmaz
- prefab instance override mantigi Unity icinde dogal sekilde calisir
- editor otomatik olarak aktif sahnedeki veya secili objedeki controller'i hedefler

Bu, "HubPreview'de ayarla, onaylaninca kopyalayip production akisa al" kuralina uygundur.

### Ray editor kullanim sirasi

1. Level hub arka plan PNG'si `HubPreview` sahnesine yerlestirilir.
2. `LevelHubPreviewController` bulunan obje secilir.
3. Scene View'de arka plan uzerindeki ray noktalarinin handle'lari gorulur.
4. Noktalar yol hattina manuel suruklenir.
5. Inspector'da her nokta icin `position`, `scale`, `rotation`, `alpha` ayarlanir.
6. Drag/scroll preview yapilir.
7. Tasarim onaylanirsa sahne/prefab ana Hub tarafina kontrollu aktarilir.

### MainMenu arka plan kaplama referansi

MainMenu arka planinda 19 Nisan 2026'da su kural netlesti:

- kaynak PNG'nin 9:16 portrait olmasi tek basina yeterli degildir
- `MainMenuPngPreview.prefab` icindeki arka plan katmani canvas/safe area kombinasyonunda bosluk birakmayacak boyutta olmalidir
- `WordSpinAlphaSceneBuilder.cs` ayni kaplama davranisini yeniden uretebilmelidir
- yeni arka plan eklendiginde hem prefab hem builder ciktisi kontrol edilmelidir

Bu bilgi live-config degil, presentation pipeline kuralidir. Oyuncu verisi, ekonomi, save veya level progression tarafina tasinmamalidir.

### Alpha demo disiplinine etkisi

Bu tarihten sonra alpha demo oncesi ana kural:

- yeni buyuk sayfa ekleme
- yeni buyuk mekanik ekleme
- mevcut iki yeni yuzeyi test et:
  - tasarimli `MainMenu`
  - level hub preview/ray editor akisi
- sadece kucuk UX polish ve bug fix yap

### Ana oyun akisi korunacak alanlar

Ray editoru su sistemlere dogrudan mudahale etmemelidir:

- save/session
- economy
- gameplay input
- fail/continue
- score
- store purchase/pricing
- production `Hub` akisi, onayli promote disinda

Ray editoru presentation/test tooling katmanidir. Runtime oyuncu verisini degistirmez.

### Gelecekte web/live-config ile iliski

Bu ray sistemi ileride live-config tarafina tasinacaksa once veri semasi netlestirilmelidir:

- ray point listesi
- scale/rotation/alpha degerleri
- aktif level platform pozisyonu
- node pool size
- snap ve drag hassasiyeti

Ancak alpha demo icin bu veriler once Unity editor uzerinde sabitlenir. Web panel bu fazda hedef degildir.

## 20. 21.04.2026 - HubPreview Recovery Sonrasi Editor Disiplini

21 Nisan 2026'da `HubPreview` level hub node boyutu duzeltmesi sirasinda sahne/prefab kirliligi olusmustur. Recovery sonrasi kabul edilen editor disiplini su sekildedir:

- `LevelHubPreviewController` hierarchy uretmez
- `WordSpinAlphaSceneBuilder` node gorsel mimarisinin tek kaynagidir
- `HubPreviewSceneNormalizer` yalnizca sahne kokunde birikmis orphan `NodeVisual` objelerini temizler
- temizleme veya recovery icin scene YAML regex ile toplu duzenlenmez

Recovery sonrasi dogrulanan durum:

- `Assets/WordSpinAlpha/Generated/Prefabs/LevelHubPreview.prefab` temiz kaynak olarak yeniden olusturulmustur
- `Assets/WordSpinAlpha/Scenes/HubPreview.unity` tekrar rebuild edilmistir
- orphan `NodeVisual` root objeleri temizlenmistir
- kirli prefab instance override birikimi sifirlanmistir

Kalan acik is:

- node boyutu/orani hedefe yaklastirilmistir
- fakat level kutularinin altinda beyaz zemin gorunumu devam etmektedir

Bu nedenle sonraki editte uygulanacak kural nettir:

1. recovery katmanina yeniden mudahale etme
2. ray/path editor verisini elle veya kodla yeniden yazma
3. sadece `NodeVisual` gorsel katmani, sprite alpha ve node altindaki beyaz zemin kaynagini incele
4. degisiklik once preview/prefab/builder ucgeninde dogrulanir, sonra sahneye bakilir

Bu madde, sonraki sayfa kurulumlari icin de geneldir:

- oran/boyut sorunu varsa once asset alpha ve bos canvas kontrol edilir
- sonra prefab rect'i
- sonra builder uretimi
- en son scene rebuild

Bu sira disina cikilmaz.
