# WordSpin Alpha - Alpha Demo Oncesi Guncellenmis Ana Plan

## Ozet

Bu surum, repodaki onceki `WORDSPIN_MASTER_PLAN` ile `01.04.2026-guncellenmis PLAN.md` iceriginin caprazlanmis ve mevcut kod tabaniyla dogrulanmis halidir.

Temel prensip:

- Daha once `YAPILDI` olarak isaretlenen maddeler korunmustur.
- Yeni tamamlanan sistemler, aktif riskler ve nasil kurulduklari plana eklenmistir.
- Kodda gercekten olmayan hicbir madde plana "tamamlandi" diye yazilmamistir.

Guncel kararlar:

- `Telemetry / AI / cloud / developer panel` alpha demo sonrasina ertelendi.
- Referans ekran `9:16`.
- Ilk gercek dogrulama ortami bilgisayar preview degil, aktif Android cihaz.
- Proje `mobil-first`, `safe-area aware`, `cok dilli` ve `data-driven` ilerliyor.
- Ekonomi tarafinda `Default` gercek gelistirme/runtime modudur; `FreePlayer` ve `PremiumPlayer` yalnizca test sandbox modlaridir.
- Birlesik editor, telemetry, hotfix ve web panel gecisi icin detayli risk ve sira plani ayri olarak `UNIFIED_EDITOR_TO_LIVEOPS_PLAN.md` icinde tutulur. Alpha demo suresince buyume riskleri bu yardimci plan dikkate alinarak yonetilecektir.
- Market cikisina production-safe gecis, promo/gift omurgasi, billing/rewarded/pricing production provider gecisleri ve final release testi icin detayli plan ayri olarak `MARKET_RELEASE_READINESS_PLAN.md` icinde tutulur.

## Tarih Bazli Ozet ve 08.04.2026 Konumu

Bu plan dosyasinda iki farkli tarih katmani vardir:

- `03.04.2026 oncesi kurulu temel omurga`
- `03.04.2026`, `04.04.2026`, `06.04.2026` ve `08.04.2026` oturumlarinda acikca izlenen ve dokumante edilen isler

Onemli not:

- Bu dosyadaki eski `YAPILDI` maddeler, 03.04.2026 oncesinde repoda zaten kurulu ve dogrulanmis temel sistemleri de icerir.
- 03.04.2026 oncesi her alt sistem icin tek tek kesin gun bilgisi repoda tutulmadigi icin burada yapay tarih uydurulmaz.
- Gun gun kesin izlenebilen buyuk kilometre taslari asagida ayrica yazilir.

### 03.04.2026

- ekonomi sandbox katmani netlestirildi
- `Default / FreePlayer / PremiumPlayer` davranis ayrimi sabitlendi
- save snapshot izolasyonu kuruldu
- fake rewarded ad test katmani eklendi
- store pricing abstraction ve preview fiyat katmani kuruldu
- ekonomi ve gelecekteki Play Store bolgesel fiyat gecisi icin teknik raporlar yazildi

### 04.04.2026

- 4 dilli icerik ve level editoru kuruldu
- referans gorselden shape uretimi ve manuel shape authoring kuruldu
- content tarafinda `Kaydet ve Canli Uygula` akisi baglandi
- shape preview/runtime parity icin birden fazla duzeltme yapildi
- editor ve shape tarafindaki yeni sistemler plan ve teknik dokumanlara islendi

### 06.04.2026

- eksik kalan tuning editorleri tamamlandi
- tum editorler tek shell altinda `Toplu Tek Editor` icine toplandi
- editorler arasi commit-sonrasi sync katmani kuruldu
- live config ve web panel gecisine yonelik detayli mimari kılavuzu yazildi
- plan dosyalari ile teknik kılavuzlar capraz referansli hale getirildi

### 08.04.2026

- ilk manuel bug sweep turu fiilen baslatildi
- su maddeler manuel olarak cogu ltilip orijinal hipotez kapsami icinde sorun vermedi:
  - `BUG-007`
  - `BUG-009`
  - `BUG-010`
  - `BUG-011`
  - `BUG-013`
- ayni tarama sirasinda ek bir save/session turevi sorun yakalandi:
  - aktif level puani restore edilmiyor, session geri gelse bile skor sifirdan basliyordu
- sorun kapatildi:
  - score state save/session snapshot zincirine eklendi
  - restore sirasinda gameplay state ile birlikte score state de geri yuklenir hale getirildi
- ayni turda `BUG-014` dogrulandi ve kapatildi:
  - fail/continue modal ve info card acikken klavye inputu kapanmiyordu
  - gameplay input state'i merkezi kapatilarak ve klavye tuslari `interactable = false` yapilarak cozuldu
- ayni turda `BUG-027` manuel olarak denendi ve mevcut hipotez kapsami icinde sorun vermedi
- ayni turda `BUG-029` dogrulandi ve kapatildi:
  - progress tek global state yerine dil bazli ilerleme okuyacak sekilde genisletildi
  - secili dil ilk kez oynaniyorsa level 1'den baslar
  - daha once oynanmis dil kendi kaldigi levelden devam eder
  - first-clear level odulu global birakildigi icin dil degistirerek ayni coin odulu tekrar kazanilamaz
- ayni gun ikinci blok kontrolunde:
  - `BUG-024` artik aktif bug adayi olmaktan cikti
  - `BUG-025` ana hipotezi cogu ltulemedi
  - ancak continue sonrasi menu/store donusunde klavye kilitlenmesi seklinde turev bir sorun bulundu ve kapatildi
  - `BUG-026`, `BUG-034`, `BUG-068` manuel olarak denenip sorun vermedi
  - `BUG-037` ve `BUG-040` final live-config / web gecisi tarafinda tekrar acilmak uzere ertelendi

### 06.04.2026 itibariyla tam olarak neredeyiz

- `Faz 1-3` teknik olarak kuruldu
- tum ana tuning yuzeyleri editorlestirilmis durumda
- tek shell editor mevcut, fakat tekli editorler parity icin halen saklaniyor
- editorler arasi veri uyumu ve apply zinciri icin stabilizasyon yapildi
- `Faz C` giris kontrolu aktif; parity dogrulamasi ve bug sweep'e resmi gecis asamasindayiz
- dolayisiyla proje, `editor kilidi tamamlanmis / bug sweep girisi acilmis` noktasindadir

### 08.04.2026 itibariyla guncel konum

