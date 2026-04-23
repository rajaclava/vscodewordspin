# PNG Katmanli UI Tasarimlarini Unity Sahnesine Kurma Raporu

Tarih: 19 Nisan 2026  
Kapsam: HubPreview sahnesinde tek sayfa main menu tasariminin PNG katmanlariyla Unity UI olarak kurulmasi, 180 derece ters gorunme sorununun analizi ve sonraki tasarimlar icin standart pipeline.

## 1. Amaç

Bu dokumanin amaci, tasarim aracindan gelen tek sayfa UI tasarimlarini Unity'ye kontrollu, tekrar uretilebilir ve oyun mekaniklerini bozmadan tasimak icin net bir is akisi tanimlamaktir.

Bu rapor ozellikle su calismayi referans alir:

- Main menu tasarimi once tek parca referans PNG olarak incelendi.
- Ayni tasarim daha sonra arka plan, logo, dil secimi, rotator ve oyna butonu gibi ayri PNG katmanlari halinde Unity'ye alindi.
- Bu katmanlar gercek oyun akisini bozmamak icin `HubPreview` adli test/preview sahnesine kuruldu.
- Kurulum sirasinda tum ekranin 180 derece ters gorunmesi sorunu yasandi.
- Sorun analiz edildi. Ilk ara cozumde preview root'a 180 derece counter-rotation uygulanmis olsa da, gercek cihaz testi bunun dogru kalici cozum olmadigini gosterdi. Nihai kural: UI root'u cevrilmez, Android/app orientation dogru sabitlenir.

Bu dokuman, bundan sonra yeni main menu, level hub, store, profil, gorev veya teklif ekranlari tasarimdan Unity'ye aktarilirken ayni hatalarin tekrar edilmemesi icin kullanilmalidir.

## 2. Uygulanan Mevcut Kurulum

### 2.1 Kullanilan Sahne ve Dosya Mantigi

Kurulum gercek oyun akisi uzerine dogrudan yapilmadi. Bunun yerine test ve gorsel dogrulama icin ayri bir sahne kullanildi.

Kullanilan ana sahne:

```text
Assets/WordSpinAlpha/Scenes/HubPreview.unity
```

Kullanilan preview prefab:

```text
Assets/WordSpinAlpha/Generated/Prefabs/MainMenuPngPreview.prefab
```

Kullanilan editor builder:

```text
Assets/WordSpinAlpha/Scripts/Editor/WordSpinAlphaSceneBuilder.cs
```

Kullanilan sahne normalizer:

```text
Assets/WordSpinAlpha/Scripts/Editor/HubPreviewSceneNormalizer.cs
```

Kullanilan texture import ayari:

```text
Assets/WordSpinAlpha/Scripts/Editor/MainMenuPreviewTexturePostprocessor.cs
```

Bu ayrim bilincli yapildi. Amac, final oyun akisi onaylanmadan once tasarimi sahnede gormek, olculeri ayarlamak, PNG kalitesini kontrol etmek ve gerekirse prefab uzerinden ince ayar yapabilmekti.

### 2.2 Gelen PNG Katmanlari

Main menu tasarimi icin su mantik kullanildi:

- `mainmenu_reference.png`: Tasarimin tek parca referans hali. Unity sahnesinde birebir karsilastirma icin tutulur, runtime'da aktif UI gibi kullanilmaz.
- `arkaplan.png`: Tam ekran arka plan.
- `logo_crop.png`: WordSpin logo alani.
- `Dilsecimi_crop.png`: Dil secimi gorsel alani.
- `rotator_crop.png`: Rotator/ana gorsel alani.
- `playbutton_crop.png`: Oyna butonu gorsel alani.
- `PlayButton_Title`, `PlayButton_Subtitle`, `StartLevel_Label`: Bunlar gorsel PNG yerine TextMeshPro UI metni olarak sahnede uretilir.

Bu ayrim dogrudur. Cunku lokalizasyon, dinamik level numarasi, buton metni veya can/coin gibi degisen degerler PNG olarak kilitlenmemelidir. Sabit dekoratif kisimlar PNG, degisebilir metinler Unity UI metni olmalidir.

### 2.3 PNG Import Ayarlari

`MainMenuPreviewTexturePostprocessor.cs` ile main menu preview klasorlerine giren PNG'ler otomatik olarak su ayarlara cekilir:

- Texture Type: `Sprite`
- Sprite Import Mode: `Single`
- Alpha Is Transparency: `true`
- Mipmap: `false`
- sRGB: `true`
- Compression: `Uncompressed`
- Standalone / Android / iPhone max texture size: `2048`

