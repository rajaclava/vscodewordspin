# Teknik Zihin Haritasi

Bu dokuman, Unity projesinin teknik yapisini baska bir AI modeliyle tartismak, planlamak veya yeni is cikarmak icin hazirlandi. Kodun tamami burada verilmez; ana klasorler, siniflar, veri kaynaklari ve sistemler arasi baglanti mantigi ozetlenir.

## Snapshot ve Tarih Notu

Bu teknik haritanin ana snapshot tarihi:

- `06.04.2026`

Tarih bazli ozet:

### 03.04.2026

- ekonomi sandbox, pricing abstraction ve test mode katmanlari sisteme girdi

### 04.04.2026

- icerik editoru, shape authoring ve content canli apply zinciri teknik mimariye eklendi

### 06.04.2026

- eksik tuning editorleri tamamlandi
- tek shell editor ve editorler arasi sync katmani sisteme eklendi
- bu nedenle bu harita, hem tekli editorleri hem de toplu editor kabugunu birlikte anlatan bir mimari snapshot’tir

Onemli not:

- Bu dokuman zamanlama plani degildir.
- 06.04.2026 itibariyla projedeki mevcut teknik durumun haritasidir.
- Bu snapshot'a gore hemen sonraki is, yeni editor kurmak degil mevcut editor/runtime parity'sini dogrulamak ve bug sweep fazina girmektir.

---

## 1. Dosya Mimarisi

### Assets/WordSpinAlpha altindaki ana klasorler

- `Art/`
  - tema, sprite, gorsel pipeline ciktilari
- `Data/`
  - proje icin yardimci veri / uretilmis taslaklar
- `Docs/`
  - master plan, ekonomi raporu, bu teknik harita gibi dokumanlar
- `Generated/`
  - editor tarafinda uretilmis ara ciktilar / destek verileri
- `Resources/`
  - runtime `Resources.Load` ile yuklenen JSON config, locale content ve ScriptableObject assetleri
- `Scenes/`
  - `Boot`, `MainMenu`, `Gameplay`, `Store` ve kurulum rehberleri
- `Scripts/`
  - asagidaki ana kod katmanlari burada

### Scripts klasor yapisi

#### `Scripts/Core`
Ana runtime, state, save, gameplay ve manager katmani.

Onemli scriptler:

- `BootLoader.cs`
- `SceneBootstrap.cs`
- `SceneNavigator.cs`
- `SaveManager.cs`
- `PlayerSaveModels.cs`
- `SessionManager.cs`
- `GameManager.cs`
- `LevelFlowController.cs`
- `HitEvaluator.cs`
- `SlotManager.cs`
- `Slot.cs`
- `TargetRotator.cs`
- `PinLauncher.cs`
- `Pin.cs`
- `QuestionLifeManager.cs`
- `ScoreManager.cs`
- `ScoreTuningProfile.cs`
- `EconomyManager.cs`
- `LevelEconomyManager.cs`
- `EconomyBalanceProfile.cs`
- `EnergyManager.cs`
- `InputManager.cs`
- `InputBuffer.cs`
- `TestPlayerModeManager.cs`
- `TestPlayerModeProfile.cs`
- `MobileRuntimeController.cs`
- `GameEvents.cs`
- `Singleton.cs`

#### `Scripts/Presentation`
UI, HUD, modallar, runtime tema ve feedback katmani.

Onemli scriptler:

- `MainMenuPresenter.cs`
- `GameplayHudPresenter.cs`
- `GameplaySceneTuner.cs`
- `KeyboardPresenter.cs`
- `KeyboardLayoutTuningProfile.cs`
- `GameSceneNavigationButtons.cs`
- `InfoCardPresenter.cs`
- `ResultPresenter.cs`
- `FailModalPresenter.cs`
- `StorePresenter.cs`
- `MembershipPresenter.cs`
- `ThemeRuntimeController.cs`
- `RotatorPlaquePresenter.cs`
- `ImpactFeedbackController.cs`
- `ImpactFeelProfile.cs`
- `DebugRewardedAdPresenter.cs`

#### `Scripts/Services`
Content, telemetry, purchase ve pricing abstraction katmani.

Onemli scriptler:

