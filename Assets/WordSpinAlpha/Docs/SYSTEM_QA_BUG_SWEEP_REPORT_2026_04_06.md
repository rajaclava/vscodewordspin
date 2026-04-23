# SYSTEM_QA_BUG_SWEEP_REPORT_2026_04_06

## Amac

Bu dokuman 06.04.2026 itibariyla WordSpin Alpha icin yapilan **kod + dokuman tabanli statik QA taramasidir**.

Bu rapor:

- dogrulanmis bug listesi degildir
- mevcut altyapi, planlar, editor mimarisi, runtime akislar ve production gecis hedefleri uzerinden uretilmis **olasi bug hipotezleri** listesidir
- hotfix asamasindan once manuel cogu ltmaya uygun bir senaryo matrisi sunar

Bu raporun kullanimi:

1. Buradaki senaryolar manuel olarak tek tek denenir.
2. Cogu ltilabilenler "dogrulanmis bug" havuzuna alinir.
3. Dogrulananlar hotfix backlog'una tasinir.
4. Cogu ltulmesi zor ama kritik kalanlar icin hedefli otomasyon testleri yazilir.

## Taranan Kaynaklar

Bu rapor asagidaki ana dokumanlar ve altyapi uzerinden cikarildi:

- `Assets/WordSpinAlpha/Docs/WORDSPIN_MASTER_PLAN.md`
- `Assets/WordSpinAlpha/Docs/UNIFIED_EDITOR_TO_LIVEOPS_PLAN.md`
- `Assets/WordSpinAlpha/Docs/UNIFIED_EDITOR_AND_LIVE_CONFIG_GUIDE.md`
- `Assets/WordSpinAlpha/Docs/MARKET_RELEASE_READINESS_PLAN.md`
- `Assets/WordSpinAlpha/Docs/TEKNIK_ZIHIN_HARITASI.md`
- `Assets/WordSpinAlpha/Docs/ECONOMY_NOW_AND_FUTURE_REPORT.md`
- `Assets/WordSpinAlpha/Scripts/Core/*`
- `Assets/WordSpinAlpha/Scripts/Presentation/*`
- `Assets/WordSpinAlpha/Scripts/Services/*`
- `Assets/WordSpinAlpha/Scripts/Editor/*`

## 06.04.2026 Itibariyla Durum

Planlarin ortak gosterdigi mevcut konum:

- Birlesik tuning editor seti kurulmus durumda.
- `Toplu Tek Editor` shell'i kurulmus durumda.
- Tekli editorler parity kontrolu icin halen tutuluyor.
- Editorler arasi sync / refresh / runtime apply katmanlari kurulu.
- Proje su anda **Faz C - Pre-Polish Sistem Taramasi ve Bug Kilidi** girisindedir.
- Production provider gecisi henuz yapilmadi:
  - `MockPurchaseService`
  - `PreviewStorePricingProvider`
  - `DebugRewardedAdPresenter`
  halen mevcuttur.

## 08.04.2026 Manuel Cogu ltma Durum Guncellemesi

08.04.2026 tarihinde bu rapordaki ilk manuel sweep turunun bir kismi fiilen denenmistir.

Bu turda manuel olarak cogu ltilip **orijinal hipotez kapsami icinde sorun vermedigi gorulen** maddeler:

- `BUG-007`
- `BUG-009`
- `BUG-010`
- `BUG-011`
- `BUG-013`

Onemli not:

- Bu maddelerin rapordaki orijinal senaryo hipotezleri, 08.04.2026 manuel cogu ltma turunda **beklenen bozuk davranisi uretmemistir**.
- Ancak ayni save/session/restore turu sirasinda, ana hipotezlerden ayri bir **ek puan persistence sorunu** yakalanmistir.

Ek yakalanan sorun:

- aktif level sirasinda biriken skor save/session snapshot icine yazilmadigi icin
- menuye cikis, store donusu, session restore ve uygulama yeniden acilis gibi akislar sonrasinda
- gameplay state geri gelse bile **puan 0'dan basliyormus gibi gorunuyordu**

Sorunun teknik nedeni:

- `SessionSnapshot` icinde aktif skor state'i tutulmuyordu
- `ScoreManager` runtime state'i save zincirine hic girmiyordu
- `GameManager` restore sirasinda `LevelFlowController` state'ini geri yukluyor ama skor state'i ayrica geri kurulmuyordu

Nasil cozuldu:

- `SessionSnapshot` modeline aktif skor alanlari eklendi
- `ScoreManager` icine session snapshot doldurma ve session restore metodlari eklendi
- `GameManager` restore akisinda gameplay state ile birlikte skor state'i de geri yukler hale getirildi
- test/sandbox reset akisinda bu yeni skor alanlari da temizlendi

Bu guncelleme ile 08.04.2026 itibariyla:

- `BUG-007`, `BUG-009`, `BUG-010`, `BUG-011`, `BUG-013` ilk manuel sweep turunda **dogrudan cogu ltu rulmus bug olarak acik kalmamistir**
- fakat bu maddeleri test ederken yakalanan **puan persistence sorunu** kapatilmistir
- ayni save/session ailesine ait oldugu icin ileride benzer regressions tekrar kontrol edilmelidir

08.04.2026 ayni manuel sweep turunda ek olarak su maddeler de netlestirilmistir:

- `BUG-014`
  - manuel olarak cogu ltulmus
  - fail/continue modal ve info card acikken klavye inputunun acik kalmasi dogrulanmistir
  - sorun kapatilmistir
  - cozum:
    - fail modal, info card ve level completion state'lerinde gameplay input merkezi olarak kapatildi
    - klavye tuslari sadece mantiksal olarak degil, gorsel olarak da `interactable = false` hale getirildi
- `BUG-027`
  - manuel olarak cogu ltulmus
  - mevcut hipotez kapsami icinde sorun vermemistir
  - ilk turda acik dogrulanmis bug olarak kalmamistir
- `BUG-029`
  - manuel olarak cogu ltulmus ve gercek sorun dogrulanmistir
  - dil degisimi sonrasi ilerleme tek global progress uzerinden okundugu icin oyuncu secili dilin kendi level ilerlemesinden degil, baska dilin acik ilerlemesinden basliyordu
  - sorun kapatilmistir
  - cozum:
    - progress modeli dil bazli ilerleme tutacak sekilde genisletildi
    - eski save yapisi bozulmadan migrate edildi
    - `Play` ve level summary akislari secili dilin kendi progress state'ini okuyacak sekilde guncellendi
    - level reward / first-clear coin takibi global birakildi; dil degistirerek ayni level odulu tekrar kazanilamaz

08.04.2026 ilk manuel bug cogu ltma turu sonucu:

- ilk tur hedef listesinde test edilen maddeler:
  - `BUG-007`
  - `BUG-009`
  - `BUG-010`
  - `BUG-011`
  - `BUG-013`
  - `BUG-014`
  - `BUG-027`
  - `BUG-029`
- bu turun sonunda:
  - save/session ailesindeki puan persistence sorunu kapatildi
  - `BUG-014` kapatildi
  - `BUG-029` kapatildi
  - `BUG-027` icin sorun cogu ltulemedi
- ilk tur manuel cogu ltma fazi tamamlandi
- sonraki adim, ilk tur listesinde kalan maddelerin yeni blok halinde ele alinmasidir

08.04.2026 ayni gun icindeki sonraki manuel taramada su iki madde de netlestirilmistir:

- `BUG-024`
  - `BUG-029` duzeltmesi sonrasinda onceki etkisini kaybetmistir
  - dil degisimi artik her dili kendi progress/session mantigi ile ayirdigi icin, bu bug ilk formuyla aktif cogu ltilebilir bug olmaktan cikmistir
  - bu nedenle su asamada acik hotfix maddesi olarak tutulmamaktadir
- `BUG-025`
  - mevcut akista oyuncu continue reklam akisi sirasinda menu veya magaza tuslarina gidememektedir
  - bu nedenle ilk hipotez kapsami icindeki "yanlis state'e continue callback uygulanmasi" senaryosu aktif bug olarak cogu ltulememistir

Ancak `BUG-025` cogu ltma denemesi sirasinda yeni bir turev sorun yakalanmistir:

- oyuncu fail sonrasi continue/reklam akisini bitirip oyuna dondukten sonra
- menu veya magazaya gidip geri geldiginde
- klavye kilitli kaliyor ve harf basimina izin vermiyordu

Sorunun teknik nedeni:

- session restore zincirinde `pending result` yokken bile result restore dali input'u pasif birakabiliyordu
- yani fail modal ve info card icin ekledigimiz koruma, continue sonrasi menu/store donusunde yan etki uretmisti

Nasil cozuldu:

- restore akisinda input sadece gercekten `pending info card` veya `pending result` varsa kapali tutulur hale getirildi
- normal continue sonrasi menu/store donusunde, fail cözümü pending degilse gameplay input tekrar aktiflestirilir
- boylece `BUG-014` icin kapatilan modal/input korumasi korunurken, continue sonrasi geri donuslerde klavye kilitlenmesi giderildi

08.04.2026 bu guncellemenin sonucu:

- `BUG-024` su anki mimaride aktif bug adayi olarak oncelik listesinden dusmustur
- `BUG-025` ana hipotezi aktif olarak cogu ltulememistir
- fakat ayni testte yakalanan continue-sonrasi menu/store donus klavye kilitlenmesi sorunu kapatilmistir

08.04.2026 ikinci tur manuel cogu ltma sonucunda asagidaki maddeler de netlesmistir:

- `BUG-026`
  - manuel olarak cogu ltilmistir
  - mevcut hipotez kapsami icinde sorun vermemistir
  - su asamada acik hotfix maddesi olarak kalmamistir
- `BUG-034`
  - manuel olarak cogu ltilmistir
  - editorde gorulen icerik/shape ile oyun ici gorunum arasinda bu turda sorun cikarilmamistir
  - su asamada acik hotfix maddesi olarak kalmamistir
- `BUG-068`
  - manuel olarak cogu ltilmistir
  - play sirasinda uygulanan editor degisikliklerinin aktif oyunu bozdugu bir durum bu turda cikarilmamistir
  - su asamada acik hotfix maddesi olarak kalmamistir

08.04.2026 itibariyla ikinci turdan sonra ertelenen maddeler:

- `BUG-037`
  - local editor force / remote hotfix / override precedence riski
  - su an oyuncuya donuk ana alpha runtime akisini bloklamiyor
  - asil onemi, final canli ayar ve web uygulamasi gecisinde ortaya cikacak
- `BUG-040`
  - test mode / sandbox save snapshot izolasyonu riski
  - su an gelistirici test katmani icinde izlenmesi gereken bir alan
  - asil onemi final release-safe build, liveops ve web panel gecisinde artacak

Bu nedenle 08.04.2026 karari:

- ikinci turda dogrudan oyuncu akisina dokunan runtime-kritik maddeler once kapatildi veya elendi
- `BUG-037` ve `BUG-040` ise teknik olarak notta tutulup
- final oyun hazirligi, release-safe build ve web/live-config gecisi asamasinda yeniden acilmak uzere ertelendi

## 08.04.2026 Sonu - Alpha Demo Icin Acil Bug Durumu

08.04.2026 sonunda, bu raporun manuel cogu ltulen bloklari uzerinden varilan karar su sekildedir:

- alpha demo oyuncu akisina dogrudan etki eden
- hemen hotfix gerektiren
- manuel olarak siradaki turda zorunlu cogu ltulmesi gereken

yeni bir bug maddesi kalmamistir.

Bu karar neden verilmiştir:

1. ilk iki turda test edilen runtime-kritik maddeler ya cogu ltulememis ya da kapatilmistir
2. kapanan maddeler:
   - `BUG-014`
   - `BUG-029`
   - score persistence turevi save/session sorunu
   - `BUG-025` testinde bulunan continue-sonrasi menu/store donus input kilidi
3. sorun vermeyen veya aktif bug olarak kalmayan maddeler:
   - `BUG-007`
   - `BUG-009`
   - `BUG-010`
   - `BUG-011`
   - `BUG-013`
   - `BUG-024`
   - `BUG-025` ana hipotezi
   - `BUG-026`
   - `BUG-027`
   - `BUG-034`
   - `BUG-068`

Bu nedenle bundan sonraki erteleme karari "bug unutulsun" anlami tasimaz. Anlami sudur:

- oyuncu deneyimini ve alpha demo akisini bloklamayan
- daha cok live-config, remote override, sandbox izolasyonu, telemetry veya market/release gecisiyle ilgili

maddeler ayri kovaya alinmistir.

### Erteleme Gerekceleri - Ayrintili

#### `BUG-037` neden ertelendi

