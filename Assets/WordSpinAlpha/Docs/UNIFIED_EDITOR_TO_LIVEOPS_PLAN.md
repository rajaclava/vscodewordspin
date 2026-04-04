# WordSpin Alpha - Unified Editor To LiveOps Plan

## Ozet

Bu dokumanin amaci, mevcut alpha demo planini bozmeden su hedefe giden en guvenli sirayi sabitlemektir:

- once tum ayar yuzeylerini tek bir Turkce editor icinde toplamak
- sonra sistem taramasi ve bug kilidi yapmak
- sonra gameplay ve sayfa tasarimlarini kilitlemek
- sonra 4 dil / 25 level alpha demoyu hazirlamak
- sonra telemetry, hotfix ve web tabanli canli ayar altyapisina gecmek

Bu planin ana prensibi:

- mekanik algoritmalar yeniden yazilmayacak
- authoring, telemetry, live ops ve web panel katmanlari mekanikten ayrik kurulacak
- APK boyutu ve runtime performansi korunacak
- ileride oyuna yeni tema paketleri ses/animasyon/UI dahil rahatlikla eklenebilecek

Bu plana ek olarak, mevcut kod ve belgeler uzerinden gorulen olasi buyume riskleri de artik resmi olarak bu planin parcasi kabul edilir. Yani alpha demo suresince yalnizca yeni sistem kurmak degil, o sistemlerin ileride kirilma ihtimalini dusuk tutacak sekilde calismak da hedefin kendisidir.

---

## 1. Temel Kararlar

### 1.1 Ayrik katman prensibi

Sistem dort ayri katman halinde tutulacak:

1. `Core Runtime`
   - gameplay mekanikleri
   - hit sistemi
   - save/restore
   - scene flow
2. `Authoring / Editor`
   - tum tuning ve icerik editorleri
   - validation
   - live apply
3. `LiveOps / Telemetry`
   - event toplama
   - config versiyonlama
   - hotfix kontrolu
   - export / backup
4. `Web Dashboard`
   - cloud uzerinden ayar guncelleme
   - telemetry inceleme
   - content/economy/theme operasyonu

Bu ayrim korunursa:

- editor buyurken APK sismez
- telemetry buyurken gameplay mekanigi kirilmaz
- web panel gecisi icin runtime yeniden yazilmaz

### 1.2 Neler uzaktan degistirilebilir

Final hedefte internetten degistirilebilir olmasi gerekenler:

- ekonomi degerleri
- level bazli oduller
- offer gorunurlukleri
- UI metinleri
- locale icerikleri
- shape/layout tuning profilleri
- keyboard tuning profilleri
- impact/feel tuning profilleri
- theme paket metadata ve theme aktifligi
- gorsel/ses/animasyon paket referanslari
- feature flag ve kill switch'ler

Uzaktan degistirilemeyecek sey:

- compile edilmis gameplay algoritmasi
- yeni kod davranisi
- core mekanik mantiginin kendisi

Yani final hedef `her ayari uzaktan yonetmek` olabilir, ama `her kod degisikligini uzaktan yapmak` olamaz. Bu ayrim acik tutulmali.

### 1.3 APK ve performans koruma prensibi

APK boyutu ve runtime performansi icin su kurallar sabit kalacak:

- tum editor kodu `Editor` altinda veya editor-only asmdef icinde kalacak
- referans gorseller, draft asset'ler ve authoring cache'leri build'e girmeyecek
- runtime datasi ile authoring datasi ayrik tutulacak
- telemetry queue boyutu sinirli olacak
- batch upload, log rotation ve eski veri temizligi olacak
- tema paketleri base APK'ye gomulmeyecek; uzaktan indirilebilir olacak
- test kodu player build'e girmeyecek

---

## 2. Nihai Hedef Mimarisi

### 2.1 Runtime tarafinda kalacak ana veriler

- localized `questions / info_cards / levels`
- `shape_layouts`
- `difficulty_profiles`
- `rhythm_profiles`
- `score tuning`
- `impact feel tuning`
- `economy tuning`
- `theme metadata`
- live config manifest'ten gelen override degerleri

### 2.2 Editor tarafinda kalacak authoring verileri

- shape referans PNG'leri
- editor preview cache'leri
- draft layout state'leri
- bulk import/export yardimci verileri
- validation raporlari
- test otomasyon konfigleri

### 2.3 LiveOps tarafinda olusacak yeni katmanlar