- `ContentService.cs`
- `IContentProvider.cs`
- `LocalContentProvider.cs`
- `RemoteContentProvider.cs`
- `TelemetryService.cs`
- `TelemetryModels.cs`
- `MetricLogger.cs`
- `IPurchaseService.cs`
- `MockPurchaseService.cs`
- `IStorePricingProvider.cs`
- `StorePricingManager.cs`
- `PreviewStorePricingProvider.cs`

#### `Scripts/Content`
JSON modelleri ve locale/content veri tipleri.

- `WordSpinContentModels.cs`
- `ShapeLayoutGeometry.cs`

#### `Scripts/Editor`
Editor tooling, scene builder, tuning pencereleri ve Android build yardimcilari.

Onemli scriptler:

- `WordSpinAlphaSceneBuilder.cs`
- `WordSpinAlphaLevelGenerator.cs`
- `GameplaySceneTunerEditor.cs`
- `WordSpinAlphaUnifiedEditorWindow.cs`
- `WordSpinAlphaEditorRuntimeRefreshUtility.cs`
- `WordSpinAlphaRuntimeConfigRepository.cs`
- `WordSpinAlphaEditorSyncUtility.cs`
- `WordSpinAlphaEditorUiTuningUtility.cs`
- `KeyboardLayoutTuningWindow.cs`
- `EconomyBalanceWindow.cs`
- `WordSpinAlphaContentEditorWindow.cs`
- `WordSpinAlphaContentEditorData.cs`
- `SlotHitTuningWindow.cs`
- `PinInputTuningWindow.cs`
- `RotatorRhythmTuningWindow.cs`
- `QuestionFailFlowTuningWindow.cs`
- `UiSurfaceTuningWindow.cs`
- `FeelVisualTuningWindow.cs`
- `ThemeStoreConfigWindow.cs`
- `ThemePackagePreviewWindow.cs`
- `MobileRuntimeTuningWindow.cs`
- `AmbientPulseTuningWindow.cs`
- `TelemetryPolicyWindow.cs`
- `RemoteContentHotfixWindow.cs`
- `ValidationAuditWindow.cs`
- `SaveSessionDebugWindow.cs`
- `DeveloperTelemetryWindow.cs`
- `AndroidDeviceBuildTools.cs`

### Resources klasoru

#### `Resources/Configs`
Runtime config ve ScriptableObject assetleri.

Onemli dosyalar:

- `energy_config.json`
- `keyboard_config.json`
- `store_catalog.json`
- `membership_profile.json`
- `ScoreTuningProfile.asset`
- `KeyboardLayoutTuningProfile.asset`
- `EconomyBalanceProfile.asset`
- `TestPlayerModeProfile.asset`

#### `Resources/Content`
Ana icerik kataloglari.

Onemli dosyalar:

- `levels.json`
- `questions.json`
- `themes.json`
- `info_cards.json`
- `campaigns.json`
- `difficulty_profiles.json`
- `difficulty_tiers.json`
- `rhythm_profiles.json`
- `shape_layouts.json`

#### `Resources/Content/Locales`
`tr / en / de / es` icin locale bazli content dosyalari.

---

## 2. Sinif Iliskileri (Dependencies)

Bu projede iki ana baglanti modeli var:

- **Singleton tabanli global manager'lar**
- **GameEvents uzerinden event-based iletisim**

### Singleton / servis omurgasi

`SceneBootstrap` gameplay/store/main menu gibi sahnelerde ayakta olmasi gereken singleton'lari garanti eder.

Ana singleton zinciri:

- `SceneBootstrap`
  -> `SaveManager`
  -> `LocalContentProvider`
  -> `RemoteContentProvider`
  -> `ContentService`
  -> `MetricLogger`
  -> `TelemetryService`
  -> `TestPlayerModeManager`
  -> `EconomyManager`
  -> `EnergyManager`
  -> `QuestionLifeManager`
  -> `InputManager`
  -> `MockPurchaseService`
  -> `PreviewStorePricingProvider`
  -> `StorePricingManager`
  -> `SceneNavigator`
  -> `StatsManager`
  -> `ScoreManager`
  -> `LevelEconomyManager`
  -> `MobileRuntimeController`
  -> `DebugRewardedAdPresenter`

### Event-based omurga

`GameEvents` proje icindeki ana event bus'tir.

