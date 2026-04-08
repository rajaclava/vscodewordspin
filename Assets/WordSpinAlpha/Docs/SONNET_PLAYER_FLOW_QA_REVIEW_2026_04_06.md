# SONNET_PLAYER_FLOW_QA_REVIEW_2026_04_06

> **Bu rapor SYSTEM_QA_BUG_SWEEP_REPORT_2026_04_06.md dosyasina ikinci goz olarak hazirlanmistir.**
> **Bu rapor dogrulanmis bug listesi degildir.**
> **Bu rapor ozellikle oyuncu davranisi ve QA senaryo bosluglarini bulmaya hedefler; kod degisikligi onermez.**

---

## 1. Amac

Bu dokuman, `SYSTEM_QA_BUG_SWEEP_REPORT_2026_04_06.md` adli ana bug sweep raporuna bagimsiz ikinci goz QA incelemesi olarak hazirlanmistir. Odak noktalari:

- Mevcut raporda ele alinmamis oyuncu davranis senaryolari
- Back, retry, continue, store, dil degistirme, pause/resume, uygulama kapatma ve sahne gecisi sirasindaki oyuncu davranislari
- Fail modal, info card, result, menu, store, membership, hint, energy, shape, keyboard, localization yuzeylerindeki boskluklar
- Cihaz yasam dongusu (background, kill, warm start) ile kesisen oyuncu aksiyonlari
- Mevcut 77 senaryo icinde zayif kalan veya yanlis onceliklendirilmis maddeler
- Oyuncu bakis acisindan kritik ama mevcut raporda temsil edilmemis kod yapisi riskleri

Bu rapor kod degisikligi onermez, hotfix gorevi degildir, ana rapordaki senaryolari iptal etmez.

---

## 2. Incelenen Dosyalar

### Dokumanlar

- `SYSTEM_QA_BUG_SWEEP_REPORT_2026_04_06.md`
- `WORDSPIN_MASTER_PLAN.md`
- `TEKNIK_ZIHIN_HARITASI.md`
- `MARKET_RELEASE_READINESS_PLAN.md`

### Dogrudan okunan script dosyalari

- `Scripts/Core/GameManager.cs`
- `Scripts/Core/SessionManager.cs`
- `Scripts/Core/LevelFlowController.cs`
- `Scripts/Core/SceneNavigator.cs`
- `Scripts/Core/EnergyManager.cs`
- `Scripts/Core/EconomyManager.cs`
- `Scripts/Core/SaveManager.cs`
- `Scripts/Core/PlayerSaveModels.cs`
- `Scripts/Core/InputBuffer.cs`
- `Scripts/Core/QuestionLifeManager.cs`

### Referans uzerinden analiz edilen dosyalar

- `FailModalPresenter.cs`, `KeyboardPresenter.cs`, `StorePresenter.cs`, `ResultPresenter.cs`, `InfoCardPresenter.cs`, `DebugRewardedAdPresenter.cs`

---

## 3. Genel Degerlendirme

Ana bug sweep raporu altyapi odakli, mimari riskleri dogru saptayan guclu bir senaryo matrisidir. 77 senaryo ile save/session, content/localization ve editor/apply zinciri alanlarinda iyi kapsama sahiptir.

Bu ikinci goz incelemesi su eksiklikleri tespit etmistir:

- **Oyuncu davranisi zamanlama ve siralama senaryolari** yetersiz temsil edilmektedir. Bircok senaryo yalnizca "bu durum olusabilir" seklinde kurgulanmis, oyuncunun bunu nasil tetikleyebilecegi sorgulanmamistir.
- **Cihaz yasam dongusu** (Android geri tusu, OS kill without lifecycle callback, warm vs cold start) neredeyse hic ele alinmamistir.
- **InputBuffer davranisi** tamamen gozden kacmistir. Hizli ardisik basislar, target degisiminde buffer icerigi, buffer tasmasinda sessiz input kaybi kritik kor noktadir.
- **Enerji tuketim yollari** (`SceneNavigator.OpenGameplayLevel()`, `GameManager.StartLevel()`, `ReturnFromStore()` + dil degisikligi kombinasyonu) haritalanmamistir.
- **Hint mekanizmasi** ekonomi acisindan ele alinmis fakat gameplay akisina baglantisi tartisilmamistir.
- **Bazi senaryolarin onceligi** projenin alpha asamasina gore yeniden degerlendirilmelidir.

---

## 4. Mevcut Raporda Guclu Bulunan Alanlar

**Save / Session (BUG-007 ila BUG-015):** Save throttle penceresi riski, bozuk save reset, content degisikligi sonrasi index uyumsuzlugu ve shape slot sayisi degisikligi iyi orneklendirilmistir.

**Content / Localization (BUG-031 ila BUG-038):** Remote/local merge sirasi, duplicate id override, localized content fallback tutarsizligi ve language change sirasinda tam rehydrate olmaması dogru tespit edilmistir.

**Economy / Store / Membership (BUG-039 ila BUG-048):** Baslangic grant policy, test mode snapshot, tema unlock/premium cakismasi ve energy pack doluluk uzerine satin alma iyi kapsanmistir.

**Fail/Info/Result Flow (BUG-024 ila BUG-030):** Pending fail stale, info card/result flag cakismasi, fake rewarded callback gec donus riski ve store donusunde pending UI kaybi yerinde tespitlerdir.

**Production Readiness (BUG-073 ila BUG-077):** Debug presenter sizması ve provider gecis riski acik ve dogru onceliklendirilmistir.

---

## 5. Mevcut Raporda Eksik veya Zayif Kalan Alanlar

**5.1 InputBuffer davranisi tamamen eksik**
`InputBuffer.cs` dosyasi mevcut raporda hic referans edilmemektedir. Hizli ardisik basislar, target degisiminde buffer temizlenmemesi, buffer kapasitesi asilmasinda sessiz input kaybi tamamen kor noktadir.

**5.2 Cihaz yasam dongusu ve platform davranisi eksik**
Android geri tusu (hardware/gesture) hicbir senaryoda ele alinmamistir. App background sonrasi OS kill (SIGKILL, low memory) sirasinda `OnApplicationQuit()` tetiklenip tetiklenmedigini sorgulayan senaryo yoktur. `OnApplicationPause` execution order (SaveManager vs SessionManager) tartisilmamistir.

**5.3 Enerji tuketimi yollari tam haritalanmamis**
`ReturnFromStore()` + dil degisikligi kombinasyonunun `GameManager.StartLevel(consumeEntryEnergy:true)` yoluyla beklenmedik enerji tuketimine yol acabileceği gorulmemistir.

**5.4 Hint akisi gameplay'e baglanmamis**
`EconomyManager.TrySpendHint()` metodunun oyun icinde nasil tetiklendigi, hint butonu fail state sirasinda dokunulabilir mi, hangi harfi acar, save/restore edilir mi — hicbiri raporda yer almamaktadir.

**5.5 Continue sonrasi ikinci fail davranisi belirsiz**
`_usedContinueInCurrentLevel` flag set edildikten sonra oyuncu tekrar fail olursa fail modal context'te bu bilgi iletilmemektedir. Oyuncuya yeniden continue teklif edilip edilmeyecegi sorgulanmamistir.

**5.6 OpenMainMenu() snapshot eksikligi**
`OpenStore()` acikca `SessionManager.TakeSnapshot()` cagirirken, `OpenMainMenu()` bu cagrıyı YAPMAZ. Bu fark dogrudan sorgulanmamistir.

**5.7 CanRestoreActiveSession / CanResumeSavedSession kod tekrari**
Ayni mantik iki ayri private metodda kopyalanmistir. Biri degistirilirse diger taraf sessizce uyumsuz kalar. Mevcut raporda hic yer almamaktadir.

