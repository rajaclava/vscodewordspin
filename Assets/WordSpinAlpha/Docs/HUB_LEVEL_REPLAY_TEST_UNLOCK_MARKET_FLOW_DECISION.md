# Hub Level Replay / Test Unlock / Market Flow Decision

**Belge türü:** Ürün akışı / karar notu  
**Durum:** Save hardening sonrası ele alınacak  
**Kod uygulaması:** Şu anda uygulanmayacak  
**Konum:** `Assets/WordSpinAlpha/Docs/`  
**Amaç:** Hub level seçimi, replay davranışı, geliştirme/test kilit açma modu ve market öncesi oyuncu akışı kararını kayıt altına almak.

---

## 1. Kararın Kısa Özeti

Hub level akışında iki farklı kullanım modu ayrılacak:

1. **Geliştirme / test modu**
   - Test kolaylığı için level kilitleri açık olacak.
   - Geliştirici aktif progress’e takılmadan istediği leveli seçip deneyebilecek.
   - Bu mod sadece geliştirme ve test sürecini hızlandırmak için kullanılacak.

2. **Market / oyuncu modu**
   - Oyuncu tamamladığı eski levelleri haritada görebilecek.
   - Oyuncu eski levelleri tekrar oynayabilecek.
   - Henüz açılmamış yeni leveller kilitli kalacak.
   - Oyuncunun yarım kalmış aktif session’ı varsa ve farklı bir eski leveli başlatmak isterse uyarı akışı devreye girecek.
   - Aktif/kayıtlı session levelı ile ray path üzerindeki seçili node ayrı kavramlar olarak ele alınacak.

Bu karar şu anda uygulanmayacak. Önce SaveManager hardening patch’i tamamlanacak, manuel testler geçecek ve save davranışı stabil kabul edilecek.

---

## 2. Bu Karar Neden Gündeme Geldi?

Save hardening manuel testi sırasında şu davranış gözlendi:

- Oyuncu örneğin 4. levelde bir harfi slota attıktan sonra hub’a dönüyor.
- Hub doğru şekilde 4. level node’una odaklanıyor.
- Oyuncu mevcut seviyeyi değiştirmeden “Oyna / Devam Et” akışını kullanırsa 4. levelde kaldığı yerden devam edebiliyor.
- Fakat oyuncu daha önce oynanmış bir leveli, örneğin 3. leveli seçip oynamak istediğinde aktif kayıt 4. levelde olduğu için akış 3. leveli doğrudan başlatmıyor.

Bu davranış save sistemi hatası olarak değerlendirilmedi. Daha doğru sınıflandırma:

- Hub level selection
- Active session / resume
- Replay behavior
- Market player flow
- Development test unlock policy

Yani bu konu SaveManager patch’inin parçası değil; ayrı bir Hub / Level Flow kararıdır.

---

## 3. Ürün Kararı

Nihai ürün kararı:

- Oyuncu tamamladığı eski levelleri görmeye devam etmeli.
- Oyuncu eski levelleri tekrar oynayabilmeli.
- Oyuncuya henüz açılmamış yeni leveller kilitli gösterilmeli.
- Hub açıldığında aktif/kayıtlı session varsa sistem aktif levela odaklanabilir.
- Ana devam akışı aktif/kayıtlı session’a dönmeli.
- Oyuncu eski bir level seçerse bu davranış “Devam Et” değil, “Tekrar Oyna” olarak ele alınmalı.
- Eğer aktif/kayıtlı session farklı bir leveldeyse ve eski level replay bu session’ı etkileyecekse kullanıcıya uyarı gösterilmeli.
- Aktif/kayıtlı session levelı hub içinde ayrı bir “devam hedefi” olarak korunmalı.
- Ray path üzerindeki seçili node, oyuncunun o an baktığı/seçtiği level olabilir; bu her zaman aktif session levelı değildir.
- Seçili node kilitliyse ve aktif session varsa Oyna davranışı aktif session levelına yumuşak geri hizalama yapmalı.
- Açılmış eski level seçimi replay davranışıdır.
- Replay aktif session’ı etkileyecekse uyarı gösterilmeli.