Ana event tipleri:

- level ve question:
  - `LevelStarted`
  - `QuestionStarted`
  - `QuestionCompleted`
  - `LevelCompleted`
- input ve hit:
  - `PinLoaded`
  - `PinFired`
  - `PinReleased`
  - `HitEvaluated`
  - `ImpactOccurred`
  - `ImpactFeelResolved`
- score ve ekonomi:
  - `ScoreChanged`
  - `LevelScoreFinalized`
  - `LevelEconomyFinalized`
  - `SoftCurrencyChanged`
  - `EntryEnergyChanged`
  - `MembershipChanged`
- UI ve durum:
  - `TargetSlotUpdated`
  - `QuestionHeartsChanged`
  - `QuestionFailed`
  - `InfoCardRequested`
  - `InfoCardClosed`
  - `FailModalRequested`
  - `LanguageChanged`

### Ana sinif bagimlilik haritasi

#### Sahne acilis ve navigation

- `BootLoader`
  -> `SceneNavigator.OpenMainMenu()`

- `MainMenuPresenter`
  -> `SceneNavigator.OpenGameplayForProgress()`
  -> `SceneNavigator.OpenGameplayLevel(...)`
  -> `GameEvents.LanguageChanged`
  -> `SaveManager`

- `SceneNavigator`
  -> `SaveManager`
  -> `EnergyManager`
  -> `SessionManager`
  -> `ContentService`
  -> `SceneManager.LoadScene`

#### Gameplay ana loop

- `GameManager`
  -> `LevelFlowController`
  -> `HitEvaluator`
  -> `SessionManager`
  -> `QuestionLifeManager`
  -> `SceneNavigator`
  -> `SaveManager`
  -> `GameEvents`

- `LevelFlowController`
  -> `ContentService`
  -> `SlotManager`
  -> `TargetRotator`
  -> `InputBuffer`
  -> `PinLauncher` (restore sirasinda)
  -> `QuestionLifeManager`
  -> `GameEvents`

- `InputManager`
  -> `ContentService` (keyboard config)
  -> `KeyboardPresenter` tarafindan kullanilir

- `KeyboardPresenter`
  -> `InputManager.ProcessLetterButton(...)`
  -> `GameEvents.PinReleased`
  -> `GameEvents.QuestionFailed`
  -> `GameEvents.LevelCompleted`
  -> `GameEvents.LanguageChanged`

- `PinLauncher`
  -> `InputManager.LetterPressed`
  -> `InputManager.SwipeUpRequested`
  -> `InputBuffer`
  -> `FireGate`
  -> `GameManager`
  -> `ContentService` / `ThemeCatalog`
  -> `GameEvents`

- `Pin`
  -> hareket ve collision davranisi
  -> `GameManager.ResolvePinHit(...)` veya miss akisi

- `HitEvaluator`
  -> `SlotManager.EvaluatePlaqueHit(...)`

- `SlotManager`
  -> aktif hedef slot
  -> plaque hit zonlari
  -> slot layout / random slot target davranisi

#### Score ve ekonomi

- `ScoreManager`
  -> `GameEvents.HitEvaluated`
  -> `GameEvents.LevelCompleted`
  -> `ScoreTuningProfile`
  -> `GameEvents.ScoreChanged`
  -> `GameEvents.LevelScoreFinalized`

- `LevelEconomyManager`
  -> `EconomyBalanceProfile`
  -> `TestPlayerModeManager.AppliedMode`
  -> `SaveManager.Data.progress.levelRewards`
  -> `EconomyManager.GrantSoftCurrency(...)`
  -> `GameEvents.LevelEconomyFinalized`

- `EconomyManager`
  -> `SaveManager.Data.economy`
  -> `ContentService.LoadStoreCatalog()`
  -> `TestPlayerModeManager` (membership / no-ads override)
  -> `GameEvents.SoftCurrencyChanged`
  -> `GameEvents.MembershipChanged`

- `EnergyManager`
  -> `SaveManager.Data.energy`
  -> `ContentService.LoadEnergyConfig()`
  -> `TestPlayerModeManager` (energy override)
  -> `GameEvents.EntryEnergyChanged`

#### Store ve monetization test katmani