- `ConfigRepository`
- `ConfigVersionManifest`
- `RemoteOverrideResolver`
- `TelemetryEventBuffer`
- `TelemetryUploader`
- `HotfixPolicy`
- `FeatureFlagRegistry`

Bu katmanlar mevcut mekanige yeni davranis yazmayacak. Sadece mevcut data-driven yuzeylere veri girecek.

---

## 3. Yapim Sirasi

### Faz 1 - Tum Tuning Yuzeylerini Taramak

Ilk is su olacak:

- projedeki tum ayarlanabilir yuzeyler listelenecek
- hangi ayarin zaten editoru var, hangisinin yok oldugu belirlenecek
- hangi ayarin gameplay kilidi icin gerekli, hangisinin sonra acilabilecegi ayrilacak

Tarama sonucu su kategorilerde liste cikacak:

- gameplay layout
- keyboard layout
- mobile safe area
- score tuning
- impact / juicy tuning
- economy tuning
- free/premium sandbox tuning
- shape library / shape authoring
- 4 dil content editing
- store offer / preview pricing
- theme metadata
- fail/result/info card layout
- build / smoke / validation / reset araclari

Bu listenin hedefi:

- hicbir ayar yuzeyini sonradan unutup koddan duzeltmek zorunda kalmamak

### Faz 2 - Eksik Editorlerin Tamamlanmasi

Tarama sonucunda editoru eksik olan her ayar yuzeyi icin Turkce editor modulu yazilacak.

Bu asamada hedef:

- ayarin kod icinde kalmamasi
- veri odakli hale gelmesi
- editor icinde duzenlenebilir olmasi
- gerekiyorsa play modda `canli apply` desteklemesi

Bu fazda eksik olmasi muhtemel moduller:

- gameplay layout toplu tuning modulu
- impact/juicy tuning modulu
- fail/result/info card layout tuning modulu
- theme package metadata editoru
- validation ve diff/export modulu
- build smoke araclari

### Faz 3 - Tek Bir Toplu Editor Icinde Birlestirme

Tum moduller tek shell editor icinde toplanacak.

Hedef pencere:

- sekmeli
- moduler
- Turkce
- validation odakli
- play mod live apply destekli

Onerilen sekmeler:

1. `Genel`
2. `Leveller ve Locale`
3. `Sekil Kutuphanesi`
4. `Gameplay Layout`
5. `Keyboard ve Safe Area`
6. `Score ve Impact`
7. `Ekonomi ve Store`
8. `Theme Paketleri`
9. `Test / Sandbox`
10. `Validation`
11. `Build ve Smoke`
12. `Telemetry ve LiveOps`

Tekli editorler bu noktada hemen silinmeyecek. Once:

- yeni toplu editor parity saglayacak
- tum isler yeni panelden yapilabilir hale gelecek
- sonra eski editorler `deprecated` ilan edilecek
- en son sadece kullanilmayanlar temizlenecek

Bu sira teknik borcu azaltir.

### Faz 4 - Happy Path / Edge Case Tarama ve Bug Kilidi

Editor kilitlendikten sonra sistemsel tarama yapilacak.

Adimlar:

1. olasi bug listesi cikacak
2. kullanici manuel yeniden uretmeye calisacak
3. yeniden uretilen buglar fixlenecek
4. yeniden uretilemeyen ama riskli kalan alanlar icin hedefli test yazilacak

Burada tum sisteme yaygin agir test yazilmayacak. Yalnizca hedefli test yazilacak:

- unit test: saf hesaplama ve karar kurallari
- integration test: save/restore, popup restore, sandbox switch, content parity
- smoke checklist: APK build, scene acilisi, store, locale, continue/result flow

Kural:

- testler silinmeyecek
- ama runtime performansini etkilemeyecek sekilde editor/test assembly'lerinde pasif tutulacak

### Faz 5 - Gorsel Tasarim ve Sayfa Tasarimlari

Bug kilidi sonrasinda su yuzeyler tasarlanacak:

- gameplay
- main menu
- store
- fail modal
- result
- info card
- level select

Bu asamaya kadar tum layout tuning editorleri hazir olmus olmali. Boylece ince ayarlari editor uzerinden yapabileceksin.

### Faz 6 - Ses, Animasyon ve Oynanis Hissiyati

Bu faza gecmeden once editor icinde su tuning alanlari hazir olmus olmali:

