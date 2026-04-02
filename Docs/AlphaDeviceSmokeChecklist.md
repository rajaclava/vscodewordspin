# WordSpin Alpha — Android Cihaz Smoke Checklist

## Build
- `Tools/WordSpin Alpha/Android/Build And Run APK (USB Device)` kullan.
- Referans format `9:16`.
- Test cihazı düşük-orta altı Android kabul edilir.

## Cihaz Ayarları
- Telefon dikey kullanımda test edilir.
- Sistem ölçeklendirme ve yazı boyutu varsayılan seviyede olmalı.
- Telefon pil tasarruf modunda olmamalı.

## Ana Akış
1. `MainMenu` açılır.
2. `Play` çalışır.
3. İlk 3 level oynanır.
4. `Fail -> Continue -> Retry` akışı doğrulanır.
5. Oyundan çıkıp geri girilerek restore doğrulanır.
6. Dil değiştirilip tekrar `Play` denenir.
7. `Store -> Back` akışı doğrulanır.

## Gameplay Kabul
- İlk harf doğru kabul edilir.
- Doğru slota atış yanlış sayılmaz.
- `Perfect` ilk hit normal çalar.
- Art arda `Perfect` hitlerde pitch yükselir.
- `Good` hit zinciri resetler.
- Wrong slotta parçalanma sesi ve shake çalışır.
- Continue sonrası hedef kutu vurgusu sürer.
- Restore sonrası pin dış saplanma noktasında görünür.

## Mobil Yerleşim Kabul
- `1080x1920`
- `1080x2160`
- `1080x2340`
- `1080x2400`

Her çözünürlükte:
- Top bar safe area içinde kalır.
- Soru paneli taşmaz.
- Rotator kesilmez.
- Keyboard alanı sıkışmaz.
- Alt butonlar keyboard üstüne binmez.
- Modal butonları ekran dışına çıkmaz.

## Görsel Kabul
- Sahne sisli/puslu görünmez.
- Sürekli açık glow katmanları göz yormaz.
- Accent renkler canlı görünür.
- Hit anındaki flash okunur ama sahneyi yıkamaz.

## Dil Kabul
- `TR`
- `EN`
- `DE`
- `ES`

Her dilde:
- `Play` boş soru açmaz.
- `Level Select` doğru içerik açar.
- Klavye cevap karakterleriyle uyumludur.
- Info card eşleşmesi çalışır.

## Performans Kabul
- 10 dakika oynanışta belirgin frame hitch olmamalı.
- Audio rastgele susmamalı.
- UI güncellemeleri input hissini geciktirmemeli.
