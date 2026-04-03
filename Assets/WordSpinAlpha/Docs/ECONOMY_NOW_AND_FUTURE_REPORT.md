# Economy Simdi Ve Gelecek Raporu

## Amac

Bu dokumanin amaci iki seyi ayni anda sabitlemektir:

1. Bugun projede kurulu olan ekonomi, reklam, store, test sandbox ve fiyat preview katmanlarini net sekilde belgelemek.
2. Market oncesi nihai surume gecilirken hangi kod katmaninin korunacagini, hangisinin kapatilacagini ve bolgesel Play Store fiyatlandirmasina nasil gecilecegini netlestirmek.

Bu dosya hem bugunku referans raporu, hem de gelecekte baska bir modelin veya yeni bir oturumun projeyi hizli anlamasi icin teknik yol haritasidir.

---

## Temel Karar

Ekonomi sistemi iki ayri katmanda kurgulandi:

- `Default` modu gercek gelistirme ve gercek runtime ekonomisidir.
- `FreePlayer` ve `PremiumPlayer` modlari yalnizca test sandbox modlaridir.

Bu ayrim bilincli yapildi. Nedenleri:

- Free ve premium oyuncu akislarini gorebilmek.
- Ana runtime ekonomisini test denemeleri ile kirletmemek.
- Market oncesi nihai ekonomi degerlerini `Default` ustunde ayarlayabilmek.
- Gerekirse release build oncesi test modlarini kapatip sadece `Default` ile ilerleyebilmek.

---

## Kurulu Ana Sistemler

### 1. Gercek ekonomi katmani

Ana ekonomi davranisi su dosyalar ustunde kuruludur:

- `Assets/WordSpinAlpha/Scripts/Core/EconomyBalanceProfile.cs`
- `Assets/WordSpinAlpha/Scripts/Core/LevelEconomyManager.cs`
- `Assets/WordSpinAlpha/Scripts/Core/EconomyManager.cs`

Bu katmanin sorumluluklari:

- level bazli coin odulu
- first clear / replay ayrimi
- yildiz -> coin carpani
- continue sonrasi yildiz limiti
- premium membership coin carpani
- theme coin fiyati
- gameplay coin hook ve yakinda teaser gorunurlugu

### 2. Test sandbox katmani

Bu katman ana runtime mantigini dagitmadan ayri tutulur:

- `Assets/WordSpinAlpha/Scripts/Core/TestPlayerModeProfile.cs`
- `Assets/WordSpinAlpha/Scripts/Core/TestPlayerModeManager.cs`
- `Assets/WordSpinAlpha/Scripts/Presentation/DebugRewardedAdPresenter.cs`

Bu katmanin sorumluluklari:

- `Default / FreePlayer / PremiumPlayer` modlarini tutmak
- sandbox save snapshot izolasyonu
- free oyuncu akisini test etmek
- premium oyuncu akisini test etmek
- fake rewarded reklam countdown gostermek
- enerji / membership / question hearts gibi override'lari merkezden uygulamak

### 3. Editor katmani

Butun ekonomi tuning ve sandbox test akisi tek editor penceresi ustunden yonetilir:

- `Assets/WordSpinAlpha/Scripts/Editor/EconomyBalanceWindow.cs`

Bu pencere:

- `Default`, `FreePlayer`, `PremiumPlayer` ekonomi profillerini ayri ayri duzenler
- runtime'a hangi modun uygulanacagini belirler
- sandbox reset butonlarini sunar
- level bazli coin ayari yapar
- theme paket ayarlarini gunceller
- fiyat taslaklarini tutar
- basit ekonomi simulasyonu sunar

---

## Default, Free ve Premium Modlarin Tam Davranisi

### Default

`Default`, gercek oyun ekonomisidir.

Bu mod:

- ana save durumunu kullanir
- gercek ekonomi profilini kullanir
- test override uygulamaz
- nihai dengeleme icin kullanilacak ana moddur

Default modda yapilan ekonomi degisiklikleri, nihai oyunun davranisina en yakin referans olarak dusunulmelidir.

### FreePlayer

`FreePlayer`, free oyuncu akisini gormek icin test sandbox'idir.

Bu modda test edilebilecek alanlar:

- premium kapali akisi
- enerji limiti
- refill dakikasi
- rewarded continue zorunlulugu
- fake reklam countdown
- theme kilitli akisi
- coin ile unlock davranisi

### PremiumPlayer

`PremiumPlayer`, premium uye akisini gormek icin test sandbox'idir.

Bu modda test edilebilecek alanlar:

- premium membership aktif
- no ads aktif
- giris enerjisi bypass
- yuksek question hearts
- reklamsiz continue akisi
- premium davranisa gore store / fail flow

---