Konu:

- editor local force / remote hotfix precedence riski

Neden alpha demo icin acil degil:

- su an canli web panel, cloud publish ve production remote rollout aktif kullanilmiyor
- bugun oyuncunun telefondaki alpha demo akisini bozan ana risk bu degil
- bu madde daha cok "finalde local override mi, remote override mi kazaniyor" sorusuna ait

Ne zaman yeniden acilacak:

- web uygulamasi
- remote manifest/version
- cloud uzerinden canli ayar

aktif olarak kurulmaya baslandiginda

Neden o zaman daha dogru:

- bu bug ancak gercek remote publish zinciri varken anlamli sekilde dogrulanabilir
- simdi zorlayici test yapmak, teorik kalir ve gereksiz zaman kaybettirir

#### `BUG-040` neden ertelendi

Konu:

- test mode / sandbox snapshot restore yeni state'i ezebilir

Neden alpha demo icin acil degil:

- bu risk oyuncu runtime'indan cok geliştirici/test mod izolasyonuna ait
- su an alpha oyuncu build'inin ana deneyimini bozan bir davranis olarak cogu ltulmedi
- asil kritikligi, release-safe build ve liveops oncesi artacak

Ne zaman yeniden acilacak:

- test katmanlarini shipping davranisindan ayirma
- release build profile
- markete hazir package

asamasinda

Neden o zaman daha dogru:

- o noktada `Default / Free / Premium` ayriminin oyuncu build'ine sizmadigi kesinlestirilecek
- sandbox snapshot mantigi tam release baglaminda yeniden kontrol edilecek

#### `BUG-043` neden ertelendi

Konu:

- store presenter production provider'i bypass edebilir

Neden alpha demo icin acil degil:

- su an store akisi bilerek `MockPurchaseService` ve preview/practice provider mantigiyla calisiyor
- gercek billing provider olmadigi icin bu bugun tam dogrulanmasi bugunden verimli degil

Ne zaman yeniden acilacak:

- production billing
- gercek pricing provider
- entitlement / restore purchase

gecisinde

#### `BUG-046` neden ertelendi

Konu:

- theme fiyat fallback ve provider uyumsuzlugu

Neden alpha demo icin acil degil:

- final storefront pricing henuz aktif degil
- su an preview pricing ile gameplay/store akisi test ediliyor

Ne zaman yeniden acilacak:

- gercek Play Billing fiyatlari ve storefront quote sistemi baglandiginda

#### `BUG-064` ve `BUG-065` neden ertelendi

Konu:

- telemetry queue trim
- pending telemetry count / queue uyumu

Neden alpha demo icin acil degil:

- telemetry su an production veri operasyonu degil
- oyuncunun anlik alpha deneyimini bozan ana blocker bunlar degil

Ne zaman yeniden acilacak:

- telemetry upload/flush/ack zinciri finalize oldugunda
- web panel ve analiz tarafina veri akmaya basladiginda

#### `BUG-066` ve `BUG-067` neden ertelendi

Konu:

- partial remote manifest
- tek dil remote override / diger dil local fallback

Neden alpha demo icin acil degil:

- bu maddeler canli locale/content operasyonu ile ilgilidir
- bugun yerel editor ve local content odakli calisiyoruz

Ne zaman yeniden acilacak:

- web panelden locale bazli publish
- cloud remote override
- staged rollout

devreye girdiginde

#### `BUG-074` neden ertelendi

Konu:

- release build'e debug/test presenter sizmasi

Neden alpha demo icin acil degil:

- bu madde markete cikis oncesi son kapidan gecis testidir
- alpha icinde ic tooling ve test katmanlari bilerek acik tutuluyor

Ne zaman yeniden acilacak:

- release-safe build
- market package
- final smoke matrix

asamasinda

#### `BUG-075` ve `BUG-076` neden ertelendi

Konu:

- resources yukunun buyumesi
- editor/reference assetlerinin build'e sizmasi

Neden alpha demo icin acil degil:

- bunlar final content lock ve build optimizasyon asamasinda gercek degerini gosterir
- bugun erken optimize etmek yerine once alpha kapsam kilitlenmelidir

Ne zaman yeniden acilacak:

- alpha content lock sonrasi
- final APK boyutu ve memory olcumleri yapilirken

#### `BUG-077` neden ertelendi

Konu:

- production provider gecisinde store/editor parity kirilmasi

Neden alpha demo icin acil degil:

- gercek production provider'lar henuz baglanmadi
- bugun bunu zorlamak teorik kalir

Ne zaman yeniden acilacak:

- production monetization pass
- billing/rewarded/pricing gercek provider gecisi

asamasinda

### 08.04.2026 Sonu Net Karar

Bu rapora gore su an:

- alpha demo icin acil manuel bug cogu ltma zorunlulugu kalmamistir
- siradaki mantikli odak yeni runtime bug kovalamak degil
- alpha icerik, tasarim, hissiyat ve final hazirlik sirasini surdurmektir

Ertelenen maddeler unutulmaz. Bunlar:

- `final live-config gecisi`
- `web panel`
- `release-safe build`
- `market hazirligi`

asamalarinda yeniden acilacak resmi bekleme listesidir.

## Metodoloji

Bu raporun her maddesi su mantikla yazildi:

- kodda veya mimaride muhtemel kirilma noktasi bulundu
- bu noktanin oyuncu akisinda nasil bozulabilecegi dusunuldu
- manuel cogu ltma adimlari cikartildi
- olasi kok neden ve etkilenen dosyalar not edildi

## Oncelik Skalasi

- `P0`: save kaybi, yanlis odul, yanlis satin alma, blocker crash, ilerleme kaybi
- `P1`: ana oyun akisinin bozulmasi, resume/fail/result/store parity bozulmasi
- `P2`: UI/layout/presentation bozulmasi, editor apply parity bozulmasi
- `P3`: minor sunum, stale state, telemetry veya operasyonel rahatsizliklar

## Olasilik Skalasi

- `Yuksek`: koddan dogrudan sezilen veya mimari olarak kolay tetiklenebilir
- `Orta`: belirli kosullarda ortaya cikabilecek
- `Dusuk`: daha ozel kosul veya gelecekte buyume ile tetiklenecek

## Kapsam Ozet Tablosu

- Boot / Scene / Singleton / Init: 6 senaryo
- Save / Session / Restore: 9 senaryo
- Gameplay / Hit / Input / Rotator: 8 senaryo
- Question / Fail / Info / Result Flow: 7 senaryo
- Content / Localization / Shape: 8 senaryo
- Economy / Store / Membership / Energy: 10 senaryo
- Unified Editor / Apply / Sync: 7 senaryo
- UI / Mobile / Theme / Presentation: 7 senaryo
- Telemetry / Remote Content / Hotfix: 7 senaryo
- Release / Performance / Scale Riskleri: 8 senaryo

Toplam: **77 olasi bug senaryosu**

---

## 1. Boot / Scene / Singleton / Init

### BUG-001 - Runtime presenterlarin cift olusmasi
- Tur: Happy Path + Scene init
- Oncelik: P1
- Olasilik: Orta
- Supheli neden:
  - `GameManager.EnsureRuntimePresenters()` her sahnede `FailModalPresenter`, `RotatorPlaquePresenter`, `ImpactFeedbackController` ariyor.
  - Sahne icinde benzer presenter var ama inactive, farkli parent altinda ya da gec aciliyorsa duplicate olusabilir.
- Etkilenen dosyalar:
  - `Scripts/Core/GameManager.cs`
  - `Scripts/Presentation/FailModalPresenter.cs`
  - `Scripts/Presentation/RotatorPlaquePresenter.cs`
- Manuel cogu ltma adimlari:
  1. Gameplay sahnesini ac.
  2. Sahnedeki ilgili presenter objelerinden birini inactive yap ya da prefab varyanti ile sahneye ikinci kopya koy.
  3. Oyunu Play ile baslat.
  4. Fail modal veya rotator plaque cizimini tetikle.
  5. Hierarchy ve davranisi kontrol et.
- Olasi belirti:
  - iki modal ust uste acilabilir
  - plaque gorselleri iki kez cizilebilir
  - ayni event iki farkli presenter tarafindan dinlenebilir
- Sonraki otomasyon adayi:
  - sahnede runtime presenter sayisi assertion testi

### BUG-002 - Yanlis Canvas veya Camera secimi
- Tur: Edge Case
- Oncelik: P1
- Olasilik: Yuksek
- Supheli neden:
  - `FindObjectOfType<Canvas>()`, `FindObjectOfType<Camera>()`, `Camera.main` secimleri cok sayida yerde kullaniliyor.
  - Birden fazla canvas veya camera oldugunda yanlis referans secilebilir.
- Etkilenen dosyalar:
  - `Scripts/Core/GameManager.cs`
  - `Scripts/Presentation/FailModalPresenter.cs`
  - `Scripts/Presentation/ThemeRuntimeController.cs`
  - `Scripts/Presentation/DebugRewardedAdPresenter.cs`
- Manuel cogu ltma adimlari:
  1. Gameplay sahnesine ikinci bir canvas ekle.
  2. Veya ikinci bir camera ekle ve tag/priority degistir.
  3. Play ile baslat.
  4. Fail modal, debug ad, theme glow, HUD gibi yuzeyleri tetikle.
- Olasi belirti:
  - modal yanlis canvas altinda acilir
  - debug ad baska overlay altinda kalir
  - audio listener yanlis camera'ya eklenir

### BUG-003 - Pending gameplay request ile aktif session restore cakismasi
- Tur: Happy Path transition
- Oncelik: P0
- Olasilik: Orta
- Supheli neden:
  - `GameManager.Start()` once `SceneNavigator` pending request'ini tuketiyor, sonra aktif session restore deniyor.
  - Gecis durumlarinda yanlis yol secilebilir.
- Etkilenen dosyalar:
  - `Scripts/Core/GameManager.cs`
  - `Scripts/Core/SceneNavigator.cs`
  - `Scripts/Core/SessionManager.cs`
- Manuel cogu ltma adimlari:
  1. Oyun sirasinda aktif session olustur.
  2. Store'a git.
  3. Store'dan belirli bir level icin gameplay request tetikle.
  4. Oyunu kapatip ac veya sahne gecisini yarida kes.
  5. Baslangicta hangi level/session yukleniyor kontrol et.
- Olasi belirti:
  - yanlis level acilir
  - resume gereken session yerine yeni level baslar
  - progress veya fail/result pending durumu kaybolur

### BUG-004 - SceneBootstrap duplicate singleton riski
- Tur: Edge Case
- Oncelik: P1
- Olasilik: Orta
- Supheli neden:
  - Scene bootstrap ve runtime singleton persist kurallari birlikte duplicate instance uretebilir.
  - Ozellikle persist across scenes olan servislerle non-persist olanlarin karisimi riskli.
- Etkilenen dosyalar:
  - `Scripts/Core/SceneBootstrap.cs`
  - `Scripts/Core/Singleton.cs`
  - `Scripts/Services/*`
- Manuel cogu ltma adimlari:
  1. Menu -> gameplay -> store -> gameplay -> menu akisini birkac kez tekrarla.
  2. Hierarchy'de purchase/pricing/content/telemetry singleton sayilarini izle.
  3. Olasi duplicate log veya farkli state goruluyor mu kontrol et.
- Olasi belirti:
  - birden fazla singleton instance
  - bir event iki kez tetiklenir
  - stale cache veya farkli instance state'i olusur

### BUG-005 - Missing reference oldugunda sessiz fail
- Tur: Edge Case
- Oncelik: P1
- Olasilik: Yuksek
- Supheli neden:
  - Cok sayida `FindObjectOfType` fallback kullaniminda null donulurse kod sessiz sekilde davranis kaybedebilir.
- Etkilenen dosyalar:
  - `GameManager`, `LevelFlowController`, `PinLauncher`, `ThemeRuntimeController`, editor runtime refresh utility
- Manuel cogu ltma adimlari:
  1. Sahneden bir ana component'i gecici olarak kaldir.
  2. Play ile baslat.
  3. Hangi gameplay veya UI akislarinin bozuldugunu izle.
- Olasi belirti:
  - crash yerine sessiz ozellik kaybi
  - fail modal acilmamasi
  - shape/plaque build olmamasi

### BUG-006 - Play mode ve edit mode davranis farki
- Tur: Happy Path + Editor/runtime parity
- Oncelik: P2
- Olasilik: Orta
- Supheli neden:
  - Bir kisim runtime presenter play modda auto-create oluyor, edit modda prebuilt objelere yaslaniyor.
- Etkilenen dosyalar:
  - `GameManager`
  - `FailModalPresenter`
  - `StorePresenter`
  - `ThemeRuntimeController`