- bug sweep fiilen baslamistir
- ilk manuel sweep turunda save/session/restore cekirdeginin bir kismi kontrol edilmistir
- `BUG-007`, `BUG-009`, `BUG-010`, `BUG-011`, `BUG-013` icin orijinal hipotezler bu turda cogu ltulememistir
- buna karsin ayni aile icinde puan persistence regressioni bulunmus ve kapatilmistir
- `BUG-014` ve `BUG-029` gercek bug olarak dogrulanip kapatilmistir
- `BUG-027` bu turda cogu ltulememistir
- `BUG-024` ve `BUG-025` ilk halleriyle aktif bug olarak kalmamistir
- `BUG-025` testinde bulunan continue-sonrasi menu/store donus input kilidi sorunu kapatilmistir
- `BUG-026`, `BUG-034`, `BUG-068` acik bug olarak kalmamistir
- `BUG-037` ve `BUG-040` alpha oyuncu akisindan cok final liveops/release gecisine bagli riskler olarak ertelenmistir
- ilk tur manuel bug cogu ltma islemleri tamamlanmistir
- ikinci turdaki runtime-kritik maddeler de temizlenmistir
- siradaki is, final oyun hazirligi ve web/live-config gecisine yaklasirken ertelenen architecture/release maddelerini yeniden acmaktir

08.04.2026 sonu net karar:

- alpha demo icin acil manuel cogu ltulmesi gereken runtime-kritik bug kalmamistir
- ertelenen maddeler sunlardir:
  - `BUG-037`
  - `BUG-040`
  - `BUG-043`
  - `BUG-046`
  - `BUG-064`
  - `BUG-065`
  - `BUG-066`
  - `BUG-067`
  - `BUG-074`
  - `BUG-075`
  - `BUG-076`
  - `BUG-077`
- bu maddeler alpha akisini bloklamadigi icin degil, asil anlamli test baglamlari daha sonra gelecegi icin ertelenmistir
- dogru baglam:
  - web/live-config gecisi
  - production monetization pass
  - release-safe build
  - market oncesi final smoke ve optimizasyon turu

---

## 1. Mevcut Durum

### Gameplay Core Loop - `YAPILDI`

- Akis calisiyor:
  - `harf sec -> pin yukle -> swipe -> firlat -> hit -> reveal -> soru tamamla -> level ilerle`
- `Store -> Back -> Gameplay` akis omurgasi bagli.
- `InfoCard / Result / Level progression` omurgasi bagli.
- Save/restore sonrasinda:
  - cevap ilerlemesi geri geliyor
  - aktif hedef geri geliyor
  - saplanmis pinler geri kuruluyor
- Device Simulator tarafindaki pin hit kacirma problemi segment-tabanli hit tespitiyle guclendirildi.

Nasil kuruldu:

- `GameManager`, `LevelFlowController`, `Pin`, `SlotManager`, `RotatorPlaquePresenter` ve bagli event zinciri ayni ana loop'u tasiyor.
- Tek frame nokta kontrolu yerine segment taramasi kullanilarak simulator/cihaz parity problemi azaltildi.

### Fail / Continue Economy - `YAPILDI`

- `3 hata -> otomatik reset` kaldirildi.
- `Fail Modal` calisiyor.
- `Devam Et`:
  - enerji yemez
  - mevcut soru state'ini korur
  - `1 can` ile geri doner
- `Tekrar Dene` enerji maliyetlidir.
- `Continue` sonrasi hedef kutusu tekrar yanar.
- `Level complete -> next level` gecisindeki yanlis enerji tuketimi duzeltildi.

Nasil kuruldu:

- `GameManager`, `FailModalPresenter`, `QuestionLifeManager`, `EnergyManager` ve `SceneNavigator` uzerinden continue/retry ayrimi yapildi.
- Result ekranindaki `Next` akisi enerji tuketmeden sonraki levele aciliyor.

### Plaque Tabanli Hit Sistemi - `AKTIF`

- `Magnet snap` kaldirildi.
- Hit sonucu `pin ucu` uzerinden hesaplanir.
- `Perfect / Good / NearMiss / WrongSlot / WrongLetter / Miss` ayrimi bagli.
- Pin basarili hitte merkeze cekilmez; degdigi gercek noktada kalir.
- Dis saplanma yuzeyi esas alinir.
- Restore sirasinda pinler save'deki lokal poz/rot ile geri gelir.

Nasil kuruldu:

- Hit siniflandirmasi plaque zonlari uzerinden yapilir.
- Restore icin pin lokal pozisyon ve rotasyonlari save modelinde tutulur.
- Mekanik cekirdek hazirdir; final tuning ve parity kontrolu hala aciktir.

### HUD Hedef Gosterimi - `YAPILDI`

- `Sunu at` metni kaldirildi.
- Aktif hedef cevap alanindaki kutu vurgusuyla gosterilir.
- Bu yapi random slot order mantigiyla uyumludur.

Nasil kuruldu:

- `GameplayHudPresenter` cevap kutularini hedef indeks uzerinden pulse ederek gunceller.

### Wrong Slot Kirilma Hissi - `YAPILDI`

- Dogru harf yanlis slota saplandiginda pin kaybolmaz.
- Pin ve harf birlikte parcalanma efektine girer.

Nasil kuruldu:

- Hit sonucunda `WrongSlot` dali artik pin yok etme yerine kirilma hissi ve fail feedback uretir.

### Camera Shake / Vibrate Feedback - `AKTIF`

- `WrongSlot`, `WrongLetter`, `Miss`, `NearMiss` icin shake bagli.
- Mobil titreşim cagrisi bagli.
- Dogru saplanma hissi ve audio zinciri henuz tam kilitlenmedi.

Nasil kuruldu:

- `GameEvents` ile hit sonucu yayilip presentation katmaninda shake/vibration tetikleniyor.

### Coklu Dil Icerik ve Klavye Omurgasi - `YAPILDI`

- `tr / en / de / es` locale icerik klasorleri mevcut.
- Dil degisimi event akisi bagli.
- Locale bazli klavye dizilimleri mevcut:
  - Turkce Q
  - Ingilizce QWERTY
  - Ispanyolca locale duzeni
  - Almanca QWERTZ
- Locale bazli icerik yukleme `ContentService + Local/RemoteProvider` uzerinden calisiyor.

Nasil kuruldu:

- `WordSpinContentModels`, `ContentService`, `LocalContentProvider`, `RemoteContentProvider` ve locale dosyalari ayni veri omurgasini kullaniyor.
- `KeyboardPresenter` dil degisim event'inde layout'u yeniden kuruyor.

### Score Sistemi - `YAPILDI`

- `ScoreManager` ve `ScoreTuningProfile` mevcut.
- `Perfect / Good / hiz bonusu / level sonu bonusu` omurgasi bagli.
- Carpan ve hiz bonusu veri odakli ayarlanabilir.

Nasil kuruldu:

- Skor sonucu `LevelScoreFinalized` uzerinden result ekranina ve ekonomi katmanina geciyor.

### Mobil Runtime ve 9:16 Omurgasi - `AKTIF`