- hit ses zinciri
- perfect/good/miss varyasyonlari
- shake/haptic/pulse
- hit-stop
- wrong-slot break hissi
- UI transition timing
- glow/bloom/contrast yogunlugu

Bu ayarlar da tek editor icinde veri odakli tutulacak. Boylece his ayarlari icin tekrar tekrar kod degistirmek gerekmeyecek.

### Faz 7 - 4 Dil / 25 Level Alpha Demo Lock ve Optimizasyon

Bu fazin cikti hedefi:

- TR / EN / DE / ES
- 25 level
- gameplay, store, fail, result, info card stabil
- Android cihazda stabil calisma

Bu fazda optimizasyon su sirayla yapilacak:

1. base APK boyut olcumu
2. scene acilis sureleri
3. GC / allocation takibi
4. texture, audio ve prefab agirlik taramasi
5. gereksiz Resources kullanim taramasi
6. editor-only asset sizinti kontrolu
7. Addressables/remote theme hazirlik taramasi

Optimizasyon checklist'i:

- Editor kodu build'e girmiyor mu
- shape ref PNG'leri build'e girmiyor mu
- gereksiz locale veya debug veri ship edilmiyor mu
- sandbox/test asset'leri player'a girmiyor mu
- remote indirilebilir olacak seyler base APK'de mi

### Faz 8 - Telemetry, Hotfix ve Metrik Altyapisi

Bu faz alpha demo mantigi oturduktan sonra acilacak.

Hedef:

- oyunu yormayan minimum runtime telemetry
- uzaktan config guncelleme
- rollback / kill switch
- hotfix benzeri davranis degisimi
- build degistirmeden ayar guncelleyebilme

Bu fazda ilk kurulacak seyler:

1. `event schema`
2. `config manifest`
3. `feature flag` sistemi
4. `local event buffer`
5. `batch upload`
6. `hotfix rollback` stratejisi

Toplanacak minimum metrikler:

- app start
- session start/end
- locale secimi
- level start
- fail nedeni
- continue kullanimi
- retry kullanimi
- result ekranina ulasim
- level complete
- store visit
- offer click
- coin earn/spend
- ad prompt gorme
- fake/gercek rewarded sonucu

Bu fazdan sonra telemetry paneli toplu editorun icine baglanacak.

### Faz 9 - Web Dashboard ve Bulut Tabanli Ayar Sistemi

Bu fazda editorun cloud versiyonu kurulacak.

Hedef:

- tum runtime tuning verilerini web panelden yonetebilmek
- oyuncunun telefonunda oyun kurulu iken ayarlari degistirebilmek
- theme paketleri, ekonomi, locale icerik, layout tuning gibi data-driven alanlari uzaktan guncelleyebilmek

Bu noktada Unity editor icindeki toplu panel, cloud panelin authoring referansi olacak. Sonra gerekli alt paneller web'e tasinacak.

### Faz 10 - Markete Cikisa Hazir Altyapi

Bu faz sonunda oyun hala alpha demo olabilir ama su farkla:

- mimari markete cikis hazir olur
- telemetry/hotfix/live config yolu kurulmus olur
- tema paketleri sonra da rahat eklenebilir
- ekonomiyi buluttan yonetme altyapisi hazir olur
- Play Billing ve store fiyat gecisi icin yer acik olur

### Faz 11 - Pre-Market Risk Minimization Pass

Bu faz alpha demo bittikten ve su katmanlar kurulduktan sonra acilacak:

- birlesik editor
- telemetry
- hotfix/live config
- web dashboard
- remote theme/content operasyonu

Bu fazin amaci yeni ozellik eklemek degil, markete cikmadan once birikmis mimari riskleri kontrollu sekilde dusurmektir.

Bu fazda yapilacak isler:

1. `FindObjectOfType / GameObject.Find` baglantilarini azaltmak
2. `GameManager / LevelFlowController` uzerindeki orkestrasyon yukunu parcali hale getirmek
3. telemetry queue ve snapshot dosyalarini rotation/cap mantigina almak
4. sandbox snapshot'larini runtime asset yerine editor/debug odakli saklama modeline tasimak
5. localization metinlerini daha merkezi bir tabloya veya locale key modeline toplamak
6. runtime `Resources.Load` bagimliliklarini theme/content bazli provider mantigina tasimak
7. editor/runtime asmdef sinirlarini netlestirmek
8. market build icin editor/test/debug katmanlarini release-safe moda almak

