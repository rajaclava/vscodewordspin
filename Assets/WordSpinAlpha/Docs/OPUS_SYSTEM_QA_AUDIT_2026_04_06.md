# OPUS_SYSTEM_QA_AUDIT_2026_04_06

## 1. Amac

Bu dokuman, `SYSTEM_QA_BUG_SWEEP_REPORT_2026_04_06.md` (bundan sonra "ana rapor") uzerinde yapilan **ikinci goz teknik denetim** raporudur.

Hedef: ana raporun genel kalitesini degerlendirmek, atlanmis sistem risklerini tespit etmek, onceliklendirme tutarsizliklarini belirlemek, eksik bug hipotezlerini eklemek ve alpha/market oncesi kritik ayrimini netlistirmek.

Bu rapor dogrulanmis bug listesi degildir. Ana rapor gibi, **olasi risk ve iyilestirme hipotezleri** dokumanidir.

---

## 2. Incelenen Dosyalar ve Kapsam

### Dokumanlar
- `SYSTEM_QA_BUG_SWEEP_REPORT_2026_04_06.md` - Denetlenen ana rapor (77 senaryo)
- `WORDSPIN_MASTER_PLAN.md` - Proje ana plani, faz yapisi
- `MARKET_RELEASE_READINESS_PLAN.md` - Production gecis plani
- `ECONOMY_NOW_AND_FUTURE_REPORT.md` - Ekonomi mimarisi, sandbox, pricing
- `UNIFIED_EDITOR_TO_LIVEOPS_PLAN.md` - Editor/liveops fazlama, buyume riskleri
- `UNIFIED_EDITOR_AND_LIVE_CONFIG_GUIDE.md` - Editor mimari kilavuzu
- `TEKNIK_ZIHIN_HARITASI.md` - Teknik mimari haritasi, hassas alanlar
- `SONNET_PLAYER_FLOW_QA_REVIEW_2026_04_06.md` - Oyuncu akisi QA incelemesi

### Kod Dosyalari
- **Core**: `SaveManager.cs`, `SessionManager.cs`, `GameManager.cs`, `SceneBootstrap.cs`, `PlayerSaveModels.cs`, `Singleton.cs`, `EconomyManager.cs`, `EnergyManager.cs`, `GameEvents.cs`, `LevelFlowController.cs`, `SceneNavigator.cs`, `TestPlayerModeManager.cs`, `InputBuffer.cs`, `QuestionLifeManager.cs`, `SlotManager.cs`, `Pin.cs`, `PinLauncher.cs`
- **Services**: `ContentService.cs`, `MockPurchaseService.cs`, `IPurchaseService.cs`, `IStorePricingProvider.cs`, `StorePricingManager.cs`, `PreviewStorePricingProvider.cs`, `TelemetryService.cs`, `RemoteContentProvider.cs`, `LocalContentProvider.cs`
- **Presentation**: `StorePresenter.cs`, `FailModalPresenter.cs`, `ThemeRuntimeController.cs`, `RotatorPlaquePresenter.cs`, `GameplayHudPresenter.cs`, `InfoCardPresenter.cs`, `ResultPresenter.cs`, `DebugRewardedAdPresenter.cs`, `KeyboardPresenter.cs`, `MainMenuPresenter.cs`
- **Editor**: `WordSpinAlphaEditorSyncUtility.cs`, `WordSpinAlphaEditorRuntimeRefreshUtility.cs`, `ValidationAuditWindow.cs`, `RemoteContentHotfixWindow.cs`, `EconomyBalanceWindow.cs`
- **Content**: `WordSpinContentModels.cs`, `ShapeLayoutGeometry.cs`

Toplam: **8 dokuman**, **~50 kod dosyasi**, **77 mevcut bug senaryosu** incelendi.

---

## 3. Ana Rapor Hakkinda Genel Teknik Degerlendirme

### Genel Notu: 8/10

Ana rapor, 06.04.2026 itibariyla projenin mevcut durumuna gore **yuksek kalitede** bir statik QA taramasi sunmaktadir. Sistematik formati, koddan turetilmis hipotezleri ve anlamli oncelik/olasilik skalasi guclu yonleridir.