- `MobileRuntimeController` sahne bazinda portrait/orientation ve safe area uygular.
- `SceneBootstrap` singleton runtime kurulumunu garanti eder.
- `Gameplay` sahnesinde mobil-first layout tuning omurgasi vardir.
- Klavye ve safe area tarafinda ciddi ilerleme var, ancak final estetik kilit henuz tamamlanmis degildir.

Nasil kuruldu:

- Runtime safe area offset + size duzeltmesi birlikte uygulanir.
- Gameplay alt klavye dock'u ve ilgili presentation tuning'i hem builder hem runtime katmanina dagitildi.

### Android Cihaz Test Altyapisi - `AKTIF`

- `AndroidDeviceBuildTools` ile editor menusunden APK build akisi eklendi.
- Cihaz smoke checklist dokumana islendi.
- Repo tarafinda cihaz test akisi hazir; kullanici makinesindeki Unity Android modulu/toolchain kurulumu ayrica dogrulanir.

Nasil kuruldu:

- `Tools/WordSpin Alpha/Android/Build APK (Device Test)`
- `Tools/WordSpin Alpha/Android/Build And Run APK (USB Device)`
- `Tools/WordSpin Alpha/Android/Open Build Folder`

### Turkce Gelistirici Editorleri - `YAPILDI`

- Turkce telemetry/editor altyapisi mevcut.
- Klavye layout tuning editoru eklendi:
  - dil bazli ayar
  - canli onizleme
  - oran secimi
  - dil ayari kopyalama
- Ekonomi denge editoru eklendi:
  - economy profile secimi
  - sandbox mod secimi
  - level bazli coin ayari
  - theme offer ayari
  - bolgesel fiyat taslagi
  - simulasyon
  - test reset araclari

Nasil kuruldu:

- `KeyboardLayoutTuningWindow`
- `EconomyBalanceWindow`
- ilgili `ScriptableObject` profilleri ile asset tabanli tuning modeli kuruldu.

### Icerik ve Level Editoru - `YAPILDI`

- Kod veya JSON acmadan 4 dil icerik duzenlenebiliyor:
  - soru
  - cevap
  - bilgi karti baslik/govde
  - level metadata
- Yeni level ekleme, level kopyalama, soru blogu ekleme/silme destekleniyor.
- Level satirlari ve dil panelleri varsayilan kapali geliyor.
- Shape kutuphanesi ayni editor icinde yonetiliyor.
- Play modda `Kaydet ve Canli Uygula` ile aktif icerik sahneye tekrar yuklenebiliyor.

Nasil kuruldu:

- `WordSpinAlphaContentEditorWindow`
- `WordSpinAlphaContentEditorData`
- `ContentService.RefreshEditorContent()`
- `GameManager.ReloadCurrentSessionForEditorContent()`
- `MainMenuPresenter.RefreshEditorContent()`

bu zincir ile lokal JSON icerik editor icinde tek model olarak aciliyor, kayit sirasinda tekrar ilgili locale dosyalarina yaziliyor ve play modda canli apply ediliyor.

### Referans Gorselden Shape Uretimi ve Manuel Shape Authoring - `AKTIF`

- Shape kutuphanesinde referans gorselden otomatik nokta uretimi var.
- Kutu sayisi artinca veya azalinca custom pointler yeniden ornekleniyor.
- Manuel duzenleme icin:
  - turuncu handle ile slot merkezi tasinabiliyor
  - mavi handle ile slot gorsel acisi tek tek dondurulebiliyor
- `Bagimsiz Manuel Duzenleme` ile preview'da komsu plaque etkisi kapatilabiliyor.
- `Otomatik Gameplay Fit` ile runtime auto-fit acilip kapatilabiliyor.

Nasil kuruldu:

- `ShapeLayoutGeometry`
- `ShapeLayoutDefinition.customPoints`
- `ShapeLayoutDefinition.editorReferenceImagePath`
- `ShapeLayoutDefinition.plaqueVisualAngleOffsets`
- `WordSpinAlphaContentEditorWindow`
- `WordSpinAlphaContentEditorData`

bu katman shape'i veri odakli saklar; referans gorsel, manuel point duzenleme ve slot-basi angle offset ayni JSON uzerinden yasar.

### Plaque Gorsel Adaptasyon Katmani - `AKTIF`

- Core hit mantigi korunur.
- Buna karsin plaque'in yalnizca gorsel katmani shape'e daha estetik uyum saglayabilir:
  - genislik/yukseklik varyasyonu
  - disa itme
  - konturu takip eden hafif aci offset
- Custom/reference shape'lerde arka disk/plaque band baskisi azaltildi.

Nasil kuruldu:

- `ShapeLayoutGeometry.ResolvePlaqueVisualLayout(...)`
- `RotatorPlaquePresenter`
- shape layout icindeki yeni plaque visual tuning alanlari

Presentation katmani mekanikten ayrildi; `perfect/good/near miss/wrong slot` siniflandirmasi ayni kalirken plaque gorseli ayarlanabilir hale getirildi.

### Local + Remote Content / Telemetry Omurgasi - `YAPILDI`

- `ContentService`
- `LocalContentProvider`
- `RemoteContentProvider`
- `TelemetryService`
- snapshot mantigi

mevcuttur.

Not:

- Bu katman client-contract seviyesinde kuruldu.
- Production-ready telemetry pipeline olarak kabul edilmemelidir.

### Completion UI Persistence ve Restore - `YAPILDI`

- Level bittikten sonra acilan `InfoCard` ve `Result` popup state'i save'e yaziliyor.
- Oyuncu bu ekranlarda menuye gider veya oyunu kapatirsa geri geldiginde dogrudan ayni completion UI'ye donebiliyor.
- Boylece oyuncu haksiz enerji harcamadan sonraki level akisini tamamlayabiliyor.

Nasil kuruldu:

- `PlayerSaveModels` icine pending info/result alanlari eklendi.
- `GameManager` restore sirasinda pending info card veya pending result varsa ilgili UI'yi geri aciyor.
- `ResultPresenter.RestorePendingResultFromSave()` ve `InfoCard` restore akisi buna baglandi.

### Gameplay, Store ve Completion UI Localization - `YAPILDI`

- Gameplay HUD uzerindeki `Level`, `Score/Puan`, `Hearts/Can`, alt `Menu/Store`, swipe hint gibi sabit metinler locale'e baglandi.
- Info card altindaki `Continue` butonu locale'e baglandi.
- Result ekranindaki baslik, skor satirlari, yildiz/coin satirlari ve `Next/Menu` butonlari locale'e baglandi.
- Store ekranindaki store coin, teaser ve pricing notlari locale'e baglandi.

Nasil kuruldu:

- `GameplayHudPresenter`
- `GameSceneNavigationButtons`
- `InfoCardPresenter`
- `ResultPresenter`
- `StorePresenter`

bu siniflar `GameEvents.LanguageChanged` dinleyip label'lari yeniden yaziyor.

### Ekonomi / Monetization Test Sandbox Katmani - `YAPILDI`

- `Default` gercek gelistirme/runtime ekonomisi olarak korunuyor.
- `FreePlayer` ve `PremiumPlayer` yalnizca test sandbox modlari olarak calisiyor.
- Runtime override'lari `AppliedMode` uzerinden okunuyor; editor dropdown degismesi tek basina davranisi degistirmiyor.
- Her modun ayri save snapshot'i tutuluyor:
  - default snapshot
  - free snapshot
  - premium snapshot
- Mod degisimi sirasinda snapshot geri yukleniyor; sandbox denemeleri `Default` verisini kirletmiyor.

Nasil kuruldu:

- `TestPlayerModeProfile`
- `TestPlayerModeManager`
- `SaveManager.ReplaceData`
- `EconomyBalanceProfile` icin ayri `Default / Free / Premium` asset yolu

ile bu izolasyon saglandi.

### Fake Rewarded Ad ve Free/Premium Akis Testi - `YAPILDI`

- Free test modunda continue akisi fake rewarded reklam countdown ile test edilebiliyor.
- Premium test modunda premium/no-ads/enerji bypass davranisi gorulebiliyor.
- Bu yapi ana mekanige dagitilmadan ayri test katmani olarak tutuldu.

Nasil kuruldu:

- `DebugRewardedAdPresenter`
- `FailModalPresenter`
- `TestPlayerModeManager`
- `EnergyManager`
- `QuestionLifeManager`

uclerinden baglandi.

### Store Pricing Abstraction ve Regional Pricing Hazirligi - `YAPILDI`

- Store ekrani artik fiyat kaynagini dogrudan bilmez.
- Yeni katman kuruldu:
  - `IStorePricingProvider`
  - `StorePricingManager`
  - `PreviewStorePricingProvider`
- Simdilik test/preview icin dil -> varsayilan bolge eslemesi kullanilir:
  - `tr -> TR`
  - `en -> US`
  - `de -> DE`
  - `es -> ES`
- Bu final ticari mantik degildir; yalnizca test preview katmanidir.

Nasil kuruldu:

- `StorePresenter` fiyatlari `StorePricingManager` uzerinden alir.
- `PreviewStorePricingProvider` preview fiyatlarini `EconomyBalanceProfile` icindeki bolgesel taslaklardan ceker.
- Bu sayede ileride `GooglePlayStorePricingProvider` eklenip manager icinden provider degistirilerek store UI bozulmadan gercek storefront fiyatina gecilebilir.

---

## 2. Guncellenen Kararlar

### Hit Sistemi

- `Magnet-assisted snap` iptal edildi.
- Nihai alpha mantigi:
  - pin ucu gecerli plaque bolgesine degerse saplanir
  - `Perfect / Good / NearMiss` plaque ici zonlara gore verilir
  - pin saplandigi gercek noktada sabit kalir

### Rotator Yapisi

- Alpha demo icin `9 plaque` sabit.
- Shape/layout verisi JSON tabanli kalir.
- Kutu sayisi ve gorsel form ileride veriyle degisebilir.

### Coklu Dil

- Her dil icin ayri soru/cevap/icerik seti kullanilir.
- Sorular birebir ceviri olmak zorunda degildir.
- Klavye ve icerik locale bazli ayrisir.

### Mobil Oncelik

- Referans ekran `9:16`.
- Ilk oynanis dogrulamasi bilgisayar Game view degil, gercek Android cihaz olacaktir.
- Tum scene yerlesimleri safe area icinde calismalidir.

### Telemetry / AI / Cloud

- Alpha demo oncesi kapsam disi.
- Client omurgasi korunur, production pipeline daha sonra acilir.

### Ekonomi Sandbox Karari

- `Default` gercek oyun ve nihai tuning ortamidir.
- `FreePlayer` ve `PremiumPlayer` sadece test akisini gormek icin sandbox modlaridir.
- Release oncesi test katmani silinmek zorunda degildir; gerekirse kapatilir.

### Fiyatlandirma Karari

- Test asamasinda preview fiyatlar dil bazli varsayilan bolge eslemesi ile gosterilebilir.
- Nihai surumde fiyat dil bazli olmayacak.
- Fiyat Google Play storefront bolgesinden ve Billing product details sonucundan alinacaktir.

### Shape Authoring Karari

- Custom/reference shape authoring veri odakli kalacak.
- Shape noktalari ile plaque gorsel davranisi ayri ama baglantili yuzeyler olarak tutulacak.
- `Otomatik Gameplay Fit` acik oldugunda runtime okunurlugu korunacak.
- `Otomatik Gameplay Fit` kapali oldugunda manuel duzenlenen point set runtime'da birebir korunacak.

---

## 3. Sonraki Uygulama Sirasi

### Faz A - Android Gercek Cihaz Test Kilidi - `AKTIF`

- Unity Android modulu/toolchain kurulumu kesin dogrulanacak.
- `Build And Run APK (USB Device)` akisi tek cihaz uzerinde uctan uca calisir hale gelecek.
- Ilk smoke test sirasi sabitlenecek:
  - `MainMenu`
  - `Play`
  - ilk 3 level
  - `Fail -> Continue / Retry`
  - `save/quit -> resume`
  - dil degistirip tekrar `Play`
  - `Store -> Back`

### Faz B - 9:16 Mobil Layout ve Safe Area Polish - `AKTIF`

- `Gameplay`, `MainMenu`, `Store`, `FailModal`, `InfoCard`, `Result` ayni mobil yerlesim mantigina cekilecek.
- Ozellikle su yuzeyler kilitlenecek:
  - ust bar
  - soru/cevap paneli
  - rotator alanı
  - launcher/saplanma ekseni
  - alt keyboard dock
- `safe area` pozisyon + boyut + ic margin birlikte ele alinacak.
- Alt keyboard dock tum telefon oranlari icin hem sigan hem estetik hale getirilecek.

### Faz C - Pre-Polish Sistem Taramasi ve Bug Kilidi - `AKTIF`

- Gorsellik tasarimi ve juicy hissiyat pass'ine gecmeden once, son ufak iterasyonlar tamamlanacak.
- Ardindan tum yeni katmanlar uzerinde kapsamli bir sistem taramasi yapilacak:
  - gameplay core loop
  - save/restore
  - pending info/result popup restore
  - localization
  - mobile safe area
  - keyboard docking
  - content editor -> JSON -> runtime parity
  - shape kutuphanesi -> preview -> gameplay parity
  - manuel shape duzenleme persistence
  - referans gorselden shape uretimi
  - default/free/premium sandbox izolasyonu
  - economy/store/coin senkronizasyonu
  - pricing preview davranisi