Bu faz marketten once yapilacak cunku:

- alpha demo doneminde hiz daha onemlidir
- market oncesinde ise kirilma maliyeti artar
- o noktada yeni feature yerine stabilizasyon daha dogru yatirim olur

---

## 4. Birlesik Editor Icin Nihai Kapsam

Toplu editor icinde finalde bulunmasi gereken moduller:

### 4.1 Content

- level olustur / kopyala / sil
- soru / cevap / info card
- 4 dil locale icerigi
- difficulty / rhythm / shape atama
- toplu level araligi islemleri

### 4.2 Shape

- referans gorsel atama
- otomatik shape uretimi
- manuel point duzenleme
- plaque angle duzenleme
- slot sayisi arttirma/azaltma
- gameplay fit kontrolu
- plaque visual adaptasyon tuning
- shape onizleme
- level araligina shape atama

### 4.3 Gameplay Layout

- rotator alani
- launcher alani
- top bar
- question panel
- fail/info/result panel konumlari
- bottom bar

### 4.4 Keyboard ve Mobile

- locale bazli keyboard tuning
- safe area margin
- oran profilleri
- keyboard dock

### 4.5 Score ve Feel

- score tuning
- multiplier tuning
- impact/juicy tuning
- hit audio event degerleri
- shake/haptic degerleri

### 4.6 Economy ve Store

- level coin
- replay coin
- star kurallari
- theme offer metadata
- preview price
- default/free/premium sandbox

### 4.7 Theme Paketleri

- theme id
- hangi asset setlerini kullandigi
- keyboard skin
- pin skin
- rotator skin
- panel style
- SFX set
- VFX set
- UX/UI renk/fill/font metadata

### 4.8 Validation ve Build

- validation raporu
- eksik locale
- duplicate id
- shape/cevap slot uyumsuzlugu
- build smoke araclari
- Android device test yardimcilari

### 4.9 Telemetry ve LiveOps

- event schema editoru
- config manifest editoru
- feature flag editoru
- remote override preview
- son sync/version durumu

---

## 5. Oyun Dosyalarinin Sismemesi Icin Teknik Kurallar

### 5.1 Authoring asset ayrimi

Asagidaki seyler runtime'a girmemeli:

- shape referans PNG'leri
- editor cache dosyalari
- draft backup'lar
- validation ciktilari
- telemetry raw export kopyalari

Bunlar `Editor` veya runtime disi authoring klasorlerinde tutulacak.

### 5.2 Theme paketleri base APK'ye gomulmeyecek

Tema paketleri gelecekte su mantikla yonetilmeli:

- base APK sadece temel alpha content ve base theme icermeli
- yeni tema paketleri `remote Addressables / AssetBundles` mantigiyla indirilmeli
- metadata base uygulamada olabilir, ama buyuk assetler uzaktan gelmeli

Bu sayede tema paketi:

- ses
- animasyon
- pin
- keyboard
- rotator
- UI

eklenirken APK sismez.

### 5.3 Telemetry veri temizligi

Telemetry icin:

- local queue boyutu capped olacak
- belirli limitten sonra eski ham veri silinecek
- batch upload sonrasi ack alinan veri temizlenecek
- günlük/haftalik/aylik archive export alinan veriler cihazdan temizlenebilecek

Bu mantikla hem veri saklanir hem oyun ici dosya birikmez.

### 5.4 Test kodu ve debug katmani

- unit/integration test kodlari player build'e girmeyecek
- debug overlay'ler release'te pasif kalacak
- fake ad/test sandbox katmani runtime'a minimum iz birakacak

Bu sayede ileride tekrar test icin saklanir, ama oyuncuya maliyeti cok dusuk olur.

### 5.5 Authoring ve runtime ayrimi icin zorunlu klasor kurali

Alpha demo suresince bu kurala ozellikle dikkat edilecek:

- runtime tarafinda gereken JSON, ScriptableObject ve minimal metadata ayri tutulacak
- editor preview, referans PNG, draft backup, validation dump ve gecici ciktilar ayri tutulacak
- ayni veri hem runtime hem authoring tarafinda gerekiyorsa, runtime'in kullandigi minimal model ayri tutulacak

Bu kural su iki sorunu bastan engeller:

- APK'ye yanlislikla authoring asset girmesi
- editor araci buyudukce runtime content'in de gereksiz buyumesi

---