Bu ayarlar kaliteyi korumak icin dogrudur. UI PNG'lerinde mipmap genelde gereksizdir ve mobilde yazilar/gorsel kenarlari bulaniklastirabilir. Compression kapali tutuldugu icin preview kalitesi korunur. Final build optimizasyonunda buyuk tam ekran arka planlar icin platform bazli sikistirma tekrar degerlendirilebilir, ama tasarim dogrulama asamasinda kalite kaybi olmamasi onceliklidir.

### 2.4 Unity Icindeki Kurulum Mantigi

`WordSpinAlphaSceneBuilder.cs` icinde `BuildMainMenuPngPreviewPrefab` su yapida prefab uretir:

- Root: `MainMenuPngPreviewRoot`
- Root RectTransform:
  - Anchor Min/Max: merkez
  - Pivot: merkez
  - Size: `1024 x 1536`
  - Scale: `1,1,1`
  - Rotation: preview duzeltmesi icin root seviyesinde kontrol edilir
- Child katmanlar:
  - `Background`
  - `Logo`
  - `LanguageSelect`
  - `Rotator`
  - `PlayButton`
  - TMP metinleri

Child katmanlarin her biri merkez anchor/pivot ile kurulur. Bu sayede referans cozunurluk koordinatlari dogrudan sahneye aktarilabilir. Katmanlar tek tek ters cevrilmez, negatif scale verilmez, rastgele rotation uygulanmaz.

## 3. 180 Derece Ters Donme Sorunu

### 3.1 Belirti

Unity Simulator ve Game view icinde butun preview ekran 180 derece ters gorundu.

Belirtiler:

- `WordSpin` logosu bas asagi gorundu.
- `OYNA` butonu bas asagi gorundu.
- Dil secimi ters gorundu.
- Rotator ve arka plan da ayni sekilde ters gorundu.
- Sorun tek bir PNG'de degil, tum root hiyerarsisinde goruldu.

Bu belirti, tekil bir sprite import hatasindan cok root/canvas/transform seviyesinde bir yonelim problemi oldugunu gosterdi.

### 3.2 Kontrol Edilen Noktalar

Sorun cozulmeden once su alanlar kontrol edildi:

- Kaynak PNG'lerin kendisi: tasarim dosyalari normal yonelimdeydi.
- Proje orientation ayarlari: portrait yonelim aktifti, upside-down portrait kapaliydi.
- Prefab child katmanlari: child image ve TMP nesnelerinde negatif scale veya 180 derece rotation yoktu.
- Kamera: preview camera rotation normaldi.
- Canvas render mode: Screen Space Overlay kullaniliyordu.
- Scene instance override'lari: prefab root override'lari kontrol edildi.
- `HubPreviewCanvas` RectTransform degerleri: burada gecersiz/dejenere degerler bulundu.
- `HubPreviewSceneNormalizer`: acik sahneyi normalize ederken root rotation duzeltmesini geri silebilecek durumdaydi.
- Gercek cihaz APK manifest'i kontrol edildi ve `android:screenOrientation=0x9` uretildigi goruldu. Android'de `0x9 = reversePortrait` oldugu icin cihaz sistem arayuzunun ters portreye gecmesinin asil kok nedeni budur.

### 3.3 Kök Neden

Sorun tek bir yerden degil, iki ana durumun birlesiminden olustu.

Birinci durum:

`HubPreviewCanvas` sahne dosyasinda bozuk RectTransform degerleriyle serialize edilmisti.

Problemli degerler:

```yaml
m_LocalScale: {x: 0, y: 0, z: 0}
m_AnchorMax: {x: 0, y: 0}
m_Pivot: {x: 0, y: 0}
```

Canvas root icin `scale 0` ciddi bir risklidir. Screen Space Overlay ve CanvasScaler bazi durumlarda editor/runtime tarafinda bu degerleri override etse bile, sahne acilisinda ve Simulator hesaplamalarinda dejenere transform matrisi uretme riski vardir.

Ikinci durum:

Preview root uzerinde yapilan duzeltmeler generator ve normalizer tarafindan kalici hale getirilmiyordu. Yani elle yapilan duzeltme sahne yeniden acildiginda, normalize edildiginde veya rebuild calistiginda geri bozulabiliyordu.

Bu nedenle sadece Inspector'dan scale/pivot duzeltmek yeterli olmadi. Duzeltmenin hem builder koduna, hem prefab'a, hem sahne instance override'ina, hem de normalizer'a islenmesi gerekti.

### 3.4 Uygulanan Düzeltme

Canvas icin uygulanan duzeltme:

```text
HubPreviewCanvas
Scale: 1,1,1
AnchorMin: 0,0
AnchorMax: 1,1
Pivot: 0.5,0.5
AnchoredPosition: 0,0
SizeDelta: 0,0
Rotation: identity
```

Preview root icin nihai duzeltme:

```text
MainMenuPngPreviewRoot
Rotation Z: 0 derece
Scale: 1,1,1
Pivot: 0.5,0.5
Size: 1024 x 1536
```