- Bu taramadan `muhtemel bug listesi` cikarilacak ve risk seviyesine gore siralanacak.
- Once manuel olarak yeniden uretilmeye uygun happy-path ve edge-case senaryolari denenecek.
- Dogrulanan buglar once fixlenecek.
- Daha sonra kalan yuksek riskli mantiklar icin hedefli test katmani yazilacak:
  - saf hesaplama ve karar kurallari icin unit test
  - save/restore, sandbox mode switch, level completion flow gibi alanlar icin integration test
- Bu faz tamamlanmadan gameplay juicy/visual polish baslatilmayacak.
- Bu fazla birlikte, `UNIFIED_EDITOR_TO_LIVEOPS_PLAN.md` icindeki buyume riskleri ve alpha sureci calisma disiplini de fiilen uygulanmaya baslanacak.
- 06.04.2026 itibariyla editor yazim fazi ve tek shell editor kurulumu tamamlandigi icin, proje bu fazin giris noktasina gelmistir.
- Hemen sonraki somut is: parity dogrulamasi ve bug sweep'tir.

Neden eklendi:

- Projeye cok sayida yeni sistem eklendi ancak henuz kapsamli bir test gecisi yapilmadi.
- Entegrasyon tipi kiriklar, polish sonrasi degil once yakalanmalidir.

### Faz D - Gameplay Visual Lock - `SIRADA`

- Gameplay sahnesinin gorsel dili kilitlenecek:
  - panel kontrasti
  - plaque okunurlugu
  - glow/haze azaltimi
  - alt dock final estetik

### Faz E - Hit Feel / Audio / Rhythm Stabilizasyonu - `SIRADA`

- Kilit davranislar sabitlenecek:
  - ilk perfect normal hit sesi
  - ardışık perfect'lerde tizlesen zincir
  - good hitte reset
  - wrong-slot parcalanma sesi + shake
  - harf yukleme / fire sesi
- `ImpactFeelProfile` ve mevcut event zinciri korunacak.

### Faz F - Alpha Kabul Matrisi - `SIRADA`

- Her buildte:
  - `TR / EN / DE / ES`
  - `Play`
  - `Level Select`
  - `Store -> Back`
  - `save/quit -> resume`

akislari calistirilacak.

- Buna ek olarak yeni zorunlu kabul:
  - info card restore
  - result popup restore
  - default/free/premium snapshot izolasyonu
  - sandbox'tan `Default`e donus
  - store preview fiyatinin profile/language degisimine cevap vermesi

### Faz F - Monetization Production Pass - `SIRADA`

- Gercek `IPurchaseService` implementasyonu
- Gercek `IStorePricingProvider` implementasyonu
- Play Billing product details entegrasyonu
- Play Console urun tanimi
- Preview fiyat katmanindan storefront pricing'e gecis

### Faz G - Alpha Demo Sonrasi Backlog - `SIRADA`

- Turkce metrics / hotfix editor genislemesi
- AI telemetry / cloud / developer panel
- gelismis canli yonetim araclari
- telemetry schema ve aggregate snapshot production pass

---

## 4. Onemli Arayuz / Tip / Veri Guncellemeleri

### Yeni veya Guncellenmis Veri Yuzeyleri

- `ScoreTuningProfile : ScriptableObject`
- `ImpactFeelProfile : ScriptableObject`
- `KeyboardLayoutTuningProfile : ScriptableObject`
- `EconomyBalanceProfile : ScriptableObject`
- `TestPlayerModeProfile : ScriptableObject`
- locale bazli `questions.json / info_cards.json / levels.json`
- `shape_layouts.json`, `difficulty_profiles.json`, `rhythm_profiles.json`
- `store_catalog.json`
- `membership_profile.json`
- `ShapeLayoutDefinition`
  - `customPoints`
  - `editorReferenceImagePath`
  - `gameplayAutoFit`
  - `plaqueVisualAngleOffsets`
  - plaque visual tuning alanlari

### Yeni Editor Yuzeyleri

- `DeveloperTelemetryWindow`
- `AndroidDeviceBuildTools`
- `WordSpinAlphaUnifiedEditorWindow`
  - tum editor yuzeylerini tek shell icinde toplar
  - mevcut alt editorleri host ederek parity bozmadan gecis saglar
  - `GameplaySceneTuner` custom inspectorunu ayni kabukta cizdirir
  - build/generate/android yardimci komutlarini ayni panelde toplar
- `KeyboardLayoutTuningWindow`
  - dil bazli tuning
  - onizleme oran secimi
  - dil ayari kopyalama
- `EconomyBalanceWindow`
  - default/free/premium ekonomi profili secimi
  - test modu secimi
  - level coin ayari
  - theme offer ayari
  - bolgesel fiyat taslagi
  - simulasyon
  - reset araclari
- `WordSpinAlphaContentEditorWindow`
  - level olusturma / kopyalama / silme
  - 4 dil soru/cevap/info karti duzenleme
  - shape kutuphanesi
  - referans gorsel atama
  - canli kaydet ve uygula
  - manuel point ve plaque aci duzenleme
- `SlotHitTuningWindow`
  - slot/plaque boyutu
  - active/passive renkler
  - perfect/good/hata feedback renkleri
  - hit toleranslari
- `PinInputTuningWindow`
  - pin boyutu
  - pin/input pacing
  - swipe ve cooldown ayarlari
- `RotatorRhythmTuningWindow`
  - rhythm profile
  - difficulty profile/tier
  - baz donus ve pacing tuning'i
- `QuestionFailFlowTuningWindow`
  - soru cani
  - fail akis ve CTA tuning'i
- `UiSurfaceTuningWindow`
  - gameplay HUD
  - fail/result/info/store/membership/menu yuzeyleri
- `FeelVisualTuningWindow`
  - score
  - impact
  - tema runtime pressure tuning'i
- `ThemeStoreConfigWindow`
  - theme/store/membership/energy runtime config
- `ThemePackagePreviewWindow`
  - tema paket preview ve saglik kontrolu
- `MobileRuntimeTuningWindow`
  - safe area ve cihaz runtime tuning'i
- `AmbientPulseTuningWindow`
  - glow/ambiyans/pulse mikro gorsel tuning'i
- `TelemetryPolicyWindow`
  - telemetry queue/flush/trim policy tuning'i
- `RemoteContentHotfixWindow`
  - remote manifest/hotfix publish ve refresh
- `ValidationAuditWindow`
  - global validation ve referans taramasi
- `SaveSessionDebugWindow`
  - save/session inspect ve reset araci