**Zayif yonleri:**
- Bazi maddeler ayni kok nedenden tureyen tekrarlar iceriyor
- "Gelecek" riskleri ile "simdi" riskleri ayni oncelikle listelenmis
- Save atomiklik ve tutarlilik riskleri yeterince derinlestirilmemis
- Localization yapisal borcu (merkezi tablo yoklugu) eksik
- GameEvents static event bus bellek sizintisi riski hic ele alinmamis
- Performans riskleri icin olcum esikleri tanimlanmamis
- Production gecis maddelerinin bir kismi somut test adimsiz

---

## 4. Guclu Bulunan Bolumler

**Save / Session / Restore (Bolum 2)**: BUG-007, BUG-008, BUG-010 gercekten kritik ve iyi formule edilmis. Coklu pending state cakisma riskleri dogru yakalanmis.

**Economy / Store / Membership (Bolum 6)**: Mock/preview/debug provider gecis riskleri isabetli. BUG-040, BUG-043, BUG-074 kritik ve dogru.

**Content / Localization (Bolum 5)**: MergeById order drift (BUG-031), duplicate id override (BUG-032), remote/local hibrit state (BUG-066, BUG-067) onemli senaryolar.

**Unified Editor (Bolum 7)**: Stale state, last-write-wins ve revision eksikligi riskleri dogru tespit edilmis.

---

## 5. Eksik Bulunan Bolumler

### 5.1 Save Atomikligi
`SaveManager.WriteToDisk()` icindeki `File.WriteAllText` atomik degil. Yarim yazim + backup yoklugu ayri bir yapisal risk olmali. BUG-008 sonucu anlatir ama kok nedeni (atomik write eksikligi) tanimlamaz.

### 5.2 GameEvents Static Event Bus Riskleri
`GameEvents` tamamen `static event Action<>` uzerine kurulu. Presenter destroy sonrasi handler kalintisi, bellek sizintisi ve `MissingReferenceException` riski hic ele alinmamis.

### 5.3 Singleton Lifecycle ve Persist/Non-Persist Karisimi
`SessionManager` `PersistAcrossScenes = false`, diger singleton'lar `true`. Scene gecisinde `SessionManager.Instance` null donebilir ama bagimli siniflar kontrol etmeyebilir.

### 5.4 Energy Refill Ticks Reset Davranisi
`EnergyManager.TryConsumeEntryEnergy()` ve `GrantEnergy()` her cagrildiginda `lastRefillUtcTicks`'i sifirliyor. Bu refill zamanlayicisini her enerji isleminde resetliyor.

### 5.5 Save Model Versiyonlama Yoklugu
`PlayerSaveData` icinde `saveVersion` alani yok. Model degisikliklerinde sessiz veri kaybi veya yanlis varsayilan deger riski.

### 5.6 Localization Yapisal Borcu
Merkezi locale key sistemi yok. TEKNIK_ZIHIN_HARITASI bunu "hassas alan" olarak isareliyor ama bug sweep'te somut senaryo yok.

### 5.7 StorePresenter IPurchaseService Kullanmiyor
`StorePresenter` dogrudan `MockPurchaseService.Instance.Purchase(...)` cagiriyor. `IPurchaseService` interface'i var ama kullanilmiyor. Mimari tutarsizlik.

---

## 6. Atlanmis Buyuk Sistem Riskleri

### 6.1 KRITIK: Save dosyasi atomik yazim yoklugu
- `File.WriteAllText` crash/kill sirasinda yarim dosya olusturabilir
- Backup/rotation mekanizmasi yok
- BUG-007 ve BUG-008'in ortak altyapi nedeni

### 6.2 KRITIK: GameEvents static delegate leak
- Destroy edilmis objelere referans kalabilir
- `MissingReferenceException`, bellek sizintisi riski
- Tum event-based iletisimi etkiler

### 6.3 YUKSEK: SessionManager non-persist lifecycle cakismasi
- Scene gecisinde `SessionManager.Instance` null olabilir
- Store donusu veya scene reload sirasinda snapshot kaybi riski

### 6.4 YUKSEK: Save model versiyonu yok
- Model degisikliklerinde sessiz veri kaybi
- Her save yapisina yeni alan eklendiginde tetiklenir