**5.8 Result ekranindan hizli cift aksiyonlar**
Result ekraninda cift "Next" basimi, level tamamlanma ile info card pending state kombinasyonu ve skor/ekonomi event zincirinin hizli tekrar tetiklenmesi ele alinmamistir.

---

## 6. Yeni Onerilen Bug Senaryolari

---

### PFQA-001 — InputBuffer hedef degisiminde temizlenmemesi

- **Kategori:** Gameplay / Input / Hit
- **Oncelik:** P1
- **Olasilik:** Yuksek
- **Neden atlanmis olabilir:** `InputBuffer.cs` mevcut raporda hic referans edilmemistir. Temel altyapi odakli sweep klavye input kuyrugunu es gecmistir.
- **Muhtemel kok neden:** `LevelFlowController.RefreshCurrentTarget()` yeni hedef harfi belirlemek icin `inputBuffer.SetExpectedLetter(targetLetter)` cagirır fakat `inputBuffer.ClearExpectedLetter()` ya da `_buffer.Clear()` CAGIRMAZ. Buffer'da onceki hedeften kalma harfler bulunabilir. Basarili bir hit → `RevealNextLetter()` → `RefreshCurrentTarget()` → yeni hedef belirlenir. Buffer'daki eski harfler `PinLauncher` tarafindan yeni hedefe karsi degerlendirilir. Eski harf yeni hedefle eslesmiyorsa `WrongLetter` → can duşer.
- **Etkilenen dosyalar:** `Scripts/Core/InputBuffer.cs`, `Scripts/Core/LevelFlowController.cs` (RefreshCurrentTarget), `Scripts/Core/PinLauncher.cs`
- **Manuel cogaltma adimlari:**
  1. Bir levelde dogru harf tusuna arka arkaya 3 kez hizlica bas (pin yuklenir, swipe yok).
  2. Ilk pin icin swipe yap → basarili hit → `RefreshCurrentTarget()` ile yeni hedef belirlenir.
  3. Buffer'da kalan 2 onceki harfin islendigini gozlemle.
  4. Yeni hedef harfi oncekinden farkliysa: beklenmedik WrongLetter event'i, can dusu.
  5. Yeni hedef oncekiyle ayni harfse: oyuncu dokunmadan ekstra reveal tetiklenebilir.
- **Beklenen bozuk belirti:** Basarili hit sonrasi beklenmedik can kaybi; veya oyuncu dokunmadan otomatik harf acilmasi.
- **Ana rapora eklenecek yer:** Bolum 3 (Gameplay / Hit / Input / Rotator), BUG-018'in hemen ardina

---

### PFQA-002 — OnApplicationPause execution order race: SaveManager onceden yazar, SessionManager sonra snapshot alir

- **Kategori:** Save / Session / Yasam Dongusu
- **Oncelik:** P0
- **Olasilik:** Orta
- **Neden atlanmis olabilir:** BUG-007 throttle penceresini ele almaktadir. Ancak iki farkli MonoBehaviour'in ayni lifecycle event'ine tepkisindeki sira bagımliligi ayri bir risk olarak incelenmemistir.
- **Muhtemel kok neden:** Unity MonoBehaviour execution order'da `SaveManager.OnApplicationPause(true)` ONCE calısirsa → `WriteToDisk()` eski state ile cagrılır. Ardindan `SessionManager.OnApplicationPause(true)` → `TakeSnapshot()` → `_data` guncellenir → `SaveManager.Save()` cagrılır. Ancak son save'den bu yana 0.35s gecmediyse `WriteToDisk()` ertelenir (`_savePending=true`). OS uygulamayi SIGKILL ile sonlandirirsa guncel snapshot diske ulasmaz.
- **Etkilenen dosyalar:** `Scripts/Core/SaveManager.cs` (OnApplicationPause, WriteToDisk), `Scripts/Core/SessionManager.cs` (OnApplicationPause, TakeSnapshot)
- **Manuel cogaltma adimlari:**
  1. Script Execution Order'da SaveManager'i SessionManager'dan once koy.
  2. Oyun ortasinda harf actir.
  3. Son harfin hemen ardindan (ayni frame veya 0.35s icerisinde) uygulamayi arka plana al.
  4. Android Recents'tan swipe ile kapat (SIGKILL).
  5. Yeniden ac, son harfin kaydedilip kaydedilmedigini kontrol et.
- **Beklenen bozuk belirti:** Son reveal state kaydedilmemis, eski duruma donulmus; dusuk RAM cihazlarda daha sik tekrarlanabilir.
- **Ana rapora eklenecek yer:** Bolum 2 (Save / Session / Restore), BUG-007'nin yanina ayri madde olarak

---

### PFQA-003 — ReturnFromStore() + dil degisikligi = sessiz session kaybi + beklenmedik enerji tuketimi

- **Kategori:** Store / Session / Enerji / Navigasyon
- **Oncelik:** P0
- **Olasilik:** Orta
- **Neden atlanmis olabilir:** BUG-012 dil degisikligi sonrasi answer length mismatch'ini ele almaktadir. Ancak dil degisikliginin `ReturnFromStore()` → `CanResumeSavedSession()=false` → `GameManager.Start()` → `StartLevel(consumeEntryEnergy:true)` zincirinde enerji tuketimine yol acmasi gorulmemistir.
- **Muhtemel kok neden:** Tam kod izi: (1) Oyuncu Turkce gameplay sirasinda magazaya gider → `SessionManager.TakeSnapshot()` → `_returnSceneName=Gameplay`. (2) Magazada dili Ingilizceye degistirir → `SaveManager.Data.languageCode="en"`. (3) Geri basar → `SceneNavigator.ReturnFromStore()`. (4) `CanResumeSavedSession()` dil uyumsuzlugu → **false**. (5) Pending request SET EDILMEZ. Gameplay sahnesi yuklenir. (6) `GameManager.Start()` → pending request yok, `CanRestoreActiveSession()` → yine **false**. (7) Alt kola dusar: `StartLevel(progressLevelId, true)` → `consumeEntryEnergy=true` → **ENERJİ TUKETILIR**. (8) Turkce session sessizce silinir.
- **Etkilenen dosyalar:** `Scripts/Core/SceneNavigator.cs` (ReturnFromStore, CanResumeSavedSession), `Scripts/Core/GameManager.cs` (Start, CanRestoreActiveSession), `Scripts/Core/EnergyManager.cs`
- **Manuel cogaltma adimlari:**
  1. Turkce gameplay basla, birkas harf ac.
  2. Store'a git (session kaydedilir).
  3. Magazada dili Ingilizceye degistir.
  4. Geri / Back tusuna bas.
  5. Enerji sayisinin dustugunu ve eski session'in kayboldigunu dogrula.
  6. Baslanilan level, oyuncunun biraktigi level degil progress level'dir.
- **Beklenen bozuk belirti:** Oyuncu magazadan "geri dondugunu" sanir; session kaybolmus, enerji harcanmis. Enerji 0 ise StartLevel basarisiz → bos gameplay sahnesinde kalinir.
- **Ana rapora eklenecek yer:** Bolum 2 (Save / Session) veya Bolum 6 (Economy / Store / Energy), P0 olarak

---

### PFQA-004 — SceneNavigator.OpenMainMenu() oncesinde snapshot alinmamasi