Child katmanlar icin uygulanan kural:

```text
Child katmanlar identity rotation ve scale 1 kalir.
Terslik child seviyesinde cozulmez.
```

Bu onemlidir. Eger her child PNG tek tek cevrilseydi:

- Katman hizasi bozulabilirdi.
- Sonraki asset eklemelerinde tutarsizlik olusurdu.
- Buton hitbox'lari ile gorseller ayrisabilirdi.
- Normalizer veya rebuild sirasinda farkli katmanlar farkli davranabilirdi.

Ilk ara cozumde root seviyesinde 180 derece rotation uygulanmisti. Gercek cihaz testinde bu cozumun Android reverse-portrait problemini maskeledigi anlasildi. Nihai cozumde preview root rotation `0` kalir; yonelim problemi Android build orientation/manifest tarafinda cozulur.

Ilgili sabit:

```csharp
HubPreviewDisplayRotationZ = 0f
```

## 4. Neden Bu Çözüm Oyun Akışını Bozmaz

Bu islem yalnizca preview/test sahnesi icin yapildi.

Dokunulmayan alanlar:

- Gercek gameplay sahnesi
- Gercek hub akisi
- Gercek main menu akisi
- Save sistemi
- Economy sistemi
- Input sistemi
- Reklam/premium/store sistemleri
- Level ilerleme mekanikleri

Preview sahnesindeki tasarim, final onaydan sonra kontrollu sekilde gercek akisa tasinabilir. Bu asamada dogrudan production sahneye gomulmemesi dogru karardi.

## 5. Sonraki Tasarımlar İçin Optimal Pipeline

### 5.1 Tasarım Aracı Çıkış Standardı

Her yeni sayfa tasarimi icin tasarim aracindan su paket alinmalidir:

```text
screen_reference.png
background.png
decor_layer_01.png
decor_layer_02.png
primary_button.png
secondary_button.png
panel_main.png
panel_small.png
icon_*.png
README veya DESIGN notu
```

Zorunlu kurallar:

- Referans tasarim tek parca PNG olarak alinmali.
- Unity'de ayri kontrol edilecek her gorsel ayri PNG olarak alinmali.
- Buton gorseli ile buton metni mumkunse ayrilmali.
- Dinamik metinler PNG icinde kilitlenmemeli.
- Dil, coin, can, level numarasi, sure, kampanya metni gibi degisen alanlar TextMeshPro veya runtime UI ile yazilmali.
- Arka plan tam ekran ise referans cozunurlukle ayni oran ve kalitede verilmeli.
- Transparan PNG'lerde gereksiz bos canvas birakilmamali; gereksiz bosluk layout hesaplarini zorlastirir.
- Butonlar icin normal/pressed/disabled varyantlari varsa ayri dosya olarak alinmali.
- Animasyon yapilacak gorseller statik tek PNG yerine frame strip, spritesheet veya parca katmani olarak alinmali.

### 5.2 Dosya Yerleşim Standardı

Preview ve tasarim aktarimlari icin onerilen klasor yapisi:

```text
Assets/WordSpinAlpha/Art/UI/{ScreenName}/Source/
Assets/WordSpinAlpha/Art/UI/{ScreenName}/Cropped/
Assets/WordSpinAlpha/Generated/Prefabs/
Assets/WordSpinAlpha/Scenes/
```

Ornek:

```text
Assets/WordSpinAlpha/Art/UI/MainMenu/Source/mainmenu_reference.png
Assets/WordSpinAlpha/Art/UI/MainMenu/Source/arkaplan.png
Assets/WordSpinAlpha/Art/UI/MainMenu/Cropped/logo_crop.png
Assets/WordSpinAlpha/Art/UI/MainMenu/Cropped/playbutton_crop.png
```

Yeni ekran icin ayni mantik korunmalidir:

```text
Assets/WordSpinAlpha/Art/UI/LevelHub/Source/levelhub_reference.png
Assets/WordSpinAlpha/Art/UI/LevelHub/Source/arkaplan.png
Assets/WordSpinAlpha/Art/UI/LevelHub/Cropped/level_node.png
Assets/WordSpinAlpha/Art/UI/LevelHub/Cropped/stone_base.png
```

### 5.3 Unity Import Standardı

UI PNG dosyalari icin varsayilan ayarlar:

```text
Texture Type: Sprite
Sprite Mode: Single
Alpha Is Transparency: true
sRGB: true
Mipmap: false
Compression: Uncompressed
Max Size: 2048 veya ihtiyaca gore 4096
```

Final build optimizasyonunda:

- Tam ekran arka planlar icin platform bazli texture compression denenebilir.
- Buton, icon ve metin benzeri keskin UI assetlerinde agresif compression kullanilmamalidir.
- UI'da mipmap sadece o asset farkli olceklerde ciddi sekilde kullaniliyorsa dusunulmelidir.