### 6.5 YUKSEK: Energy refill ticks reset davranisi
- Enerji harcama/grant refill zamanlayicisini sifirliyor
- Oyuncu icin adil olmayan bekleme suresi

### 6.6 ORTA: Localization merkezi key sistemi eksikligi
- Presenter ici switch bloklari ile daginik ceviri
- Yeni dil ekleme maliyeti yuksek

---

## 7. Onceliklendirmesi Sorunlu Maddeler

### Onceligi cok dusuk tutulanlar

| Madde | Mevcut | Onerilen | Gerekce |
|---|---|---|---|
| BUG-031 (katalog order drift) | P2 | P1 | Dictionary Values sirasi garanti yok, level sirasi oyuncu deneyimini dogrudan etkiler |
| BUG-055 (validation eksik raporlama) | P2 | P1 | Validation false-clean verirse tum parity kontrolleri guvenilmez olur |
| BUG-064 (queue trim sessiz kayip) | P2 | P1 | Telemetry verisi production analizde kritik |

### Onceligi cok yuksek tutulanlar

| Madde | Mevcut | Onerilen | Gerekce |
|---|---|---|---|
| BUG-020 (extreme hit tuning) | P1 | P2 | Extreme degerler normal kullanim disi, editor validation ile onlenebilir |
| BUG-047 (promo tekrarli claim) | P1 | P2 | Promo sistemi henuz kurulmamis, gelecek tasarim riski |

### Olasiligi yanlis belirlenenler

| Madde | Mevcut | Onerilen | Gerekce |
|---|---|---|---|
| BUG-002 (yanlis Canvas/Camera) | Yuksek | Orta | Proje tek sahne/canvas uzerine kurulu, ikinci eklenmediginde dusuk risk |
| BUG-007 (throttle save kaybi) | Yuksek | Yuksek (dogru ama aciklama eksik) | `OnApplicationPause/Quit` icinde `WriteToDisk()` dogrudan cagriliyor. Gercek risk yalnizca OS force kill |

---

## 8. Fazla Spekulatif veya Zayif Kalan Maddeler

### Fazla spekulatif
| Madde | Neden |
|---|---|
| BUG-047 (promo tekrarli claim) | Promo sistemi henuz yok. Tamamen gelecek tasarim riski. Bug degil, tasarim notu. |
| BUG-077 (production provider parity) | Gercek provider henuz yok. Gecis plani maddesi, bug hipotezi degil. |

### Zayif kalan
| Madde | Neden | Oneri |
|---|---|---|
| BUG-006 (play/edit mode farki) | Cok genel, hangi presenter/kosul belirtilmemis | Somut presenter ve kosul belirtilmeli |
| BUG-051 (play modda scene tuning) | Unity bilinen davranis, bug degil | Kullanici egitimi notu olarak isaretlenmeli |
| BUG-076 (editor asset sizmasi) | Somut asset listesi yok | `Art/ShapeRefs/*.png` gibi dosyalar listelenmeli |

### Daha sonra ele alinabilir
| Madde | Gerekce |
|---|---|
| BUG-047 | Promo sistemi henuz yok |
| BUG-075 | Alpha icin tema sayisi sinirli |
| BUG-077 | Provider gecisi alpha sonrasi |
| BUG-073 | Alpha icin tema/hit yogunlugu sinirli |
| BUG-063 | Telemetry alpha demo'da production onceligi degil |

---

## 9. Yeni Eklenmesi Gereken Bug Senaryolari

### AUDIT-001 - Save dosyasi atomik yazim yoklugu nedeniyle tam veri kaybi

- **ID**: AUDIT-001
- **Baslik**: Save dosyasi atomik yazim yoklugu nedeniyle tam veri kaybi
- **Kategori**: Save / Session / Restore
- **Oncelik**: P0
- **Olasilik**: Orta
- **Sistemsel gerekce**: `SaveManager.WriteToDisk()` icindeki `File.WriteAllText` atomik degil. Force kill sirasinda yarim JSON olusabilir. `Load()` bozuk JSON'i catch ederek tum veriyi sifirlar.
- **Muhtemel kok neden**: Atomik write pattern (write-to-temp + rename) ve backup rotation yok.
- **Etkilenen dosyalar**: `Scripts/Core/SaveManager.cs`, `Scripts/Core/PlayerSaveModels.cs`
- **Manuel cogultma adimlari**:
  1. Oyunda ilerleme/coin/tema/session olustur.
  2. Save tetikleyen islem yap.
  3. Task Manager'dan process'i oldur.
  4. Yeniden ac, save dosyasinin bozuk olup olmadigini kontrol et.