Önerilen kullanıcı algısı:

- “Devam Et” = yarım kalan aktif levela dön.
- “Tekrar Oyna” = tamamlanmış eski leveli yeniden başlat.
- “Kilitli” = henüz açılmamış level.

---

## 4. Geliştirme / Test Modu

Save sistemi ve diğer temel altyapılar üzerinde çalışırken test kolaylığı için tüm level kilitleri açık olabilir.

Bu modun amacı:

- hızlı level erişimi sağlamak,
- aktif progress’e takılmadan test yapabilmek,
- farklı level ve sahne davranışlarını daha hızlı doğrulamak,
- Codex / mini model patch sonrası manuel test süresini azaltmak.

Ancak bu mod market oyuncu akışı değildir.

Test modu açıkken:

- oyuncu/geliştirici istediği leveli seçebilir,
- kilit kontrolü gevşetilebilir,
- progress gating test dışı bırakılabilir.

Market finaline geçerken bu mod güvenli şekilde kapatılmalıdır.

---

## 5. Market / Oyuncu Modu

Market öncesi final oyuncu akışında beklenen davranış:

- Geçilmiş leveller haritada görünür.
- Geçilmiş leveller tekrar oynanabilir.
- Aktif/kayıtlı level görünür ve devam edilebilir.
- Aktif leveldan sonraki açılmamış leveller kilitli görünür.
- Oyuncu eski leveli tekrar oynamak isterse sistem bunu replay olarak algılar.
- Eğer replay aktif session’ı temizleyecekse veya etkileyebilecekse uyarı paneli gerekir.
- Oyuncu ray path’i kaydırsa bile aktif kayıtlı level kaybolmuş gibi hissettirilmemeli.
- Kilitli gelecek levelde Oyna’ya basıldığında oyuncu sessizce bloklanmamalı; aktif levela yönlendirme veya bilgilendirme yapılmalı.
- Aktif levela yumuşak geri hizalama hedef davranıştır.

Örnek uyarı mantığı:

> “Level 4’te kayıtlı ilerlemen var. Level 3’ü tekrar oynarsan kayıtlı ilerleme sıfırlanabilir. Devam etmek istiyor musun?”

Bu metin nihai UI metni değildir; sadece davranış örneğidir.

---

## Aktif Session Level / Ray Path Hizalama Kararı

- Aktif/kayıtlı level = devam hedefi.
- Ray path seçili node = oyuncunun o an baktığı node.
- Bu iki kavram ayrılmalı.
- Aktif session varsa hub bunu bilmeli.
- Oyuncu kilitli ileri leveldeyken Oyna’ya basarsa sistem aktif session levelına animasyonlu/yumuşak dönüş yapmalı.
- Oyuncu eski açılmış leveli seçerse replay akışı başlar.
- Replay aktif session’ı etkilerse popup gerekir.
- Bu popup şimdilik mekanik olarak basit olabilir; tasarım sonra yapılır.

---

## 6. Tek Ayar / Policy Gereksinimi

Bu davranış dağınık kod değişiklikleriyle yönetilmemeli.

İstenen yapı:

- Test unlock ve market lock davranışı tek bir açık/kapalı policy ile yönetilebilir olmalı.
- İleride yapay zekaya “test unlock modunu aç” veya “market flow moduna dön” dendiğinde tek satır / tek ayar üzerinden yönlendirilebilir olmalı.
- Bu policy’nin amacı kod içinde açıkça not edilmeli.
- Market default’u güvenli olmalı; yani yanlışlıkla tüm level kilitleri açık market build’e gitmemeli.