Not:

- tekli editorler bu asamada silinmedi
- once `WordSpinAlphaUnifiedEditorWindow` uzerinden davranis parity'si dogrulanacak
- sonra kullanilmayan tekli menuler kademeli olarak kaldirilacak

### Yeni Runtime ve Editor Baglanti Katmanlari

- `TestPlayerModeManager`
- `DebugRewardedAdPresenter`
- `StorePricingManager`
- `PreviewStorePricingProvider`
- `ShapeLayoutGeometry`
- `WordSpinAlphaEditorRuntimeRefreshUtility`
- `WordSpinAlphaRuntimeConfigRepository`
- `WordSpinAlphaEditorSyncUtility`
- `WordSpinAlphaEditorUiTuningUtility`

Not:

- ilk bes madde runtime veya runtime'a baglanan veri/servis katmanlaridir
- son dort madde editorlerin guvenli apply, veri erisimi ve editorler arasi senkronu icin eklenmis editor destek katmanlaridir

### Runtime Baglantilari

- `KeyboardPresenter` klavye tuning profilini okur
- `GameplaySceneTuner` locale bazli keyboard dock tuning'ini profilden alir
- `MobileRuntimeController` safe area ve mobil runtime davranisini uygular
- `SceneBootstrap` uzerinden singleton'lar garanti edilir
- `GameManager` completion UI restore'ini yonetir
- `GameManager` editor canli apply sirasinda aktif session'i yeni content ile yeniden yukleyebilir
- `ResultPresenter` pending result state'i save ile geri acabilir
- `StorePresenter` fiyat ve ekonomi bilgisini provider + economy manager uzerinden gunceller
- `ContentService` editor kaydi sonrasinda lokal icerik cache'ini yenileyebilir
- `RotatorPlaquePresenter` shape'e gore adaptif plaque visual layout hesaplayabilir

---

## 5. Test Plani

### Zaten Dogrulananlar

- `3 hata -> Fail Modal`
- `Devam Et -> 1 canla devam`
- `Tekrar Dene -> enerji maliyeti`
- `Continue` sonrasi hedef kutusunun yeniden yanmasi
- Save/restore sonrasi cevap ilerlemesi
- Save/restore sonrasi saplanmis pinlerin geri gelmesi
- Yanlis slotta parcalanma efekti
- Hedef kutunun cevap panelinden gosterilmesi
- Locale icerik dosya omurgasi
- Score sisteminin veri odakli omurgasi
- Klavye tuning editorunun asset tabanli calismasi
- Info card localization
- Result screen localization
- Gameplay HUD localization
- Store preview price abstraction katmani
- Ekonomi sandbox snapshot izolasyonu
- Icerik editorunden 4 dil soru/cevap/info karti duzenleme
- Shape kutuphanesi ve referans gorsel baglama
- Play modda `Kaydet ve Canli Uygula`

### Alpha Oncesi Zorunlu Kabul Testleri

- `Perfect` hit:
  - net audio
  - gorunur feel
  - multiplier/score duzgun
- `Good` hit:
  - dogru ses
  - chain reset davranisi
- `WrongSlot`:
  - parcalanma + fail feedback
- `WrongLetter`:
  - klavye bazli fail feedback
- `Android`:
  - `1080x1920`
  - `1080x2160`
  - `1080x2340`
  - `1080x2400`
- `Locale`:
  - `TR / EN / DE / ES` sorular dogru geliyor
  - klavyeler locale'e gore dogru aciliyor
  - gameplay/store/result/info card metinleri locale'e gore degisiyor
- `Keyboard`:
  - safe area disina tasmiyor
  - kenar tuslar frame koselerine yaslanmiyor
  - oran degisiminde estetik bozulmuyor
- `Completion Restore`:
  - info card ekraninda quit -> relaunch
  - result ekraninda quit -> relaunch
  - sonraki level akisi ekstra enerji harcamadan devam ediyor
- `Economy Sandbox`:
  - `Default -> FreePlayer -> Default`
  - `Default -> PremiumPlayer -> Default`
  - snapshot izolasyonu bozulmuyor
  - default ekonomi free/premium testleri ile kirlenmiyor
- `Store Pricing Preview`:
  - preview fiyat dil eslemesine gore degisiyor
  - farkli economy profile seciminde store preview fiyati da degisiyor
  - coin fiyati ve preview para fiyatlari ayni anda tutarli gorunuyor
- `Content Editor`:
  - yeni level ekleme JSON'a dogru yaziliyor
  - 4 dil soru/cevap/info karti kaydi dogru dosyalara gidiyor
  - `Kaydet ve Canli Uygula` gameplay ve main menu tarafinda yeni icerigi aciyor
- `Shape Editor`:
  - referans gorselden shape uretimi calisiyor
  - manuel point tasima persistence bozmaz
  - slot-basi plaque aci duzenleme persistence bozmaz
  - `Otomatik Gameplay Fit` acik/kapali davranisi beklendigi gibi ayrisir
  - preview ile runtime gorsel sonucu ayni mantigi izler

---

## 6. Varsayimlar ve Sabitler

- Alpha demo icin `9 plaque` sabit.
- Gorsel kalite hala placeholder'dan cikan ama tam final olmayan seviyede; oncelik gameplay kilidi ve mobil okunurluk.
- `Continue` sonrasi donus cani `1`.
- `Retry` enerji maliyetlidir.
- `Impact feel`, `score`, `keyboard layout` ve `economy` veri odakli kalacak.
- Theme yalniz presentation yogunlugunu yonetecek; mekanik kararlari yonetmeyecek.
- `Telemetry / AI / cloud / developer panel` alpha demo sonrasina ertelendi.
- Ilk gercek kalite esigi bilgisayar Game view degil, Android cihaz ustu test olacak.
- Preview fiyatlar test amaclidir; nihai fiyat storefront bolgesinden gelecektir.
- Test sandbox katmani release oncesi silinmek zorunda degil; gerekirse kapatilacaktir.

---

## 7. Ek Not

Bu plan dosyasi artik yalnizca "ne hedefleniyor" listesi degildir. Ayni zamanda:

- repoda gercekten kurulu sistemlerin kaydi
- nasil kuruldugunun ozeti
- hangi katmanin finala yakin, hangisinin test-only oldugunun ayrimi
- market oncesi gecis icin teknik referans

olarak tutulmalidir.

---

## 08.04.2026 - Hub ve Gameplay Navigasyon Akisi Guncellemesi

08.04.2026 itibariyla oyun ici ana navigasyon kurgusu yeniden netlestirildi ve kod altyapisina islendi.

### Guncel akisin ozet hali