- **Kategori:** Save / Navigasyon / Session
- **Oncelik:** P1
- **Olasilik:** Yuksek
- **Neden atlanmis olabilir:** BUG-029 "store donusu sonrasi pending UI kaybi"ni ele almaktadir. Menu yolunun snapshot almadigi ise dogrudan sorgulanmamistir.
- **Muhtemel kok neden:** `SceneNavigator.OpenStore()` acikca `SessionManager.Instance?.TakeSnapshot()` cagirır (satir 31). `SceneNavigator.OpenMainMenu()` bu cagrıyı YAPMAZ. Oyuncu "Menu" tusuna bastigında son snapshot, son reveal veya hata aniyla kalinir. `TakeSnapshotAfterReveal()` coroutine 1 frame gecikmeli calisir. Reveal ile Menu tusu arasinda 1 frame'den kisa sure gecmisse son state eksik kaydedilir.
- **Etkilenen dosyalar:** `Scripts/Core/SceneNavigator.cs` (OpenMainMenu), `Scripts/Core/SessionManager.cs`
- **Manuel cogaltma adimlari:**
  1. Gameplay ortasinda bir harf ac.
  2. Reveal animasyonu tamamlanmadan hemen Menu tusuna bas.
  3. Ana menuye git, tekrar Play'e bas.
  4. Session restore edildiginde son harfin kaydedilip kaydedilmedigini kontrol et.
  5. Ayni testi Store yoluyla karsilastir (Store yolunda snapshot alindigi icin farkli davranis beklenir).
- **Beklenen bozuk belirti:** Menu yoluyla gidilip geri donulunce son reveal eksik restore edilir; Store yoluyla gidilince bu fark gorulmez.
- **Ana rapora eklenecek yer:** Bolum 2 (Save / Session / Restore), BUG-009'un yanina

---

### PFQA-005 — Hint mekanizmasinin gameplay akisina baglantisi belirsiz veya eksik