- **Beklenen bozuk belirti**: Tum ilerleme, coin, tema, enerji, session sifirlanir.
- **Neden ayri madde olmali**: BUG-008 sonucu anlatir ama kok neden ve cozum yonu (atomik write + backup) ayri bir maddedir.

### AUDIT-002 - GameEvents static delegate leak

- **ID**: AUDIT-002
- **Baslik**: GameEvents static delegate'lerinde destroy sonrasi handler kalintisi
- **Kategori**: Boot / Scene / Singleton / Init
- **Oncelik**: P1
- **Olasilik**: Yuksek
- **Sistemsel gerekce**: `GameEvents` tamamen static event. Presenter'lar `OnEnable/OnDisable` ile subscribe/unsubscribe yapiyor. Unsubscribe unutulursa veya `OnDisable` cagrilmadan destroy olursa kalinti referans kalir.
- **Muhtemel kok neden**: Static event bus + MonoBehaviour lifecycle uyumsuzlugu.
- **Etkilenen dosyalar**: `Scripts/Core/GameEvents.cs`, tum event subscriber siniflar
- **Manuel cogultma adimlari**:
  1. Gameplay -> Store -> Gameplay gecisleri yap.
  2. Console'da `MissingReferenceException` izle.
  3. Event'lerin cift tetiklenip tetiklenmedigini kontrol et.
- **Beklenen bozuk belirti**: `MissingReferenceException`, cift event, bellek sizintisi.
- **Neden ayri madde olmali**: Projenin tum event iletisiminin temel riski.

### AUDIT-003 - SessionManager non-persist singleton scene gecisinde kaybolmasi

- **ID**: AUDIT-003
- **Baslik**: SessionManager non-persist singleton'un scene gecisinde null olmasi
- **Kategori**: Boot / Scene / Singleton / Init
- **Oncelik**: P1
- **Olasilik**: Yuksek
- **Sistemsel gerekce**: `SessionManager.PersistAcrossScenes = false`. Scene degistiginde destroy olur. `SceneBootstrap` listesinde yok, sahne icinde bulunuyor. Gameplay'den cikilip tekrar girildiginde lifecycle sorunlari olusabilir.
- **Muhtemel kok neden**: Non-persist/persist singleton karisimi. `SceneBootstrap` `SessionManager`'i yonetmiyor.
- **Etkilenen dosyalar**: `Scripts/Core/SessionManager.cs`, `Scripts/Core/Singleton.cs`, `Scripts/Core/SceneBootstrap.cs`
- **Manuel cogultma adimlari**:
  1. Gameplay'de session olustur.
  2. Store'a gec.
  3. Gameplay'e don.
  4. `SessionManager.Instance` null mu kontrol et.
- **Beklenen bozuk belirti**: Scene donusunde snapshot alinmiyor, pause/quit'te session kaybi.
- **Neden ayri madde olmali**: Save/session katmaninin temel guvenilirligini etkiler.

### AUDIT-004 - Save model versiyonu yoklugu

- **ID**: AUDIT-004
- **Baslik**: Save model versiyonu yoklugu nedeniyle sessiz migration hatasi
- **Kategori**: Save / Session / Restore
- **Oncelik**: P1
- **Olasilik**: Yuksek
- **Sistemsel gerekce**: `PlayerSaveData` icinde `saveVersion` yok. `JsonUtility.FromJson` eski JSON'dan yeni alani varsayilan degerle atar. Bazi durumlarda bu yanlis davranisa yol acar.
- **Muhtemel kok neden**: Versiyon kontrolu ve migration pipeline yok.
- **Etkilenen dosyalar**: `Scripts/Core/PlayerSaveModels.cs`, `Scripts/Core/SaveManager.cs`
- **Manuel cogultma adimlari**:
  1. Mevcut save'i yedekle.
  2. `PlayerSaveData`'ya yeni alan ekle.
  3. Eski save ile oyunu ac.
  4. Varsayilan degerin dogru mu yanlis mi kontrol et.