- `Boot` acilisinda oyuncu artik dogrudan eski ana menuye degil, giris ekrani rolu verilen `MainMenu` sahnesine gider.
- Bu giris ekrani sadece:
  - dil secimi
  - oyuna basla
  akisina hizmet eder.
- `Oyna` secildiginde oyuncu artik direkt gameplay'e degil, yeni ana merkez olan `Hub` sahnesine gecer.
- `Hub` sahnesi artik oyunun asagidaki ana meta yuzeylerini barindirir:
  - yol tabanli seviye secimi
  - gorevler
  - profil
  - magaza
- Bu yuzeyler alt menuden gecilir ve ayni sahne icinde kayarak panel gecisi mantigiyla calisir.

### Gameplay -> pause -> hub kurali

- Gameplay ekranindaki sol alt navigasyon dugmesi artik direkt sahne gecisi yapmaz.
- Bu dugme `Duraklat` davranisi verir.
- Basildiginda:
  - oyun donar
  - gameplay arkada gorunur kalir
  - popup acilir
  - popup icinde `Devam Et` ve `Ana Merkeze Don` secenekleri gorunur
- `Devam Et`:
  - popup'i kapatir
  - oyunu kaldigi yerden surdurur
  - ceza uygulamaz
- `Ana Merkeze Don`:
  - aktif session snapshot alir
  - diske flush eder
  - oyuncuyu `Hub` sahnesine geri gonderir

### Hub -> ayni levele geri donus kurali

- Oyuncu `Hub` icindeyken aktif save/session bulunan levele tekrar basarsa artik direkt yeni giris yapilmaz.
- Bunun yerine ikinci bir karar popup'i acilir:
  - `Devam Et`
  - `Bastan Basla`
  - `Vazgec`
- `Devam Et`:
  - oyuncuyu ayni levele kaldigi yerden geri alir
  - can veya enerji cezasi uygulamaz
- `Bastan Basla`:
  - leveli sifirdan acar
  - normal yeniden giris / tekrar dene maliyet mantigini uygular

### Bu degisikligin sebebi

Bu guncelleme ile:

- oyuncu yanlislikla hub'a dondugunde ilerlemesini kaybetmez
- gameplay icindeki duraklat davranisi daha anlasilir hale gelir
- ana merkez / alt menulu hub kurgusu daha tutarli bir omurgaya oturur
- ileride store, profil, gorevler ve teklif kartlari ayni hub yapisinda buyutulebilir

### Uygulama notu

Bu akis artik alpha demo davranisinin parcasi kabul edilir. Bundan sonraki bug sweep, polish ve live-config planlari bu yeni omurga uzerinden ilerlemelidir.

---

## 08.04.2026 - Stitch Tasarimindan Unity'ye Gecis Is Akisi

08.04.2026 itibariyla sayfa tasarimlarini Unity'ye aktarma konusunda asagidaki calisma modeli esas alinacaktir.

### Ana karar

- Tasarimci once ekrani `Stitch` icinde tam kompozisyon olarak hazirlar.
- Ancak bu kompozisyon Unity'ye tek parca final ekran olarak alinmaz.
- Unity'ye tasinacak yapi `hibrit` modeldir:
  - buyuk statik gorsel katmanlar tasarimdan gelir
  - butonlar, kartlar, ikonlar ve popup skinleri parcali gelir
  - metin, sayaç, badge ve tum dinamik veriler Unity icinde uretilir

### Neden tek parca ekran alinmayacak

Cunku tek buyuk ekran PNG modeli:

- responsive yapida zor kirilir
- safe area uyumunu zorlastirir
- localization alanlarini bozar
- kucuk bir degisiklikte tum ekranin yeniden export edilmesine neden olur
- alt menu, popup, teklif karti ve level node gibi interaktif yuzeylerin tekrar kullanimini zayiflatir

### Neden her kucuk parcayi da tek tek export etmiyoruz

Cunku bu da asset karmasina yol acar:

- gereksiz dosya sayisi artar
- klasor ve prefab bakimi zorlasir
- atlas ve sprite yonetimi sertlesir
- kucuk degisikliklerde is akisi yavaslar

### Uygulanacak dogru model

#### 1. Stitch teslimi

Tasarim once tek tam ekran kompozisyon olarak gelir.

Bu asamada geliştirici tarafinda yapilacak is:

- mevcut Unity sahnesi ile tasarimi karsilastirmak
- hangi alanlarin Unity'de dinamik kalmasi gerektigini ayirmak
- hangi parcilarin ayrik PNG olarak alinacagini listelemek

#### 2. Teknik export listesi

Tam ekran tasarim geldikten sonra su tip teknik liste uretilir:

- hangi alan tek buyuk arka plan katmani olacak
- hangi alan ayri buton skin'i olacak
- hangi alan ikon olarak ayri alinacak
- hangi panel `9-slice` icin uygun
- hangi alan Unity text ile doldurulacak
- hangi alan hic export edilmeyecek ve Unity'de tekrar kurulacak

#### 3. Tasarimdan alinabilecek katman turleri

Stitch tasarimindan su katmanlar alinabilir:

- `BG_Sky`
- `BG_City`
- `BG_Road`
- `BG_ForegroundDecor`
- `UI_TopBar_Skin`
- `UI_BottomNav_Skin`
- `CARD_LevelActive`
- `CARD_GenericOffer`
- `POPUP_Pause`
- `POPUP_Resume`
- `ICON_*`

#### 4. Unity'de kalmasi zorunlu olan alanlar

Su alanlar gorselin icine gomulmeyecek:

- level numaralari
- coin, enerji, ipucu sayilari
- sureler ve countdown alanlari
- dil degisen metinler
- bildirim badge sayilari
- popup baslik ve govde metinleri
- market / teklif / gorev sayaclari

Bu alanlar `TextMeshPro`, runtime badge ve veri bagli UI ile Unity icinde kalacak.

### Level secim hub icin ozel kural

Level secim hub ekraninda:

- arka plan ve dekoratif buyuk yol kompozisyonu Stitch'ten gelebilir
- ama level node'lari, aktif level odagi, progress, alt menu ve teklif butonlari Unity icinde ayrik kalacaktir

Boylece:

- level kaydirma
- aktif node buyume/kuculme
- odak degisimi
- runtime bildirimler
- popup ve navigation davranislari

tasarimi tekrar bozmadan devam eder.

### En pratik calisma sekli

Bu projede kabul edilen hizli pipeline su sekildedir:

1. tasarimci tam ekran Stitch kompozisyonu uretir
2. bu kompozisyon gelistiriciye tek gorsel olarak gonderilir
3. gelistirici mevcut Unity sahnesiyle karsilastirir
4. gelistirici teknik export listesi verir
5. tasarimci sadece istenen PNG/SVG parcilarini uretir
6. Unity tarafinda bu parcilar mevcut sahne iskeletine skin olarak baglanir

