# WordSpin Alpha — Alpha Demo Öncesi Güncellenmiş Ana Plan

## Özet
Bu sürüm, mevcut `aPLAN` içeriğinin repo ile doğrulanmış halidir. Daha önce `YAPILDI` işaretlenen maddeler korunmuştur; yalnızca yeni tamamlanan işler, aktif riskler ve sıradaki en sağlıklı geliştirme sırası eklenmiştir.  
Önemli karar güncellemesi:
- `Telemetry / AI / cloud / developer panel` işleri alpha demo sonrasına ertelendi.
- Referans ekran artık `9:16` ve ilk gerçek doğrulama ortamı `aktif Android cihaz`.
- Proje artık `mobil-first`, `safe-area aware` ve `çok dilli` ilerleyecek.

---

## 1. Mevcut Durum

### Gameplay Core Loop — `YAPILDI`
- Akış çalışıyor:
  - `harf seç -> pin yükle -> swipe -> fırlat -> hit -> reveal -> soru tamamla -> level ilerle`
- `Store -> Back -> Gameplay` akışı bağlı.
- `InfoCard / Result / Level progression` omurgası bağlı.
- Save/restore sonrası:
  - cevap ilerlemesi geri geliyor
  - sıradaki hedef yeniden aktif oluyor
  - saplanmış pinler sahnede yeniden kuruluyor
- Device Simulator için pin hit kaçırma problemi segment-tabanlı hit kontrolüyle güçlendirildi.

### Fail / Continue Economy — `YAPILDI`
- `3 hata` sonrası otomatik reset kaldırıldı.
- `Fail Modal` çalışıyor.
- `Devam Et`:
  - enerji yemez
  - mevcut soru state’ini korur
  - `1 can` ile döner
- `Tekrar Dene` enerji maliyetlidir.
- `Continue` sonrası hedef kutusu yeniden yanar.
- `Level complete -> next level` geçişinde yanlış enerji tüketimi kaynaklı blokaj düzeltildi.

### Plaque Tabanlı Hit Sistemi — `AKTİF`
- `Magnet snap` kaldırıldı.
- Hit sonucu `pin ucu` üzerinden hesaplanıyor.
- `Perfect / Good / NearMiss / WrongSlot / WrongLetter / Miss` ayrımı bağlı.
- Pin başarılı hit’te merkeze çekilmiyor; değdiği gerçek noktada kalıyor.
- Dış saplanma yüzeyi esas alınıyor.
- Restore sırasında pinler save’deki gerçek lokal poz/rot ile geri geliyor.
- Mekanik çekirdek hazır; final tuning ve parity kontrolü sürüyor.

### HUD Hedef Gösterimi — `YAPILDI`
- `Şunu at` metni kaldırıldı.
- Aktif hedef cevap alanındaki kutu vurgusuyla gösteriliyor.
- Bu yapı random slot order mantığıyla uyumlu.

### Wrong Slot Kırılma Hissi — `YAPILDI`
- Doğru harf yanlış slota saplandığında pin kaybolmuyor.
- Pin ve harf birlikte parçalanma efektine giriyor.

### Camera Shake / Vibrate Feedback — `AKTİF`
- `WrongSlot`, `WrongLetter`, `Miss`, `NearMiss` için shake bağlı.
- Mobil titreşim çağrısı bağlı.
- Doğru saplanma hissi ve audio zinciri kullanıcı hissi açısından hâlâ kilitlenme aşamasında.

### Çoklu Dil İçerik ve Klavye Omurgası — `YAPILDI`
- `tr / en / de / es` locale içerik klasörleri ve soru/info card/level dosyaları mevcut.
- Dil değişimi event akışı bağlı.
- Locale bazlı klavye dizilimleri mevcut:
  - Türkçe Q klavye
  - İngilizce QWERTY
  - İspanyolca locale düzeni
  - Almanca QWERTZ
- Locale bazlı içerik yükleme omurgası `ContentService + Local/RemoteProvider` üzerinde çalışıyor.

### Score Sistemi — `YAPILDI`
- `ScoreManager` ve `ScoreTuningProfile` mevcut.
- `Perfect / Good / hız bonusu / level sonu bonusu` omurgası bağlı.
- Çarpan ve hız bonusu veri odaklı ayarlanabilir durumda.