- **Beklenen bozuk belirti**: Eski save'ler yanlis varsayilan degerle yuklenince beklenmeyen grant veya kilitleme.
- **Neden ayri madde olmali**: Her save model degisikliginde tekrarlayan risk. BUG-039 kismi kapsar ama yapisal cozum ayri maddedir.

### AUDIT-005 - Energy refill ticks her spend/grant'te sifirlanmasi

- **ID**: AUDIT-005
- **Baslik**: EnergyManager lastRefillUtcTicks spend/grant sirasinda sifirlanmasi
- **Kategori**: Economy / Store / Membership / Energy
- **Oncelik**: P1
- **Olasilik**: Yuksek
- **Sistemsel gerekce**: `TryConsumeEntryEnergy()` ve `GrantEnergy()` her cagrildiginda `lastRefillUtcTicks = DateTime.UtcNow.Ticks` yapiyor. Bu refill zamanlayicisini sifirlar.
- **Muhtemel kok neden**: Ticks guncelleme sadece refill sonrasi yapilmali.
- **Etkilenen dosyalar**: `Scripts/Core/EnergyManager.cs`
- **Manuel cogultma adimlari**:
  1. Enerji azalt.
  2. Refill suresinin yarisinda tekrar enerji harca.
  3. Toplam bekleme suresini olc.
- **Beklenen bozuk belirti**: Refill suresi beklenenden uzun.
- **Neden ayri madde olmali**: BUG-044 genel refill hatasini isaret ediyor ama bu spesifik ticks reset mekanizmasini tanimlamiyor.

### AUDIT-006 - ContentService MergeById sort garantisi yok

- **ID**: AUDIT-006
- **Baslik**: ContentService MergeById dictionary values siralama garantisi yok
- **Kategori**: Content / Localization / Shape
- **Oncelik**: P1
- **Olasilik**: Yuksek
- **Sistemsel gerekce**: `MergeById` sonucu `Dictionary.Values` uzerinden diziye kopyalaniyor. Dictionary siralama garantisi vermiyor.
- **Muhtemel kok neden**: Merge sonrasi explicit sort islemi yok.
- **Etkilenen dosyalar**: `Scripts/Services/ContentService.cs`
- **Manuel cogultma adimlari**:
  1. 10+ level ile farkli ID siralarindan merge et.
  2. Birden fazla acilista level sirasini karsilastir.
- **Beklenen bozuk belirti**: Level sirasi her acilista farkli olabilir.
- **Neden ayri madde olmali**: BUG-031 bunu P2 olarak isareliyor ama P1 olmali; level sirasi progression deneyimini dogrudan etkiler.

### AUDIT-007 - Singleton Awake sirasi bagimlilik cakismasi

- **ID**: AUDIT-007
- **Baslik**: SceneBootstrap singleton olusturma sirasi bagimlilik riski
- **Kategori**: Boot / Scene / Singleton / Init
- **Oncelik**: P1
- **Olasilik**: Orta
- **Sistemsel gerekce**: `SceneBootstrap` singleton'lari sirayla olusturuyor. `AddComponent` aninda `Awake` cagrilir. Singleton'lar kendi `Awake` icinde diger singleton'lara bagimli. Sira degisirse bagimlilik kirilir.
- **Muhtemel kok neden**: Sira-bagimliligi acik dokumante edilmemis. Yeni singleton eklendikce risk buyur.
- **Etkilenen dosyalar**: `Scripts/Core/SceneBootstrap.cs`, tum Singleton siniflari
- **Manuel cogultma adimlari**:
  1. `EnsureSingleton` sirasini degistir.
  2. Baslangicta deger bozulmasini izle.
- **Beklenen bozuk belirti**: Baslangicta yanlis enerji, coin, premium state.
- **Neden ayri madde olmali**: BUG-004 duplicate riskini kapsar ama sira bagimliligini tanimlamamis.