- `StorePresenter`
  -> `EconomyManager`
  -> `LevelEconomyManager`
  -> `StorePricingManager`
  -> `MockPurchaseService`
  -> `SceneNavigator`
  -> `GameEvents.LanguageChanged`
  -> `GameEvents.SoftCurrencyChanged`
  -> `GameEvents.MembershipChanged`

- `StorePricingManager`
  -> aktif fiyat kaynagi
  -> su an `PreviewStorePricingProvider`

#### Icerik editoru ve canli apply zinciri

- `WordSpinAlphaUnifiedEditorWindow`
  -> mevcut tum tuning editorlerini tek shell icinde host eder
  -> dogrudan veri sahibi degildir
  -> tekli editor parity'sini bozmadan merkezi erisim yuzeyi saglar

- `WordSpinAlphaContentEditorWindow`
  -> `WordSpinAlphaContentEditorData`
  -> locale JSON dosyalari
  -> `WordSpinAlphaEditorSyncUtility`
  -> `GameManager.ReloadCurrentSessionForEditorContent()`
  -> `MainMenuPresenter.RefreshEditorContent()`

- `WordSpinAlphaContentEditorData`
  -> `ShapeLayoutGeometry`
  -> `AssetDatabase`
  -> `ContentService.RefreshEditorContent()` ile dolayli runtime yenileme

- `WordSpinAlphaRuntimeConfigRepository`
  -> `themes.json`
  -> `store_catalog.json`
  -> `membership_profile.json`
  -> `energy_config.json`
  -> `difficulty_profiles.json`
  -> `difficulty_tiers.json`
  -> `rhythm_profiles.json`
  -> `shape_layouts.json`

- `WordSpinAlphaEditorRuntimeRefreshUtility`
  -> `ContentService.RefreshEditorContent()`
  -> `RefreshRotatorPresentation()`
  -> `RefreshThemePresentation()`
  -> `RefreshCurrentTargetState()`
  -> play mode icinde editor apply zincirini merkezilesir

- `WordSpinAlphaEditorSyncUtility`
  -> `Scene`
  -> `RuntimeConfig`
  -> `Content`
  -> `ScriptableAssets`
  -> `Telemetry`
  -> commit-sonrasi editorler arasi stale state'i temizler

- `ShapeLayoutGeometry`
  -> `SlotManager`
  -> `RotatorPlaquePresenter`
  -> `WordSpinAlphaContentEditorWindow`

Bu zincir shape noktasi, referans gorsel, plaque visual layout ve runtime slot yerlesimini ayni veri modeli uzerinden baglar.

- `PreviewStorePricingProvider`
  -> `LevelEconomyManager.Profile.RegionalPricePreviews`
  -> `ContentService.LoadStoreCatalog()`

- `MockPurchaseService`
  -> `EconomyManager`
  -> `EnergyManager`
  -> `GameEvents.Metric`

- `TestPlayerModeManager`
  -> `TestPlayerModeProfile`
  -> `SaveManager`
  -> `EconomyManager`
  -> `EnergyManager`
  -> `QuestionLifeManager`
  -> `LevelEconomyManager`
  -> `GameEvents`

- `FailModalPresenter`
  -> `GameManager`
  -> `EconomyManager`
  -> `EnergyManager`
  -> `DebugRewardedAdPresenter`
  -> `TestPlayerModeManager`

#### UI ve presentation dinleyicileri

- `GameplayHudPresenter`
  -> `GameEvents.QuestionStarted`
  -> `GameEvents.LetterRevealed`
  -> `GameEvents.TargetSlotUpdated`
  -> `GameEvents.ScoreChanged`
  -> `GameEvents.SoftCurrencyChanged`
  -> `GameEvents.QuestionHeartsChanged`
  -> `GameEvents.LevelCompleted`
  -> `GameEvents.LanguageChanged`

- `InfoCardPresenter`
  -> `ContentService.LoadInfoCards()`
  -> `GameEvents.InfoCardRequested`
  -> `GameEvents.InfoCardClosed`
  -> `GameEvents.LanguageChanged`

- `ResultPresenter`
  -> `GameEvents.LevelCompleted`
  -> `GameEvents.LevelScoreFinalized`
  -> `GameEvents.LevelEconomyFinalized`
  -> `SceneNavigator`
  -> `SaveManager`
  -> `GameEvents.LanguageChanged`