Not:
Bu belge teknik uygulama talimatı değildir. Uygulama sırasında Codex önce mevcut HubPresenter / LevelPathMapView / level selection flow tarafını raporlamalıdır. Doğrudan patch istenmemelidir.

---

## 7. Kod İçi Not Beklentisi

Uygulama aşamasında ilgili kod bölgesine kısa ve açık bir geliştirici notu eklenmelidir.

Bu notun amacı:

- test modunun neden var olduğunu anlatmak,
- market modunda hangi davranışın korunacağını belirtmek,
- AI destekli gelecek düzenlemelerde yanlış alanların değiştirilmesini önlemek,
- “kilitleri aç/kapat” davranışının tek policy üzerinden yönetildiğini hatırlatmak.

Not davranışı yönetmeyecek; gerçek davranış policy/toggle üzerinden yönetilecek.

Yani yorum satırı tek başına sistem olmayacak. Yorum, sistemi kullanan geliştiriciye ve AI modele sınırı anlatacak.

---

## 8. Uygulama Sırası

Bu kararın uygulama sırası:

1. SaveManager hardening patch’i tamamlanır.
2. Save patch’i manuel testlerden geçer.
3. Save davranışı stabil kabul edilir.
4. Bu belge tekrar açılır.
5. Codex’e önce Hub / Level Flow repo gerçekliği raporu yaptırılır.
6. Rapor sonucunda:
   - test unlock policy yeri,
   - market lock policy yeri,
   - active session / replay ayrımı,
   - active session level / selected node / locked future node / replay old level ayrımı,
   - uyarı paneli ihtiyacı,
   - eski level replay davranışı
   netleştirilir.
7. Uygulama gerekiyorsa küçük ve ayrı patch olarak yapılır.
8. Market final akışında test unlock kapalı, market flow açık olmalıdır.

---

## 9. Uygulama Öncesi Codex Rapor Soruları

Bu karar uygulanmadan önce Codex’e şu sorular raporlatılmalıdır:

1. Hub’da selected level, progress level ve active session şu anda nasıl ayrılıyor?
2. “Oyna” butonu hangi durumda Continue, hangi durumda OpenGameplayLevel davranışına gidiyor?
3. Level node kilit kontrolü nerede yapılıyor?
4. Eski level seçimi ile aktif session aynı anda varsa şu an ne oluyor?
5. Replay için session temizleme gerekiyor mu?
6. Test unlock policy için en güvenli merkezi nokta neresi?
7. Market final lock policy için en güvenli merkezi nokta neresi?
8. Bu iş UI/prefab/scene wiring tarafına dokunmadan yapılabilir mi?
9. Büyük refactor gerekiyor mu, yoksa küçük policy patch yeterli mi?
10. Aktif/kayıtlı session levelı HubPreview içinde nereden okunuyor?
11. Ray path seçili node ile aktif session levelı şu anda ayrı tutuluyor mu?
12. Oyna butonu seçili node kilitliyken ne yapıyor?
13. Aktif session varken seçili node kilitliyse ray path’i aktif levela geri hizalamak için mevcut LerpScrollTo / scrollOffset akışı kullanılabilir mi?
14. Açılmış eski level replay davranışı aktif session’ı nasıl etkiliyor?
15. Replay öncesi uyarı popup’ı için mevcut resume prompt altyapısı genişletilebilir mi, yoksa ayrı basit popup mı gerekir?
16. Bu iş LevelHubPreviewController içinde küçük patch olarak çözülebilir mi?
17. Scene/prefab değişmeden yapılabilir mi?
18. Hub.unity / HubPresenter eski akışına dokunmadan yapılabilir mi?

---

## 10. Riskler

### Risk 1 — Test modu market build’e açık kalabilir

Tüm level kilitleri açık test modu yanlışlıkla market sürümünde kalırsa progression hissi bozulur.

Önlem:
Market final checklist içinde bu policy’nin kapalı olduğu kontrol edilmeli.