### Beklenen fayda

Bu model ile:

- gereksiz asset satin alma ihtiyaci azalir
- tasarim hizli ilerler
- Unity sahnesi bozulmadan skinlenir
- daha sonra ayni ekran kolayca revize edilir
- popup, alt menu, teklif karti ve ikon sistemi yeniden kullanilabilir hale gelir

### Hatirlatma

Bu akis, bundan sonra hub, pause popup, resume popup, store, profil ve benzeri tum yeni meta sayfalar icin varsayilan tasarimdan-Unity'ye gecis kuralidir.

---

## 19.04.2026 - Alpha Demo Son Faz Karari ve UI Akis Kilidi

19 Nisan 2026 itibariyla alpha demo icin ana karar degisti: yeni buyuk sayfa veya yeni buyuk mekanik eklemek yerine, eklenen iki ana meta yuzeyin oyun akisi ile uyumlulugu test edilecek ve kucuk iterasyonlarla polish/fix yapilacak.

### Bugunku ana karar

- `MainMenu` giris ekrani artik tasarimli PNG katmanli yapiyla ana akisa alinmistir.
- `Oyna` aksiyonu artik eski designless ana menuden degil, tasarimli MainMenu uzerinden `Hub` sahnesine gider.
- `HubPreview` bundan sonra yeni UI sayfalarinin test/sandbox sahnesi olarak kalir.
- `HubPreview` uzerinde onaylanan tasarimlar, kontrollu kopyalama/promote akisi ile ana oyun akisana alinir.
- Onaylanmamis preview hicbir zaman dogrudan production sahneye gomulmez.

### MainMenu icin bugun dogrulananlar

- `HubPreview` tasarimi kopyalanarak runtime `MainMenu` sahnesine aktarildi.
- Eski tasarimsiz MainMenu davranisi yerine tasarimli giris ekraninin kullanilmasi kabul edildi.
- Dil secimi MainMenu uzerinden calismaya devam eder.
- `OYNA` butonu `PointerDown` temelli hizli gecis davranisina alindi.
- `OYNA` basim efekti korunur; buton basili kalmis gibi beklemez.
- Gecis hissi: buton once kisa press feedback verir, release animasyonu baslarken `Hub` yuklemesi tetiklenir.
- `Button.onClick` kaynakli gecikmeli click davranisi play hitbox icin kaldirildi.
- 9:16 portrait arka plan PNG'lerinde bile bosluk kalabildigi dogrulandi; sorun kaynak gorsel oranindan degil, `MainMenuPngPreview.prefab` icindeki sabit arka plan boyutu ve builder yerlesim davranisindan kaynaklandi.
- Arka plan katmani prefabda bosluk kalmayacak sekilde buyutuldu ve `WordSpinAlphaSceneBuilder.cs` ayni cover davranisina cekildi. Yeni MainMenu arka planlarinda sadece PNG oranini degil, prefab + builder kaplama sonucunu da kontrol et.

### Level Hub icin bugun kurulan karar

Level secim yolu artik sabit kodlu noktalara bagli kalmamali. Yeni arka plan tasarimi geldiginde:

1. Arka plan once `HubPreview` sahnesine koyulur.
2. `LevelHubPreviewController` secilir.
3. Scene View uzerinde arka plan gorunurken ray noktalarinin yeri manuel duzenlenir.
4. Level kutulari bu ray noktalarini takip eder.
5. Scroll/drag test edilir.
6. Onaylaninca bu sahne/prefab ana Hub akisana tasinir.

Bu sistem icin:

- `LevelHubPreviewController.cs`
- `LevelHubPreviewControllerEditor.cs`

katmanlari eklendi.

Ray verisi gecici editor state'inde degil, controller component'i uzerinde serialize edilir. Bu nedenle `HubPreview` sahnesi kopyalanip ana Hub sahnesine tasindiginda ray ayarlari da beraber gider.

### Alpha demo icin guncel disiplin

Bu tarihten sonra alpha demo oncesi calisma sirasi:

1. Yeni buyuk sayfa ekleme.
2. Yeni buyuk mekanik ekleme.
3. MainMenu -> Hub gecisini cihazda ve editor'de test et.
4. HubPreview level yolu ray editoru ile level hub tasarimini oturt.
5. Level hub onaylaninca ana Hub akisana kontrollu sekilde al.
6. Kucuk gorsel/UX polish yap.
7. Kritik akislari tekrar test et:
   - ilk acilis
   - dil secimi
   - play
   - hub
   - level baslatma
   - pause
   - hub'a donus
   - resume/restart
   - fail/continue
   - skor/save

Bu notun amaci, alpha demo sonuna yaklasirken kapsam kaymasini engellemektir. Bundan sonra ana risk yeni sistem eklemek degil, mevcut calisan akisi fazla degistirerek bozmak olarak kabul edilir.

## 21.04.2026 - HubPreview Level Hub Recovery Durumu

21 Nisan 2026'da level hub tarafinda sadece kutu boyut/oran duzeltmesi hedeflenmisken, `HubPreview` sahnesi scene/prefab override kirliligi nedeniyle bozulmustur. Aynı gun icinde yapilan teknik recovery sonrasi su durum dogrulanmistir:

- `HubPreview` sahnesi source assetlerden temiz rebuild ile geri kurulmustur
- `LevelHubPreview.prefab` temiz kaynak olarak yeniden uretilmistir
- orphan `NodeVisual` root objeleri temizlenmistir
- kirli `m_AddedGameObjects` birikimi temizlenmistir
- controller artik hierarchy mutasyonu yapmaz
- node gorsel mimarisi builder tarafinda tek kaynakta tutulur

Bu nedenle 21 Nisan itibariyla level hub konusu iki faza ayrilir:

1. recovery ve veri butunlugu
   - tamam
2. son gorsel duzeltme
   - acik

Acik kalan tek dogrulanmis gorsel is:

- level kutularinin altinda beyaz zemin gorunumu devam etmektedir

Bu nedenle sonraki editte uygulanacak sinir:

- scene recovery veya prefab recovery yeniden acilmayacak
- ray/path editor verisi korunacak
- `LevelHubPreviewController` icine hierarchy mutasyonu geri sokulmayacak
- bir sonraki is yalnizca node altindaki beyaz zemin kaynagini temizlemek olacak

Kalici is akisi guncellemesi:

Bir preview sayfasinda sorun sadece oran/boyut veya gorsel uyum ise, edit sirasi sunun disina cikmayacak:

1. asset alpha / bos canvas kontrolu
2. prefab rect ve gorsel katman kontrolu
3. builder uretim davranisi
4. en son scene rebuild

Dogrudan scene hierarchy veya YAML toplu regex mudahalesi yasaktir.