### AUDIT-008 - OnApplicationPause SessionManager/SaveManager yarisi

- **ID**: AUDIT-008
- **Baslik**: OnApplicationPause sirasinda SessionManager ve SaveManager yazim yarisi
- **Kategori**: Save / Session / Restore
- **Oncelik**: P1
- **Olasilik**: Orta
- **Sistemsel gerekce**: Her ikisi de `OnApplicationPause(true)` dinliyor. Unity cagri sirasi garanti etmiyor. Once `SaveManager.WriteToDisk()` calisirsa eski data yazilir, sonra `SessionManager` snapshot alir ama uygulama kapanirsa yeni snapshot diske yazilmamis olur.
- **Muhtemel kok neden**: `OnApplicationPause` cagri sirasi belirsiz.
- **Etkilenen dosyalar**: `Scripts/Core/SessionManager.cs`, `Scripts/Core/SaveManager.cs`
- **Manuel cogultma adimlari**:
  1. Gameplay ortasinda uygulamayi arka plana at.
  2. Hemen kapat.
  3. Tekrar ac, session restore dogru mu kontrol et.
- **Beklenen bozuk belirti**: Son session state kaybolur.
- **Neden ayri madde olmali**: BUG-009 benzer senaryoyu kapsar ama pause sirasindaki yarisi ve throttle etkisini tanimlamiyor.

### AUDIT-009 - SetNoAdsOwned event payload noAds-spesifik degil

- **ID**: AUDIT-009
- **Baslik**: NoAds degisiminde MembershipChanged event payload'i yanlis
- **Kategori**: Economy / Store / Membership / Energy
- **Oncelik**: P2
- **Olasilik**: Yuksek
- **Sistemsel gerekce**: `SetNoAdsOwned` icinde `GameEvents.RaiseMembershipChanged(PremiumMembershipActive)` cagriliyor. Payload NoAds degil, Premium degeri.
- **Muhtemel kok neden**: Tek boolean payload NoAds/Premium ayrimi yapamiyor.
- **Etkilenen dosyalar**: `Scripts/Core/EconomyManager.cs`, `Scripts/Core/GameEvents.cs`
- **Manuel cogultma adimlari**:
  1. NoAds satin alim simule et.
  2. Membership UI guncellemesini izle.
- **Beklenen bozuk belirti**: NoAds alindiktan sonra UI yanlis guncellenir.
- **Neden ayri madde olmali**: BUG-042 kismi isaret ediyor ama event payload problemini tanimlamiyor.

### AUDIT-010 - Dil degisiminde InputBuffer stale state

- **ID**: AUDIT-010
- **Baslik**: Dil degisimi sirasinda InputBuffer beklenen harfi guncellemez
- **Kategori**: Gameplay / Hit / Input / Rotator
- **Oncelik**: P1
- **Olasilik**: Orta
- **Sistemsel gerekce**: `InputBuffer` `LanguageChanged` event'ine abone degil. Dil degisimi sonrasi cevap degisirse `expectedLetter` stale kalir.
- **Muhtemel kok neden**: `InputBuffer` dil degisimi dinlemiyor.
- **Etkilenen dosyalar**: `Scripts/Core/InputBuffer.cs`, `Scripts/Core/LevelFlowController.cs`
- **Manuel cogultma adimlari**:
  1. Soru ortasinda dil degistir.
  2. Beklenen harf ile gercek target'i debug log ile karsilastir.
- **Beklenen bozuk belirti**: Dogru harfe basildiginda wrong letter sonucu.
- **Neden ayri madde olmali**: BUG-018 stale state'i kapsar ama dil degisimi trigger'ini belirtmez.

---

## 10. Alpha Oncesi Kritik Maddeler

Su maddeler **alpha demo oncesinde** mutlaka ele alinmali veya dogrulanmalidir:

### Ana rapordan
- **BUG-003** (P0): Pending request ile session restore cakismasi
- **BUG-007** (P0): Throttle araliginda save kaybi
- **BUG-008** (P0): Bozuk save'de tam reset
- **BUG-010** (P0): Content degisikligi sonrasi restore uyumsuzlugu
- **BUG-027** (P0): Pending result cift odul
- **BUG-040** (P0): Test mode snapshot restore ezmesi
- **BUG-013** (P1): Pending fail stale kalmasi
- **BUG-014** (P1): Pending info/result cakismasi
- **BUG-018** (P1): InputBuffer stale harf
- **BUG-022** (P1): Shape rebuild pin parity
- **BUG-029** (P1): Store donusu pending UI kaybi
- **BUG-034** (P1): Shape editor/runtime parity
- **BUG-038** (P1): Dil degisimi tam yenilenmeme
- **BUG-062** (P1): Safe area / klavye parity

### Bu denetimden
- **AUDIT-001** (P0): Atomik save yazim yoklugu
- **AUDIT-002** (P1): GameEvents delegate leak
- **AUDIT-003** (P1): SessionManager non-persist lifecycle
- **AUDIT-005** (P1): Energy ticks reset davranisi
- **AUDIT-006** (P1): MergeById sort garantisi yok
- **AUDIT-010** (P1): Dil degisimi InputBuffer stale

---

## 11. Market/Release Oncesi Kritik Maddeler

Su maddeler alpha demo'dan sonra, **market cikisi oncesinde** mutlaka ele alinmalidir:

### Ana rapordan
- **BUG-043** (P0): Store mock purchase bypass
- **BUG-074** (P0): Debug/test presenter release sizmasi
- **BUG-077** (P0): Production provider gecis parity
- **BUG-039** (P1): Baslangic grant policy degisikliklerinde eski save
- **BUG-041** (P1): Theme unlock / premium cakismasi
- **BUG-048** (P2): Membership revoke sonrasi theme tutarsizligi
- **BUG-075** (P2): Resources yuku buyume
- **BUG-076** (P2): Editor asset build sizmasi

### Bu denetimden
- **AUDIT-004** (P1): Save model versiyonu yoklugu
- **AUDIT-007** (P1): Singleton Awake sirasi risk dokumantasyonu
- **AUDIT-009** (P2): NoAds event payload problemi

### MARKET_RELEASE_READINESS_PLAN caprazlamasi
- `BuildProfile`, `FeatureFlagRegistry`, `ReleaseSafePolicy` katmanlari zorunlu
- `GooglePlayPurchaseService`, `GooglePlayStorePricingProvider` implementasyonlari zorunlu
- `IRewardedAdService` production provider zorunlu
- Promo/gift orkestrasyon katmani zorunlu
- Final market test paketi (8 kategori, MARKET_RELEASE_READINESS_PLAN Asama 6) zorunlu

---

## 12. Ana Rapor Icin Onerilen Revizyon Ozeti

### 12.1 Birlestirilebilir maddeler

Asagidaki maddeler ayni kok nedenden turedigi icin birlestirme adayidir:

| Maddeler | Ortak kok neden | Oneri |
|---|---|---|
| BUG-007 + BUG-008 | Save atomiklik eksikligi | Tek "Save Atomiklik ve Backup Yoklugu" maddesi altinda birlestirilsin |
| BUG-010 + BUG-011 + BUG-012 | Content/shape degisikligi sonrasi eski snapshot uyumsuzlugu | "Content Degisikligi Sonrasi Snapshot Invalidation" olarak birlestirilebilir |
| BUG-041 + BUG-048 | Premium/theme ownership cakismasi | Tek "Theme Ownership ve Membership Lifecycle" maddesi |
| BUG-049 + BUG-050 + BUG-054 | Editor stale state / sync eksikligi | "Editor Sync ve Stale State" olarak birlestirilebilir |
| BUG-063 + BUG-064 + BUG-065 | Telemetry veri butunlugu | "Telemetry Queue ve Veri Butunlugu" olarak birlestirilebilir |

### 12.2 Ayristirilmasi gereken maddeler

| Madde | Neden | Oneri |
|---|---|---|
| BUG-074 | Bes farkli debug/test component'i tek maddede | Her component icin ayri kontrol maddesi: MockPurchaseService, PreviewStorePricingProvider, DebugRewardedAdPresenter, TestPlayerModeManager, debug overlay |
| BUG-038 | "Dil degisimi tam yenilenmemesi" cok genis | Her presenter katmani icin ayri senaryo: HUD, InfoCard, Store, Result, Keyboard |