- `ThemeRuntimeController`
  -> tema asset ve audio/sprite runtime baglantisi
  -> dil ve ekonomi/hud ile dolayli iliskiler

- `ImpactFeedbackController`
  -> `GameEvents.ImpactOccurred`
  -> `ImpactFeelProfile`
  -> `GameEvents.ImpactFeelResolved`

---

## 3. Oyun Akis Mantigi (Game Loop)

### Baslangic akisi

1. `Boot` sahnesi acilir.
2. `BootLoader.Start()` calisir.
3. `SceneNavigator.OpenMainMenu()` ile `MainMenu` yuklenir.

### MainMenu -> Gameplay gecisi

1. `MainMenuPresenter` dil, play veya level select secimini alir.
2. `SceneNavigator`:
   - gerekiyorsa enerjiyi tuketir
   - kayitli oturumu resume edip etmeyecegine karar verir
   - hedef gameplay istegini `pending` olarak tutar
   - `Gameplay` sahnesini yukler

### Gameplay sahnesi yuklenince

1. `SceneBootstrap` gerekli singleton'lari olusturur.
2. `GameManager.Start()` calisir.
3. `GameManager` su uc ihtimalden birini secer:
   - `SceneNavigator` uzerinden gelen pending gameplay request'i tuketir
   - kayitli aktif session varsa restore eder
   - hicbiri yoksa progress'e gore uygun level'i baslatir

### Level baslatma

1. `GameManager.StartLevel(...)`
2. `LevelFlowController.LoadLevel(levelId)`
3. Content cache'leri build edilir:
   - levels
   - questions
   - difficulty profiles
   - rhythm profiles
   - shape layouts
4. `TargetRotator` ve `SlotManager` aktif level verisine gore ayarlanir.
5. `QuestionLifeManager.ResetQuestionHearts()`
6. `LevelFlowController.LoadCurrentQuestion()` cagrilir.
7. `GameEvents.LevelStarted` ve `GameEvents.QuestionStarted` yayilir.
8. HUD, keyboard, theme, rotator ve diger presenter'lar eventlerden beslenir.

### Input -> Pin -> Hit zinciri

1. `KeyboardPresenter` locale layout'a gore tuslari kurar.
2. Tus basimi:
   - `KeyboardPresenter -> InputManager.ProcessLetterButton(letter, screenPos)`
3. `InputManager`:
   - `LetterPressed`
   - `SwipeUpRequested`
   eventlerini uretir
4. `PinLauncher` bu eventlere baglidir:
   - harf basiminda pin yukler
   - swipe yukari ile pimi firlatir
5. `Pin` hedefe gider.
6. Collision/hit sonucunda `GameManager.ResolvePinHit(...)` veya miss akisi calisir.
7. `HitEvaluator -> SlotManager.EvaluatePlaqueHit(...)`
8. Sonuca gore:
   - dogru harf reveal edilir
   - hata ise can duser
   - `QuestionLifeManager` fail durumunu tetikler
   - `ScoreManager` skor toplar
   - `ImpactFeedbackController` hissiyat uretir

### Soru ve level tamamlama

1. Tum harfler acilinca `GameManager.HandleQuestionCompleted()`
2. Info card varsa:
   - pending state save'e yazilir
   - `GameEvents.InfoCardRequested`
3. Info card kapaninca:
   - yeni soruya gecilir veya level complete akisi baslar
4. Level complete oldugunda:
   - `GameEvents.LevelCompleted`
   - `ScoreManager` summary finalize eder
   - `LevelEconomyManager` stars/coin hesaplar
   - `ResultPresenter` sonucu gosterir

### Save / restore akisi

1. `SessionManager` su zamanlarda snapshot alir:
   - harf reveal sonrasinda
   - menuye giderken
   - pause
   - quit
2. `SaveManager` JSON save'i `Application.persistentDataPath` altina yazar.
3. Oyun yeniden acildiginda:
   - aktif session varsa `LevelFlowController.RestoreSession(...)`
   - `GameManager` pending fail/info/result durumlarini restore eder

### Editor -> runtime canli apply akisi