### Mobil Runtime ve 9:16 Omurgası — `AKTİF`
- `MobileRuntimeController` sahne bazında portrait/orientation ve safe area uygular.
- `SceneBootstrap` üzerinden runtime singleton kuruluyor.
- `Gameplay` sahnesinde mobil-first layout tuning omurgası var.
- Ancak `safe area`, alt keyboard dock ve bazı UI yüzeylerinde final görsel kilit tamamlanmış değil.

### Android Cihaz Test Altyapısı — `AKTİF`
- `AndroidDeviceBuildTools` ile editor menüsünden APK build akışı eklendi.
- Cihaz smoke checklist dokümana işlendi.
- Repo tarafı hazır; kullanıcı makinesindeki Unity Android modül/toolchain kurulumu ayrı doğrulama gerektiriyor.

### Türkçe Geliştirici Editörleri — `YAPILDI`
- Türkçe telemetry paneli mevcut.
- Yeni olarak klavye layout tuning için profil + Türkçe editor penceresi eklendi:
  - dil bazlı ayrı ayar
  - canlı önizleme
  - dil ayarı kopyalama
- Bu, ileride tek bir birleşik tuning editorüne taşınacak modüler altyapı olarak değerlendirilecek.

### Local + Remote Content / Telemetry Omurgası — `YAPILDI`
- `ContentService`
- `LocalContentProvider`
- `RemoteContentProvider`
- `TelemetryService` ve snapshot mantığı mevcut
- Production-ready değiller; istemci kontratı seviyesinde kuruldular.

---

## 2. Güncellenen Kararlar

### Hit Sistemi
- `Magnet-assisted snap` iptal edildi.
- Nihai alpha mantığı:
  - pin ucu geçerli plaque bölgesine değerse saplanır
  - `Perfect / Good / NearMiss` plaque içi zonlara göre verilir
  - pin saplandığı gerçek noktada sabit kalır

### Rotator Yapısı
- Alpha demo için `9 plaque` sabit.
- Shape/layout verisi JSON tabanlı kalacak.
- Kutu sayısı ve görsel form ileride veriyle değişebilir.

### Çoklu Dil
- Her dil için ayrı soru/cevap/içerik seti kullanılacak.
- Sorular birebir çeviri olmak zorunda değil.
- Klavye ve içerik locale bazlı ayrışacak.

### Mobil Öncelik
- Referans ekran `9:16`.
- İlk oynanış doğrulaması bilgisayar Game view değil, gerçek Android cihaz olacak.
- Tüm scene yerleşimleri safe area içinde çalışmalı.

### Telemetry / AI / Cloud
- Alpha demo öncesi kapsam dışı.
- İstemci omurgası korunacak ama üretim sistemine geçilmeyecek.

---

## 3. Sonraki Uygulama Sırası

### Faz A — Android Gerçek Cihaz Test Kilidi — `AKTİF`
- Unity Android modül/toolchain kurulumu doğrulanacak.
- `Build And Run APK (USB Device)` akışı tek cihaz üzerinde uçtan uca çalışır hale getirilecek.
- İlk smoke test sırası sabitlenecek:
  - `MainMenu`
  - `Play`
  - ilk 3 level
  - `Fail -> Continue / Retry`
  - `save/quit -> resume`
  - dil değiştirip tekrar `Play`
  - `Store -> Back`

### Faz B — 9:16 Mobil Layout ve Safe Area Polish — `AKTİF`
- `Gameplay`, `MainMenu`, `Store`, `FailModal`, `InfoCard`, `Result` aynı mobil yerleşim mantığına çekilecek.
- Özellikle şu yüzeyler kilitlenecek:
  - üst bar
  - soru/cevap paneli
  - rotator alanı
  - launcher/saplanma ekseni
  - alt keyboard dock
- `safe area` yalnız pozisyonla değil, boyut ve iç margin ile birlikte ele alınacak.
- Alt keyboard dock final estetik ve tüm telefon oranları için taşmasız hale getirilecek.

### Faz C — Görsel Okunurluk ve Canlılık Pass — `SIRADA`
- Sürekli açık büyük haze/glow katmanları düşürülecek.
- Canlılık:
  - kontrast
  - sıcak accent
  - hit anı flash
  üzerinden verilecek.
- Amaç: puslu değil, net ve doygun mobil görünüm.

### Faz D — Hit Feel / Audio / Rhythm Stabilizasyonu — `SIRADA`
- Kilit davranışlar sabitlenecek:
  - ilk perfect normal hit sesi
  - ardışık perfect’lerde tizleşen zincir
  - good hitte reset
  - wrong-slot parçalanma sesi + shake
  - harf yükleme / fire sesi