### Risk 2 — Eski level replay aktif session’ı sessizce silebilir

Oyuncu fark etmeden yarım kalan levelini kaybederse güven kırılır.

Önlem:
Aktif session varsa ve eski level replay bunu etkileyecekse uyarı gösterilmeli.

### Risk 3 — Ray path kaydırması aktif level hissini bozabilir

Oyuncu ray path’i kaydırınca aktif levelini kaybolmuş gibi hissedebilir.

Önlem:
Aktif session levelı ayrı korunmalı ve gerekirse yumuşak geri hizalama yapılmalı.

### Risk 4 — Kilitli levelde sessiz başarısızlık olabilir

Kilitli levelde Oyna’ya basınca sessiz başarısızlık güven kırabilir.

Önlem:
Aktif levela yönlendirme veya bilgilendirme akışı kullanılmalı.

### Risk 5 — Ray path hizalama animasyonu bozulabilir

Ray path hizalama animasyonu yanlış yapılırsa level map hissi bozulabilir.

Önlem:
Hizalama akışı ray/path animasyonunu bozmayacak şekilde ele alınmalı.

### Risk 6 — Active session ve selected node karışabilir

Aktif session level ile selected node karıştırılırsa dil bazlı progress/session testleri zorlaşır.

Önlem:
Devam hedefi ve seçili node ayrımı karar notunda açık tutulmalı.

### Risk 7 — Save hardening ile karışabilir

Bu konu save patch’ine dahil edilirse kapsam büyür ve test zorlaşır.

Önlem:
Save patch’i tamamlanmadan bu işe girilmez.

### Risk 8 — Yorum satırı sistem sanılabilir

Sadece yorum eklemek yeterli değildir.

Önlem:
Gerçek davranış policy/toggle ile yönetilir; yorum sadece geliştirici notudur.

### Risk 9 — Replay ödül/score/economy davranışı belirsiz kalabilir

Eski level replay edildiğinde ödül tekrar verilecek mi, skor güncellenecek mi, yıldız kaydı tutulacak mı gibi sorular ayrı ürün kararıdır.

Önlem:
Bu belge sadece replay giriş akışını kayıt altına alır. Reward/economy kararı ayrı ele alınır.

---

## 11. Şimdilik Açık Sorular

1. Test unlock sadece Editor/Development build’de mi açık olacak, yoksa elle yönetilen bir policy mi olacak?
2. Market finalinde eski level replay aktif session’ı otomatik temizlemeli mi?
3. Uyarı paneli şart mı, yoksa demo sürecinde basit geçiş yeterli mi?
4. Replay edilen eski levelde ödül tekrar verilecek mi?
5. Replay skor/star/perfect sistemi market öncesi mi, market sonrası mı ele alınacak?
6. “Oyna” butonu label’ı hangi durumda “Devam Et”, “Tekrar Oyna”, “Baştan Başla” olacak?
7. Kilitli level seçiliyken Oyna’ya basınca direkt aktif levela mı hizalansın, yoksa küçük bilgilendirme popup’ı da çıksın mı?
8. Eski level replay aktif session’ı otomatik sıfırlamalı mı, yoksa uyarı şart mı?
9. Replay popup metni demo sürecinde geçici Türkçe mi olacak, yoksa locale key sonra mı eklenecek?
10. Aktif session level node’u haritada görsel olarak ayrıca işaretlenecek mi?
11. Ray path aktif levela dönerken animasyon süresi ne kadar olmalı?

---

## 12. Mevcut Karar

Bu belge yalnızca karar kaydıdır.

Şu an uygulanmayacak.

SaveManager dayanıklılık düzeltmesi ve manuel testler tamamlandıktan sonra, Hub / Level Replay / Test Unlock / Market Flow ayrı bir çalışma olarak açılacak.

Beklenen uygulama yaklaşımı:
Önce Codex raporu, sonra küçük policy patch, sonra manuel test.