## Snapshot ve Veri Izolasyonu

Bu yapinin en kritik noktasi save izolasyonudur.

Her modun ayri snapshot verisi tutulur:

- default snapshot
- free snapshot
- premium snapshot

Bu alanlar su dosyada tutulur:

- `Assets/WordSpinAlpha/Scripts/Core/TestPlayerModeProfile.cs`

Calisma mantigi:

1. Kullanici editor penceresinde bir test modu secer.
2. Sadece dropdown degistirmek runtime'i etkilemez.
3. `Aktif Modu Kayda Uygula` butonuna basildiginda:
   - mevcut uygulanmis modun snapshot'i alinir
   - hedef modun snapshot'i geri yuklenir
   - hedef modun override'lari uygulanir
   - ilgili runtime manager'lar refresh edilir

Bu sayede:

- Free/Premium denemeleri `Default` save'ini kirletmez.
- Default moda donuldugunde gercek gelistirme durumu geri gelir.
- Sandbox modlar kendi ic test durumlarini koruyabilir.

---

## ActiveMode ve AppliedMode Ayrimi

Bu ayrim teknik olarak bilerek eklendi.

- `ActiveMode`: editorde secili mod
- `AppliedMode`: runtime'da gercekten calisan mod

Neden gerekli:

- Kullanici editorde farkli profillere bakarken runtime'in anlik bozulmasini engellemek
- Yalnizca bilincli olarak `Aktif Modu Kayda Uygula` dediginde davranisin degismesi

Bu mantik su dosyada merkezi olarak yonetilir:

- `Assets/WordSpinAlpha/Scripts/Core/TestPlayerModeManager.cs`

Runtime override sorgulari da buradan okunur:

- premium override
- no ads override
- energy kurallari
- question hearts override
- rewarded continue zorunlulugu
- fake reklam suresi

---

## Ekonomi Profil Asset Yapisi

Ayri asset mantigi kullanilir:

- `Resources/Configs/EconomyBalanceProfile`
- `Resources/Configs/EconomyBalanceProfile_Free`
- `Resources/Configs/EconomyBalanceProfile_Premium`

Bu sayede:

- `Default` = gercek oyun tuning profili
- `Free` = free sandbox tuning profili
- `Premium` = premium sandbox tuning profili

Bir sandbox profil ilk kez olusturulursa, default profilden klonlanir. Boylece test profili sifirdan anlamsiz degerlerle baslamaz.

Bu davranis su dosyalarda vardir:

- `Assets/WordSpinAlpha/Scripts/Core/EconomyBalanceProfile.cs`
- `Assets/WordSpinAlpha/Scripts/Editor/EconomyBalanceWindow.cs`
- `Assets/WordSpinAlpha/Scripts/Core/LevelEconomyManager.cs`

---

## Editor Uzerinden Neler Yapilabiliyor

### Genel ekonomi ayarlari

- coin sadece ilk tamamlama verilsin mi
- varsayilan first clear coin
- varsayilan replay coin
- premium membership coin carpani

### Yildiz ve reklam ayarlari

- max stars
- 0 hata yildizi
- 1 hata yildizi
- 2+ hata yildizi
- continue sonrasi max yildiz
- 3 / 2 / 1 yildiz coin carpanlari
- reklam catch-up hook
- reklam sadece first clear mi
- catch-up hedef yildiz
- reklam bonus carpani

### HUD ve store hook ayarlari

- gameplay coin hook goster
- upcoming themes teaser goster

### Level bazli ayar

Level satirlari foldout yapidadir. Varsayilan acik gelmez. Her level icin:

- ilk tamamlama coin
- replay coin

ayarlanabilir.

### Theme paket ayarlari

Her theme offer icin:

- theme id
- coin fiyati
- coin ile alinabilir mi
- membership ile acilir mi
- dogrudan satin alinabilir mi
- iap tier id

### Bolgesel fiyat taslagi

Bu alan su an test ve planlama amaclidir.

Her satirda:

- region code
- currency code
- membership taslak fiyat
- theme pack taslak fiyat
- not

tutulur.

### Simulasyon

Yaklasik ekonomi hizi gormek icin editor icinde:

- test level
- yildiz sayisi
- first clear / replay secimi
- premium aktif mi
- gunluk tamamlama sayisi

ile tahmini kazanma hizi kontrol edilir.

---

## Test ve Reset Araclari

Editorde play modda kullanilabilen araclar:

- `Aktif Modu Kayda Uygula`
- `Puan ve Sonucu Sifirla`
- `Coinleri Sifirla`
- `Tema Kilitlerini Sifirla`
- `Uyelik Flaglerini Sifirla`
- `Enerjiyi Moda Gore Doldur`

Bu reset araclari su mantikla calisir:

- test akisini hizlandirir
- ana ekonomi kodunu dagitmaz
- sandbox denemelerinden sonra tekrar temiz baslangic verir

Puan ve sonuc reset'i icin:

- `Assets/WordSpinAlpha/Scripts/Core/ScoreManager.cs`

destegi eklendi.

Baslangic soft currency ve hints tekrar otomatik verilmesin diye:

- `Assets/WordSpinAlpha/Scripts/Core/PlayerSaveModels.cs`
- `Assets/WordSpinAlpha/Scripts/Core/EconomyManager.cs`

icinde `startingHintsGranted` ve `startingSoftCurrencyGranted` bayraklari eklendi.

Bu kritik bir hardening adimidir.

---

## Fake Rewarded Ad Kurulumu

Gercek reklam SDK'si henuz eklenmedi.

Onun yerine test icin:

- `Assets/WordSpinAlpha/Scripts/Presentation/DebugRewardedAdPresenter.cs`

dosyasi kullanilir.

Bu presenter:

- runtime'da kendi overlay'ini kurar
- countdown gosterir
- secili dile gore fake reklam metni yazar
- sure bitince success callback doner

Bu yapi ileride gercek rewarded ad entegrasyonuna gecis icin UI akis mantigini simdiden sabitler.

---

## Store, Fiyat Katmani ve Preview Sisteminin Kurulumu

### Su an kurulu fiyat altyapisi

Store tarafinda yeni bir fiyat abstraction katmani kuruldu:

- `Assets/WordSpinAlpha/Scripts/Services/IStorePricingProvider.cs`
- `Assets/WordSpinAlpha/Scripts/Services/StorePricingManager.cs`
- `Assets/WordSpinAlpha/Scripts/Services/PreviewStorePricingProvider.cs`

Store UI:

- `Assets/WordSpinAlpha/Scripts/Presentation/StorePresenter.cs`

### Katman yapisi

`StorePresenter` artik fiyat kaynagini dogrudan bilmez.

Onun yerine:

- `StorePresenter`
  -> `StorePricingManager`
  -> aktif fiyat provider

su an aktif fiyat provider:

- `PreviewStorePricingProvider`

### Preview fiyat mantigi

Test kolayligi icin preview eslemesi su sekilde yapildi:

- `tr -> TR`
- `en -> US`
- `de -> DE`
- `es -> ES`

Bu final ticari mantik degildir. Yalnizca test/preview katmanidir.

Store ekraninda artik:

- theme coin fiyati
- theme paket taslak fiyati
- premium membership taslak fiyati

gorulebilir.

Ayrica store icinde kullaniciya not olarak su mantik gosterilir:

- mevcut preview fiyatlar test amaclidir
- final surumde fiyat Play Store storefront bolgesinden gelecektir

---

## Nihai Surumde Bolgesel Fiyatlandirma Icin Dogru Yol

### En kritik karar

Final surumde fiyat dil bazli olmayacak.

Dogru kaynak:

- Google Play storefront bolgesi
- Play Billing product details

Yanlis model:

- `de dili seciliyse EUR goster`

Dogru model:

- kullanici hangi Play Store bolgesindeyse, o bolgenin para birimi ve formatted price degeri gosterilir

### Mevcut altyapi uyumlu mu

Evet, yapisal olarak uyumlu.

Neden:

- store UI fiyat kaynagini soyut arayuzden aliyor
- preview provider ayri
- manager katmani ayri
- store presenter fiyat kaynagina bagimli degil

Yani market oncesi geciste tum store ekranini yeniden yazmak gerekmez.

### Market oncesi eklenmesi gerekenler

1. Gercek satin alma servisi
   - `IPurchaseService` icin Google Play implementasyonu
2. Gercek fiyat servisi
   - `IStorePricingProvider` icin Google Play Billing implementasyonu
3. StorePricingManager delegasyonu
   - preview yerine gercek provider'a baglanacak
4. Play Console urun tanimi
   - premium membership
   - no ads
   - theme pack IAP urunleri

### Hedef final mimari

- `StorePresenter` degismeden kalir
- `StorePricingManager` icinden gercek provider secilir
- `PreviewStorePricingProvider` sadece debug/test fallback olarak kalabilir

---

## Store Catalog ve Membership Temeli

Store tarafinin veri omurgasi:

- `Assets/WordSpinAlpha/Resources/Configs/store_catalog.json`
- `Assets/WordSpinAlpha/Resources/Configs/membership_profile.json`

Bu dosyalarda:

- theme urun kimlikleri
- energy/hint urunleri
- premium membership urun kimligi
- no ads urun kimligi

gibi alanlar tutulur.

Bu iyi bir temel cunku product id mantigi zaten ayri tutulmus durumdadir.