1. `WordSpinAlphaContentEditorWindow` icinde veri degistirilir.
2. `Tumunu Kaydet` veya play modda `Kaydet ve Canli Uygula` secilir.
3. Repository ilgili locale/content/shape JSON dosyalarini yeniden yazar.
4. `ContentService.RefreshEditorContent()` lokal cache'i yeniler.
5. Gameplay aciksa `GameManager.ReloadCurrentSessionForEditorContent()`:
   - mevcut snapshot'i alir
   - cache'leri yeniler
   - aktif session'i yeni icerikle geri yukler
6. Main menu aciksa `MainMenuPresenter.RefreshEditorContent()` level listesi ve ilgili menu state'ini yeniler.

### Store akisi

1. Gameplay veya MainMenu'den `SceneNavigator.OpenStore()`
2. Gameplay'den gidiliyorsa `SessionManager.TakeSnapshot()`
3. `StorePresenter`:
   - coin bakiyesi
   - theme lock durumu
   - preview fiyatlar
   - membership durumu
   bilgilerini refresh eder

### Default / Free / Premium test akisi

1. `EconomyBalanceWindow` uzerinden aktif test modu secilir.
2. `Aktif Modu Kayda Uygula` denir.
3. `TestPlayerModeManager`:
   - mevcut modu snapshot alir
   - hedef modun snapshot'ini geri yukler
   - override'lari uygular
   - economy/energy/question life/runtime eventlerini refresh eder

---

## 4. Veri Yapisi

### Save verisi

`SaveManager`, `PlayerSaveData` nesnesini JSON olarak saklar.

Ana bolumler:

- `languageCode`
- `session`
- `energy`
- `membership`
- `themes`
- `economy`
- `progress`
- `metrics`
- `remoteContent`
- `telemetry`

En kritik save bolumu:

- `SessionSnapshot`
  - aktif session
  - current level/question
  - revealed letters
  - target slot
  - hearts
  - revealed pin lokal poz/rot
  - pending fail
  - pending info card
  - pending result summary

### JSON katalog verileri

`Resources/Content` ve `Resources/Content/Locales` altinda tutulur.

Temel veri tipleri `WordSpinContentModels.cs` icinde tanimlidir:

- `LevelDefinition`
- `QuestionDefinition`
- `DifficultyProfileDefinition`
- `DifficultyTierDefinition`
- `RhythmProfileDefinition`
- `ShapeLayoutDefinition`
- `ThemePackDefinition`
- `InfoCardDefinition`
- `CampaignPackDefinition`
- `StoreCatalogDefinition`
- `MembershipProfileDefinition`
- `EnergyConfigDefinition`
- `KeyboardConfigDefinition`

### ShapeLayoutDefinition icindeki kritik yeni alanlar

- `customPoints`
  - manuel veya referans gorselden uretilmis slot merkezleri
- `editorReferenceImagePath`
  - shape referans gorseli
- `gameplayAutoFit`
  - runtime'da shape'i okunurluk icin otomatik fit edip etmeyecegi
- `adaptivePlaqueVisuals`
  - plaque presentation katmaninin shape'e estetik uyum saglayip saglamayacagi
- `plaqueVisualAngleOffsets`
  - slot-basi plaque gorsel aci offset'i
- `plaqueVisualPadding / min-max width-height scale / outwardOffset / contourFollow`
  - plaque'in presentation-only gorsel uyum ayarlari

### Content provider yapisi

- `LocalContentProvider`
  - `Resources.Load<TextAsset>` ile lokal JSON okur
- `RemoteContentProvider`
  - uzaktan gelen JSON'i okur
- `ContentService`
  - ikisini birlestiren servis katmanidir
  - "local mi remote mu" kararini burada verir

### ScriptableObject yapisi

Projede ayarlanabilir profil mantigi icin ScriptableObject kullanilir.

Onemli ScriptableObject'ler:

- `ScoreTuningProfile`
  - skor ve bonus ayarlari
- `ImpactFeelProfile`
  - hit hissi / feedback tuning
- `KeyboardLayoutTuningProfile`
  - locale bazli klavye spacing/size/layout tuning
- `EconomyBalanceProfile`
  - level coin, stars, ad catch-up, theme offer, preview price tuning
- `TestPlayerModeProfile`
  - default/free/premium test sandbox ayarlari ve snapshot'lari