### 5.4 Prefab Üretim Standardı

Her tasarim icin once preview prefab uretilmelidir.

Root prefab kurallari:

```text
Root name: {ScreenName}PngPreviewRoot
Anchor: center
Pivot: 0.5,0.5
Size: referans cozunurluk
Scale: 1,1,1
Rotation: normalde 0; yalniz preview terslik gerekiyorsa tek root sabiti ile yonetilir
CanvasGroup: interactable false, blocksRaycasts false
```

Child katman kurallari:

```text
Anchor: center
Pivot: 0.5,0.5
Scale: 1,1,1
Rotation: 0
RaycastTarget: false, eger sadece gorselse
RaycastTarget: true, eger gercek buton hitbox'i ise Button objesinde
```

Butonlarda ideal yapi:

```text
ButtonRoot
Image: PNG buton gorseli
Button component
TMP child text
Optional pressed animation script
```

### 5.5 Sahne Kurulum Standardı

Yeni bir tasarim ana oyuna direkt konmamalidir. Once izole preview sahnesi kurulmalidir.

Onerilen adimlar:

1. Yeni preview sahnesi olustur.
2. Sahneye sadece kamera, event system ve canvas koy.
3. Canvas'i Screen Space Overlay yap.
4. CanvasScaler reference resolution degerini tasarim referansina gore ayarla.
5. Preview prefab'i canvas altina instance olarak koy.
6. Game view ve Device Simulator ile kontrol et.
7. Referans tek parca PNG ile katmanli prefab'i gorsel olarak karsilastir.
8. Buton hitbox ve metin alanlarini ayri kontrol et.
9. Onaydan sonra production sahneye tasima planini ayri yap.

## 6. 180 Derece Terslik Yaşamamak İçin Kontrol Listesi

Yeni tasarim Unity'ye eklendiginde asagidaki kontrol mutlaka yapilmalidir.

### 6.1 Kaynak Dosya Kontrolü

- PNG dosyasini Unity disinda ac ve yonelimin dogru oldugunu dogrula.
- PNG'nin Photoshop/Figma/Stitch tarafinda canvas'i ters export edilmedigini kontrol et.
- Transparan PNG'lerde bos alanlarin bilincli oldugunu dogrula.
- Tek parca referans ile parca PNG'lerin ayni tasarim versiyonundan geldigini dogrula.

### 6.2 Import Kontrolü

- Texture Type Sprite mi?
- Alpha Is Transparency acik mi?
- Mipmap kapali mi?
- Compression preview asamasinda kapali mi?
- Sprite pivot merkez mi?
- Asset isimlerinde Turkce karakter, bosluk veya karisik surum ekleri var mi?

Not: Turkce karakter Unity icinde genelde calisir, ancak pipeline stabilitesi icin dosya adlarinda ASCII kullanmak daha sagliklidir. Ornek: `Dilsecimi_crop.png`, `level_node.png`, `play_button.png`.

### 6.3 Canvas Kontrolü

Canvas root icin asagidaki degerler dogru olmalidir:

```text
Scale: 1,1,1
Rotation: 0,0,0
AnchorMin: 0,0
AnchorMax: 1,1
Pivot: 0.5,0.5
SizeDelta: 0,0
```

Canvas uzerinde `scale 0`, negatif scale veya rastgele rotation kesinlikle birakilmamalidir.

### 6.4 Prefab Root Kontrolü

Prefab root icin:

```text
Scale: 1,1,1
Pivot: 0.5,0.5
Anchor: center veya ihtiyaca gore full stretch
Rotation: normalde 0
```

Eger bir preview sahnesinde tum ekran ters gorunuyorsa:

- Once kaynak PNG'yi kontrol et.
- Sonra Canvas root transformunu kontrol et.
- Sonra Camera orientation ve Project Settings orientation kontrol et.
- Sonra prefab root rotation kontrol et.
- Android APK'da manifest `screenOrientation` degerini kontrol et.
- `reversePortrait` gorulurse UI root'unu dondurme; build sirasinda `Portrait` orientation zorlanmali.

### 6.5 Normalizer / Rebuild Kontrolü

Her editor automation dosyasi ayni kurali bilmelidir.

Builder, prefab ve normalizer icin nihai kural:

- Preview root rotation `0` kalmali.
- Child katmanlar rotation `0`, scale `1` kalmali.
- Normalizer rotation'i `0` olarak korumali.
- Rebuild komutu Android orientation problemini UI rotation ile maskelememeli.
- Cihaz APK testi icin manifest `screenOrientation` degeri portrait olmalidir, reversePortrait olmamalidir.