- Manuel cogu ltma adimlari:
  1. Play oncesi sahneyi inspect et.
  2. Play modda ayni UI yuzeylerini tekrar incele.
  3. Runtime olusan objelerin edit-time layout ile birebir olup olmadigini karsilastir.
- Olasi belirti:
  - play modda farkli layout
  - runtime label olusmasi nedeniyle kayma

---

## 2. Save / Session / Restore

### BUG-007 - Throttle araliginda save kaybi
- Tur: Edge Case
- Oncelik: P0
- Olasilik: Yuksek
- Supheli neden:
  - `SaveManager.Save()` yazmayi throttle ediyor.
  - Oyun kapanisi/crash throttle penceresine denk gelirse son state yazilmamis olabilir.
- Etkilenen dosyalar:
  - `Scripts/Core/SaveManager.cs`
  - `Scripts/Core/SessionManager.cs`
- Manuel cogu ltma adimlari:
  1. Harf reveal, continue, store alimi veya dil degisikligi gibi save tetikleyen bir eylem yap.
  2. Eylemden hemen sonra uygulamayi zorla kapat.
  3. Uygulamayi yeniden ac.
  4. Son eylemin persisted olup olmadigini kontrol et.
- Olasi belirti:
  - son harf acilmasi, coin, theme, energy ya da session state geri gitmis olur

### BUG-008 - Bozuk save dosyasinda tum verinin sifirlanmasi
- Tur: Edge Case
- Oncelik: P0
- Olasilik: Orta
- Supheli neden:
  - `SaveManager.Load()` try/catch'te hata olursa tum save'i yeni `PlayerSaveData` ile resetliyor.
- Etkilenen dosyalar:
  - `Scripts/Core/SaveManager.cs`
  - `Scripts/Core/PlayerSaveModels.cs`
- Manuel cogu ltma adimlari:
  1. Save dosyasina elle kismi bozuk JSON yaz.
  2. Oyunu ac.
  3. Progress, coin, tema, membership, telemetry state'lerini kontrol et.
- Olasi belirti:
  - tum ilerleme sifirlanir
  - bozuk save icin grace/backup davranisi yoktur

### BUG-009 - Pause ve quit snapshot sirasinda daha yeni state'in ezilmesi
- Tur: Edge Case
- Oncelik: P1
- Olasilik: Orta
- Supheli neden:
  - `SessionManager.TakeSnapshotAfterReveal()` coroutine ile gec snapshot aliyor.
  - Pause/quit ayni anda olursa daha yeni state eski snapshot ile ezilebilir.
- Etkilenen dosyalar:
  - `Scripts/Core/SessionManager.cs`
  - `Scripts/Core/SaveManager.cs`
- Manuel cogu ltma adimlari:
  1. Harf reveal olustur.
  2. Reveal frame'ine yakin anda uygulamayi pause et.
  3. Sonra app'i kapatip ac.
  4. Revealed letter count ve target slot'u karsilastir.
- Olasi belirti:
  - bir harf geri gitme
  - yanlis target slot restore

### BUG-010 - Content degistikten sonra session restore index uyumsuzlugu
- Tur: Edge Case
- Oncelik: P0
- Olasilik: Yuksek
- Supheli neden:
  - `SessionSnapshot` questionIndex, revealedLetters, currentTargetSlotIndex gibi ham sayilar tutuyor.
  - Editor/content degisikligi sonrasi ayni level sorulari degisirse restore anlamsizlasabilir.
- Etkilenen dosyalar:
  - `Scripts/Core/SessionManager.cs`
  - `Scripts/Core/LevelFlowController.cs`
  - `Scripts/Services/ContentService.cs`
- Manuel cogu ltma adimlari:
  1. Bir session ortasinda oyunu birak.
  2. Icerik editorunden ayni levelin soru sayisini veya soru sirasini degistir.
  3. `Kaydet ve Canli Uygula` veya yeniden acilis yap.
  4. Session restore davranisini izle.
- Olasi belirti:
  - yanlis soru acilir
  - restore crash ya da sessiz tutarsizlik olur
  - reveal sayisi soru uzunlugunu asar

### BUG-011 - Shape slot sayisi degisince eski snapshot parity bozulmasi
- Tur: Edge Case
- Oncelik: P1
- Olasilik: Yuksek
- Supheli neden:
  - `revealedSlotIndices`, `revealedPinLocalPositions`, `revealedPinLocalRotations` eski layout'a gore tutuluyor.
  - Yeni shape veya yeni slot count ile restore edilince anlam kaybeder.
- Etkilenen dosyalar:
  - `LevelFlowController`
  - `RotatorPlaquePresenter`
  - `WordSpinAlphaContentEditorWindow`
- Manuel cogu ltma adimlari:
  1. Shape kullanan bir levelde session birak.
  2. Shape library'de slot sayisini degistir veya farkli shape ata.
  3. Oyunu yeniden baslat ve restore et.
- Olasi belirti:
  - pinned pinler havada kalir
  - wrong slot active olur
  - plaque/pin goruntusu kayar

### BUG-012 - Dil degisimi sonrasi answer length mismatch
- Tur: Edge Case
- Oncelik: P1
- Olasilik: Orta
- Supheli neden:
  - Session restore `revealedLetters` sayisini sakliyor.
  - Farkli dilde cevap uzunlugu degisirse ayni snapshot yeni dile uymayabilir.
- Etkilenen dosyalar:
  - `LevelFlowController`
  - `ContentService`
  - localization content dosyalari
- Manuel cogu ltma adimlari:
  1. Bir soruda reveal ilerlet.
  2. Dili baska bir dile cevir.
  3. Oyunu kapat/ac veya session restore tetikle.
  4. Reveal ve target akisini kontrol et.
- Olasi belirti:
  - reveal sayisi fazla/eksik olur
  - target harf yanlis olur

### BUG-013 - Pending fail resolution stale kalmasi
- Tur: Happy Path transition
- Oncelik: P1
- Olasilik: Orta
- Supheli neden:
  - `pendingFailResolution` save'e yaziliyor.
  - Retry/continue/store/menu donusunde her yol bunu temizlemeyebilir.
- Etkilenen dosyalar:
  - `GameManager`
  - `SessionManager`
  - `FailModalPresenter`
- Manuel cogu ltma adimlari:
  1. Fail modal ac.
  2. Continue yerine store/menu/back gibi farkli gecisler dene.
  3. Uygulamayi kapat/ac.
  4. Fail modalin gereksiz geri gelip gelmedigine bak.
- Olasi belirti:
  - gameplay donunce input kilitli kalir
  - fail modal kendiliginden tekrar acilir

### BUG-014 - Pending info card ve pending result flag cakismasi
- Tur: Edge Case
- Oncelik: P1
- Olasilik: Orta
- Supheli neden:
  - `GameManager` hem pending info card hem pending result state tutuyor.
  - Belirli sira hatalarinda biri temizlenmeden digeri setlenebilir.
- Etkilenen dosyalar:
  - `GameManager`
  - `InfoCardPresenter`
  - `ResultPresenter`
- Manuel cogu ltma adimlari:
  1. Info card olan son soruyu tamamla.
  2. Info card acikken uygulamayi kapat/ac.
  3. Sonra tekrar continue/retry/scene donusleri yap.
  4. Info card ve result sirasini izle.
- Olasi belirti:
  - result hic acilmaz
  - info card iki kez acilir
  - dogrudan level complete'e sicrama olur

### BUG-015 - Session temizleme yerine eski state'in kalmasi
- Tur: Happy Path
- Oncelik: P1
- Olasilik: Orta
- Supheli neden:
  - `SessionManager.ClearSnapshot()` sadece session alanini replace ediyor.
  - Bagli pending UI veya side effect state baska yerlerde stale kalabilir.
- Etkilenen dosyalar:
  - `SessionManager`
  - `GameManager`
  - `ScoreManager`
- Manuel cogu ltma adimlari:
  1. Fail/result/info card / continue state ureterek session olustur.
  2. `Save ve Session Paneli` ile reset veya retry yap.
  3. Oyunu yeniden ac.
- Olasi belirti:
  - score/result stale kalir
  - pending UI yeniden acilir

---

## 3. Gameplay / Hit / Input / Rotator

### BUG-016 - Active slot detection esit acilarda kararsiz
- Tur: Happy Path + tuning
- Oncelik: P1
- Olasilik: Orta
- Supheli neden:
  - `SlotManager.DetectActiveSlot()` yalnizca en kucuk aciyi seciyor.
  - Simetrik veya yakin acili slotlarda flicker olabilir.
- Etkilenen dosyalar:
  - `SlotManager`
  - `RotatorPlaquePresenter`
- Manuel cogu ltma adimlari:
  1. Aktivasyon acisini dar/genis editorle degistir.
  2. Simetrik shape kullan.
  3. Dondurme sirasinda aktif hedefin flicker yapip yapmadigini izle.
- Olasi belirti:
  - target plaque sik sik degisir
  - wrong slot hissi olusur

### BUG-017 - Wrong slot ve miss akislarinda cift ceza veya tutarsiz metric
- Tur: Edge Case
- Oncelik: P1
- Olasilik: Orta
- Supheli neden:
  - `ResolvePinHit`, `HandlePinFlightMiss`, `RegisterQuestionError` farkli yollardan metric ve heart azaltma akisi yaratir.
- Etkilenen dosyalar:
  - `GameManager`
  - `HitEvaluator`
  - `QuestionLifeManager`
- Manuel cogu ltma adimlari:
  1. Wrong slot, wrong letter, near miss, tam miss senaryolarini ayri ayri dene.
  2. Her birinde heart dusumu ve telemetry/metric sayilarini karsilastir.
  3. Ardisik hizli denemelerde tutarsizlik var mi bak.
- Olasi belirti:
  - bir senaryoda gereksiz fazla ceza
  - metric turu ile gercek etki uyusmaz

### BUG-018 - InputBuffer beklenen harfi stale tutmasi
- Tur: Happy Path transition
- Oncelik: P1
- Olasilik: Orta
- Supheli neden:
  - `RefreshCurrentTarget()` icinde `inputBuffer.SetExpectedLetter(targetLetter)` yapiliyor.
  - Question advance, restore veya language/content refresh zamanlarinda stale kalabilir.
- Etkilenen dosyalar:
  - `LevelFlowController`
  - `InputBuffer`
  - `InputManager`
- Manuel cogu ltma adimlari:
  1. Soru ortasinda dil degistir.
  2. Info card sonrasi sonraki soruya gec.
  3. Editorle soru cevabini degistir.
  4. Beklenen harf ile gercek hedef ayni mi test et.
- Olasi belirti:
  - dogru harfte wrong letter
  - klavye ve target hint uyusmaz

### BUG-019 - Random slot sequence ile reveal order uyumsuzlugu
- Tur: Edge Case
- Oncelik: P1
- Olasilik: Orta
- Supheli neden:
  - `_revealOrder`, `_slotSequence`, `_answerIndexToSlotIndex` birlikte olusturuluyor.
  - Random ve shape/slot count kombinasyonlarinda hata olabilir.
- Etkilenen dosyalar:
  - `LevelFlowController`
- Manuel cogu ltma adimlari:
  1. `randomSlots` acik bir level sec.
  2. Farkli slot sayili shape ile ayni leveli dene.
  3. Ilk hedef harf, reveal sirasi ve gercek plakadaki harfleri takip et.
- Olasi belirti:
  - reveal edilen harf ile hedef slot uyusmaz
  - answer indexing kayar

### BUG-020 - Extreme hit tuning ile collider mantiginin terslenmesi
- Tur: Edge Case
- Oncelik: P1
- Olasilik: Yuksek
- Supheli neden:
  - editorlerden perfect width/height, near miss padding, plaque size ayarlari cok serbest.
  - Menzil alanlari birbirini anlamsiz sekilde kapsayabilir.
- Etkilenen dosyalar:
  - `Slot`
  - `SlotManager`
  - `SlotHitTuningWindow`
- Manuel cogu ltma adimlari:
  1. Perfect ve near miss degerlerini asiri yuksek/asiri dusuk yap.
  2. Play modda farkli impact tipleri olusturmaya calis.
  3. Beklenen zone ayrimi calisiyor mu bak.
- Olasi belirti:
  - near miss hic gelmez
  - her sey perfect olur
  - wrong slot/miss ayrimi bozulur

### BUG-021 - Rotator assist state restart/restore sonrasi temizlenmemesi
- Tur: Edge Case
- Oncelik: P2
- Olasilik: Orta
- Supheli neden:
  - `_perfectMomentumLevel`, `_persistentFlowSpeedMultiplier`, `_perfectChainWindowUntil` state alanlari var.
  - Her gecis yolunda resetlenmediyse pacing stale kalir.