---

## Runtime'a Baglanan Diger Dosyalar

Economy/test yapisini ayakta tutan diger onemli dosyalar:

- `Assets/WordSpinAlpha/Scripts/Core/SceneBootstrap.cs`
- `Assets/WordSpinAlpha/Scripts/Core/EnergyManager.cs`
- `Assets/WordSpinAlpha/Scripts/Core/QuestionLifeManager.cs`
- `Assets/WordSpinAlpha/Scripts/Presentation/FailModalPresenter.cs`
- `Assets/WordSpinAlpha/Scripts/Presentation/StorePresenter.cs`
- `Assets/WordSpinAlpha/Scripts/Core/SaveManager.cs`
- `Assets/WordSpinAlpha/Scripts/Core/PlayerSaveModels.cs`

Bootstrap icinde singleton olarak ayağa kaldirilan yeni katmanlar:

- `TestPlayerModeManager`
- `LevelEconomyManager`
- `DebugRewardedAdPresenter`
- `PreviewStorePricingProvider`
- `StorePricingManager`

---

## Editor Uzerinden Tipik Test Senaryolari

### Senaryo 1: Free oyuncu akisi test etme

1. `Ekonomi Denge Editoru` ac.
2. `Duzenlenen Ekonomi Profili = FreePlayer`.
3. Istedigin free ekonomi ayarlarini yap.
4. `Aktif Test Modu = FreePlayer`.
5. `Aktif Modu Kayda Uygula`.
6. Gameplay, fail, continue, rewarded countdown ve store akisini test et.

### Senaryo 2: Premium oyuncu akisi test etme

1. `Duzenlenen Ekonomi Profili = PremiumPlayer`.
2. Premium override degerlerini ayarla.
3. `Aktif Test Modu = PremiumPlayer`.
4. `Aktif Modu Kayda Uygula`.
5. Premium enerji, no ads, high hearts ve store davranisini test et.

### Senaryo 3: Gercek economy tuning'e donme

1. `Duzenlenen Ekonomi Profili = Default`.
2. Nihai oyun ekonomisini burada ayarla.
3. `Aktif Test Modu = Default`.
4. `Aktif Modu Kayda Uygula`.

Bu durumda free/premium sandbox etkileri kapanir ve gercek gelistirme ortamina donulur.

### Senaryo 4: Sandbox profilini default'a tasima

1. Free veya premium sandbox'ta begenilen degerleri ayarla.
2. `Secili Profili Su Moda Kopyala` ile hedef olarak `Default` sec.
3. `Profili Kopyala`.
4. Sonra runtime'da `Default` uygula ve gercek ekonomi gibi test et.

---

## Release Oncesi Kapatma Mantigi

Test katmanini silmek yerine kapatmak hedeflenmistir.

Release oncesi minimum uygulanacak davranis:

- `Aktif Test Modu = Default`
- runtime sadece `Default` ekonomi ile calisir
- preview fiyat katmani debug olarak kalabilir ama shipping build'de kullanilmayabilir

Istersek daha sert kapanis icin:

- ekonomi editorde test mode section gizlenebilir
- `StorePricingManager` sadece gercek billing provider'a baglanabilir
- `DebugRewardedAdPresenter` debug flag arkasina alinabilir

Ama bugunku mimaride ana hedef:

- test sistemini ana koda dagitmadan tutmak
- silmek yerine kapatabilmek

Bu hedef saglandi.

---

## Bilinmesi Gereken Sinirlar

Bu sistem bugun icin guvenli bir temel verir ama henuz final ticari entegrasyon degildir.

Su alanlar henuz final degildir:

- gercek Google Play Billing entegrasyonu
- gercek rewarded ad SDK entegrasyonu
- backend purchase validation
- final storefront bazli formatted price akisi
- nihai monetization degerleri

Bu karar bilincli alinmistir.

Amac:

- altyapiyi kurmak
- tuning ve test akislarini sabitlemek
- final ticari degerleri market oncesine kadar acik birakmak

---

## Gelecekte Bu Dokumanla Ne Yapilabilir

Bu rapor tekrar verildiginde baska bir model veya yeni bir oturum su sorulara dogrudan cevap verebilir:

- proje ekonomisi hangi dosyalarda
- default / free / premium ayrimi nasil calisiyor
- sandbox neden ana runtime'i kirletmiyor
- editor nasil kullaniliyor
- fake rewarded ad ne yapiyor
- store fiyat preview sistemi nasil calisiyor
- final Play Store bolgesel fiyata gecis icin ne eklenmeli
- hangi kod katmani korunup hangisi degistirilecek

Bu nedenle bu dokuman hem bugunku teknik rapor, hem de market oncesi gecis kilavuzu olarak tutulmalidir.