## 6. Buyume Riskleri ve Simdiden Uygulanacak Koruma Kurallari

Bu bolum, bug raporu degil; mevcut sistem buyudukce ortaya cikmasi en muhtemel yuk noktalarini ve alpha demo suresince bunlara karsi nasil calisacagimizi listeler.

### 6.1 Runtime baglanti kirilma riski

Mevcut risk:

- bazi presenter ve manager'lar hala `FindObjectOfType` ve `GameObject.Find` kullaniyor
- yeni sahne, prefab veya sayfa geldikce sessiz kirik uretme riski var

Alpha demo suresince uygulanacak kural:

- yeni eklenen hicbir sistemde ad/isim aramasi ile baglanti kurulmayacak
- yeni UI yuzeyleri inspector referansi, bootstrap kaydi veya acik servis baglantisi ile calisacak
- mevcut `Find` kullanimlari izleme listesine alinacak, yeni kullanim eklenmeyecek

Market oncesi en saglikli azaltma yolu:

- tek tek butun `Find` kullanimlarini kaldirmak yerine
- once en cok sahneye bagli olanlari adapter/registry modeliyle degistirmek
- ardindan runtime UI binder veya scene reference registry olusturmak

### 6.2 Asiri merkezilesmis gameplay orkestrasyonu

Mevcut risk:

- `GameManager` ve `LevelFlowController` birden fazla sorumluluk tasiyor
- yeni popup, yeni level rule, yeni progression adimi bu siniflarda yigilabilir

Alpha demo suresince uygulanacak kural:

- yeni mekanik eklenecekse once veri/profil seviyesi dusunulecek
- `GameManager` icine yeni branch eklemek yerine mevcut event akisini kullanan kucuk siniflar tercih edilecek
- yeni UI state'leri icin dogrudan `GameManager` icine gomulu mantik yazilmamaya calisilacak

Market oncesi en saglikli azaltma yolu:

- `completion flow`, `fail flow`, `session restore`, `question progression` gibi akislari ayri orchestrator siniflarina bolmek
- bu bolme marketten hemen once degil, liveops katmanlari oturduktan sonra kontrollu yapilmali

### 6.3 Save ve telemetry dosya buyume riski

Mevcut risk:

- save ve telemetry dosyalari tam JSON rewrite ile yaziliyor
- telemetry queue dogal cap/rotation olmadan buyuyebilir

Alpha demo suresince uygulanacak kural:

- telemetry event seti minimum tutulacak
- her yeni event icin "buna gercekten ihtiyac var mi" sorulacak
- debug odakli fazla ham veri toplanmayacak
- queue boyutu ve dosya buyuklugu editor uzerinden gozlemlenecek

Market oncesi en saglikli azaltma yolu:

- queue cap
- batch upload
- ack sonrasi trim
- archive/export
- eski veriyi otomatik silme
- snapshot agirligini azaltma

### 6.4 Sandbox save snapshot riski

Mevcut risk:

- free/premium test snapshot'lari profile asset icinde string JSON olarak tutuluyor
- save modeli buyudukce bu asset buyur ve kirli hale gelir

Alpha demo suresince uygulanacak kural:

- sandbox modlari aktif kullanilacak ama kalici operasyon araci gibi buyutulmeyecek
- yeni buyuk save alanlari eklerken sandbox snapshot maliyeti not edilecek

Market oncesi en saglikli azaltma yolu:

- test snapshot'larini editor/debug klasorundeki ayri dosyalara veya debug save slot mantigina tasimak
- production runtime asset'inde buyuk JSON snapshot tutmamak

### 6.5 Resources tabanli content ve theme yukleme riski

Mevcut risk:

- runtime `Resources.Load` ile tema, prefab, audio, sprite ve config cekiyor
- tema sayisi ve uzaktan guncellenebilir asset sayisi arttikca bu model sertlesir

Alpha demo suresince uygulanacak kural:

- yeni runtime asset eklenirken resource path standardi bozulmayacak
- tema metadata ile theme agir assetleri zihinsel olarak ayri dusunulecek
- editor tarafinda "build'e girmemesi gereken asset" listesi korunacak

Market oncesi en saglikli azaltma yolu:

- metadata provider ile asset provider'i ayirmak
- base tema metadata local kalirken remote theme bundle sistemine gecmek
- Addressables veya remote bundle indirme stratejisini o noktada aktiflestirmek

### 6.6 Localization daginikligi riski