Bu rapordaki en kritik ders budur: Unity'de bir sahne sorunu sadece Inspector'da duzeltilirse kalici olmayabilir. Duzeltme generator, prefab, scene override ve normalizer zincirinin tamaminda ayni kurala baglanmalidir.

## 7. Tasarım Kalitesi ve Performans İçin Kurallar

### 7.1 Tek Parça Görsel Ne Zaman Kullanılır

Tek parca PNG su durumlarda kullanilabilir:

- Sadece referans karsilastirmasi icin.
- Arka plan gibi hic tiklanmayan, hic degismeyen gorseller icin.
- Henuz hizli mockup yapiliyorsa.

Tek parca PNG su durumlarda ana UI olmamalidir:

- Butonlar tiklanacaksa.
- Metinler dinamikse.
- Dil destegi olacaksa.
- Can, coin, hint, level gibi degerler degisecekse.
- Animasyon veya pressed feedback olacaksa.

### 7.2 Katmanlı PNG Ne Zaman Kullanılır

Katmanli PNG su durumlarda en dogru tercihtir:

- Arka plan, logo, panel ve buton gorselleri ayri kontrol edilecekse.
- Buton pressed/hover/disabled efektleri eklenecekse.
- Sahnede parallax, scale, fade veya path animasyonu yapilacaksa.
- Level hub'da node'lar yol uzerinde buyuyup kuculecekse.

### 7.3 Runtime Metin Kuralı

Asagidaki metinler PNG icine gomulmemelidir:

- OYNA / DEVAM ET / TEKRAR BASLA gibi aksiyon metinleri
- Level numarasi
- Dil kodlari
- Can, coin, hint degerleri
- Store fiyatlari
- Promo/hediye metinleri
- Gorev ve etkinlik metinleri

Bunlar TextMeshPro ile yazilmali ve ileride localization/live config sistemine baglanabilmelidir.

### 7.4 Performans Kuralı

Mobil performans icin:

- UI'da gereksiz buyuk PNG kullanma.
- 1024 x 1536 preview icin dogru olabilir, ancak final cihaz hedefinde her asset tek tek optimize edilmelidir.
- Tam ekran arka plan tek draw call mantigiyla dusunulebilir.
- Cok sayida transparent full-screen overlay kullanilmamalidir.
- Buton/icon gibi kucuk gorseller mumkunse atlas mantigiyla toplanabilir.
- Animasyonlar icin her frame devasa PNG olmamalidir; spritesheet veya parca animasyonu tercih edilmelidir.

## 8. Level Hub Tasarımlarına Uygulama

Level secim hub tasarimi icin onerilen paket:

```text
levelhub_reference.png
background_path.png
level_node_unlocked.png
level_node_locked.png
level_node_current.png
level_node_completed.png
level_stone_base.png
play_button.png
bottom_nav_bg.png
bottom_nav_icon_journey.png
bottom_nav_icon_tasks.png
bottom_nav_icon_profile.png
bottom_nav_icon_store.png
top_bar_bg.png
currency_icon_coin.png
currency_icon_energy.png
currency_icon_hint.png
```

Level node sistemi icin:

- Yol arka plani sabit veya scrollable image olabilir.
- Level kutulari runtime'da veriyle uretilmelidir.
- Level kutusu PNG'si tek prefab olmalidir.
- Uzerindeki level numarasi TMP olmalidir.
- Star sayisi ya TMP/sembol ya da ayri icon prefab ile uretilmelidir.
- Scroll sirasinda buyume/kuculme etkisi kodla node scale uzerinden verilmeli, farkli boy PNG uretilmemelidir.
- Yol basindaki tas veya platform ayri PNG olarak kullanilabilir.

Bu sayede level sayisi arttiginda yeni gorsel uretmeden sistem calisir.

## 9. Buton Efekti ve Animasyon Standardı

Oyna butonu veya diger butonlar icin onerilen basim efekti:

```text
PointerDown: scale 0.96, renk hafif koyulasir
PointerUp: scale 1.0, renk normale doner
Click: kisa glow veya bounce
Disabled: alpha 0.55 veya grayscale alternatif
```

Bu efekt PNG'yi degistirmeden UI transform ve color uzerinden uygulanabilir. Performans yuk bindirmez.

Level node buyume/kuculme efekti:

```text
Ekran merkezine yaklasan node: scale 1.0 - 1.12
Uzaklasan node: scale 0.72 - 0.9
Kilitli node: alpha/renk ile ayrilir
Secili node: glow veya platform efekti alir
```

Bu efekt de runtime'da transform scale ve CanvasGroup alpha ile yapilmalidir. Her node icin ayri buyuk/kucuk PNG uretilmemelidir.

## 10. Hata Tekrarlanmaması İçin Net Kurallar

Asagidaki kurallar zorunludur:

1. Yeni tasarim once preview sahnesinde denenir, direkt production sahneye kurulmaz.
2. Her tasarim icin tek parca referans PNG zorunludur.
3. Her tiklanabilir veya animasyonlu parca ayri PNG veya runtime UI elementi olmalidir.
4. Degisebilir metin PNG icine gomulmez.
5. Canvas root asla `scale 0` veya negatif scale ile kalmaz.
6. Terslik gorulurse child katmanlar tek tek cevrilmez.
7. Terslik gorulurse root seviyesinde 180 derece rotation kalici cozum olarak kullanilmaz.
8. Builder, prefab, scene override ve normalizer ayni rotation/scale kurallarini kullanir: root rotation `0`, child rotation `0`.
9. Unity sahnesi disaridan degistiginde Editor "Reload" isterse, preview dogrulamasinda `Reload` secilir.
10. Rebuild Play Mode icinde calistirilmaz; gerekiyorsa Play Mode kapaninca otomatik rebuild kuyruga alinir.
11. Her gorsel aktarimindan sonra Game View ve Device Simulator beraber kontrol edilir.
12. Onaylanmayan preview sahnesi ana oyun akisi icine alinmaz.

## 11. Gelecek Tasarım Aktarımında Önerilen Adımlar

Yeni bir tasarim geldiginde uygulanacak pratik sira:

1. Tasarim paketini kontrol et: referans PNG ve katman PNG'leri var mi?
2. PNG'leri Unity disinda ac: yonelim ve alpha dogru mu?
3. Dosyalari `Assets/WordSpinAlpha/Art/UI/{ScreenName}/Source` ve `Cropped` altina yerlestir.
4. Import ayarlarinin otomatik dogru uygulanip uygulanmadigini kontrol et.
5. `{ScreenName}PngPreview.prefab` uret.
6. Ayrilmis preview sahnesine bu prefab'i koy.
7. Canvas ve prefab root transformlarini kontrol et.
8. Tek parca referans ile katmanli sahneyi gozle karsilastir.
9. Butonlar icin hitbox ve TMP metin yerlesimini dogrula.
10. Device Simulator portrait cihazda kontrol et.
11. Terslik veya kayma varsa once kaynak PNG, Canvas, prefab root ve Android manifest orientation degerlerini kontrol et.
12. Onaydan sonra production entegrasyonu icin ayri gorev ac.

## 12. Mevcut Durum Özeti

19 Nisan 2026 itibariyla:

- Main menu PNG katmanlari Unity'ye preview olarak kuruldu.
- `HubPreview` sahnesi ana oyun akisini etkilemeyecek sekilde ayrildi.
- 180 derece ters donme sorunu analiz edildi.
- Canvas'in gecersiz transform degerleri duzeltildi.
- Preview root icin izole 180 derece counter-rotation kuralı eklendi.
- Normalizer bu duzeltmeyi artik geri silmeyecek hale getirildi.
- PNG import ayarlari kalite kaybi yasatmayacak sekilde otomatiklestirildi.

Guncel cihaz testi notu: Bu maddelerdeki 180 derece counter-rotation, gercek cihaz testinden sonra kalici cozum olmaktan cikarildi. APK manifest'inde `reversePortrait` uretildigi goruldugu icin nihai cozum UI root'unu cevirmek degil, HubPreview-only Android build sirasinda `Portrait only` orientation zorlamaktir. Gecerli kural: preview root rotation `0`, child rotation `0`, Android manifest portrait.

Bu is akisi level hub ve sonraki UI sayfalarinda da kullanilabilir. Ancak production entegrasyonundan once her sayfa preview sahnesinde onaylanmali, sonra oyun veri akisi ve input sistemiyle kontrollu sekilde baglanmalidir.

---

## 13. 19 Nisan 2026 Ek Guncelleme - MainMenu Promote ve Level Hub Ray Editor

Bu bolum, ayni gun icinde yapilan ikinci UI/pipeline kararlarini kayda alir.

### 13.1 MainMenu tasariminin runtime akisa alinmasi

HubPreview uzerinde test edilen main menu PNG katmanli tasarimi, onaydan sonra runtime `MainMenu` sahnesine aktarildi.

Gecerli karar:

- `HubPreview` artik kalici production sahne degil, tasarim test alani olarak kalir.
- Onaylanan preview tasarimi kopyalanir/promote edilir.
- Kopya runtime sahneye baglanir.
- Orijinal `HubPreview` sonraki tasarim testleri icin sandbox olarak kullanilir.

Bu yaklasim, ana oyun akisini korurken tasarim iterasyonlarini hizlandirir.

### 13.2 Play butonu feedback ve gecis standardi

MainMenu `OYNA` butonunda asagidaki davranis kabul edildi:

- Unity `Button.onClick` gecikmeli click davranisi play hitbox icin kullanilmaz.
- `PointerDown` aninda basim feedback'i baslar.
- Hitbox tekrar tiklamayi engellemek icin kapatilir.
- Buton kisa sure basilir, sonra normale donmeye baslar.
- `Hub` gecisi bu release akisi baslarken tetiklenir.

Bu sayede oyuncu:

- bastigini gorur
- buton basili kalmis gibi beklemez
- Hub ekranina hizli gecer

### 13.2.1 MainMenu arka plan bosluk sorunu

MainMenu cihaz ve editor testlerinde, arka plan PNG'si 9:16 portrait oraninda olsa bile ust-alt kisimlarda bos bant kalabildigi goruldu.

Kok neden:

- aktif arka plan gorseli `Assets/WordSpinAlpha/Generated/Prefabs/MainMenuPngPreview.prefab` icinde sabit bir boyutta kaliyordu
- ekran orani, safe area ve canvas/prefab layout kombinasyonu bu sabit boyutu tam ekran kaplama olarak yorumlamiyordu
- bu nedenle kaynak PNG dogru oranda olsa bile runtime yerlesimde tam kaplama elde edilmiyordu

Kalici cozum:

- `Assets/WordSpinAlpha/Generated/Prefabs/MainMenuPngPreview.prefab` icindeki arka plan katmani bosluk kalmayacak sekilde buyutuldu
- ayni prefab icinde arka plan sprite baglantisi yeni gorselde korunarak gorsel kalite kaybi olmasi engellendi
- `Assets/WordSpinAlpha/Scripts/Editor/WordSpinAlphaSceneBuilder.cs` tarafindaki builder davranisi da ayni kaplama mantigina cekildi

Referans kural:

- Tam ekran arka plan icin sadece "PNG 9:16" kontrolu yeterli degildir.
- Prefab ve builder tarafinda arka plan katmaninin hedef safe area/canvas icinde bosluk birakmayacak cover davranisi da dogrulanmalidir.
- Gelecekte yeni MainMenu arka plani eklenirse once prefabda, sonra builder rebuild sonrasi sahnede ayni kaplama dogrulanmalidir.
- Bu duzeltme MainMenu gorsel yerlesimiyle sinirlidir; gameplay, save, economy, level progression veya HubPreview ray sistemiyle iliskili degildir.

### 13.3 Level Hub ray editor pipeline

Level hub tasarimi icin sadece sabit node pozisyonlari yeterli kabul edilmedi. Cunku arka plan yolu ileride degistiginde level kutularinin yeni yola elle hizalanmasi gerekecek.

Bu nedenle `LevelHubPreviewController` artik ray noktalarini component uzerinde serialize eder.

Ray noktasi verileri:

- `position`
- `scale`
- `rotation`
- `alpha`

Editor tarafinda `LevelHubPreviewControllerEditor` eklendi.

Kullanim modeli:

1. Yeni level hub arka plani `HubPreview` sahnesine koyulur.
2. `LevelHubPreviewController` secilir.
3. Scene View'de arka plan gorunurken ray noktalarinin handle'lari suruklenir.
4. Level kutulari bu ray uzerinde akar.
5. Drag/scroll/snap testi yapilir.
6. Onaylaninca sahne veya prefab ana Hub akisana tasinir.

### 13.4 Neden ray verisi controller uzerinde

Ray verisi ayri bir gecici editor state'inde tutulmaz.

Sebep:

- `HubPreview` sahnesi kopyalaninca ray ayari da kopyalanir.
- Ana Hub'a promote edilirken hangi ayarin kullanilacagi belirsiz kalmaz.
- Prefab instance override mantigi bozulmaz.
- Editor aktif sahnede veya secili objede bulunan controller uzerinden calisir.

Bu karar, "editör otomatik nerede olduğunu bilmeli" ihtiyacini karsilar.

### 13.5 Alpha demo kapsam karari

19 Nisan 2026 itibariyla alpha demo icin yeni karar:

- yeni buyuk sayfa ekleme yok
- yeni buyuk mekanik ekleme yok
- eklenen iki sayfa/yuzey test edilecek:
  - tasarimli `MainMenu`
  - level hub preview/ray editor akisi
- kucuk gorsel ayarlar, feedback ayarlari ve bug fix yapilacak
- ana QA akisi bu yeni omurga uzerinden tekrarlanacak

Bu karar, alpha demo sonuna yaklasirken kapsam kaymasini azaltmak icin alindi.

## 14. 21 Nisan 2026 Ek Guncelleme - Level Hub Recovery ve Sonraki Edit Siniri

21 Nisan 2026'da `HubPreview` level hub uzerinde yalnizca kutu oranini duzeltme hedefiyle baslanan edit, sahne/prefab override kirliligine donustu. Aynı gun icinde yapilan iki asamali recovery sonrasi su durum netlestirildi:

- `Assets/WordSpinAlpha/Scripts/Presentation/LevelHubPreviewController.cs` artik hierarchy mutasyonu yapmaz.
- `Assets/WordSpinAlpha/Scripts/Editor/WordSpinAlphaSceneBuilder.cs` level node gorsel mimarisini tek kaynak olarak `NodeVisual` child modelinde toplar.
- `Assets/WordSpinAlpha/Scripts/Editor/HubPreviewSceneNormalizer.cs` sahne kokunde orphan `NodeVisual` birikmesini temizler.
- `Assets/WordSpinAlpha/Generated/Prefabs/LevelHubPreview.prefab` temiz kaynak olarak yeniden uretilmistir.
- `Assets/WordSpinAlpha/Scenes/HubPreview.unity` temiz rebuild ile tekrar kurulmustur.

Dogrulanan teknik sonuc:

- prefab tarafinda `7 LevelNode_*` ve `7 NodeVisual` vardir
- `HubPreview.unity` icinde kirli `m_AddedGameObjects` temizlenmistir
- sahne kokunde orphan `NodeVisual` kalmamistir
- batch rebuild logu `RebuildLevelHubPreviewScene` calistigini ve basarili ciktigini dogrular

Ancak gorsel kabul henuz tamamlanmamistir:

- level kutularinin boyutu referansa yaklastirilmistir
- fakat kutularin altinda beyaz zemin/white plate gorunumu devam etmektedir
- bu nedenle recovery tamamlanmis olsa da level hub gorsel polish isi kapanmamis kabul edilir

Bir sonraki editte zorunlu sinir:

- sahne/prefab recovery katmanina yeniden dokunulmayacak
- ray/path editor verisine dokunulmayacak
- `OnValidate` veya `ExecuteAlways` icinde hierarchy ureten kod yazilmayacak
- scene YAML dosyasi regex ile toplu duzenlenmeyecek
- bir sonraki is yalnizca `NodeVisual` gorsel alani, sprite alpha/canvas boyutu ve node altindaki beyaz zemin kaynagini presentation katmaninda netlestirmek olacak

Kalici pipeline kuralina eklenen madde:

Bir UI sayfasinda hedef sadece oran/boyut duzeltmekse, ilk mudahale scene veya prefab hierarchy uzerinden degil, su sira ile yapilmalidir:

1. kaynak PNG alpha ve bos canvas alani dogrulanir
2. prefab icindeki gorsel katman rect'i kontrol edilir
3. builder ayni rect'i tekrar uretebiliyor mu bakilir
4. ancak bundan sonra scene rebuild uygulanir

Bu siralama disina cikilmasi, 21 Nisan 2026'da goruldugu gibi sahne kirliligi ve truncated YAML riskine yol acar.

## 15. 21 Nisan 2026 Ek Guncelleme - White Plate Dogrulamasi ve Rail Rebuild Kurali

Dosya ustunden dogrulanan sonuc:

- `Assets/WordSpinAlpha/Scripts/Presentation/LevelHubPreviewController.cs` icinde `SetNodeAlpha()` host node `Image` bileşenini gizli tutar
- alpha artik yalnizca `Button.targetGraphic` olan `NodeVisual` ve label'a uygulanir
- `Assets/WordSpinAlpha/Scripts/Editor/WordSpinAlphaSceneBuilder.cs` ayni host-gizli / visual-render ayrimini prefab build ve normalize katmaninda korur
- `Assets/WordSpinAlpha/Generated/Prefabs/LevelHubPreview.prefab` icinde 7 host `LevelNode_*` `Image` alpha degeri `0` olarak kaydedilmistir

Bu nedenle beyaz zemin sorununun asset kaynakli degil, host `Image` render davranisi kaynakli oldugu kesinlesmistir.

Ayri olarak ray editor veri kaybi akisi da netlestirildi:

- `LevelHubPreviewControllerEditor` ray verisini dogrudan controller uzerinde serialize eder
- `HubPreviewSceneNormalizer` ray verisini degistirmez
- veri kaybi, plain scene rebuild zinciri kullanildiginda olur:
  - `RebuildLevelHubPreviewScene()`
  - `BuildLevelHubPreviewScene()`

Sebep:

- plain rebuild yeni `HubPreview` sahnesini prefabdan sifirdan kurar
- prefab script varsayilan `railPoints` ile gelir
- bu yol mevcut scene controller ray ayarini tasimadigi icin bir onceki kaydedilmis ray duzeni bozulmus gibi gorunur

Bu tarihten itibaren pipeline kurali:

1. node/presentation editleri icin plain rebuild kullanilmaz
2. rail state korunacaksa state capture/apply iceren repair yolu kullanilir
3. ray editor dogrulamasi, her visual edit sonrasinda rebuild degil scene-instance uzerinden yapilir