- Etkilenen dosyalar:
  - `LevelFlowController`
  - `TargetRotator`
- Manuel cogu ltma adimlari:
  1. Art arda perfect zinciri olustur.
  2. Fail, retry, restore veya question advance yap.
  3. Rotator hizinin gereksiz kalici kalip kalmadigini izle.
- Olasi belirti:
  - yeni soruda onceki momentum hissi devam eder

### BUG-022 - Shape rebuild sirasinda pinned pin parity bozulmasi
- Tur: Edge Case
- Oncelik: P1
- Olasilik: Yuksek
- Supheli neden:
  - `RotatorPlaquePresenter.RebuildLayout()` ve `SlotManager.ApplyShapeLayout()` yeni konumlar veriyor.
  - Halihazirda restore edilmis pinler veya current target bu rebuild ile kayabilir.
- Etkilenen dosyalar:
  - `RotatorPlaquePresenter`
  - `LevelFlowController`
  - `PinLauncher`
- Manuel cogu ltma adimlari:
  1. Birkac harf saplanmis bir session olustur.
  2. Shape veya plaque tuning uygula.
  3. `Kaydet ve Uygula` de.
  4. Saplanan pinler ve plaque'ler hizali mi bak.
- Olasi belirti:
  - pinler plaque disinda kalir
  - active target farkli yere atlar

### BUG-023 - Launcher / pin tuning ile actual hit point kaymasi
- Tur: Happy Path + tuning
- Oncelik: P1
- Olasilik: Orta
- Supheli neden:
  - pin visual size, tip offset, load tween, spawn offset gibi alanlar ayarlanabiliyor.
  - Visual degisiklik hit evaluator'un kullandigi uc nokta ile ayrisabilir.
- Etkilenen dosyalar:
  - `Pin`
  - `PinLauncher`
  - `PinInputTuningWindow`
- Manuel cogu ltma adimlari:
  1. Pin boyutu ve uc offsetlerini ciddi sekilde degistir.
  2. Ayni hedefe birkac kez ates et.
  3. Gozle goren hit ile evaluator sonucu ayni mi kontrol et.
- Olasi belirti:
  - gorunurde plaque'e degiyor ama miss oluyor
  - gorunurde disarda ama perfect oluyor

---

## 4. Question / Fail / Info / Result Flow

### BUG-024 - Info card acikken dil degisince stale icerik
- Tur: Happy Path transition
- Oncelik: P2
- Olasilik: Orta
- Supheli neden:
  - `InfoCardPresenter.HandleLanguageChanged()` sadece `_catalog = null` ve `RefreshLocalizedTexts()` yapiyor.
  - Acik kart icerigi yeniden resolve edilmeyebilir.
- Etkilenen dosyalar:
  - `InfoCardPresenter`
- Manuel cogu ltma adimlari:
  1. Info card ac.
  2. Kart acikken dili degistir.
  3. Title/body metni yeni dilde mi kontrol et.
- Olasi belirti:
  - buton dili degisir ama kart icerigi eski dilde kalir

### BUG-025 - Fake rewarded callback gec donerse yanlis state'e uygulanmasi
- Tur: Edge Case
- Oncelik: P1
- Olasilik: Orta
- Supheli neden:
  - `DebugRewardedAdPresenter.ShowCountdown()` callback tutuyor.
  - Kullanici countdown sirasinda store/menu/scene degistirirse callback eski gameplay state'ine donebilir.
- Etkilenen dosyalar:
  - `FailModalPresenter`
  - `DebugRewardedAdPresenter`
  - `GameManager`
- Manuel cogu ltma adimlari:
  1. Fail modal ac.
  2. Continue ile fake rewarded countdown baslat.
  3. Countdown bitmeden store/menu/back gibi akislari dene.
  4. Callback geldikten sonra state'i izle.
- Olasi belirti:
  - yanlis sahnede continue uygulanir
  - modal kapanmadan can geri gelir

### BUG-026 - Retry enerji kurali ile premium bypass parity bozulmasi
- Tur: Happy Path
- Oncelik: P1
- Olasilik: Orta
- Supheli neden:
  - `FailModalPresenter.RefreshText()` retry interactivity'sini premium veya currentEnergy > 0 ile belirliyor.
  - Gercek retry akisi `GameManager.RetryCurrentLevel()` ve `SceneNavigator.OpenGameplayLevel(levelId, true)` uzerinden gidiyor.
- Etkilenen dosyalar:
  - `FailModalPresenter`
  - `GameManager`
  - `EnergyManager`
- Manuel cogu ltma adimlari:
  1. Premium acik/kapali, enerji 0/1 farkli kombinasyonlari dene.
  2. Retry butonu gorunumu ile gercek davranis ayni mi kontrol et.
- Olasi belirti:
  - buton aktif ama retry basarisiz
  - buton pasif ama retry aslinda mumkun

### BUG-027 - Pending result restore cift odul riski
- Tur: Edge Case
- Oncelik: P0
- Olasilik: Orta
- Supheli neden:
  - result pending state save'e yaziliyor.
  - app restart veya yeniden restore durumunda odul mantigi tekrar uygulanabilir.
- Etkilenen dosyalar:
  - `GameManager`
  - `ResultPresenter`
  - `LevelEconomyManager`
- Manuel cogu ltma adimlari:
  1. Leveli tamamla.
  2. Result ekranina yakin anda uygulamayi kapat.
  3. Yeniden ac.
  4. Coin, star, progress odulu bir kez mi iki kez mi kontrol et.
- Olasi belirti:
  - cift coin/stars
  - lastCompletedLevel iki kez ileri sarabilir

### BUG-028 - Info card kapanisinda wrong continuation branch
- Tur: Edge Case
- Oncelik: P1
- Olasilik: Orta
- Supheli neden:
  - `HandleInfoCardClosed()` once `_pendingQuestionAdvanceAfterInfoCard`, sonra `_pendingLevelCompleteAfterInfoCard` kontrol ediyor.
  - Stale flag varsa wrong branch calisabilir.
- Etkilenen dosyalar:
  - `GameManager`
  - `InfoCardPresenter`
- Manuel cogu ltma adimlari:
  1. Orta soru ve son soru info card akislari ayri ayri dene.
  2. Kapatma oncesi app pause/resume yap.
  3. Continue sonrasi next question mi result mi dogru aciliyor bak.
- Olasi belirti:
  - orta sorudan sonra level complete
  - son sorudan sonra yeni soru yuklenmesi

### BUG-029 - Store donusu sonrasi pending fail veya result UI'nin kaybolmasi
- Tur: Happy Path transition
- Oncelik: P1
- Olasilik: Yuksek
- Supheli neden:
  - Store acilisi gameplay state'inden ayriliyor.
  - Donduste `RestorePendingCompletionUi` veya fail restore zinciri her yol icin ayni olmayabilir.
- Etkilenen dosyalar:
  - `GameManager`
  - `SceneNavigator`
  - `StorePresenter`
- Manuel cogu ltma adimlari:
  1. Fail modal acikken store'a git.
  2. Result veya info card pending halde store'a git.
  3. Geri don.
  4. Gerekli UI geri geliyor mu bak.
- Olasi belirti:
  - input kilitli ama modal yok
  - pending info/result kaybolur

### BUG-030 - Main menu donusunde yanlis resume yolu
- Tur: Edge Case
- Oncelik: P1
- Olasilik: Orta
- Supheli neden:
  - aktif session, pending request ve manual level open akislari menu uzerinden tekrar kesisebilir.
- Etkilenen dosyalar:
  - `SceneNavigator`
  - `GameManager`
  - `MainMenuPresenter`
- Manuel cogu ltma adimlari:
  1. Oyun ortasinda menu'ye don.
  2. Menu'den ayni leveli veya farkli leveli ac.
  3. Resume/save restore beklentisi ile gercek davranisi karsilastir.
- Olasi belirti:
  - eski session geri gelir
  - secilen yeni level yerine eski level acilir

---

## 5. Content / Localization / Shape

### BUG-031 - Local/remote merge sirasinda katalog order drift
- Tur: Edge Case
- Oncelik: P2
- Olasilik: Yuksek
- Supheli neden:
  - `ContentService.MergeById` dictionary `Values` uzerinden donuyor.
  - Sort garantisi yok.
- Etkilenen dosyalar:
  - `ContentService`
- Manuel cogu ltma adimlari:
  1. Local ve remote'da ayni kataloglara farkli id siralari ver.
  2. Oyunu ac veya remote refresh yap.
  3. Level sirasi, shape listesi, difficulty listesi ayni mi bak.
- Olasi belirti:
  - liste sirasinda drift
  - editor ile runtime order farki

### BUG-032 - Duplicate id sessiz override
- Tur: Edge Case
- Oncelik: P1
- Olasilik: Yuksek
- Supheli neden:
  - `MergeById` duplicate id'leri sessizce son gelenle override ediyor.
  - Runtime veya remote conflictlerde veri kaybi gizlenebilir.
- Etkilenen dosyalar:
  - `ContentService`
  - `ValidationAuditWindow`
  - `WordSpinAlphaContentEditorData`
- Manuel cogu ltma adimlari:
  1. Ayni id'li iki question/shape/theme tanimi olustur.
  2. Local + remote kombinasyonlariyla test et.
  3. Runtime'da hangisinin gectigini kontrol et.
- Olasi belirti:
  - beklenmeyen veri sessizce kullanilir

### BUG-033 - Eksik localized content fallback tutarsizligi
- Tur: Edge Case
- Oncelik: P2
- Olasilik: Yuksek
- Supheli neden:
  - Remote localized dosya varsa onu aliyor, yoksa root dosyaya dusuyor.
  - Lokalize kataloglar ile root kataloglar arasinda kapsama farki olabilir.
- Etkilenen dosyalar:
  - `RemoteContentProvider`
  - `LocalContentProvider`
  - content locale dosyalari
- Manuel cogu ltma adimlari:
  1. Bir dilde localized question var, info card yok durumunu kur.
  2. O dilde runtime'i ac.
  3. Soru, info card, level metadata birlikte dogru mu kontrol et.
- Olasi belirti:
  - soru yeni dilde, info card root dilde
  - locale parity bozulur

### BUG-034 - Shape editor preview ile gameplay parity farki
- Tur: Happy Path + tuning
- Oncelik: P1
- Olasilik: Yuksek
- Supheli neden:
  - editor preview, geometry resolve, runtime plaque presenter uc farkli katman.
  - Esitlik icin birden fazla patch yapildi; yine drift riski var.
- Etkilenen dosyalar:
  - `WordSpinAlphaContentEditorWindow`
  - `WordSpinAlphaContentEditorData`
  - `ShapeLayoutGeometry`
  - `RotatorPlaquePresenter`
- Manuel cogu ltma adimlari:
  1. Referans gorselden shape olustur.
  2. Elle noktalar ve acilarla ince ayar yap.
  3. Play ekraninda ayni layout'u karsilastir.
- Olasi belirti:
  - preview guzel, runtime cirkin
  - spacing farkli
  - plaque rotation farkli

### BUG-035 - Visual prefab path fallback'inin sessiz kirilmasi
- Tur: Edge Case
- Oncelik: P2
- Olasilik: Orta
- Supheli neden:
  - `RotatorPlaquePresenter.TryBuildPrefabVisual` prefab bulamazsa procedural build'e donuyor.
  - Kullanici eksik path'i fark etmeyebilir.
- Etkilenen dosyalar:
  - `RotatorPlaquePresenter`
  - shape layout content
- Manuel cogu ltma adimlari:
  1. Bir shape'e invalid `visualPrefabResourcePath` ver.
  2. Play ile ac.
  3. Expected prefab yerine fallback procedural visual geldi mi izle.
- Olasi belirti:
  - gorsel sessizce degisir
  - ekip prefab sanirken runtime procedural calisir

### BUG-036 - Slot sayisi degisince manuel custom aci/point kaybi
- Tur: Edge Case
- Oncelik: P2
- Olasilik: Orta
- Supheli neden:
  - custom pointler ve per-slot aci offsetleri yeniden ornekleniyor.
  - Karmasik shape'lerde manuel estetik bozulabilir.
- Etkilenen dosyalar:
  - `WordSpinAlphaContentEditorData`
  - `ShapeLayoutGeometry`
- Manuel cogu ltma adimlari:
  1. Elle ince ayarlanmis bir shape olustur.
  2. Slot sayisini azalt/arttir.
  3. Seklin onceki karakteri korunuyor mu kontrol et.