Mevcut risk:

- dil degisen bazi metinler presenter icinde switch bloklariyla yaziliyor
- yeni sayfa ve yeni UI arttikca tutarsizlik cikar

Alpha demo suresince uygulanacak kural:

- yeni eklenen her sabit UI metni locale key mantigina yakin dusunulecek
- presenter icinde sert string kopyalamasi artirilmayacak

Market oncesi en saglikli azaltma yolu:

- ortak UI locale tablosu veya key bazli localization resolver katmani eklemek
- store/result/fail/info/gameplay ortak etiketlerini merkezi hale getirmek

### 6.7 Shape preview/runtime parity riski

Mevcut risk:

- shape editor preview, geometry cozumleme ve runtime plaque sunumu ayni mantigi izlemek zorunda
- yeni shape turleri ve yeni plaque skinleri geldikce parity bozulabilir

Alpha demo suresince uygulanacak kural:

- yeni shape edit islemlerinde preview ve runtime birlikte kontrol edilecek
- shape visual adaptasyon katmani mekanikten ayrik tutulmaya devam edecek
- hit zone mantigina gorsel nedenlerle dokunulmayacak

Market oncesi en saglikli azaltma yolu:

- preview ve runtime'in kullandigi geometry hesaplarinin tek utility katmanda kalmasini zorunlu tutmak
- shape validation'a overlap/oran/slot-yuzey uyarilari eklemek

### 6.8 Theme runtime presentation yuk riski

Mevcut risk:

- `ThemeRuntimeController` palette, sprite, audio ve coroutine tabanli flash islerini tek yerde tasiyor
- tema ve hissiyat karmasiklastikca bu sinif buyuyebilir

Alpha demo suresince uygulanacak kural:

- yeni effect eklenecekse once tuning profiline alinacak
- sinifa yeni sabit degerler gommek yerine profile baglanacak

Market oncesi en saglikli azaltma yolu:

- audio, visual flash ve palette uygulamasini ayri presenter'lara bolmek
- hit basi coroutine sayisini azaltacak pooled veya lightweight efekt modeline gecmek

### 6.9 Store pricing gecis riski

Mevcut risk:

- preview pricing abstraction var ama gercek billing provider henuz yok
- bu geciste UI'ya yeni logic sizdirma riski var

Alpha demo suresince uygulanacak kural:

- store UI icine "hangi provider" bilgisi yazilmayacak
- tum fiyat sorgusu tek manager katmanindan gececek

Market oncesi en saglikli azaltma yolu:

- `StorePricingManager` icine provider secimi koymak
- `PreviewStorePricingProvider` ile `GooglePlayStorePricingProvider` ayni arayuzu implement etmeli
- presenter degismeden kalmali

### 6.10 Editor buyudukce derleme ve bagimlilik riski

Mevcut risk:

- asmdef sinirlari gorunur degil
- tek editor buyudukce compile suresi ve bagimlilik karmasasi artabilir

Alpha demo suresince uygulanacak kural:

- yeni editor modulleri editor klasorunde kalacak
- runtime'a editor utility sizdirilmayacak
- tek editor shell kurarken moduller ayrik siniflarda tutulacak

Market oncesi en saglikli azaltma yolu:

- editor asmdef
- runtime asmdef
- test asmdef
- liveops/editor-only asmdef

sinirlarini netlestirmek

---

## 7. Telemetry ve Hotfix Icin Onerilen Dis Servisler

Bu kisim final canli operasyon hedefi icin oneri stack'tir.

### 6.1 Ana oneri stack

En dengeli yapi:

- `Supabase`
  - auth
  - Postgres
  - storage
  - edge functions
- `Cloudflare R2`
  - remote config snapshotlari
  - remote theme bundle metadata
  - buyuk dosya / bundle dagitimi
- `Cloudflare CDN`
  - config ve asset cache
- `Sentry`
  - crash / exception tracking
- `Next.js + TypeScript`
  - web dashboard

Bu secim neden dengeli:

- Firebase gibi buyuk tek paket bagimliligina mahkum kalinmaz
- web panel tarafi esnek kurulur
- Unity runtime tarafinda sadece ince REST/JSON istemcisi yeterli olabilir
- buyuk assetleri ayri CDN uzerinden verebiliriz

### 6.2 Neden her seyi Firebase yapmiyoruz

Firebase kullanilabilir, ama bu projede tek secenek olmak zorunda degil.

