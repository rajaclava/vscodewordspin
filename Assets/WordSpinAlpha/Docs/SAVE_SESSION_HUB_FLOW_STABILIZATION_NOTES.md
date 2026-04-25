# Save / Session / Hub Flow Stabilization Notes

## 1. Kısa Özet
Bugünkü çalışma, save hardening testleri sırasında ortaya çıkan HubPreview, level replay, dil bazlı session ve dev/test toggle ihtiyaçlarını stabilize etmek için yapıldı.

- Save sistemi test edilirken enerji, hub dönüşü, level lock, replay uyarısı ve dil bazlı session sorunları test akışını engelliyordu.
- Bu engeller ayrı ayrı sınıflandırıldı.
- HubPreview tarafında active session ile selected node ayrımı netleştirildi.
- Active session artık dil bazlı korunacak hale getirildi.
- TR / EN / ES / DE hızlı regresyon testleri geçti.

## 2. Neden Yapıldı?
- Save hardening testleri enerji sistemi yüzünden tamamlanamıyordu.
- Dil değişimi testi için HubPreview’den MainMenu’ye dönüş gerekliydi.
- Oyuncu ray path’i kaydırınca aktif/kayıtlı level geride kalabiliyordu.
- Kilitli levelde Oyna’ya basınca oyuncu sessiz şekilde bloklanıyordu.
- Aktif session varken farklı level başlatılırsa mevcut kayıt sessizce sıfırlanabiliyordu.
- Dil değişiminde progress korunuyor ama active session global olduğu için başka dildeki session tarafından eziliyordu.

## 3. Çözülen Ana Davranışlar

### 3.1. Dev/Test Energy Bypass
- Enerji sistemi silinmedi.
- Test/dev sürecinde level girişini enerji engeli bloklamasın diye policy/toggle eklendi.
- Market/final davranış için kapatılabilir.
- Release guard mantığı korunur.
- Toggle hatırlatma:
  - DisableEntryEnergyForManualTesting = true → testte enerji kapısı bypass
  - DisableEntryEnergyForManualTesting = false → enerji sistemi normal

### 3.2. Hub Level Lock Test Toggle’ları
- Unlock-all test davranışı ayrı toggle’a bağlandı.
- Highest unlocked test override eklendi.
- Böylece save dosyasındaki yüksek progress testleri yanıltmadan kilitli node davranışı test edilebildi.
- Toggle hatırlatma:
  - UnlockAllHubLevelsForManualTesting = false → normal lock davranışı
  - UnlockAllHubLevelsForManualTesting = true → dev/editor’da tüm Hub level node’ları açık kabul edilebilir
  - ForceHubHighestUnlockedLevelForManualTesting = 0 → gerçek save/progress kullanılır
  - ForceHubHighestUnlockedLevelForManualTesting = N → dev/editor’da Hub N’e kadar açık kabul eder

### 3.3. HubPreview Active Session / Selected Node Ayrımı
- Active session level ile ray path selected node ayrı kavramlar olarak ele alındı.
- Oyuncu ray path’i ileri/geri kaydırsa bile aktif kayıtlı level kaybolmuş gibi davranılmamalı.
- Kilitli ileri node seçiliyken Oyna’ya basılırsa gameplay açılmaz.
- Aktif session varsa ray path aktif session levelına yumuşak şekilde geri hizalanır.

### 3.4. Farklı Level Başlatma Uyarısı
- Aktif session varken oyuncu active session levelından farklı açılmış bir leveli başlatmak isterse uyarı popup’ı çıkar.
- Popup hangi levelin başlatılacağını ve hangi leveldeki kayıtlı ilerlemenin sıfırlanabileceğini gösterir.
- Onay verilirse selected level başlar.
- Vazgeç seçilirse Hub’da kalınır ve active session korunur.
- Bu sadece geriye dönüş için değil, active session levelından farklı herhangi bir açılmış level için geçerlidir.

### 3.5. Per-Language Active Session
- Önceden progress dil bazlıydı ama active session tek globaldi.
- Bu yüzden TR’deki Level 4 session, EN’de Level 1 oynanınca eziliyordu.
- Active session artık progress mantığına benzer şekilde dil bazlı korunacak şekilde düzenlendi.
- TR session, EN / ES / DE session tarafından ezilmemeli.
- Yanlış dilin session’ı resume edilmemeli.
- Eski global session alanı backward compatibility / legacy mirror olarak korunabilir.
- Runtime source-of-truth per-language session olmalıdır.

## 4. Onaylanan Manuel Testler
- TR’de Level 4 session oluşturuldu.
- EN’e geçildi.
- EN Level 1 session oluşturuldu.
- TR’ye dönüldüğünde TR Level 4 resume popup’ı çıktı.
- Devam Et TR kayıtlı oyununu yükledi.
- EN kendi session davranışını korudu.
- Diğer diller için hızlı regresyon yapıldı.
- TR / EN / ES / DE testleri geçti.
- Hub kilitli node geri hizalama davranışı geçti.
- Farklı level başlatma popup davranışı geçti.
- Dev/test toggle davranışları doğrulandı.

## 5. Model / Kota Kullanım Notu
- Medium dar runtime patchlerde verimli çıktı.
- Örnek ölçüm: Medium dar patch yaklaşık %1 kota tüketti.
- High per-language active session patch için kullanıldı.
- Başlangıç 5 saatlik kota: %80
- Bitiş 5 saatlik kota: %73
- Tüketim: %7
- Süre: 5m 13s
- Sonuç: Bu görev High için uygundu ve maliyet makul kabul edildi.
- High; save/session/progress veri modeli, migration, çoklu call-site ve omurga sistemler için kullanılmalı.
- Medium; dar HubPreview patchleri, toggle, docs, hedefli rapor ve tek controller düzeltmeleri için uygun.

## 6. Korunması Gereken Davranışlar
- SaveManager atomic/temp write hardening korunmalı.
- Progress dil bazlı davranışı korunmalı.
- Active session dil bazlı korunmalı.
- Yanlış dil session’ı resume edilmemeli.
- HubPreview kilitli node hizalama korunmalı.
- Replay/farklı level uyarısı korunmalı.
- DevTestPolicy toggle’ları market/final öncesi kontrol edilmeli.
- Energy/economy/telemetry/reward davranışları bu patchlerle karıştırılmamalı.
- Scene/prefab değişiklikleri bu fazın parçası değildir.

## 7. İleride Dikkat Edilecekler
- Market/final öncesi dev/test toggle değerleri mutlaka gözden geçirilmeli.
- Dil bazlı session davranışı yeni level flow değişikliklerinde regresyon testine alınmalı.
- Replay ödül/economy semantiği ayrı karar konusu olarak ele alınmalı.
- Popup metinleri ve localization polish daha sonra yapılmalı.
- HubPreview görsel lock state ayrı UI/polish konusudur.
- DevTestPolicy highest unlocked override ileride gerekirse dil bazlı hale getirilebilir ama bu çalışma içinde bilinçli olarak ayrı tutuldu.

## 8. Bu Dosya Ne Değildir?
- Bu dosya yeni patch talimatı değildir.
- Bu dosya aktif yapılacaklar listesi değildir.
- Bu dosya teknik implementasyon rehberi değildir.
- Bu dosya yatırımcı veya dış paylaşım dokümanı değildir.
- Bu dosya bug fix sonrası mimari hafıza ve test kabul notudur.