- Olasi belirti:
  - manuel iscilik kayar
  - shape organik formunu kaybeder

### BUG-037 - Editor local force durumu remote testini maskelemesi
- Tur: Edge Case
- Oncelik: P2
- Olasilik: Orta
- Supheli neden:
  - `ContentService.RefreshEditorContent()` `_forceLocalEditorContent = true` yapiyor.
  - Remote testing sonrasi stale local override kalabilir.
- Etkilenen dosyalar:
  - `ContentService`
  - `RemoteContentHotfixWindow`
- Manuel cogu ltma adimlari:
  1. Editor ile local content degistir.
  2. Sonra remote override publish et.
  3. Play sirasinda remote refresh dene.
  4. Gercekten remote mu yoksa local mi kullaniliyor kontrol et.
- Olasi belirti:
  - remote aktif sanilir ama local kalir

### BUG-038 - Language change sirasinda acik level content tam yenilenmemesi
- Tur: Happy Path transition
- Oncelik: P1
- Olasilik: Yuksek
- Supheli neden:
  - Dili degistirmek bazı presenterlari refresh ediyor, ama current question/shape/store/info/result tum zincir ayni anda rehydrate olmayabilir.
- Etkilenen dosyalar:
  - `GameManager`
  - `ContentService`
  - `InfoCardPresenter`
  - `GameplayHudPresenter`
  - `StorePresenter`
- Manuel cogu ltma adimlari:
  1. Gameplay sirasinda dili degistir.
  2. Ardindan info card, store, fail, result, HUD alanlarini kontrol et.
- Olasi belirti:
  - sahnenin bir bolumu yeni dilde, digeri eski dilde kalir

---

## 6. Economy / Store / Membership / Energy

### BUG-039 - Baslangic grant policy degisince eski save'lerde yeni grant verilmemesi
- Tur: Future-ready edge case
- Oncelik: P1
- Olasilik: Yuksek
- Supheli neden:
  - `startingHintsGranted` ve `startingSoftCurrencyGranted` flagleri bir kez setleniyor.
  - Sonradan starter grant miktari degisirse eski oyuncular bunu alamayabilir.
- Etkilenen dosyalar:
  - `EconomyManager`
  - `EnergyConfigDefinition`
  - save models
- Manuel cogu ltma adimlari:
  1. Bir save olustur, starter grant flaglerini true yap.
  2. Config'te starter miktarlari artir.
  3. Oyunu yeniden ac.
  4. Yeni starter promosyon/grant'in uygulanip uygulanmadigina bak.
- Olasi belirti:
  - eski oyuncular yeni onboarding hediyesini alamaz

### BUG-040 - Test mode snapshot restore'un yeni state'i ezmesi
- Tur: Edge Case
- Oncelik: P0
- Olasilik: Yuksek
- Supheli neden:
  - `TestPlayerModeManager` save snapshot json restore ediyor.
  - Uzun sure sonra mode degisirse eski stale snapshot yeni state'i tamamen ezebilir.
- Etkilenen dosyalar:
  - `TestPlayerModeManager`
  - `TestPlayerModeProfile`
  - `SaveManager`
- Manuel cogu ltma adimlari:
  1. Default modda ilerleme, coin, tema kazan.
  2. Free/Premium mode'a gec.
  3. Orada baska degisiklikler yap.
  4. Tekrar default'a don.
  5. Beklenen son durum mu yoksa eski stale snapshot mi geliyor kontrol et.
- Olasi belirti:
  - oyuncu state'i geriye sarar
  - yeni unlock veya coin kaybolur

### BUG-041 - Theme unlock ve premium unlock kurallarinin cakismasi
- Tur: Happy Path + business rule
- Oncelik: P1
- Olasilik: Orta
- Supheli neden:
  - Membership aktifken tum future theme'ler unlocked sayiliyor.
  - Soft currency ile kalici unlock ve membership'ten gelen gecici unlock ayrimi net degil.
- Etkilenen dosyalar:
  - `EconomyManager`
  - `StorePresenter`
  - membership profile
- Manuel cogu ltma adimlari:
  1. Premium ile theme aktif et.
  2. Premium'u kapat.
  3. Theme hala aktif mi, ownership ne durumda kontrol et.
  4. Coin ile tekrar unlock davranisini test et.
- Olasi belirti:
  - ulasilamaz active theme kalir
  - UI unlocked gösterirken kullanilamaz

### BUG-042 - NoAds state degisse de UI tam refresh olmamasi
- Tur: Edge Case
- Oncelik: P2
- Olasilik: Orta
- Supheli neden:
  - `SetNoAdsOwned` de `GameEvents.RaiseMembershipChanged(PremiumMembershipActive)` cagriliyor.
  - Event payload no-ads spesifik degil.
- Etkilenen dosyalar:
  - `EconomyManager`
  - `StorePresenter`
  - `MembershipPresenter`
- Manuel cogu ltma adimlari:
  1. NoAds satin alim simule et.
  2. Store/membership ekranlarini ac/kapat.
  3. NoAds copy/status UI'in guncellenip guncellenmedigini kontrol et.
- Olasi belirti:
  - no-ads alindi ama UI degismiyor

### BUG-043 - Store presenter production provider'i bypass etmesi
- Tur: Happy Path store
- Oncelik: P0
- Olasilik: Yuksek
- Supheli neden:
  - `StorePresenter` dogrudan `MockPurchaseService.Instance.Purchase(...)` cagiriyor.
  - `IPurchaseService` abstraction'i runtime'da kullanilmiyor.
- Etkilenen dosyalar:
  - `StorePresenter`
  - `MockPurchaseService`
  - `IPurchaseService`
- Manuel cogu ltma adimlari:
  1. Store butonlariyla hint/energy/membership/no ads satin alimini dene.
  2. Mock service devre disi veya null oldugunda davranisa bak.
  3. Farkli build/prod simulasyonunda akisi izle.
- Olasi belirti:
  - store path production-ready degil
  - provider degisince UI calismaz

### BUG-044 - Energy refill uzun sureli uyku/uygulama arasi hatasi
- Tur: Happy Path time-based
- Oncelik: P1
- Olasilik: Orta
- Supheli neden:
  - `lastRefillUtcTicks` her spend ve grant sirasinda setleniyor.
  - Refill araligi boyunca kazanim birikimi beklenen sekilde davranmayabilir.
- Etkilenen dosyalar:
  - `EnergyManager`
- Manuel cogu ltma adimlari:
  1. Enerjiyi azalt.
  2. Uygulamayi uzun sure kapat.
  3. Geri ac ve refill miktarini olc.
  4. Farkli spend/grant kombinasyonlarinda test et.
- Olasi belirti:
  - beklenenden az enerji dolar
  - zaman farkina gore lineer dolmaz

### BUG-045 - Energy pack satin alimi sessiz kayip yaratmasi
- Tur: Happy Path store
- Oncelik: P1
- Olasilik: Orta
- Supheli neden:
  - `GrantEnergy` MaxEnergy'e clamp ediyor.
  - Ucretli pack, dolu enerji uzerine alininca degerin bir kismi bosa gider.
- Etkilenen dosyalar:
  - `MockPurchaseService`
  - `EnergyManager`
  - store catalog
- Manuel cogu ltma adimlari:
  1. Enerji doluyken energy pack satin al.
  2. Sonraki enerji seviyesini kontrol et.
  3. Kullaniciya uyarı veya stash mantigi var mi bak.
- Olasi belirti:
  - satin alim ekonomik olarak bosa gider

### BUG-046 - Theme soft currency fiyat fallback mismatch
- Tur: Edge Case
- Oncelik: P2
- Olasilik: Orta
- Supheli neden:
  - Theme fiyatlari store catalog + economy profile resolve ile geliyor.
  - Biri yoksa fallback fiyata dusuyor.
- Etkilenen dosyalar:
  - `EconomyManager`
  - `StorePresenter`
  - `LevelEconomyManager`
- Manuel cogu ltma adimlari:
  1. Theme'i profile override ile fiyatla.
  2. Sonra override'i kaldir veya bozuk yap.
  3. UI fiyat ile gercek spend edilen miktari karsilastir.
- Olasi belirti:
  - ekranda gorulen fiyatla gercek dusulen coin farkli olur

### BUG-047 - Promo/grant altyapisinda tekrarli claim riski
- Tur: Future-ready design risk
- Oncelik: P1
- Olasilik: Yuksek
- Supheli neden:
  - Promo/grant queue sistemi henuz kurulmadigi icin ileride safe-point ve claim-once semantigi acik tasarlanmazsa cift odul riski olur.
- Etkilenen dosyalar:
  - gelecekteki promo orchestrator
  - `EconomyManager`
  - save models
- Manuel cogu ltma adimlari:
  1. Bu senaryo production hazirlik sirasinda tekrar ele alinacak.
  2. Ayni hediyenin modal, popup, app restart ve remote config degisimiyle birden fazla kez claim edilip edilmedigi test edilmeli.
- Olasi belirti:
  - cift hint
  - cift coin
  - cift zamanli sinirsiz enerji

### BUG-048 - Membership revoke sonrasi active theme / UX tutarsizligi
- Tur: Edge Case
- Oncelik: P2
- Olasilik: Orta
- Supheli neden:
  - Active theme save'e yaziliyor.
  - Membership kapandiginda aktif kalan premium theme icin fallback akisi net degil.
- Etkilenen dosyalar:
  - `EconomyManager`
  - `ThemeRuntimeController`
  - `StorePresenter`
- Manuel cogu ltma adimlari:
  1. Premium ile premium theme sec.
  2. Premium'u kapat.
  3. Uygulamayi yeniden ac.
  4. Aktif theme, UI ve gercek ownership durumunu izle.
- Olasi belirti:
  - kilitli theme yine aktif kalir
  - store locked ama gameplay theme premium kalir

---

## 7. Unified Editor / Apply / Sync

### BUG-049 - Bir editor'deki apply diger editor'de stale kalmasi
- Tur: Happy Path tooling
- Oncelik: P2
- Olasilik: Orta
- Supheli neden:
  - Sync utility revision tabanli.
  - Belirli pencerelerde `NotifyChanged` eksik kalirsa stale state gorulur.
- Etkilenen dosyalar:
  - `WordSpinAlphaEditorSyncUtility`
  - tum editor pencereleri
- Manuel cogu ltma adimlari:
  1. Ayni veri kaynagini kullanan iki editoru ac.
  2. Birinde save/apply yap.
  3. Digerinde aninda refresh olup olmadigini izle.
- Olasi belirti:
  - ayni alan iki editor'de farkli gorunur

### BUG-050 - Iki editor arasinda last write wins veri kaybi
- Tur: Edge Case
- Oncelik: P2
- Olasilik: Yuksek
- Supheli neden:
  - Runtime config JSON tam dosya olarak yaziliyor.
  - Iki pencere farkli local state tasiyorsa son save oncekini sessizce ezer.
- Etkilenen dosyalar:
  - `WordSpinAlphaRuntimeConfigRepository`
  - config editor pencereleri
- Manuel cogu ltma adimlari:
  1. Ayni config'i kullanan iki editoru ac.
  2. Birinde alan A'yi degistir ve save etme.
  3. Digerinde alan B'yi degistirip save et.
  4. Sonra ilk pencereyi save et.
- Olasi belirti:
  - alan B kaybolur

### BUG-051 - Play modda yapilan scene tuning cikista kalici sanilabilir
- Tur: Happy Path tooling
- Oncelik: P3
- Olasilik: Yuksek
- Supheli neden:
  - Sahne bazli tuning pencereleri play sirasinda canli preview verir ama kalici save mantigi her panelde ayni degil.
- Etkilenen dosyalar:
  - `UiSurfaceTuningWindow`
  - `GameplaySceneTunerEditor`
  - diger scene tuning pencereleri
- Manuel cogu ltma adimlari:
  1. Play modda UI rect/renk/offset degistir.
  2. Play'den cik.
  3. Edit mode sahnesinde degisikligin surup surmedigini kontrol et.
- Olasi belirti:
  - kullanici kalici sanir ama geri doner

### BUG-052 - AssetDatabase.Refresh kaynakli editor race
- Tur: Edge Case tooling
- Oncelik: P3
- Olasilik: Orta
- Supheli neden:
  - Her runtime config save'i `AssetDatabase.Refresh()` yapiyor.
  - Ardisik kayitlarda stale okuma veya editor jitter olabilir.
- Etkilenen dosyalar:
  - `WordSpinAlphaRuntimeConfigRepository`
  - tum config editorleri
- Manuel cogu ltma adimlari:
  1. Ardisik hizli save/apply yap.
  2. Farkli editorlerden pes pese yaz.
  3. Null, stale veya UI donmasi var mi izle.