### 12.3 Oncelik degisiklikleri

| Madde | Mevcut | Onerilen |
|---|---|---|
| BUG-031 | P2 | P1 |
| BUG-055 | P2 | P1 |
| BUG-064 | P2 | P1 |
| BUG-020 | P1 | P2 |
| BUG-047 | P1 | P2 |

### 12.4 Kategori degisiklikleri

| Madde | Oneri |
|---|---|
| BUG-047 | "Bug senaryosu" yerine "gelecek tasarim notu" olarak isaretlensin |
| BUG-051 | "Bug senaryosu" yerine "kullanici egitimi notu" olarak isaretlensin |
| BUG-077 | "Bug senaryosu" yerine "gecis plani maddesi" olarak isaretlensin |

### 12.5 Eklenmesi gereken yeni maddeler
- AUDIT-001 ile AUDIT-010 arasi bu denetimde tanimlanan 10 yeni bug senaryosu
- Toplam onerilenen yeni ekleme: **10 madde**

### 12.6 Performans riskleri icin eksik
- BUG-070, BUG-071, BUG-072, BUG-073 icin **somut olcum esikleri** eklenmeli (kabul edilebilir frame time, GC spike limiti, load time limiti)
- "Performans bozulmasi" yerine "X ms ustu frame time" gibi olculebilir kriterler kullanilmali

---

## 13. Sonuc

Ana rapor (`SYSTEM_QA_BUG_SWEEP_REPORT_2026_04_06.md`) projenin mevcut durumuna gore **guclu ve kapsamli** bir statik QA taramasidir. 77 senaryo icinde oyun akisi, save/restore, editor parity, ekonomi ve content katmanlarini dogru sekilde kapsamistir.

Bu denetimde tespit edilen ana eksiklikler:

1. **Save altyapisi**: Atomik yazim yoklugu, backup rotation yoklugu ve model versiyonlama eksikligi projenin en buyuk yapisal riski. Bu uc madde (AUDIT-001, AUDIT-004, AUDIT-008) birlikte ele alinmali.

2. **Event bus guvenilirigi**: `GameEvents` static delegate yapisi, projenin buyumesiyle orantili olarak artan bir bellek sizintisi ve stale reference riski tasir (AUDIT-002).

3. **Singleton lifecycle**: Persist/non-persist karisimi ve Awake sirasi bagimliligı, yeni singleton eklendiginde sessiz kirik uretme potansiyeli tasir (AUDIT-003, AUDIT-007).

4. **Energy refill mekanizmasi**: `lastRefillUtcTicks` reset davranisi oyuncu deneyimini olumsuz etkileyebilir (AUDIT-005).

5. **Content siralama**: `MergeById` sort garantisi yoklugu level sirasini tutarsiz yapabilir (AUDIT-006).

### Onerilen is sirasi

**Alpha oncesi (simdiki faz):**
1. AUDIT-001 (atomik save) ve BUG-008 (bozuk save reset) birlikte cozulsin
2. AUDIT-002 (event leak audit) yapilsin
3. AUDIT-003 (SessionManager lifecycle) dogrulansin
4. Mevcut P0 maddeleri (BUG-003, 007, 010, 027, 040) manuel test edilsin
5. AUDIT-005, AUDIT-006 icin kod duzeltmesi veya bilinir risk olarak kabul

**Market oncesi:**
1. AUDIT-004 (save versiyonlama) implementasyonu
2. BUG-043, BUG-074, BUG-077 production gecis maddeleri
3. Build profile / feature flag / release-safe katman
4. Final market test paketi (MARKET_RELEASE_READINESS_PLAN Asama 6)

Bu denetim, ana raporun kalitesini teyit ederken **10 yeni bug senaryosu**, **5 birlestirme onerisi**, **2 ayristirma onerisi**, **5 oncelik degisikligi** ve **3 kategori degisikligi** ile raporun kapsamini genisletmeyi onermektedir.

---

*Denetim tarihi: 06.04.2026*
*Denetlenen rapor: SYSTEM_QA_BUG_SWEEP_REPORT_2026_04_06.md*
*Denetim yapan: Opus System QA Audit*