Firebase'in guclu oldugu alanlar:

- Crashlytics
- Analytics
- Remote Config

Ama bu projede amac:

- tamamen bize ozel tuning/dashboard duzeni
- theme metadata
- remote content operasyoni
- canli editor paneli

Bu nedenle tek basina Firebase yerine daha esnek bir backend + CDN modeli daha dogru olur.

Istersek yine su hibrit modeli de kullanabiliriz:

- `Firebase Crashlytics`
- `Supabase + Cloudflare`

Bu da makul bir secimdir.

### 6.3 Web panel icin neden Next.js

Next.js secim sebebi:

- admin panel ve dashboard icin uygun
- auth, server action, API route mantigi rahat
- Supabase ile uyumlu
- deployment kolay

Alternatif:

- `React + Vite` de olur

Ama operasyon paneli icin Next.js daha duzenli olur.

### 6.4 AI analiz icin en optimal secenek

AI'yi oyun istemcisine koyma.

En dogru model:

- telemetry verisi cloud'a gider
- haftalik/gunluk export veya query ile toplanir
- AI analizi web panel disinda server-side veya dis analiz aracinda yapilir

Ilk asama icin en saglikli yol:

- telemetry verisini tablo halinde sakla
- web panelden CSV/JSON export ver
- AI analizini server-side batch veya dis asistan oturumunda yap

Finalde istenirse:

- OpenAI tabanli server-side analiz
- anomali tespiti
- retention/funnel yorumlama
- economy tuning oneri raporu

eklenebilir.

Bu sayede:

- oyun client'i sismez
- runtime AI maliyeti olmaz
- analiz kalitesi daha yuksek olur

---

## 8. Hotfix ve Canli Ayar Degisikligi Nasil Calisacak

### 7.1 Config manifest mantigi

Uygulama acilisinda ve belirli guvenli noktalarda:

- `config version manifest` ceker
- yerel config versiyonu ile kiyaslar
- fark varsa yeni override paketini indirir
- guvenli apply kurallarina gore uygular

### 7.2 Guvenli apply anlari

Canli apply icin oncelikli guvenli anlar:

- app launch
- main menu
- level start
- level complete
- store acilisi

Oyun icinde frame ortasinda core state degistirmek dogru degil.

### 7.3 Hotfix kapsaminda neler degisebilir

- economy values
- feature flag
- store offer visibility
- localized copy
- theme aktifligi
- layout/keyboard tuning
- impact/feel data

### 7.4 Rollback

Her config paketi version'li olacak.

Olasi bozulmada:

- onceki versiyona tek tik rollback
- `kill switch` ile problemli feature kapatma
- `minimum safe config` fallback

Bu katman web panelin zorunlu parcasi olmali.

---

## 9. Theme Paketlerinin Gelecekte Rahat Eklenmesi Icin Kural

Tema paketi mantigi bugunden sabitlenmeli.

Her theme paket su parcalardan olusabilmeli:

- metadata
- color/style token set
- sprite set
- pin set
- rotator skin
- keyboard skin
- UI panel style
- audio set
- VFX set
- feel/profile override baglantilari

Bu theme paketlerin eklenmesi icin:

- base oyuna sadece metadata girebilir
- agir assetler remote paket olur
- editor icinde theme package authoring modulu olur
- web panelde theme aktif/pasif ve price/offer ayarlanabilir

Bu yapi kuruldugunda, alpha demo sonrasinda bile yeni tema eklemek kolaylasir.

---

## 10. Alpha Demo Surecinde Uygulanacak Calisma Disiplini

Bu plan artik sadece hedef listesi degildir. Alpha demo suresince gundelik kararlar su disipline gore alinacak:

1. yeni ayar gerekiyorsa once editor yuzeyi dusunulecek
2. yeni runtime branch gerekiyorsa once veri/profil ile cozulebilir mi bakilacak
3. yeni content veya shape eklendiginde preview ve runtime birlikte kontrol edilecek
4. yeni telemetry event eklenmeden once eventin maliyeti ve gerekliligi sorgulanacak
5. yeni asset eklenmeden once build'e girip girmemesi kararlastirilacak
6. yeni sayfa veya popup eklenirse localization ve mobile layout ayni anda dusunulecek
7. alpha suresince hiz onemli olsa da riskli buyume noktalarina yeni teknik borc eklenmeyecek

Bu disiplinin pratik anlami:

- "simdilik boyle olsun" ile runtime'a yeni bagimlilik gommekten kacinmak
- editor ve data-driven yuzeyleri one almak
- market oncesi buyuk refactor ihtiyacini simdiden azaltmaktir

## 11. Her Faz Icin Cikis Kriterleri

### Faz 1-3 cikis kriteri

- tum tuning listesi cikmis
- eksik editor modulleri yazilmis
- tum isler yeni toplu editorden yapilabiliyor
- eski editorler deprecated edilmis

### Faz 4 cikis kriteri

- muhtemel bug listesi cikmis
- manuel yeniden uretilebilen kritik buglar fixlenmis
- hedefli testler yazilmis ve calisiyor

### Faz 5-6 cikis kriteri

- gameplay ve sayfalar kilitli
- feel/ses/animasyon tuning editorlerden yapilabiliyor
- visual ve juicy pass data-driven durumda

### Faz 7 cikis kriteri

- 4 dilde 25 level alpha stabil
- Android smoke ve optimizasyon checklist'i geciyor
- APK boyutu kabul edilebilir sinirda

### Faz 8 cikis kriteri

- telemetry schema kilitli
- config manifest sistemi calisiyor
- kill switch ve rollback var
- event upload oyunu yormuyor

### Faz 9-10 cikis kriteri

- web panelden config guncellenebiliyor
- runtime bunu versiyonlu ve guvenli sekilde aliyor
- theme paketleri uzaktan yonetilebiliyor
- markete cikis icin gerekli canli operasyon altyapisi hazir

---

## 12. Alpha Demo Sonrasi Market Oncesi Risk Minimizasyonu Nasil Yapilacak

Alpha demo bittikten, editorler ve canli katmanlar oturduktan sonra markete cikmadan once en saglikli yol su olacak:

### 12.1 Once olcum

- build boyutu
- startup suresi
- scene acilis suresi
- telemetry queue boyutu
- save dosyasi boyutu
- runtime allocation noktasi
- remote config apply gecikmesi

olculecek.

### 12.2 Sonra hedefli sertlestirme

Butun sistemi tekrar yazmak yerine yalnizca yuksek riskli alanlara mudahale edilecek:

- `Find` bagimliliklari azaltma
- telemetry trim/rotation
- sandbox snapshot tasima
- pricing provider gecisi
- theme remote asset modeli
- localization merkezi resolver
- asmdef ayirma

### 12.3 Sonra release-safe mod

- test panelleri pasif
- debug overlay'ler kapali
- fake ad presenter release kapali
- preview pricing kapali
- remote kill switch aktif
- min safe config paket hazir

### 12.4 Sonra canli operasyon guvencesi

- rollback denenmis olacak
- config versioning denenmis olacak
- fail-safe local config mevcut olacak
- network yoksa oyun local fallback ile acilacak
- theme paketi bozuksa base theme'e donecek

Bu sira en saglikli yol cunku:

- once olcum olmadan refactor dogru hedefe gitmez
- once sertlestirme olmadan release-safe moda gecmek riskli olur
- rollback denenmeden canli ayar sistemi acmak dogru olmaz

## 13. Hemen Sonraki Uygulama Sirasi

Bu dokumana gore simdi yapilmasi gerekenler:

1. tum tuning yuzeylerinin tam taramasini cikar
2. eksik editor modullerini yaz
3. tek toplu editor shell'ine topla
4. authoring asset / runtime asset ayrimini netlestir
5. editor-only klasor ve asmdef temizligini yap
6. sonra bug taramasi ve hedefli testlere gec

Yani telemetry ve web panel bugunden dusunulmeli, ama kodlama olarak editor birlesimi ve bug kilidinden once ana odak olmamali.

---

## 14. Son Not

Bu planin ana hedefi sadece alpha demo cikarmak degildir.

Asil hedef:

- alpha demo asamasinda bile mimariyi dogru kurmak
- markete cikis istendiginde altyapiyi yeniden yazmamak
- tum kritik ayarlari editor ve daha sonra web panel uzerinden yonetebilir hale gelmek
- bunu yaparken APK boyutunu, runtime performansini ve mekanik stabiliteyi korumaktir

Bu plan izlenirse:

- once editor ve hardening kilitlenir
- sonra tasarim ve content hizlanir
- sonra telemetry/liveops katmani guvenli sekilde eklenir
- en sonda da oyun markete hizli cikabilecek bir omurgaya sahip olur