- Olasi belirti:
  - bazen eski veri geri gelir
  - editor donaklama

### BUG-053 - Unified editor host icinde hosted window state bozulmasi
- Tur: Edge Case tooling
- Oncelik: P2
- Olasilik: Orta
- Supheli neden:
  - `WordSpinAlphaUnifiedEditorWindow` alt pencere `OnGUI`'lerini host ediyor.
  - Bazi pencereler standalone varsayimiyla cizim yapiyor olabilir.
- Etkilenen dosyalar:
  - `WordSpinAlphaUnifiedEditorWindow`
  - tum hosted windows
- Manuel cogu ltma adimlari:
  1. Tek shell editor icinde tum modulleri tek tek ac.
  2. Standalone ile ayni modulun davranisini karsilastir.
  3. Scroll, apply, refresh, modal acma davranislarini dene.
- Olasi belirti:
  - shell icinde farkli, standalone'da farkli davranis

### BUG-054 - Scene sync revision eksikligi nedeniyle stale play apply
- Tur: Happy Path tooling
- Oncelik: P2
- Olasilik: Orta
- Supheli neden:
  - `Scene`, `RuntimeConfig`, `Content`, `Telemetry` revisionlari ayri.
  - Bazi apply zincirleri dogru channel'i notify etmeyebilir.
- Etkilenen dosyalar:
  - `WordSpinAlphaEditorSyncUtility`
  - `WordSpinAlphaEditorRuntimeRefreshUtility`
- Manuel cogu ltma adimlari:
  1. Bir panelden scene tuning, diger panelden content/runtime config degistir.
  2. Her biri sonrasi diger bagli panelleri kontrol et.
- Olasi belirti:
  - bazilari guncel, bazilari stale

### BUG-055 - Validation penceresinin eksik alanlari temiz raporlamasi
- Tur: Edge Case tooling
- Oncelik: P2
- Olasilik: Yuksek
- Supheli neden:
  - Validation audit kapsamli ama tum runtime scene referanslarini taramayabilir.
- Etkilenen dosyalar:
  - `ValidationAuditWindow`
- Manuel cogu ltma adimlari:
  1. Bilincli olarak eksik path, duplicate id, missing locale, invalid theme asset olustur.
  2. Validation'i calistir.
  3. Hepsini yakaliyor mu kontrol et.
- Olasi belirti:
  - temiz rapor verir ama runtime kiriktir

---

## 8. UI / Mobile / Theme / Presentation

### BUG-056 - Runtime auto-create label'lar farkli cihaz/layoutta cakismasi
- Tur: Happy Path mobile
- Oncelik: P2
- Olasilik: Yuksek
- Supheli neden:
  - `StorePresenter.EnsureRuntimeLabels()` runtime'da label olusturuyor.
  - Safe area ve mevcut layout ile cakisma riski var.
- Etkilenen dosyalar:
  - `StorePresenter`
  - `MobileRuntimeController`
- Manuel cogu ltma adimlari:
  1. Farkli aspect ratio ve safe area presetleri kullan.
  2. Store ekranini ac.
  3. Runtime olusan label'larin cakisip cakismadigina bak.
- Olasi belirti:
  - fiyat etiketi, status label ust uste biner

### BUG-057 - Fail modal runtime create ile mevcut UI'nin ustune binmesi
- Tur: Happy Path mobile/UI
- Oncelik: P2
- Olasilik: Orta
- Supheli neden:
  - `FailModalPresenter.EnsureRuntimeUi()` canvas altina runtime panel yaratabiliyor.
  - Var olan tasarimla anchor/offset cakismasi olabilir.
- Etkilenen dosyalar:
  - `FailModalPresenter`
  - `UiSurfaceTuningWindow`
- Manuel cogu ltma adimlari:
  1. Farkli cihaz oranlarinda fail modal ac.
  2. UI tuning penceresinden layoutla oynadiktan sonra tekrar dene.
- Olasi belirti:
  - butonlar gorunmez
  - metin tasar

### BUG-058 - Theme asset path eksikliginde karisik eski/yeni gorunum
- Tur: Edge Case
- Oncelik: P2
- Olasilik: Yuksek
- Supheli neden:
  - `ThemeRuntimeController.LoadThemeAssets()` eksik sprite/clip icin sessizce null donuyor.
  - Onceki theme'den kalan assetler korunabilir.
- Etkilenen dosyalar:
  - `ThemeRuntimeController`
  - theme catalog
- Manuel cogu ltma adimlari:
  1. Bir theme'de eksik sprite veya clip path kullan.
  2. O theme'e gecis yap.
  3. Onceki theme'den bir assetin aynen kalip kalmadigini incele.
- Olasi belirti:
  - yarisi yeni theme, yarisi eski theme olur

### BUG-059 - Audio source seciminde yanlis source kullanimi
- Tur: Edge Case
- Oncelik: P2
- Olasilik: Orta
- Supheli neden:
  - `ThemeRuntimeController.EnsureAudioSources()` mevcut source'lari loop/non-loop'a bakarak seciyor.
  - Sahneye yeni audio source eklendiginde yanlis source rol degistirebilir.
- Etkilenen dosyalar:
  - `ThemeRuntimeController`
- Manuel cogu ltma adimlari:
  1. Sahneye ekstra audio source ekle.
  2. Play modda theme ve hit seslerini dinle.
  3. BGM/SFX/hit source ayrimi bozuluyor mu kontrol et.
- Olasi belirti:
  - BGM kesilir
  - hit sesleri yanlis kaynaktan oynar

### BUG-060 - Extreme pulse/glow tuning performans ve goruntu bozulmasi
- Tur: Edge Case tuning
- Oncelik: P2
- Olasilik: Yuksek
- Supheli neden:
  - `ThemeRuntimeController` ve `PulseSprite` tabanli amplitude/hiz alanlari cok serbest.
- Etkilenen dosyalar:
  - `ThemeRuntimeController`
  - `AmbientPulseTuningWindow`
  - `PulseSprite`
- Manuel cogu ltma adimlari:
  1. Pulse speed ve amplitude'leri asiri yuksek yap.
  2. Play modda FPS ve gorsel okunurlugu izle.
  3. Safe area ve HUD ile birlikte kontrol et.
- Olasi belirti:
  - glow ekranı yutar
  - alpha flicker
  - frame pacing bozulur

### BUG-061 - UI renk ayarlari state-driven renklerle carpisir
- Tur: Happy Path tuning
- Oncelik: P2
- Olasilik: Orta
- Supheli neden:
  - Bazi label, panel veya feedback renkleri runtime state'e gore dinamik setleniyor.
  - Editor ayari ile runtime state birbirini ezer.
- Etkilenen dosyalar:
  - `UiSurfaceTuningWindow`
  - `GameplayHudPresenter`
  - `FailModalPresenter`
  - `ResultPresenter`
- Manuel cogu ltma adimlari:
  1. UI editorunden renkleri degistir.
  2. Ardindan fail, result, active target, score update state'lerini tetikle.
  3. Renkler kalici mi yoksa state geri mi yaziyor kontrol et.
- Olasi belirti:
  - ayar yapilir ama sonra eski renk geri gelir

### BUG-062 - Safe area ve klavye dock parity bozulmasi
- Tur: Happy Path mobile
- Oncelik: P1
- Olasilik: Yuksek
- Supheli neden:
  - `KeyboardLayoutTuningWindow`, `GameplaySceneTuner`, `MobileRuntimeTuningWindow` ayni yuzeyde birden fazla katmandan etki ediyor.
- Etkilenen dosyalar:
  - `KeyboardLayoutTuningProfile`
  - `GameplaySceneTuner`
  - `MobileRuntimeController`
- Manuel cogu ltma adimlari:
  1. Safe area margin ve keyboard frame ayarlarini degistir.
  2. Cihaz/oran degistir.
  3. HUD, bottom bar ve klavye birlikte hizali mi kontrol et.
- Olasi belirti:
  - klavye safe area disina tasar
  - bottom buttons dock ile cakisir

---

## 9. Telemetry / Remote Content / Hotfix

### BUG-063 - Telemetry summary'nin ayni level altinda farkli varyantlari ezmesi
- Tur: Scale risk
- Oncelik: P2
- Olasilik: Yuksek
- Supheli neden:
  - `_levelSummaries` key'i yalnizca `levelId`.
  - locale, theme, difficulty varyasyonu ayni ozet altinda karisir.
- Etkilenen dosyalar:
  - `TelemetryService`
- Manuel cogu ltma adimlari:
  1. Ayni leveli farkli dil/theme/difficulty ile oyna.
  2. Telemetry snapshot'i incele.
  3. Ayrik ozet mi tek ozet mi bak.
- Olasi belirti:
  - analiz anlamsizlasir

### BUG-064 - Queue trim sessiz veri kaybi
- Tur: Edge Case
- Oncelik: P2
- Olasilik: Yuksek
- Supheli neden:
  - `TrimQueueToPolicyLimit()` oldest eventleri sessizce siliyor.
- Etkilenen dosyalar:
  - `TelemetryService`
  - `TelemetryPolicyProfile`
- Manuel cogu ltma adimlari:
  1. Max queue degerini kucuk tut.
  2. Cok sayida event uretecek bir oyun seansi kos.
  3. Queue ve snapshot'i incele.
- Olasi belirti:
  - en eski veri sessiz kaybolur
  - kullanici bunun farkina varmaz

### BUG-065 - Pending telemetry count save ile queue dosyasi uyusmaz
- Tur: Edge Case
- Oncelik: P2
- Olasilik: Orta
- Supheli neden:
  - queue trim, save pending count ve queue write farkli yerlerden yurutuluyor.
  - Disk yazimi basarisiz veya yarim kalirsa sayi uyusmayabilir.
- Etkilenen dosyalar:
  - `TelemetryService`
  - `SaveManager`
- Manuel cogu ltma adimlari:
  1. Telemetry acikken cok event uret.
  2. Save veya queue write aninda uygulamayi kes.
  3. Yeniden ac ve pending count ile gercek queue dosyasini karsilastir.
- Olasi belirti:
  - debug panel sayisi ile dosya icerigi farkli olur

### BUG-066 - Remote manifest acik ama kataloglar kismi geldiginde hibrit state
- Tur: Edge Case
- Oncelik: P1
- Olasilik: Yuksek
- Supheli neden:
  - remote enabled oldugunda her katalog icin remote/local merge veya preferRemote farkli calisiyor.
  - Kismi publish sonucunda hibrit state ortaya cikabilir.
- Etkilenen dosyalar:
  - `ContentService`
  - `RemoteContentProvider`
  - `RemoteContentHotfixWindow`
- Manuel cogu ltma adimlari:
  1. Sadece levels/questions remote publish et.
  2. Theme/store/membership local kalsin.
  3. Runtime'da level, store ve theme ekranlarini birlikte kontrol et.
- Olasi belirti:
  - yeni level ama eski theme/store
  - bagimli id'ler kopar

### BUG-067 - Tek bir dil remote override, diger diller local kalinca parity bozulmasi
- Tur: Edge Case localization
- Oncelik: P1
- Olasilik: Yuksek
- Supheli neden:
  - `LoadLocalizedRemoteJson` bir dil icin remote varsa onu aliyor, diger diller root/local'a dusebilir.
- Etkilenen dosyalar:
  - `RemoteContentProvider`
  - localized content files
- Manuel cogu ltma adimlari:
  1. Yalnizca bir dilin localized question/info card dosyasini publish et.
  2. Tum dillerde ayni leveli ac.
  3. Icerik ve progression parity'yi karsilastir.
- Olasi belirti:
  - bir dilde daha yeni build, digerinde eski build gorulur

### BUG-068 - Remote refresh aktif session ortasinda yapildiginda restore uyumsuzlugu
- Tur: Edge Case live config
- Oncelik: P1
- Olasilik: Yuksek
- Supheli neden:
  - Active session eski content'e dayanirken `RefreshRemoteOverrides()` yeni catalogu cache'e alabilir.
- Etkilenen dosyalar:
  - `ContentService`
  - `GameManager`
  - `LevelFlowController`
- Manuel cogu ltma adimlari:
  1. Aktif session ortasinda kal.
  2. Remote hotfix publish et ve refresh uygula.
  3. Restore, info/result/fail, target slot ve answer parity'yi dene.
- Olasi belirti:
  - current session yeni veriye uymadigi icin bozulur

### BUG-069 - Hotfix publish eksik dosyayla "basarili" sanilmasi
- Tur: Edge Case tooling
- Oncelik: P2
- Olasilik: Orta
- Supheli neden:
  - Remote publish araci secili kataloglari kopyaliyor.
  - Bagimli locale veya config dosyasi unutulursa yarim paket olusabilir.