### Icerik editoru dokuman modeli

Editor, runtime JSON'lari tek noktadan duzenlemek icin kendi ara modelini kullanir:

- `WordSpinContentEditorDocument`
- `LevelContentEditorEntry`
- `LevelQuestionEditorEntry`

Bu model:

- 4 dil soru/cevap/bilgi karti verisini tek UI'da birlestirir
- level metadata ve shape secimini ayni satirda tutar
- kayit sirasinda tekrar locale JSON kataloglarina ayrisir

### Pricing veri yapisi

Store fiyat gosterimi abstraction ile ayrilmistir.

Temel tipler:

- `StorePriceQuote`
- `IStorePricingProvider`
- `StorePricingManager`
- `PreviewStorePricingProvider`

Su an:

- preview fiyatlar `EconomyBalanceProfile.RegionalPricePreviews` icinden gelir

Gelecekte:

- Google Play Billing product details ile doldurulabilir

---

## 5. Gelistirme Notlari (Spagetti Analizi / Hassas Alanlar)

Bu bolum, degisiklik yaparken en cok dikkat edilmesi gereken yerleri listeler.

### 1. `GameManager` asiri merkezi orkestrator

Dosya:

- `Assets/WordSpinAlpha/Scripts/Core/GameManager.cs`

Risk:

- level baslatma
- hit sonucunu yorumlama
- fail state
- continue/retry
- info card
- level complete
- completion restore

gibi cok farkli sorumluluklari tek sinifta toplar.

Etkisi:

- burada yapilan degisiklikler gameplay loop, save/restore ve UI akisini birlikte bozabilir.

### 2. `LevelFlowController` hem content, hem pacing, hem restore biliyor

Dosya:

- `Assets/WordSpinAlpha/Scripts/Core/LevelFlowController.cs`

Risk:

- content cache
- question target order
- slot mapping
- rhythm pacing
- restore
- current target refresh

ayni sinifta.

Etkisi:

- shape/layout, restore veya target pacing uzerinde yapilan degisiklikler digerlerini yan etkileyebilir.

### 3. `SessionSnapshot` alani buyuk ve hassas

Dosyalar:

- `Assets/WordSpinAlpha/Scripts/Core/PlayerSaveModels.cs`
- `Assets/WordSpinAlpha/Scripts/Core/SessionManager.cs`
- `Assets/WordSpinAlpha/Scripts/Core/GameManager.cs`
- `Assets/WordSpinAlpha/Scripts/Presentation/ResultPresenter.cs`

Risk:

- session modeli hem gameplay restore'u hem info/result restore'u tasiyor.

Etkisi:

- session save alaninda yapilacak bir degisiklik info card, result popup, replay veya level restore davranisini bozabilir.

### 4. `GameEvents` ile gorunmez bagliliklar cok fazla

Dosya:

- `Assets/WordSpinAlpha/Scripts/Core/GameEvents.cs`

Risk:

- presenter'lar ve manager'lar birbirine direkt referans vermese de event sirasina bagimli.

Etkisi:

- event zamani veya event verisi degisirse score, hud, fail modal, store, localization, impact feedback zinciri kirilabilir.

### 5. `SceneBootstrap` initialization order'a hassas

Dosya:

- `Assets/WordSpinAlpha/Scripts/Core/SceneBootstrap.cs`

Risk:

- singleton'lar ayni anda ayağa kaldirilir.

Etkisi:

- yeni manager eklemek veya sira degistirmek, startup davranisini beklenmedik sekilde etkileyebilir.

### 6. Store/economy sandbox/persisted save ucgeni hassas

Dosyalar:

- `EconomyManager.cs`
- `LevelEconomyManager.cs`
- `TestPlayerModeManager.cs`
- `StorePresenter.cs`
- `StorePricingManager.cs`
- `SaveManager.cs`

Risk:

- `Default`, `FreePlayer`, `PremiumPlayer` ayrimi runtime ile save'i ayni anda etkiler.

Etkisi:

- AppliedMode / snapshot / profile secimi dogru ele alinmazsa sandbox testleri default ekonomiyi kirletebilir.

### 7. Runtime `FindObjectOfType` / `GameObject.Find` kullanimi mevcut

Gorunen yerler:

- `GameManager.EnsureRuntimePresenters`
- `GameSceneNavigationButtons`
- `GameplayHudPresenter.LevelFlowControllerFinder`
- bazi presenter label aramalari

Risk:

- sahne hiyerarsisi veya obje isimleri degisirse runtime baglanti kopabilir.

Etkisi:

- compile hatasi vermez ama sessizce UI/localization/presenter kirigi uretebilir.

### 8. Localization da merkezi tablo yerine presenter-ici switch bloklari agirlikli

Ornek dosyalar:

- `GameplayHudPresenter.cs`
- `GameSceneNavigationButtons.cs`
- `InfoCardPresenter.cs`
- `ResultPresenter.cs`
- `StorePresenter.cs`

Risk:

- ayni kavramin cevirisi birden fazla presenter icine dagilmis.

Etkisi:

- metin tutarliligi ve yeni dil ekleme maliyeti artar.

### 9. Mobil layout builder + runtime tuning birlikte yasiyor

Dosyalar:

- `WordSpinAlphaSceneBuilder.cs`
- `GameplaySceneTuner.cs`
- `MobileRuntimeController.cs`
- `KeyboardPresenter.cs`

Risk:

- builder ile runtime tuning ayni konsepti iki farkli yerde tasiyor.

Etkisi:

- bir tarafta duzeltilen layout, diger tarafta farkli kalirsa "editorde bir sey, play'de baska sey" tipi buglar dogar.

### 10. Shape editor parity alani hassas

Dosyalar:

- `WordSpinAlphaContentEditorWindow.cs`
- `WordSpinAlphaContentEditorData.cs`
- `ShapeLayoutGeometry.cs`
- `SlotManager.cs`
- `RotatorPlaquePresenter.cs`

Risk:

- preview, runtime slot merkezi ve runtime plaque presentation ayni mantigi paylasmak zorunda
- custom point normalize/auto-fit davranisi editor ile runtime arasinda ayrisirse kullanici tek kutuyu tasirken tum shape kaymis gibi hisseder

Etkisi:

- manual shape authoring guvensiz hale gelir
- preview guvenilmez olursa tasarim iterasyonu yavaslar

### 11. Shape mekanigi ile presentation ayrimi korunmali

Dosyalar:

- `Slot.cs`
- `SlotManager.cs`
- `RotatorPlaquePresenter.cs`
- `Pin.cs`

Risk:

- plaque'in gorsel formunu iyilestirmek icin hit zone veya outward band mantigina dogrudan dokunmak mekanik parity bozar

Etkisi:

- `perfect / good / near miss / wrong slot` sonuclari gorsel tuning yuzunden degismemelidir
- bu nedenle plaque visual adaptasyonu yalnizca presentation katmaninda tutulmalidir

### 12. Pricing abstraction hazir ama final billing provider yok

Dosyalar:

- `IStorePricingProvider.cs`
- `StorePricingManager.cs`
- `PreviewStorePricingProvider.cs`
- `MockPurchaseService.cs`

Risk:

- su anki fiyat gosterimi preview tabanli.

Etkisi:

- final Play Store entegrasyonunda yalnizca provider degismeli; eger store UI icine yeni logic sizdirilirse abstraction bozulur.

---

## 6. Kisa Sonuc

Bu proje su an icin:

- singleton + event-bus agirlikli
- JSON data-driven
- save/session merkezli
- runtime tuning ve editor tooling'i kuvvetli
- gameplay, economy, store ve mobile layout katmanlari birbirine bagli

bir Unity mobil oyun mimarisidir.

En onemli mental model:

- **Core** oyun mantigi ve state'i tasir.
- **Presentation** eventleri dinleyip gorsel/UI akis uretir.
- **Services** veri, telemetry, purchase ve pricing kaynaklaridir.
- **Editor** tooling katmani, runtime davranisini veri/profil uzerinden kontrol etmeyi saglar.

Bu dokuman ikinci bir AI modeline verildiginde, modeli dogrudan su sorular uzerinde calistirmak mumkundur:

- hangi sinif nereyi etkiler
- hangi sistemler reusable
- hangi alanlar hassas
- testler nereden baslamali
- yeni mekanik eklerken hangi sinifa dokunmak daha dogru
- store/billing entegrasyonu nasil gecirilmeli