- **Kategori:** Gameplay / Economy / UX
- **Oncelik:** P1
- **Olasilik:** Yuksek
- **Neden atlanmis olabilir:** Mevcut rapor hint'i yalnizca ekonomi boyutunda (BUG-047 promo grant) ele almistir. Gameplay ici hint kullanim akisi hic sorgulanmamistir.
- **Muhtemel kok neden:** `EconomyManager.TrySpendHint()` ve `GrantHints()` mevcuttur. Ancak `GameManager.cs`'de hint kullanim akisi gorulmemektedir. Sorulmasi gereken sorular: Hint butonu fail state sirasinda dokunulabilir mi (InputManager.enabled=false iken klavye disabled, ama hint butonu ayri bir UI elemani ise)? Hint kullanimi `RegisterQuestionError` mi yoksa basarili reveal mi sayilir? Hint kullanildiktan sonra save alinıyor mu? Hint count save'e yansitiliyor ve restore ediliyor mu? Tum harfler acikken hint basiminda ne olur?
- **Etkilenen dosyalar:** `Scripts/Core/EconomyManager.cs` (TrySpendHint, GrantHints), `Scripts/Core/GameManager.cs` (hint hook eksik mi?), `Scripts/Presentation/FailModalPresenter.cs` veya `GameplayHudPresenter.cs`
- **Manuel cogaltma adimlari:**
  1. Bir hint kazan (store'dan veya grant ile).
  2. Gameplay sirasinda hint kullan; hangi harfin/slot'un acildigini gozlemle.
  3. Hint kullanimi sonrasi uygulamayi kapat/ac, hint count'un restore edilip edilmedigini kontrol et.
  4. Fail state sirasinda hint butonuna dokunmayi dene.
  5. Tum harfler acik bir soruda hint kullanmayi dene.
- **Beklenen bozuk belirti:** Hint fail state sirasinda yanlis tetiklenebilir; hint count save'e yansimayabilir; save/restore dongusunde sayac tutarsizlasabilir.
- **Ana rapora eklenecek yer:** Bolum 3 (Gameplay / Hit / Input) veya Bolum 6 (Economy / Store / Energy) altinda yeni madde

---

### PFQA-006 — Hizli ardisik dogru tus basiminda sessiz input kaybi (buffer overflow)

- **Kategori:** Gameplay / Input / UX
- **Oncelik:** P2
- **Olasilik:** Yuksek
- **Neden atlanmis olabilir:** InputBuffer davranisi mevcut raporda hic ele alinmamistir.
- **Muhtemel kok neden:** `InputBuffer.maxCapacity = 3`. 4. ve sonraki basislar `TryAdd()` → `_buffer.Count >= maxCapacity` → false → sessizce yoksayilir. Oyuncu hicbir geri bildirim almaz.
- **Etkilenen dosyalar:** `Scripts/Core/InputBuffer.cs` (maxCapacity = 3), `Scripts/Core/PinLauncher.cs`
- **Manuel cogaltma adimlari:**
  1. Dogru harf tusuna 4 kez hizli bas.
  2. Yalnizca 3 pin yuklenip 4. basimin hicbir geri bildirim olmadan yoksayildigini gozlemle.
  3. 5. kez bas ve geri bildirim (ses, animasyon) var mi kontrol et.
- **Beklenen bozuk belirti:** Oyuncu "Neden basisim calismiyor?" sorusunu sorar; herhangi bir hata gostergesi veya geri bildirim yok.
- **Ana rapora eklenecek yer:** Bolum 3 (Gameplay / Hit / Input / Rotator) altina yeni madde

---

### PFQA-007 — Continue sonrasi ikinci fail modalinda CTA davranisinin dogrulugu

- **Kategori:** Gameplay / Fail Flow / UX
- **Oncelik:** P1
- **Olasilik:** Yuksek
- **Neden atlanmis olabilir:** BUG-026 retry/premium bypass cakismasini ele almaktadir. Ancak ilk continue sonrasi ikinci fail'de modal context'te `usedContinueInCurrentLevel` bilgisinin iletilip iletilmedigi incelenmemistir.
- **Muhtemel kok neden:** `GameManager.EnterFailResolutionState()` → `GameEvents.RaiseFailModalRequested(new FailModalContext {...})`. Bu context'te `premiumContinueAvailable` bulunmakta, `usedContinueInCurrentLevel` BULUNMAMAKTADIR. `FailModalPresenter`, oyuncu daha once continue kullandiysa bu bilgiye context uzerinden ulasamamaktadir. `_usedContinueInCurrentLevel` private field olduğu icin presenter tarafindan sorgulanamaz. Sonuc: FailModalPresenter ilk fail ile ikinci fail icin ayni CTA'lari gosterebilir; tasarim kurali "her level'da 1 continue" ise bu kural UI katmaninda dogrulanamaz.
- **Etkilenen dosyalar:** `Scripts/Core/GameManager.cs` (_usedContinueInCurrentLevel, EnterFailResolutionState, FailModalContext), `Scripts/Presentation/FailModalPresenter.cs`
- **Manuel cogaltma adimlari:**
  1. Bir levelde fail modal'i tetikle.
  2. "Devam Et" (rewarded ad/continue) secenegini kullan.
  3. 1 canla oynamaya devam et, tekrar fail ol.
  4. Ikinci fail modal'inda "Devam Et" butonunun durumunu kontrol et (aktif mi? devre disi mi?).
  5. Premium mode ve free mode'da ayri ayri test et.
- **Beklenen bozuk belirti:** Ikinci fail'de de "Devam Et" butonu aktif gorune, tasarim intent'ine aykiri; oyuncu ayni levelde 2 kez continue kullabilir.
- **Ana rapora eklenecek yer:** Bolum 4 (Question / Fail / Info / Result Flow), BUG-026'nin yanina

---

### PFQA-008 — GrantEnergy() ve TryConsumeEntryEnergy() refill timer sifirlama

- **Kategori:** Economy / Energy / UX
- **Oncelik:** P1
- **Olasilik:** Yuksek
- **Neden atlanmis olabilir:** BUG-044 uzun sureli uyku/uygulama arasi refill hatasini ele almaktadir. Ancak hem enerji harcama hem de enerji grant isleminin timer'i sifirladigi ve birikimli bekleme suresinin kaybolduğu tespit edilmemistir.
- **Muhtemel kok neden:** `EnergyManager.TryConsumeEntryEnergy()` enerjiden 1 azaltir ve `lastRefillUtcTicks = DateTime.UtcNow.Ticks` seter (timer sifirlanir). `EnergyManager.GrantEnergy()` enerji ekler ve yine `lastRefillUtcTicks = DateTime.UtcNow.Ticks` seter. Sonuc: Oyuncu 0/5 enerjide, 25 dakika bekledi (30 dakikalık dolumun 5/6'si gecti). Enerji paketi satin alir → `GrantEnergy()` → timer sifirlanir → 25 dakikalık bekleme kaybolur. Ayni sekilde her enerji harcamasi timer'i sifirlar, yani oyuncu level oynadiktan sonra dolum suresini bastan saymak zorunda kalır.
- **Etkilened dosyalar:** `Scripts/Core/EnergyManager.cs` (TryConsumeEntryEnergy, GrantEnergy, RefillFromElapsedTime)
- **Manuel cogaltma adimlari:**
  1. Enerjini 0'a duşur.
  2. Cihaz saatini 25 dakika ilerlet (veya gercekten bekle).
  3. Enerji paketi satin al (`GrantEnergy` cagrilir).
  4. `lastRefillUtcTicks`'in sifirlandigini, 5 dakikaya degil 30 dakikaya yeniden basladigini dogrula.
  5. Ayni testi level baslat → `TryConsumeEntryEnergy` ile tekrarla.
- **Beklenen bozuk belirti:** Enerji satin alimi beklenen dolum suresini uzatir; oyuncu "dolu oldugum halde neden bekliyorum?" sorusunu sorar.
- **Ana rapora eklenecek yer:** Bolum 6 (Economy / Store / Membership / Energy), BUG-044 revizyonu veya ayri madde

---

### PFQA-009 — QuestionLifeManager.PersistAcrossScenes=true ile GameManager.PersistAcrossScenes=false asimetrisi

- **Kategori:** Boot / Scene / Singleton / Init
- **Oncelik:** P2
- **Olasilik:** Dusuk-Orta
- **Neden atlanmis olabilir:** BUG-004 duplicate singleton riskini ele almaktadir. Ancak kasitli asimetri (bazı singletonlar persist ederken diğerleri etmez) ve bu asimetrinin edge case'lerde dogurabilecegi sorunlar incelenmemistir.
- **Muhtemel kok neden:** `QuestionLifeManager.PersistAcrossScenes = true`, `GameManager.PersistAcrossScenes = false`. Gameplay → Store → geri don: `QuestionLifeManager` yasayor, yeni `GameManager.Start()` calisiyor. Normal restore: `LevelFlowController.RestoreSession()` → `QuestionLifeManager.Instance?.Restore(snapshot.questionHeartsRemaining)` → dogru. ANCAK `RestoreSession()` basarisiz olursa (content bulunamadi), `ResetQuestionHearts()` cagrılmaz ve `QuestionLifeManager` stale hearts degerini korur. `StartLevel()` de basarisiz olursa (content yok) → `LoadLevel()` → `ResetQuestionHearts()` cagrılmaz → HUD stale can sayisi gosterir.
- **Etkilenen dosyalar:** `Scripts/Core/QuestionLifeManager.cs`, `Scripts/Core/GameManager.cs`, `Scripts/Core/LevelFlowController.cs`
- **Manuel cogaltma adimlari:**
  1. Gameplay sahnesindeyken content dosyalarını bos yap ya da kaldir.
  2. Store'dan geri don.
  3. `QuestionLifeManager.CurrentHearts` degerinin ne oldugunu gözlemle.
  4. HUD'da can sayisinin dogru gosterilip gosterilmedigini kontrol et.
- **Beklenen bozuk belirti:** Content yuklenemediginde onceki session'in can degeri yeni (basarisiz) session'da gorunur.
- **Ana rapora eklenecek yer:** Bolum 1 (Boot / Scene / Singleton / Init)

---

### PFQA-010 — Android geri tusu davranisi taниmsiz

- **Kategori:** Navigasyon / Mobil / Platform
- **Oncelik:** P1
- **Olasilik:** Yuksek
- **Neden atlanmis olabilir:** Android platformuna ozgu davranis senaryolari mevcut raporda neredeyse hic yer almamaktadir.
- **Muhtemel kok neden:** Kodda Android geri tusunu yakalayan herhangi bir `Input.GetKeyDown(KeyCode.Escape)` veya platform handler bulunmamaktadir. Unity'nin varsayilan davranisi gameplay sahnesinde geri tusuna basilinca uygulamayi kapatmak olabilir. Fail modal, info card veya result ekrani acikken geri tusuna basilirsa bu modal'lar kapanmak yerine uygulama kapanabilir. Save snapshot alinmadan kapanma → BUG-007 senaryosu tetiklenir.
- **Etkilenen dosyalar:** `Scripts/Core/SceneNavigator.cs` (BackButton handler yok), `Scripts/Core/GameManager.cs`
- **Manuel cogaltma adimlari:**
  1. Android cihazda veya emulator'de oyunu basla.
  2. Gameplay sahnesinde Android geri tusuna bas.
  3. Fail modal acikken geri tusuna bas.
  4. Info card acikken geri tusuna bas.
  5. Result ekraninda geri tusuna bas.
  6. Her durumda uygulamanin ne yaptigini (kapandi mi, modal kapandi mi, hicbir sey olmadi mi) belgele.
- **Beklenen bozuk belirti:** Uygulama beklenmedik anda kapanir; save alinmadan kapanma; modal'lar dismiss edilemez.
- **Ana rapora eklenecek yer:** Yeni Bolum 11 (Platform / Mobile / Android) veya Bolum 8 (UI / Mobile) altina

---

### PFQA-011 — GameManager.HandlePinFlightMiss() icinde hot-path FindObjectOfType

- **Kategori:** Performans / Gameplay
- **Oncelik:** P2
- **Olasilik:** Yuksek
- **Neden atlanmis olabilir:** BUG-071 FindObjectOfType performans riskini genel olarak ele almaktadir. Ancak miss event'inin her tetiklenisinde bu aramanin yapilmasi ayrica not edilmemistir.
- **Muhtemel kok neden:** `GameManager.HandlePinFlightMiss()` icinde: `SlotManager slotManager = FindObjectOfType<SlotManager>();`. Bu, her pin miss'inde sahnenin tamaminda `SlotManager` araması yapar. Yogun gameplay sirasinda (yeni oyuncu, cok hata yapan oyuncu) bu sik sik cagrilir. Dusuk cihazlarda mikroduraklama riskidir.
- **Etkilened dosyalar:** `Scripts/Core/GameManager.cs` (HandlePinFlightMiss)
- **Manuel cogaltma adimlari:**
  1. Kasitli olarak art arda 10+ miss olustur (yanlis slota fir, sureyi kacir).
  2. Dusuk-orta segment Android cihazda FPS'i izle.
  3. Miss anlarina denk gelen frame timing spike var mi kontrol et.
- **Beklenen bozuk belirti:** Miss yogun oyun sirasinda mikroduraklama; Profiler'da GC veya FindObjectOfType spike'lari.
- **Ana rapora eklenecek yer:** Bolum 10 (Release / Performance / Scale Riskleri), BUG-071 revizyonu

---

### PFQA-012 — Result ekraninda hizli cift "Next" basimi — level cift baslatma riski

- **Kategori:** Result / Gameplay / UX
- **Oncelik:** P1
- **Olasilik:** Orta
- **Neden atlanmis olabilir:** Rapid action / double-tap senaryolari mevcut raporda sistematik olarak ele alinmamistir.
- **Muhtemel kok neden:** `ResultPresenter`'daki "Next Level" butonu `StartLevel()` veya `SceneNavigator.OpenGameplayLevel()` cagirır. `GameManager.StartLevel()` icinde "zaten baslatiliyor" koruma mekanizmasi mevcut kodda gorulmemektedir. Cift hizli basista: ilk basim `StartLevel(nextLevelId, false)` → level yuklenir, `SaveManager.Save()` cagrılır. Ikinci basim ayni frame'de `StartLevel()` yeniden cagrilir → `LevelFlowController.LoadLevel()` tekrar calisir → tum cache resetlenir → `GameEvents.LevelStarted` iki kez yayilir.
- **Etkilenen dosyalar:** `Scripts/Presentation/ResultPresenter.cs`, `Scripts/Core/GameManager.cs` (StartLevel)
- **Manuel cogaltma adimlari:**
  1. Bir leveli tamamla, result ekranini ac.
  2. "Next" butonuna cok hizli iki kez bas (double-tap).
  3. Level baslangi durumunu gozlemle.
  4. Enerji, coin ve progress'in tutarli olup olmadigini kontrol et.
- **Beklenen bozuk belirti:** Level iceriginin iki kez yuklenmesi; HUD'da ani flash veya skor sifirlama; ekonomi event zincirinin cift tetiklenmesi.
- **Ana rapora eklenecek yer:** Bolum 4 (Question / Fail / Info / Result Flow), BUG-027'nin yanina

---

### PFQA-013 — Fail modal acikken klavye/input yuzeyinin dokunulabilir kalmasi riski

- **Kategori:** UI / Fail Flow / Input
- **Oncelik:** P1
- **Olasilik:** Yuksek
- **Neden atlanmis olabilir:** BUG-057 fail modal UI overlap riskini gorsel acisindan ele almistir. Ancak input engelleme mekanizmasinin dogrulugu test edilmemistir.
- **Muhtemel kok neden:** `GameManager.SetGameplayInputEnabled(false)` → `InputManager.Instance.enabled = false`. Bu `InputManager` component'ini devre disi birakir. Ancak `KeyboardPresenter` tus basimlarini `UnityEngine.UI.Button` bilesenlerinden aliyorsa ve bu butonlar `InputManager.enabled` kontrolü yapmiyorsa, fail modal arkasinda klavye tuslarına basilabilir. Unity EventSystem UI raycast'i, modal'in arkasindaki nesnelere ulaşabilir (modal tam ekran opak degilse). Ayni risk hint butonu icin de gecerlidir.
- **Etkilenen dosyalar:** `Scripts/Core/GameManager.cs` (SetGameplayInputEnabled), `Scripts/Core/InputManager.cs`, `Scripts/Presentation/KeyboardPresenter.cs`, `Scripts/Presentation/FailModalPresenter.cs`
- **Manuel cogaltma adimlari:**
  1. Fail modal'i tetikle.
  2. Modal acikken klavye tuslarına bas.
  3. `InputManager.enabled=false` iken tus basimlarinin islenip islenmedigini kontrol et.
  4. Hint butonu varsa fail modal acikken hint kullanmayi dene.
  5. Android'de modal arkasindan swipe ile klavye tusuna ulasmayi dene.
- **Beklenen bozuk belirti:** Fail modal acikken klavye tusuna basilinca pin yuklenir veya WrongLetter tetiklenir; can sayisi modal arkasindan degisir.
- **Ana rapora eklenecek yer:** Bolum 4 (Question / Fail / Info / Result Flow), BUG-025'in yanina

---

### PFQA-014 — CanRestoreActiveSession ve CanResumeSavedSession kod tekrari: sessiz divergence riski

- **Kategori:** Mimari / Session / Navigasyon
- **Oncelik:** P2
- **Olasilik:** Orta (gelecekteki kod degisikliklerinde yuksek)
- **Neden atlanmis olabilir:** Kod kalite/mimari riskleri mevcut raporda yalnizca `FindObjectOfType` baglaminda ele alinmistir.
- **Muhtemel kok neden:** `GameManager.CanRestoreActiveSession()` ve `SceneNavigator.CanResumeSavedSession()` neredeyse birebir ayni mantig icermektedir: `hasActiveSession` kontrolu, `levelId > 0` kontrolu, dil eslesme kontrolu. Her ikisi de private static metoddur. Ilerleyen bir refactoring'de (ornegin content version kontrolu eklendiyse) biri guncellenmezse: Navigator "resume et" → `resumeSavedSession=true` → GameManager "restore edemiyorum" → session silinir, yeni level baslar. Bu sessiz uyumsuzluk oyuncu bakis acisindan tespit edilmesi zor bir bug doğurur.
- **Etkilenen dosyalar:** `Scripts/Core/GameManager.cs` (CanRestoreActiveSession), `Scripts/Core/SceneNavigator.cs` (CanResumeSavedSession)
- **Manuel cogaltma adimlari:** Bu senaryo ozellikle statik analiz ile dogrulanir. Runtime test icin: iki metodun logik dallarini birbirinden farkli hale getiren bir kosul (orneg. session version field) ekle ve Navigator vs GameManager kararlarini karsilastir.
- **Beklenen bozuk belirti:** Session silme veya yanlis level baslatma, iki metod birbirine zit karar verdiginde.
- **Ana rapora eklenecek yer:** Bolum 2 (Save / Session / Restore), mimari not olarak

---

### PFQA-015 — LevelFlowController.RestoreSession() basarisizligi + _awaitingFailResolution kilitlenmesi

- **Kategori:** Session / Fail / Init
- **Oncelik:** P1
- **Olasilik:** Dusuk-Orta
- **Neden atlanmis olabilir:** BUG-010 content degisikligi sonrasi session uyumsuzlugunu ele almaktadir. Ancak bu uyumsuzlugun fail state'i kilitli birakması incelenmemistir.
- **Muhtemel kok neden:** `GameManager.Start()` akisi: `levelFlow.RestoreSession(session)` → `LoadLevel(snapshot.levelId)` basarisiz olursa erken donus (level bulunamadi). Ardindan `RestoreContinuationStateFromSession()` yine de calisir. `EnterPendingFailResolutionStateIfNeeded()` → `pendingFailResolution=true` ise → `EnterFailResolutionState(false)` → `_awaitingFailResolution=true` → `SetGameplayInputEnabled(false)` → fail modal acilir. Ancak `CurrentLevelId = 0` (level yuklenemedi). Modal'daki Continue ve Retry butonları calismaz. Input kilitli kalir, oyuncu sikisir.
- **Etkilenen dosyalar:** `Scripts/Core/GameManager.cs` (Start, EnterPendingFailResolutionStateIfNeeded), `Scripts/Core/LevelFlowController.cs` (RestoreSession, LoadLevel)
- **Manuel cogaltma adimlari:**
  1. Bir session olustur, fail state'te kaydet (pendingFailResolution=true).
  2. Content editorunden o leveli sil veya level ID'sini degistir.
  3. Uygulamayi yeniden ac.
  4. Fail modal'in acilip acilmadigini, Continue/Retry butonlarinin calisip calismadığini gozlemle.
- **Beklenen bozuk belirti:** Fail modal acilir, hicbir CTA calismaz; input kilitli kalir, oyuncu uygulamayi kapatıp acmak zorunda kalir.
- **Ana rapora eklenecek yer:** Bolum 2 (Save / Session / Restore), BUG-010'un yanina

---

### PFQA-016 — App background sirasinda fake rewarded countdown'in tamamlanmasi

- **Kategori:** Rewarded Ad / Fail Flow / Yasam Dongusu
- **Oncelik:** P1
- **Olasilik:** Yuksek
- **Neden atlanmis olabilir:** BUG-025 "gec donen callback" senaryosunu ele almaktadir. Ancak background sirasinda coroutine'in tamamlanmasi ve geri donuste callback'in eski state'e uygulanmasi ayrı bir durumu kapsasmaktadir.
- **Muhtemel kok neden:** `DebugRewardedAdPresenter.ShowCountdown()` bir Unity coroutine kullanır. Unity coroutine'leri `OnApplicationPause` sonrasinda uygulama geri geldiginde devam eder (varsayilan davranis). Oyuncu Continue tusuna basip countdown baslasinda uygulamayi arka plana alirsa: OnApplicationPause → TakeSnapshot. Uygulama arka plandayken countdown bitebilir. Geri donuste callback fire olur. Oyuncu fail state'i zaten gecmis olabilir (orneg. sahne degismis). Callback yanlis state'e uygulanir.
- **Etkilened dosyalar:** `Scripts/Presentation/DebugRewardedAdPresenter.cs`, `Scripts/Presentation/FailModalPresenter.cs`, `Scripts/Core/GameManager.cs`
- **Manuel cogaltma adimlari:**
  1. Fail modal acikken "Continue" (Debug Rewarded Ad) tusuna bas.
  2. Countdown basladiktan hemen sonra uygulamayi arka plana al.
  3. Countdown suresi dolana kadar bekle.
  4. Uygulamaya geri don.
  5. Continue callback'inin dogru tetiklenip tetiklenmedigini, game state'in tutarli olup olmadigini gözlemle.
- **Beklenen bozuk belirti:** Arka planda tamamlanan countdown geri donuste yanlis state'e continue uygular; sahne degismisse NullRef veya incorrect state.
- **Ana rapora eklenecek yer:** Bolum 4 (Question / Fail / Info / Result Flow), BUG-025'in yanina

---

### PFQA-017 — Soru tamamlanma ani: TakeSnapshotAfterReveal + HandleQuestionCompleted 1-frame penceresi

- **Kategori:** Save / Session / Gameplay
- **Oncelik:** P1
- **Olasilik:** Orta
- **Neden atlanmis olabilir:** BUG-007 genel throttle riskini ele almaktadir. Ancak son harf reveal'inin hem snapshot hem de completion persistency icerecek sekilde iki ayri kayıt zincirine boluneceği ve bunlarin sirasinin onemine bakilmamistir.
- **Muhtemel kok neden:** Son harf reveal: `RevealNextLetter()` → true → `HandleQuestionCompleted()` → `PersistPendingCompletionState()` senkron calisir ve `pendingInfoCard` / `pendingLevelResult` flag'lerini kaydeder. AYNI ANDA `SessionManager.HandleLetterRevealed()` → `TakeSnapshotAfterReveal()` coroutine 1 frame gecikmeli baslar. Bu 1 frame icinde oyuncu uygulamayi kapatirsa: `OnApplicationQuit()` → `TakeSnapshot()` calısir. Ancak `PersistPendingCompletionState()` zaten senkron calismis olduğundan flag'ler kaydedilmis olabilir. Sorun: `TakeSnapshot()` → `SaveManager.Save()` throttle'da ise yeni snapshot yazilmaz. Oyuncu hem completion state hem de session state'ini barizdan once kaydetmemis olabilir.
- **Etkilenen dosyalar:** `Scripts/Core/SessionManager.cs` (TakeSnapshotAfterReveal), `Scripts/Core/GameManager.cs` (HandleQuestionCompleted, PersistPendingCompletionState)
- **Manuel cogaltma adimlari:**
  1. Bir sorunun son harfini ac.
  2. Reveal animasyonu basladiginda hemen uygulamayi kapat (aynı frame hedefleniyor).
  3. Yeniden ac, pending completion state (info card, result) doğru restore edilip edilmedigini kontrol et.
- **Beklenen bozuk belirti:** Son harf acılmış gorunur ama level complete / info card ekrani restore edilmez; tekrar ayni soruya baslanir.
- **Ana rapora eklenecek yer:** Bolum 2 (Save / Session / Restore), BUG-014'un yanina

---

## 7. Mevcut Senaryolarda Revize Edilmesi Gereken Maddeler

### BUG-007 Revizyonu — Save throttle + quit

Mevcut senaryo throttle penceresini dogru tanimlamistir. Ancak **iki ayri notun eklenmesi** onerilir:

1. `OnApplicationPause` execution order riski: Unity'de `SaveManager.OnApplicationPause()` ve `SessionManager.OnApplicationPause()` hangisi once calisirsa sonuc degisir. Bu baginimlilik ozellikle belgelenmemistir. Senaryo "throttle penceresi" yerine "son snapshot'in diske yazılıp yazilmadigi" ust basligi altinda genisletilmelidir.
2. `OnApplicationQuit()` her zaman tetiklenmez (Android low memory kill, SIGKILL). Bu durum ayrica not edilmelidir.

**Oneri:** "Olasi Semptom" bolumune "Android Recents'tan kaldirma veya low memory kill durumunda da gecerli — OnApplicationQuit garanti degildir" notu eklenmelidir.

---

### BUG-013 Revizyonu — Pending fail state stale

Mevcut senaryo "sahne yenileme veya store donusunde pendingFailResolution flag'i temizlenmeyebilir" riskini ele almaktadir. Ancak su iki ek yolun belirtilmesi gerekmektedir:

1. **Fail modal → Store acilisi:** FailModalPresenter'dan direk store acilip acilmadigini kontrol etmek gerekir. Fail state devam ederken store'a gidilirse `_awaitingFailResolution=true` kalir. Store'dan geri donuste session restore edilir, `EnterPendingFailResolutionStateIfNeeded()` cagrılir → fail modal yeniden acilir. Bu beklenen davranis mi? Bilgi kardi yoktur.
2. **Fail state + Android back tusu:** Oyuncu fail modal acikken Android geri tusuna basarsa uygulama kapanir, `pendingFailResolution=true` kaydedilmis olur, bir sonraki acilista fail modal yeniden gosterilir. Bu beklenen davranis mi? Belirtilmemistir.

**Oneri:** Senaryo "Fail state + Store gecisi" ve "Fail state + Android geri tusu" alt senaryolarini icermek uzere genisletilmelidir.

---

### BUG-025 Revizyonu — Fake rewarded callback gec donus

Mevcut senaryo "callback gecikmesi" uzerinden kurgulanmistir. Iki ek not gereklidir:

1. **Countdown suresi belirtilmemis:** `DebugRewardedAdPresenter`'daki countdown suresi kac saniyedir? Bu bilgi test edilebilirlik icin kritiktir. Manuel cogaltma adimlari countdown suresini belirtmelidir.
2. **Background sirasinda countdown bitimine dair PFQA-016 ile kesisim:** Bu senaryo ile PFQA-016'nin baglantisi kurulmali; iki senaryo capraz referans vermeli.

**Oneri:** Countdown suresini net belirt (saniye cinsinden), background bitimine dair notu ekle.

---

### BUG-029 Revizyonu — Store donusu sonrasi pending UI kaybi

Bu senaryo onemli ve dogru tespittir. Ancak senaryo **yalnizca store yolunu** kapsamaktadir. **OpenMainMenu() yolunun da ayni riski tasidigi** (snapshot almadan navigasyon — PFQA-004) bu senaryonun genislemesi olarak belirtilmelidir.

**Oneri:** "Ayni risk OpenMainMenu() yolu icin de gecerlidir (PFQA-004 bak)" notu eklenmelidir.

---

### BUG-003 Revizyonu — Yanlis gameplay request

Mevcut senaryo "Store'dan belirli bir level icin gameplay request tetikle" demektedir. Bu mekanizma belirsizdir: StorePresenter hangi aksiyonla `OpenGameplayLevel()` cagirıyor? `resumeSavedSession=true` iken `CanRestoreActiveSession()=false` olan path (dil degisikligi durumu) senaryo icinde ayri bir kol olarak belirtilmelidir.

**Oneri:** Hangi UI aksiyonunun gameplay request tetikledigini netlestiriniz. `resumeSavedSession=true` ama restore basarisiz olan yolu ayri adim olarak ekleyiniz.

---

### BUG-026 Revizyonu — Continue/retry premium bypass

`_usedContinueInCurrentLevel` flag'inin `FailModalContext`'e dahil edilmemesi (PFQA-007) bu senaryo ile dogrudan baglantilidir. Mevcut senaryo yalnizca premium bypass'i ele alırken, "ayni levelde birden fazla continue kullanimi" riskini de kapsamalidir.

**Oneri:** PFQA-007 senaryosunu bu maddenin revizyonu olarak ekleyiniz veya "Bagli Senaryolar" bolumune referans veriniz.

---

## 8. Manuel Cogaltma Adimi Yetersiz Kalan Maddeler

### BUG-003 — "Store'dan gameplay request tetikle" belirsiz

Mevcut adim: "Store'dan belirli bir level icin gameplay request tetikle." Bu adim test edicinin hangi butona basacagini belirtmemektedir. `StorePresenter`'in `OpenGameplayLevel()` cagirdigi spesifik UI eleman veya akis tanimlanmalidir. Yoksa test edici bu adimi gerceklestiremez.

**Oneri:** Adimi sunuyla degistirin: "Store ekraninda [belirli bir level acma] veya [geri don] butonuna bas → gameplay sahnesinin yuklenmesini bekle."

---

### BUG-009 — "Reveal frame'ine yakin anda" tam olarak ne anlama geliyor?

Mevcut adim: "Harf reveal frame'ine cok yakin anda uygulamayi kapat." Bu adim zaman penceresi belirsizdir. Testin tekrarlanabilir olmasi icin: "son harfi actiktan sonra ayni frame (yaklasik 16ms) veya 350ms throttle penceresi icinde kapat" seklinde netlestirilmelidir. Ayrica "Nasil kapatacaniz" belirtilmemistir — Android Recents mi, Home + kill mi?

**Oneri:** "Son reveal'dan itibaren 350ms icinde Android Recents'tan uygulamayi swipe ile kapat" seklinde yeniden yaz.

---

### BUG-025 — Countdown suresi belirtilmemis

Mevcut adim: "Countdown bitmeden uygulamayi arka plana al." `DebugRewardedAdPresenter`'daki countdown suresi kodda belirtilmemistir (bize referans uzerinden gorulmemistir). Test edici bu sureyi bilmeden adimi uygulamasi mumkun degildir.

**Oneri:** Countdown suresini koddan tespit edip adima ekleyin. Ornegin: "X saniyeyi bekle, countdown bitiminden 1 saniye once arka plana al."

---

### BUG-027 — "Result ekranina yakin anda kapat" hangi anı hedefliyor?

Mevcut adim: "Level tamamlandiginda result ekranina yakin anda uygulamayi kapat." "Yakin anda" iki farkli anlam tasiyabilir: (a) result ekrani acilmadan hemen once, (b) result ekrani acildiktan hemen sonra. Bu iki an, ekonomi event zincirinin hangi noktasinda oldugunu belirler ve testin sonucu tamamen degisir.

**Oneri:** Adimi ikiye ayirin: (a) "LevelCompleted event'i yayildiktan sonra ama ResultPresenter gorunmeden once kapat", (b) "ResultPresenter gorundurkten hemen sonra kapat."

---

### BUG-040 — "Tekrar default'a don" adimi eksik

Mevcut senaryo FreePlayer veya PremiumPlayer modunu test eder. Son adim "Default moda don, save kontrolu yap" demektedir. Ancak "Default moda nasil donuluyor?" adimi acik degildir. `EconomyBalanceWindow`'da hangi tusun basilacagi, hangi onay adindan gecilecegi belirtilmemistir.

**Oneri:** Adima "EconomyBalanceWindow'u ac → Default modu sec → Aktif Modu Kayda Uygula tusuna bas" yazimini ekleyin.

---

### BUG-068 — "Remote hotfix publish et" production benzeri ortam olmadan test edilemez

Mevcut adim: "Remote hotfix publish et ve refresh uygula." Production ortami yokken bu adim test edicinin takip edebilecegi netlikte degildir. `RemoteContentHotfixWindow` uzerinden editor akisi belirtilmelidir.

**Oneri:** "RemoteContentHotfixWindow'u ac → test JSON payload gir → Publish tiklat → ContentService.RefreshEditorContent() tetiklenir → runtime davranisini incele" sekline cetestirme akisini netlestiriniz.

---

### BUG-044 — Zaman ilerletme mekanizmasi belirtilmemis

Mevcut adim: "Uygulamayi uzun sure kapali birak." Test ortaminda bekleme suresi test verimliligi acisindan kabullenilir degildir. Cihaz saatini ilerletme veya `lastRefillUtcTicks` degerini dogrudan save dosyasinda degistirme alternatifleri onerilmelidir.

**Oneri:** "Uygulamayi kapat → JSON save dosyasinda `lastRefillUtcTicks` degerini `N * refillMinutes` kadar gecmise set et → uygulamayi ac → enerji dolumunu dogrula" adimini ekleyin.

---

## 9. Onceligi Yeniden Degerlendirilmesi Gereken Maddeler

### BUG-043 — "StorePresenter mock bypass, release'te sizma" P0 mu?

Mevcut oncelik: P0

Bu bug, production release gecisinde MockPurchaseService'in devre disi birakilmasi gerektigi gercegini ele almaktadir. Mevcut alpha asamasinda mock kasten kullanilmaktadir — bu bir hata degil, tasarim kararidir. `MARKET_RELEASE_READINESS_PLAN.md`'de bu gecis "Production Provider Gecisleri" asamasinda planlanmistir. Dolayi ile:

- **Alpha / bug sweep bağlaminda:** P2 — "bilinen eksiklik, production oncesi tamamlanacak"
- **Pre-production release baglaminda:** P0 dogru

**Oneri:** Bu madde, sadece "Production Readiness" basligi altinda P0 olarak kalmali; alpha bug sweep matrisinde P2 veya "tasarim geregi eksiklik" olarak isaretlenmelidir.

---

### BUG-047 — "Promo/grant tekrar claim riski" P1 mi?

Mevcut oncelik: P1

Promo orkestrasyonu (`PromoService`, `PromoQueuePresenter`, `PlayerPromoState`) henuz kodda bulunmamaktadir. `MARKET_RELEASE_READINESS_PLAN.md`'de bu sistemin sifirdan kurulmasi gereken bir katman oldugu belirtilmistir. Dolayisiyla bu senaryo mevcut kod uzerinde test edilemez; gelecekte yazilacak kod icin bir tasarim riski notudur.

**Oneri:** Onceligi P3 veya "Gelecek Fazda Izlenmeli" olarak degistirin. Mevcut alpha sweep baglaminda P1 olarak cozum aranmasi yanilticidir.

---

### BUG-074 — "DebugRewardedAdPresenter release'te gorunur kalir" onceligi dogru mu?

Mevcut oncelik: P0

Bu da BUG-043 gibi bilinen bir production gecis gereksinimidir; kasitli alpha davranisidir. `MARKET_RELEASE_READINESS_PLAN.md`'de acikca belirtilmistir.

**Oneri:** Alpha baglaminda P1 veya P2; production release checklist'inde P0 olarak kalmalidir. Mevcut konumlandirma yaniltici degildir fakat not eklenmesi onerilir: "Bu bir alpha-by-design durumudur, production gecisite kapatilacaktir."

---

### BUG-005 — "Missing reference silent fail" P1 cok katı mi?

Mevcut oncelik: P1

`EnsureRuntimePresenters()` metodunda `FindObjectOfType` ile runtime'da presenter bulunamazsa yeni bir host GameObject olusturulur. Bu mekanizma, sahne hiyerarsisi bozulsa bile runtime'da kurtarma saglar. Dolayisiyla "silent fail" daha cok "dinamik olusturma ile kurtarilmis fail" dur. Gercek risk, olusturulan runtime objesinin istenen objeyle tam olarak eslesmemesidir.

**Oneri:** Onceligi P2 olarak degerlendirin ve "Kurtarma mekanizmasi mevcut fakat garanti degildir" notunu ekleyin.

---

### PFQA-003 ve PFQA-013 — Iki P0 senaryosu birbirine cok yakin

PFQA-003 ve PFQA-013 ayni temel riski (ReturnFromStore + dil degisikligi = enerji tüketimi) farkli navigasyon yollarindan ele almaktadir. Her ikisi de P0 olarak isaretlenmistir.

**Oneri:** PFQA-003 ve PFQA-013'u birlesik tek P0 senaryo olarak sunun, "Gameplay'den store'a, store'da dil degistirip geri donme — session ve enerji etkileri" ust basligi altinda. Bu iki senaryoyu test eden tek bir test case daha verimli olacaktir.

---

## 10. Sonuc ve Onerilen Sonraki Adimlar

### Genel Degerlendirme Ozeti

Ana bug sweep raporu mimarinin derinligini dogru yansitan, proje icin ciddi bir baslangic noktasi olusturan kapsamli bir calismadir. 77 senaryo ile ozellikle altyapi, save/session ve content/localization alanlarinda guvenilir bir kapsama saglanmistir.

Bu ikinci goz incelemesi sonucunda **17 yeni senaryo** (PFQA-001 ila PFQA-017) onerildigi, **5 mevcut senaryo icin revizyon** gerektigi, **6 senaryonun manuel cogaltma adimlarinin netlestirilmesi** gerektigi ve **5 senaryonun onceliklendirilmesinin** yeniden ele alinmasi gerektigi belirlenilmistir.

---

### Kritik Bulgular (Oyuncu Etki Sirasi)

**P0 Acil:**
1. **PFQA-003 / PFQA-013:** ReturnFromStore + dil degisikligi → sessiz session kaybi + beklenmedik enerji tuketimi. Oyuncunun mağazadan donerken sessizce enerji kaybetmesi ve session'ini yitirmesi en yuksek oyuncu hayal kirikligi riskidir.
2. **PFQA-002:** OnApplicationPause execution order race — son reveal diske yazilmadan OS kill. Dusuk RAM cihazlarda tekrarlanabilir.

**P1 Yuksek Etkili:**
3. **PFQA-001:** InputBuffer target degisiminde temizlenmemesi — beklenmedik WrongLetter veya otomatik reveal. Her dogru vurus sonrasi gecerli.
4. **PFQA-004:** OpenMainMenu() snapshot almamasi — Store yoluyla gitme kararlastirilmis ama Menu yolu unutulmus. Asimetri yuksek ihtimalle tutarsizlik uretir.
5. **PFQA-005:** Hint mekanizmasinin gameplay'e baglantisinin dogrulugu belirsiz. Ekonomide hint var ama oyun icinde nasil kullanildigi koda yansimiyor.
6. **PFQA-007:** Continue sonrasi ikinci fail modal CTA dogrulugu. _usedContinueInCurrentLevel private field, FailModalContext'e dahil degil.
7. **PFQA-010:** Android geri tusu tanimlanmamis. Her modal durumunda uygulamayi kapatabiliyor.
8. **PFQA-013:** Fail modal acikken klavye dokunulabilir kalmasi. InputManager.enabled=false yeterli mi?
9. **PFQA-015:** RestoreSession basarısızlığı + pendingFailResolution → input kilidi. Oyuncu sikisir.
10. **PFQA-016:** Background sirasinda fake rewarded countdown tamamlanmasi → yanlis state.
11. **PFQA-017:** Son reveal + HandleQuestionCompleted 1-frame penceresi — quit aninda hem completion hem snapshot eksik.

**P1-P2 Dusuk Etki / Dusuk Olasilik:**
12. **PFQA-006:** Buffer overflow sessiz input kaybi — UI geri bildirim eksigi.
13. **PFQA-008:** GrantEnergy refill timer sifirlama — birikimli bekleme kaybi.
14. **PFQA-009:** QuestionLifeManager/GameManager persist asimetrisi — edge case.
15. **PFQA-011:** HandlePinFlightMiss hot-path FindObjectOfType — performans.
16. **PFQA-012:** Result double-tap → cift level baslatma.
17. **PFQA-014:** CanRestoreActiveSession / CanResumeSavedSession divergence riski.

---

### Onerilen Sonraki Adimlar

**1. Oncelikli Dogrulama (hemen):**
- PFQA-003 / PFQA-013 kombinasyonunu manuel olarak cogaltın: Turkce gameplay → Store → Ingilizce → Geri. Enerji duser mi? Session silinir mi? Bu dogrulanirsa P0 olarak ana rapora hemen ekleyin.
- PFQA-001 icin InputBuffer.ClearExpectedLetter() cagrisinin RefreshCurrentTarget() icinde bulunup bulunmadigini dogrulayin. Bulunmuyorsa P1 olarak kesinlesir.
- PFQA-004 icin OpenMainMenu() ve OpenStore() implementasyonlarini karsilastirin. TakeSnapshot cagrisinin asimetrisini teyit edin.

**2. Mevcut Rapor Guncellemeleri:**
- BUG-007'ye: OnApplicationPause execution order notu ve "OnApplicationQuit garanti degildir" uyarisi ekleyin.
- BUG-025'e: DebugRewardedAdPresenter countdown suresini ekleyin.
- BUG-027'ye: "result ekranina yakin anda" adimini ikiye ayirin.
- BUG-043 ve BUG-047 onceliklerini "alpha-by-design" notu ile birlikte P2/P3'e guncelleyin.

**3. Yeni Test Blogu:**
- InputBuffer davranisi icin ayri bir test koşusu olusturun: Hizli multi-tap, target degisimi sonrasi buffer, buffer overflow.
- Android platformu icin platforma ozgu test listesi ekleyin: Geri tusu, low memory kill, warm start davranisi.

**4. Hint Akisini Teyit Edin:**
- `GameManager.cs` ve `GameplayHudPresenter.cs`'de hint butonu → `TrySpendHint()` → reveal zincirini belgeleyin. Eksikse PFQA-005 P0'a yukselir.

**5. Manuel Test Onceligi Sirasi (oyuncu bakis acisina gore):**
Ana raporun onerdigi test sirasina su ek akislari ekleyin:
- Gameplay → Store → Dil degistir → Geri (PFQA-003/013)
- Gameplay → Menu → Tekrar Play (PFQA-004)
- 3+ hizli tus basimi → basarili hit → yeni hedefe gecis (PFQA-001)
- Fail → Continue → 1 can → tekrar fail → modal CTA kontrol (PFQA-007)
- Android geri tusu: gameplay, fail modal, info card, result (PFQA-010)

---

### Bu Raporun Sinirliliklari

- `FailModalPresenter.cs`, `KeyboardPresenter.cs`, `StorePresenter.cs`, `ResultPresenter.cs` ve `DebugRewardedAdPresenter.cs` dogrudan okunmamistir. Bu dosyalardaki UI akislari bazi senaryolar (PFQA-007, PFQA-013, PFQA-016) icin tahmin bazlidir; kod dogrulamasi onerilir.
- Senaryo agirliklandirmasi oyuncu deneyimi odakli yapilmistir; teknik severity QA ekibince yeniden kalibre edilmelidir.
- Bazi senaryolar (PFQA-001, PFQA-003) "kok neden" kisminda kod analizi ile yuksek guvenle desteklenmektedir; diger birkaci (PFQA-005, PFQA-010) daha ziyade "test edilmesi gereken hipotez" niteligi tasimaktadir.

---

*Bu rapor SONNET modeli tarafindan 06.04.2026 itibariyla mevcut kod snapshot'i uzerine uretilmistir. Dogrulanmis bug listesi degildir. Ana QA ekibinin kritik senaryolari manuel olarak teyit etmesi zorunludur.*