- Etkilenen dosyalar:
  - `RemoteContentHotfixWindow`
  - remote content directory
- Manuel cogu ltma adimlari:
  1. Sadece ana kataloglari publish et, locale/config'i eksik birak.
  2. Refresh uygula.
  3. Runtime parity'yi kontrol et.
- Olasi belirti:
  - publish basarili gorunur ama oyun kismi bozulur

---

## 10. Release / Performance / Scale Riskleri

### BUG-070 - Save ve telemetry yazimlari gameplay sirasinda IO spike olusturmasi
- Tur: Performance
- Oncelik: P1
- Olasilik: Yuksek
- Supheli neden:
  - `SaveManager` ve `TelemetryService` ikisi de dosya yaziyor.
  - Yogun gameplay'de birden fazla write trigger olabilir.
- Etkilenen dosyalar:
  - `SaveManager`
  - `TelemetryService`
- Manuel cogu ltma adimlari:
  1. Telemetry acik olsun.
  2. Hizli hit/reveal/fail/retry/continue akislari yap.
  3. Cihazda frame hickup var mi bak.
- Olasi belirti:
  - micro-freeze
  - pause sirasinda takilma

### BUG-071 - FindObjectOfType agir zincirleri buyume ile yavaslamasi
- Tur: Scale risk
- Oncelik: P2
- Olasilik: Yuksek
- Supheli neden:
  - Bir cok sinif hala `FindObjectOfType` ve `GameObject.Find` kullaniyor.
  - Sahne/objeler cogaldikca init ve refresh maliyeti artar.
- Etkilenen dosyalar:
  - `GameManager`
  - `ThemeRuntimeController`
  - `GameplaySceneTuner`
  - editor runtime refresh utility
- Manuel cogu ltma adimlari:
  1. Sahneye fazla sayida obje veya UI panel eklenmis varyantla test et.
  2. Play baslangicini ve editor apply suresini olc.
- Olasi belirti:
  - scene start gecikir
  - apply sureleri uzar

### BUG-072 - Rotator plaque rebuild'in sik apply ile hitch yaratmasi
- Tur: Performance + tooling
- Oncelik: P2
- Olasilik: Yuksek
- Supheli neden:
  - `RebuildLayout()` her seferinde mevcut gorselleri destroy + yeniden create ediyor.
- Etkilenen dosyalar:
  - `RotatorPlaquePresenter`
  - `WordSpinAlphaContentEditorWindow`
  - `SlotHitTuningWindow`
- Manuel cogu ltma adimlari:
  1. Play modda plaque/shape tuning ile art arda apply yap.
  2. FPS dususune ve GC spike'ina bak.
- Olasi belirti:
  - ani takilmalar

### BUG-073 - Theme runtime coroutine / flash birikimi
- Tur: Edge Case performance
- Oncelik: P2
- Olasilik: Orta
- Supheli neden:
  - Theme ve impact katmaninda flash/pulse/coroutine akislari var.
  - Yogun hit durumunda birikim olabilir.
- Etkilenen dosyalar:
  - `ThemeRuntimeController`
  - `ImpactFeedbackController`
- Manuel cogu ltma adimlari:
  1. Yuksek frekansli hit serisi olustur.
  2. FPS, audio pitch, visual flash stabil mi izle.
- Olasi belirti:
  - efektler gecikmeli kalir
  - birkac saniye sonra dengesiz goruntu olur

### BUG-074 - Release build'e debug/test presenter sizmasi
- Tur: Production readiness
- Oncelik: P0
- Olasilik: Yuksek
- Supheli neden:
  - `SceneBootstrap` henuz debug/test/provider ayrimini release channel ile zorunlu kilmiyor.
- Etkilenen dosyalar:
  - `SceneBootstrap`
  - `MockPurchaseService`
  - `PreviewStorePricingProvider`
  - `DebugRewardedAdPresenter`
  - `TestPlayerModeManager`
- Manuel cogu ltma adimlari:
  1. Release'e yakin build profile simulasyonu yap.
  2. Store, continue ad, test mode, fiyat preview yuzeylerini kontrol et.
- Olasi belirti:
  - oyuncu fake rewarded gorur
  - preview fiyat yayinlanir
  - sandbox state aktif kalir

### BUG-075 - Resources yukunun buyume ile build ve memory sorunu
- Tur: Scale risk
- Oncelik: P2
- Olasilik: Yuksek
- Supheli neden:
  - Themes, configs, shape assets, editorle ilgili bazi kaynaklar `Resources` altinda.
  - Theme paketleri buyudukce memory/build boyutu artabilir.
- Etkilenen dosyalar:
  - `Resources/*`
  - `ThemeRuntimeController`
  - runtime config repository
- Manuel cogu ltma adimlari:
  1. Theme ve content sayisini arttirilmis test datasiyla build al.
  2. Baslangic memory ve load time olc.
- Olasi belirti:
  - yuksek RAM
  - build size sismesi

### BUG-076 - Editor reference assetlerinin build'e sizmasi
- Tur: Production readiness
- Oncelik: P2
- Olasilik: Orta
- Supheli neden:
  - Shape referans PNG, editor draft veya remote template gibi dosyalar build'e dahil olabilir.
- Etkilenen dosyalar:
  - Art/ShapeRefs
  - `Resources/Configs`
  - editor authoring varliklari
- Manuel cogu ltma adimlari:
  1. Build output'u incele.
  2. Editor-only assetlerin runtime'da erisilebilir olup olmadigina bak.
- Olasi belirti:
  - gereksiz APK sismesi

### BUG-077 - Production provider gecisinde store ve editor parity kirilmasi
- Tur: Production transition
- Oncelik: P0
- Olasilik: Yuksek
- Supheli neden:
  - Bugunku editor ve runtime `MockPurchaseService` ile `PreviewStorePricingProvider` ustune kurulu.
  - Gercek provider gecisi yapilirken UI ve economy parity bozulabilir.
- Etkilenen dosyalar:
  - `StorePresenter`
  - `StorePricingManager`
  - `IPurchaseService`
  - future billing provider
- Manuel cogu ltma adimlari:
  1. Bu senaryo production hazirlik asamasinda ele alinacak.
  2. Gercek billing provider eklenince UI, fulfilment, restore, quote ve entitlement ayni checklist ile tekrar test edilmeli.
- Olasi belirti:
  - editor dogru gorur, market runtime farkli davranir

## Manuel Cogu ltma Icin Tavsiye Edilen Uygulama Sirasi

Asagidaki sira operasyonel olarak en mantikli sira olarak onerilir:

1. Save / Session / Restore
2. Question / Fail / Info / Result
3. Gameplay / Hit / Input / Rotator
4. Content / Localization / Shape
5. Economy / Store / Membership / Energy
6. UI / Mobile / Theme
7. Unified Editor / Apply / Sync
8. Telemetry / Remote / Hotfix
9. Production readiness ve performance riskleri

## Ilk Manuel Testte Once Dogrulanmasi Gereken 20 Senaryo

Su senaryolar ilk turda en yuksek degerli adaylardir:

- BUG-007
- BUG-009
- BUG-010
- BUG-011
- BUG-013
- BUG-014
- BUG-018
- BUG-019
- BUG-022
- BUG-024
- BUG-025
- BUG-027
- BUG-029
- BUG-031
- BUG-034
- BUG-037
- BUG-040
- BUG-043
- BUG-068
- BUG-074

## Hedefli Otomasyon Adaylari

Manuel olarak cogu ltulmesi zor ama otomasyon icin en uygun alanlar:

- save/load/session snapshot roundtrip
- pending fail/info/result restore roundtrip
- content merge order ve duplicate id davranisi
- localized remote override fallback
- shape point parity ve slot count degisimi
- energy refill ve spend mantigi
- telemetry queue trim ve snapshot generation
- release-safe provider secimi

## Son Not

Bu raporun her maddesi once manuel olarak denenmelidir.

Bu dokuman uzerinde bir senaryo:

- cogu ltulduyse `DOGRULANDI`
- cogu ltulemediyse `DOGRULANAMADI`
- sonradan alakasiz ciktiginda `RED`

olarak isaretlenmelidir.

Bu ilk turda hotfix degil, **sistematik bug avciligi icin senaryo matrisi** olarak kullanilmalidir.

---

## Ek Triage Rehberi - 06.04.2026

Bu bolum, ana rapordaki maddeleri manuel bug sweep acisindan daha kullanilabilir hale getirmek icin eklendi.

Amac:

- hangi maddeleri **hemen** manuel olarak deneyecegimizi netlestirmek
- hangi maddeleri su anki aktif mekaniklere gore **simdilik park edecegimizi** ayirmak
- hangi maddeleri **alpha sonrasi architecture / release hazirlik kovasina** atacagimizi ayirmak

Bu bolum yeni bug tanimi acmaz. Asagidaki tum maddeler bu dosyada zaten var olan bug ID'lerine referans verir.

Ikinci goz raporlarindan gelen ama bu dosyada yeni ID olarak acilmayan bazi yorumlar su maddelere baglanarak dusunulmelidir:

- store donusu + dil degisikligi + session kaybi yorumu:
  - `BUG-010`
  - `BUG-012`
  - `BUG-029`
- menu donusunde snapshot eksikligi yorumu:
  - `BUG-009`
  - `BUG-015`
- continue sonrasi ikinci fail davranisi:
  - `BUG-013`
  - `BUG-026`
- atomik save / bozuk save yorumu:
  - `BUG-007`
  - `BUG-008`

## 1. Hemen Manuel Cogu ltulecek Kesin Calisma Listesi

Bu bolumdeki maddeler:

- su anki aktif oyunda gercekten gorulebilir
- mevcut mekaniklerle uyumlu
- bug sweep'in ana omurgasini olusturur

Asagidaki sira ile gitmek en rahati olur.

### 1.1 Save / Session / Restore cekirdegi

#### `BUG-007` - Save throttle araliginda save kaybi
- Neden simdi:
  - en kritik save kaybi adayi
  - dogrudan oyuncu ilerlemesini etkiler
- En kolay deneme:
  1. Oyuna gir.
  2. Bir harf ac veya can/coin degistiren bir islem yap.
  3. Hemen uygulamayi kapat.
  4. Tekrar ac.
  5. Az once yaptigin sey duruyor mu bak.
- Neye bak:
  - son acilan harf geri gitmis mi
  - coin veya tema durumu geri sarmis mi

#### `BUG-009` - Pause ve quit snapshot sirasinda yeni state'in ezilmesi
- Neden simdi:
  - mobilde cok olasi
  - save kaybi ile karisabilir
- En kolay deneme:
  1. Soruda bir harf ac.
  2. Tam o anda uygulamayi arka plana al.
  3. Sonra tamamen kapat.
  4. Tekrar ac.
  5. Son harf kayitli mi kontrol et.
- Neye bak:
  - bir onceki duruma donuyor mu

#### `BUG-010` - Content degistikten sonra session restore index uyumsuzlugu
- Neden simdi:
  - editor ve canli uygulama kullandigimiz icin cok kritik
- En kolay deneme:
  1. Bir levelin ortasinda kal.
  2. Icerik editorunden ayni levelin soru/cevap tarafinda degisiklik yap.
  3. Kaydet ve oyuna geri don.
  4. Session dogru devam ediyor mu bak.
- Neye bak:
  - yanlis soru aciliyor mu
  - reveal sayisi bozuluyor mu

#### `BUG-011` - Shape slot sayisi degisince eski snapshot parity bozulmasi
- Neden simdi:
  - shape editoru yeni kuruldu
  - aktif test edecegin alanlardan biri
- En kolay deneme:
  1. Bir shape kullanan levelin ortasinda kal.
  2. Shape kutu sayisini veya layout'unu degistir.
  3. Geri don ve restore et.
  4. Saplanan pinler ve kutular dogru yerde mi bak.
- Neye bak:
  - pinler havada mi
  - target slot kaymis mi

#### `BUG-013` - Pending fail resolution stale kalmasi
- Neden simdi:
  - fail / continue / retry akisinin temel bug'i olabilir
- En kolay deneme:
  1. Fail ol.
  2. Fail ekranindayken farkli cikis yollarini dene:
     - menu
     - store
     - geri don
  3. Sonra tekrar oyuna don.
  4. Oyun kilitli mi, fail ekrani dogru mu bak.
- Neye bak:
  - ekranda modal yok ama oyun oynamiyor mu
  - fail modal gereksiz tekrar geliyor mu

#### `BUG-014` - Pending info card ve pending result flag cakismasi
- Neden simdi:
  - info card ve result akisi alpha demoda gorecegin ana akislardan