- `ImpactFeelProfile` ve mevcut event zinciri korunacak.
- `RhythmProfile` mevcut flow’u destekleyecek kadar bağlanacak; yeni karmaşık ekonomi açılmayacak.

### Faz E — Alpha Kabul Matrisi — `SIRADA`
- Her buildte şu matris çalıştırılacak:
  - `TR / EN / DE / ES`
  - `Play`
  - `Level Select`
  - `Store -> Back`
  - `save/quit -> resume`
- Kabul testleri:
  - first letter doğru kabul ediliyor
  - doğru slot yanlış sayılmıyor
  - restore sonrası pin doğru dış saplanma noktasına geliyor
  - continue sonrası hedef vurgusu sürüyor
  - dil değişimi sonrası `Play` boş soru açmıyor
  - UI safe area dışına taşmıyor
  - keyboard farklı telefon oranlarında bozulmuyor

### Faz F — Alpha Demo Sonrası Backlog — `SIRADA`
- Türkçe metrics / hotfix editor genişlemesi
- AI telemetry / cloud / developer panel
- gelişmiş canlı yönetim araçları
- telemetry schema ve aggregate snapshot production pass

---

## 4. Önemli Arayüz / Tip / Veri Güncellemeleri

### Yeni veya Güncellenmiş Veri Yüzeyleri
- `ScoreTuningProfile : ScriptableObject`
- `ImpactFeelProfile : ScriptableObject`
- `KeyboardLayoutTuningProfile : ScriptableObject`
- locale bazlı `questions.json / info_cards.json / levels.json`
- `shape_layouts.json`, `difficulty_profiles.json`, `rhythm_profiles.json` akışı korunuyor

### Yeni Editor Yüzeyleri
- `DeveloperTelemetryWindow`
- `AndroidDeviceBuildTools`
- `KeyboardLayoutTuningWindow`
  - dil bazlı tuning
  - önizleme oran seçimi
  - dil ayarı kopyalama

### Runtime Bağlantıları
- `KeyboardPresenter` artık klavye tuning profilini okuyabiliyor
- `GameplaySceneTuner` locale bazlı keyboard dock tuning’ini profilden alabiliyor
- `MobileRuntimeController` safe area ve mobil runtime davranışını uygular
- `SceneBootstrap` üzerinden `ScoreManager` ve `MobileRuntimeController` gibi singleton’lar garanti ediliyor

---

## 5. Test Planı

### Zaten Doğrulananlar
- `3 hata -> Fail Modal`
- `Devam Et -> 1 canla devam`
- `Tekrar Dene -> enerji maliyeti`
- `Continue` sonrası hedef kutunun yeniden yanması
- Save/restore sonrası cevap ilerlemesi
- Save/restore sonrası saplanmış pinlerin geri gelmesi
- Yanlış slotta parçalanma efekti
- Hedef kutunun cevap panelinden gösterilmesi
- Locale içerik dosya omurgası
- Score sisteminin veri odaklı omurgası
- Klavye tuning editorünün asset tabanlı çalışması

### Alpha Öncesi Zorunlu Kabul Testleri
- `Perfect` hit:
  - net audio
  - görünür feel
  - multiplier/score düzgün
- `Good` hit:
  - doğru ses
  - chain reset davranışı
- `WrongSlot`:
  - parçalanma + fail feedback
- `WrongLetter`:
  - klavye bazlı fail feedback
- `Android`:
  - `1080x1920`
  - `1080x2160`
  - `1080x2340`
  - `1080x2400`
- `Locale`:
  - `TR / EN / DE / ES` sorular doğru geliyor
  - klavyeler locale’e göre doğru açılıyor
- `Keyboard`:
  - safe area dışına taşmıyor
  - kenar tuşlar frame köşelerine yaslanmıyor
  - oran değişiminde estetik bozulmuyor

---

## 6. Varsayımlar ve Sabitler

- Alpha demo için `9 plaque` sabit.
- Görsel kalite hâlâ placeholder seviyede; öncelik mekanik kilidi ve mobil okunurluk.
- `Continue` sonrası dönüş canı `1`.
- `Retry` enerji maliyetlidir.
- `Impact feel`, `score` ve `keyboard layout` veri odaklı kalacak.
- Theme yalnız presentation yoğunluğunu yönetecek; mekanik kararları yönetmeyecek.
- `Telemetry / AI / cloud / developer panel` alpha demo sonrasına ertelendi.
- İlk gerçek kalite eşiği bilgisayar Game view değil, Android cihaz üstü test olacak.