- En kolay deneme:
  1. Info card olan bir soruyu bitir.
  2. Kart acikken uygulamayi kapat/ac.
  3. Sonra devam et.
  4. Siradaki sey dogru mu bak:
     - yeni soru mu
     - result mi
- Neye bak:
  - info card iki kez aciliyor mu
  - result kayboluyor mu

### 1.2 Fail / Continue / Result / Store akis kontrolu

#### `BUG-024` - Info card acikken dil degisince stale icerik
- Neden simdi:
  - 4 dil alpha hedefi var
- En kolay deneme:
  1. Info card ac.
  2. Kart acikken dili degistir.
  3. Kart basligi ve yazi yeni dile geciyor mu bak.
- Neye bak:
  - sadece buton mu degisiyor
  - yoksa tum kart mi guncelleniyor

#### `BUG-025` - Fake rewarded callback gec donerse yanlis state'e uygulanmasi
- Neden simdi:
  - su an gercek reklam yok, test reklami var
  - bu akisi simdi yakalamak lazim
- En kolay deneme:
  1. Fail ol.
  2. Devam Et ile fake reklam sayacini baslat.
  3. Sayaç bitmeden store'a veya menuye gitmeye calis.
  4. Sonra callback ne yapiyor bak.
- Neye bak:
  - yanlis yerde can veriyor mu
  - fail ekrani kapaniyor mu

#### `BUG-026` - Retry enerji kurali ile premium bypass parity bozulmasi
- Neden simdi:
  - fail ekraninin oyuncuya gosterdigi ile gercek davranis ayni mi bunu netlestirir
- En kolay deneme:
  1. Enerji varken fail ol, Retry dene.
  2. Enerji bitikken fail ol, Retry dene.
  3. Premium test modunda ayni seyi dene.
- Neye bak:
  - buton aktif ama calismiyor mu
  - buton pasifken aslinda calisiyor mu

#### `BUG-027` - Pending result restore cift odul riski
- Neden simdi:
  - cift coin / cift odul kritik
- En kolay deneme:
  1. Bir leveli bitir.
  2. Result ekrani gelirken uygulamayi kapat.
  3. Tekrar ac.
  4. Odul bir kez mi iki kez mi sayiliyor bak.
- Neye bak:
  - coin iki kez gelmis mi
  - level progress iki kez ilerlemis mi

#### `BUG-029` - Store donusu sonrasi pending fail veya result UI'nin kaybolmasi
- Neden simdi:
  - store gecisi aktif oyun akisinda kullaniliyor
- En kolay deneme:
  1. Fail ekrani acikken store'a git.
  2. Result veya info card pending haldeyken store'a git.
  3. Geri don.
  4. Gerekli ekranlar geri geliyor mu bak.
- Neye bak:
  - oyun kilitli kalip UI gelmiyor mu
  - pending ekran kayboluyor mu

### 1.3 Shape / Content / Localization parity

#### `BUG-031` - Local/remote merge sirasinda katalog order drift
- Neden simdi:
  - content ve hotfix mimarisi aktif planin parcasi
- En kolay deneme:
  1. Ayni listeyi local ve remote tarafta farkli sirayla tut.
  2. Refresh yap.
  3. Level veya shape listesi sirasi degisiyor mu bak.
- Neye bak:
  - beklenmedik siralama

#### `BUG-034` - Shape editor preview ile gameplay parity farki
- Neden simdi:
  - su an en cok kullandigin editorlerden biri bu
- En kolay deneme:
  1. Shape'i editorde guzel gorunecek sekilde ayarla.
  2. Kaydet ve oyunda ayni leveli ac.
  3. Editordeki ile oyundaki sekli karsilastir.
- Neye bak:
  - oyunda cirkinlesme
  - kutularin birbirine girmesi
  - acilarin farkli olmasi

#### `BUG-037` - Editor local force durumu remote testini maskelemesi
- Neden simdi:
  - editor ve remote hotfix ayni anda planlandigi icin kritik
- En kolay deneme:
  1. Editorle local icerigi degistir.
  2. Sonra remote publish yap.
  3. Refresh uygula.
  4. Gercekte local mi remote mu aktif bak.
- Neye bak:
  - remote guncel sanip aslinda locali goruyor olma

### 1.4 Economy / Store / Sandbox state

#### `BUG-040` - Test mode snapshot restore'un yeni state'i ezmesi
- Neden simdi:
  - test sandbox'i aktif kullaniyoruz
- En kolay deneme:
  1. Default modda coin/tema/ilerleme kazan.
  2. Free veya Premium test moduna gec.
  3. Orada da degisiklik yap.
  4. Tekrar default'a don.
  5. Son durum dogru mu bak.
- Neye bak:
  - yeni kazanilan seyler siliniyor mu

#### `BUG-043` - Store presenter production provider'i bypass etmesi
- Neden simdi:
  - bu daha cok production gecis riski ama store akisinda bug da uretebilir
- En kolay deneme:
  1. Store'dan hint, enerji, premium, no ads al.
  2. Her birinde odul dogru geliyor mu bak.
  3. Sonra store ekranini kapat/ac.
- Neye bak:
  - UI dogru guncelleniyor mu
  - odul veriliyor ama UI eski mi kaliyor

### 1.5 Remote content ve aktif session cakismasi

#### `BUG-068` - Remote refresh aktif session ortasinda yapildiginda restore uyumsuzlugu
- Neden simdi:
  - ileride canli hotfix icin en kritik risklerden biri
  - simdiden temel davranisi gormek lazim
- En kolay deneme:
  1. Bir levelin ortasinda kal.
  2. Remote content refresh yap.
  3. Oyuna geri don.
  4. Session dogru devam ediyor mu bak.
- Neye bak:
  - target harf kayiyor mu
  - soru veya shape bozuluyor mu

## 2. Simdilik Park Edilecek Teorik Maddeler

Bu bolumdeki maddeler:

- tamamen yanlis degil
- ama su anki aktif oyun davranisina gore ilk bug sweep turunda oncelikli degil
- kafa karisikligi yaratmamasi icin simdilik park edilmeli

### `BUG-018` - InputBuffer beklenen harfi stale tutmasi
- Neden park:
  - `InputBuffer` icinde `TryAdd` ve `TryPop` su an aktif akista kullanilmiyor.
  - Mevcut oyunda:
    - [KeyboardPresenter] tek bir tusu tuketiyor
    - [PinLauncher] loaded pin varken yeni harf almiyor
  - Yani Sonnet'in buffer kuyrugu odakli korkulari su anki aktif mekanige tam oturmuyor.
- Ne zaman geri acilacak:
  - fiziksel klavye davranisi genisletilirse
  - buffer mantigi aktif kullanilmaya baslarsa

### `BUG-020` - Extreme hit tuning ile collider mantiginin terslenmesi
- Neden park:
  - bu daha cok "editori asiri uca cekersek" olur
  - ilk manuel sweep'te daha gercekci senaryolar once test edilmeli
- Ne zaman geri acilacak:
  - hit feel ve final tuning asamasinda

### `BUG-047` - Promo/grant altyapisinda tekrarli claim riski
- Neden park:
  - promo sistemi henuz feature olarak aktif degil
  - plan/risk maddesi olarak dogru ama bug sweep maddesi degil
- Ne zaman geri acilacak:
  - promo / hediye / kampanya orkestrasyonu kurulunca

### `BUG-051` - Play modda yapilan scene tuning cikista kalici sanilabilir
- Neden park:
  - bu daha cok kullanim beklentisi / editor davranisi notu
  - runtime bug onceliginde degil
- Ne zaman geri acilacak:
  - unified editor son polish asamasinda

### `BUG-063` - Telemetry summary'nin ayni level altinda farkli varyantlari ezmesi
- Neden park:
  - telemetry su an bug sweep'in degil, daha cok ileride analiz kalitesinin konusu
- Ne zaman geri acilacak:
  - telemetry/liveops fazinda

### `BUG-073` - Theme runtime coroutine / flash birikimi
- Neden park:
  - gercek yogun VFX/audio/hit feel pass'i henuz tamamlanmadi
  - simdi test etsek de final tasarim gelmeden gercek resmi vermez
- Ne zaman geri acilacak:
  - ses/animasyon/juicy pass sonrasi

## 3. Alpha Sonrasi Architecture / Release Kovasina Atilacak Maddeler

Bu bolumdeki maddeler:

- mevcut projede onemli
- ama simdiki manuel bug sweep'ten cok
- production gecisi, market hazirligi ve altyapi sertlestirme konusu

Bunlari simdi denemek yerine notlu sekilde ayri kovada tutmak daha dogru olur.

### `BUG-043` - Store presenter production provider'i bypass etmesi
- Neden release kovasi:
  - su an store test akisi `MockPurchaseService` ile bilerek calisiyor
  - bu production gecis maddesi
- Alpha sonrasi ne yapilacak:
  - gercek purchase provider eklenince yeniden ele alinacak

### `BUG-046` - Theme soft currency fiyat fallback mismatch
- Neden release kovasi:
  - pricing sistemi final storefront'a gecmeden once tam anlamli test vermez
- Alpha sonrasi ne yapilacak:
  - gercek pricing provider ile tekrar dogrulanacak

### `BUG-064` - Queue trim sessiz veri kaybi
- Neden release kovasi:
  - telemetry veri kalitesi sorunu
  - oyuncu akisindan cok liveops/veri sorunu
- Alpha sonrasi ne yapilacak:
  - telemetry politikasi finalize edilince test edilecek

### `BUG-065` - Pending telemetry count save ile queue dosyasi uyusmaz
- Neden release kovasi:
  - telemetry consistency sorunu
- Alpha sonrasi ne yapilacak:
  - snapshot ve queue akisi netlestiginde tekrar bakilacak

### `BUG-066` - Remote manifest acik ama kataloglar kismi geldiginde hibrit state
- Neden release kovasi:
  - remote hotfix aktif canli kullanimda asıl kritik olacak
- Alpha sonrasi ne yapilacak:
  - gercek remote publish kanaliyle test edilecek

### `BUG-067` - Tek bir dil remote override, diger diller local kalinca parity bozulmasi
- Neden release kovasi:
  - bu tam live config / liveops kullanim senaryosu
- Alpha sonrasi ne yapilacak:
  - cloud uzerinden partial locale publish geldiginde tekrar test edilecek

### `BUG-074` - Release build'e debug/test presenter sizmasi
- Neden release kovasi:
  - bu markete cikis oncesi son kapidan gecis kontrolu
- Alpha sonrasi ne yapilacak:
  - release-safe build profile olusturuldugunda kesin test edilecek

### `BUG-075` - Resources yukunun buyume ile build ve memory sorunu
- Neden release kovasi:
  - tema ve icerik buyudukce anlamli hale gelecek
- Alpha sonrasi ne yapilacak:
  - alpha content lock sonrasi olculecek

### `BUG-076` - Editor reference assetlerinin build'e sizmasi
- Neden release kovasi:
  - build boyutu ve temizlik konusu
- Alpha sonrasi ne yapilacak:
  - final build alinmadan once temizlenecek / ayrilacak

### `BUG-077` - Production provider gecisinde store ve editor parity kirilmasi
- Neden release kovasi:
  - gercek billing/reklam/pricing provider gelmeden bug sweep gibi davranmaz
- Alpha sonrasi ne yapilacak:
  - production provider entegrasyonu sonrasi ayri smoke test paketine alinacak

## 4. Bu Triage Bolumunu Nasil Kullanacaksin

Pratik kullanim:

1. Once sadece **Hemen Manuel Cogu ltulecek Kesin Calisma Listesi** altindaki maddeleri dene.
2. Her madde icin sonucu sunlardan biri olarak not al:
   - `DOGRULANDI`
   - `DOGRULANAMADI`
   - `KARARSIZ`
3. `Simdilik Park Edilecek Teorik Maddeler` kismina ilk turda zaman harcama.
4. `Alpha Sonrasi Architecture / Release Kovasina Atilacak Maddeler` kismini production hazirlikta tekrar ac.

## 5. Ilk Tur Icin Sade Onerilen Test Sirasi

Kafayi karistirmamak icin ilk turda sadece su sirayla git:

1. `BUG-007`
2. `BUG-009`
3. `BUG-010`
4. `BUG-011`
5. `BUG-013`
6. `BUG-014`
7. `BUG-024`
8. `BUG-025`
9. `BUG-026`
10. `BUG-027`
11. `BUG-029`
12. `BUG-034`
13. `BUG-037`
14. `BUG-040`
15. `BUG-068`

Bu 15 madde ilk tur icin yeterince yogun ve gercek bug yakalama degeri en yuksek listedir.
