# CODEX HIGH MINI WORKFLOW

Tarih: 21 Nisan 2026  
Amac: Bu dosya, bu repo icindeki kalici Codex calisma protokoludur. `GPT-5.4 high` tarafinin mimari/dogrulama rolu ile `GPT-5.4 mini xhigh` tarafinin uygulama rolunu netlestirir. Bu dosya, ileride "is akisimi kontrol et", "mevcut pipeline'i teyit et", "mini modele prompt hazirla" gibi taleplerde birincil referans olarak okunur.

## 1. Rol Dagilimi

### 1.1 GPT-5.4 high

Bu thread icindeki ana muhendis, mimar ve denetleyici roldur.

Sorumluluklari:

- repo ve ilgili dosyalari taramak
- mevcut durumu dogrulamak
- kok nedeni tespit etmek
- dokunulacak ve dokunulmayacak alanlari netlestirmek
- performans, teknik borc ve mimari butunluk sinirlarini koymak
- `GPT-5.4 mini xhigh` icin uygulama promptu yazmak
- uygulama ciktilarini dosya ustunden tekrar dogrulamak
- gerekirse ilgili `.md` dokumanlarini guncellemek

Yapmayacagi seyler:

- dogrulamadan sonucu kabul etmek
- mini modelin karar vermesine izin vermek
- gereksiz oneriler uretmek
- kapsam disi yaratici degisiklikler istemek

### 1.2 GPT-5.4 mini xhigh

Bu rolde calisan model uygulayici muhendistir.

Sorumluluklari:

- `GPT-5.4 high` tarafindan verilen sinirli promptu uygulamak
- yalnizca izin verilen dosyalara dokunmak
- istenen teknik hedefi uygulamak
- uygulama sonrasi kisa degisiklik raporu vermek

Yapmayacagi seyler:

- kendi basina mimari karar vermek
- kapsam genisletmek
- promptta yazmayan sistemlere mudahale etmek
- "gibi gorunen" ama arkada kirli cozumler birakmak

## 2. Temel Ilke

Bu repo icinde karar verici taraf `GPT-5.4 high`, uygulayici taraf `GPT-5.4 mini xhigh` kabul edilir.

Kural:

1. once high analiz eder
2. sonra high prompt yazar
3. mini uygular
4. sonra high dosya ustunden dogrular
5. ancak ondan sonra sonuc kabul edilir

## 3. Standart Is Akisi

Her teknik gorevde varsayilan sira:

1. mevcut durumu tara
2. ilgili dosyalari dogrula
3. kok nedeni cikar
4. risk sinirlarini koy
5. dokunulacak alanlari yaz
6. dokunulmayacak alanlari yaz
7. tek dogru uygulama yolunu belirle
8. mini xhigh icin prompt hazirla
9. mini cikisini tekrar dogrula
10. gerekiyorsa dokumani guncelle

## 4. Prompt Uretim Kalibi

`GPT-5.4 high` tarafindan verilecek uygulama promptlari asagidaki cekirdege sadik kalir:

1. gorevin tek cumlelik net tanimi
2. proje yolu
3. kesin hedef
4. dogru davranis
5. yanlis davranis
6. dokunulacak dosyalar
7. dokunulmayacak dosyalar ve sistemler
8. teknik uygulama kurallari
9. performans / teknik borc / runtime yuk sinirlari
10. dogrulama adimlari
11. is sonunda beklenen rapor formati

Kural:

- promptta belirsiz ifade olmayacak
- "istersen", "gerekirse dusun", "opsiyonel olarak" gibi gevsek dil kullanilmayacak
- mini modelin yorum alanini gereksiz yere acmayacak kadar net olunacak

## 5. Dogrulama Kalibi

Mini model uygulama sonrasi `GPT-5.4 high` su sirayla kontrol eder:

1. compile blocker kaldi mi
2. dokunulmamasi gereken sistemlere dokunuldu mu
3. hedeflenen dosyalarda degisiklik mantikli mi
4. scene/prefab/override kirliligi kaldi mi
5. runtime yuk olusturan yeni davranis eklendi mi
6. teknik borc veya cift mimari kaldi mi
7. sonuc gercekten istendigi gibi mi

Bir cozum ancak bu kontrollerden sonra "tamam" kabul edilir.

## 6. Ozellikle Yasaklanan Cozum Tipleri

Bu repo icinde su cozumler kabul edilmez:

- gecici gorunen workaround
- yalnizca sahne YAML regex ile toparlama
- `OnValidate` / `ExecuteAlways` icinde hierarchy uretme
- runtime'da gecici obje olusturarak gorsel gizleme
- ayni problem icin iki farkli mimariyi ayni anda tutma
- mini modelin kendi yorumuyla kapsam genisletmesi

## 7. Ne Zaman High Dogrudan Is Yapar

Normal kural mini modelin uygulamasidir. Ancak su durumlarda `GPT-5.4 high` dogrudan is yapabilir:

- yuksek riskli mimari degisiklik
- sahne/prefab veri butunlugu riski
- onceki mini uygulamalarin kirlilik biraktigi durum
- dokuman/plan/rapor guncellemesi
- uygulama degil sadece kesin dogrulama gereken kritik anlar

## 8. Unity / UI / Preview Ozel Kurallari

Bu repo icinde UI preview ve tasarim kurulumu icin ek kurallar:

- `HubPreview` sandbox/test sahnesidir
- onaylanmamis preview dogrudan production kabul edilmez
- oran/boyut sorunu varsa sira:
  1. asset alpha ve bos canvas
  2. prefab rect/gorsel katman
  3. builder uretimi
  4. en son scene rebuild
- scene YAML toplu regex duzenleme son care degil, varsayilan yol hic degildir
- controller runtime davranisindan sorumludur
- hierarchy uretimi builder/editor katmaninda kalir

## 9. Kisa Tetikleyici Cumleler

Kullanici su tip ifadeler kullanirsa bu dosya referans alinacak:

- "is akisimi kontrol et"
- "mini modele prompt ver"
- "pipeline'i teyit et"
- "bir sey yapmadan once sureci kontrol et"
- "buna gore prompt hazirla"

Bu durumda once bu dosyanin mantigi uygulanir.

## 10. Guncelleme Kurali

Bu dosya statik degildir. Is akisinda kalici degisiklik olursa bu dosya mutlaka guncellenir.

Zorunlu guncelleme durumlari:

- yeni model rol ayrimi
- prompt kalibinin degismesi
- dogrulama sirasinin degismesi
- mini model yerine baska uygulayici model kullanilmaya baslanmasi
- preview / production promote kurallarinin degismesi

## 11. Nihai Kural

Bu repo icinde:

- karar verici = `GPT-5.4 high`
- uygulayici = `GPT-5.4 mini xhigh`
- kabul mekanizmasi = tekrar dogrulama

Dogrulanmamis hicbir uygulama tamam sayilmaz.

## 12. HubPreview Ray State Koruma Kuralı

`HubPreview` level hub uzerinde gorsel edit yapilirken `railPoints` verisinin kaynagi scene controller instance'idir.

Bu nedenle:

- `RebuildLevelHubPreviewScene()` rebuild baslamadan once mevcut `HubPreview` controller state'ini capture eder
- `BuildLevelHubPreviewScene(...)` yeni scene/prefab instance kurulduktan sonra bu state'i geri apply eder
- broken-scene restore akisi da ayni capture/apply ilkesini kullanir
- bu nedenle plain rebuild artik tek basina ray state'i sifirlayan yol olarak kabul edilmez

Calisma disiplini:

1. once sorunun plain presentation mi, yoksa scene recovery mi oldugu ayrilir
2. presentation ise once mevcut scene instance uzerinde calisilmasi tercih edilir
3. rebuild gerekiyorsa, `CaptureLevelHubPreviewState()` -> `BuildLevelHubPreviewScene(...)` -> `ApplyLevelHubPreviewState()` zinciri dosya ustunden dogrulanmadan mini modele uygulama promptu verilmez

## 13. Editor-Side Gorev Paketleme KuralÄ±

22 Nisan 2026 editor-side tuning ve preview araclari sirasinda mini modelin ayni prompt icinde birden fazla bagli editor davranisini degistirdigi durumlarda regresyon urettigi dogrulandi.

Bu nedenle:

- mini modele tek promptta ayni anda `editor workflow + prefab source apply + scene rebuild + multi-element tuning + context finder` sinifi bagli degisiklikler verilmez
- `HubPreview` ray editoru ile layout tuning ayni promptta birlestirilmez
- editor-side gorevler asagidaki siniflara ayrilarak ayrica promptlanir:
  - `workflow semantics`: capture/apply/rebuild sozlesmesi
  - `element tuning`: tek UI elemaninin layout davranisi
  - `editor visualization`: scene view cizimi / handle gorunumu
  - `selection/picking`: kullanicinin yanlis secimini engelleme
- mini model ciktisi `validation clean` dese bile compile blocker ve davranis tekrar high tarafindan dosya ustunden dogrulanmadan kabul edilmez
- `Capture` ile `Rebuild` birbirine baglanan editor araclari yuksek riskli kabul edilir; once sozlesme ayrica dogrulanir
- yeni UI tuning araclari icin sira:
  1. data contract
  2. editor capture/apply
  3. rebuild entegrasyonu
  4. UX polish
- `MainMenu` ve `HubPreview` gibi farkli preview context'leri tek modulde yonetiliyorsa finder/context dogrulamasi ayri gorev paketi kabul edilir

Sonuc:

- mini model editor-side coupled task gorevlerinde daha dar paketlerle kullanilir
- high taraf ray editor, layout tuning ve rebuild akisini birbirine baglamadan ayri promptlar yazar

## 14. Model Secim ve Maliyet KuralÄ±

22 Nisan 2026 itibariyle uygulayici model secimi yalniz teknik zorluga gore degil, premium istek maliyeti ve gorevin prompta guvenli bolunebilirligine gore yapilir.

Sabitler:

- `GPT-5.3 Codex high` = `1.0` premium istek + GitHub tarafinda belirsiz token algoritmasi
- `GPT-5.4 mini xhigh` = `0.33` premium istek

Premium istek matematigi:

- `1` codex prompt = `1.0`
- `2` codex prompt = `2.0`
- `3` codex prompt = `3.0`
- `1` mini prompt = `0.33`
- `2` mini prompt = `0.66`
- `3` mini prompt = `0.99`
- `4` mini prompt = `1.32`
- `5` mini prompt = `1.65`
- `6` mini prompt = `1.98`

Dogru ekonomik esik:

- `1` codex prompt yaklasik `3` mini prompta denktir
- `2` codex prompt yaklasik `6` mini prompta denktir
- codex, yalniz `4+ mini promptta daha ekonomik` diye secilmez; once gercek prompt sayisi hesaplanir

Kalite/risk duzeltmesi:

- premium matematigi mini modeli avantajli gosterse bile, gorev `coupled editor workflow`, `scene/prefab veri butunlugu`, `capture+rebuild semantics`, `multi-context finder`, `compile blocker riski yuksek editor task` sinifindaysa `GPT-5.3 Codex high` tercih edilir
- model secimi yalniz maliyetle degil, `beklenen prompt sayisi x regresyon riski` ile yapilir

Uygulama kurali:

1. high once gorevi siniflandirir
2. gorevin mini ile guvenli sekilde kac promptta bitecegini tahmin eder

3. sonra su karari verir:
   - `<= 3 mini prompt` ve dusuk/orta bagimlilik = `GPT-5.4 mini xhigh`
   - `4-6 mini prompt` bekleniyorsa maliyet avantaji hala cogu durumda `GPT-5.4 mini xhigh` tarafindadir; burada karar regresyon riskiyle verilir
   - `1 codex prompt` ile bitecek ama mini tarafta yinelemeli deneme riski yuksekse `GPT-5.3 Codex high`
   - `2+ codex prompt` gerekecek islerde codex secimi ancak kalite/risk gerekcesiyle yapilir; maliyet avantaji varsayilmaz
4. bu karar kullaniciya net ve kisa bicimde soylenir

Not:

- `GPT-5.4 mini xhigh` hiz/maliyet odakli varsayilan uygulayicidir
- `GPT-5.3 Codex high` ise daha pahali ama daha az promptta ve daha yuksek bagli gorev toleransiyla secilen agir uygulayicidir

## 15. Gunluk Tutma KuralÄ±

22 Nisan 2026 itibariyle her calisma gunu icin `Assets/WordSpinAlpha/Docs/Gunlukler` altinda tarih bazli bir `.md` dosyasi tutulur.

Format:

- dosya adi = `YYYY-MM-DD.md`
- o gunun icerigi iki ana bolumle yazilir:
  - `Birinci 5 Saatlik Oturumda Yapilanlar`
  - `Ikinci 5 Saatlik Oturumda Yapilmasi Planlananlar`

Gunluk dosyasinin amaci:

- o gun yapilan teknik isi tekrar taramadan hatirlatmak
- hangi degisikliklerin neden yapildigini kaydetmek
- ayni sorunun ertesi gun yeniden kesfedilmesini azaltmak
- mimari karar zincirini ve acik isleri kaybetmemek

Yazim kurali:

- gunlukte yalnizca o gun dogrulanan teknik degisiklikler yazilir
- her madde icin `ne yapildi` ve `neden yapildi` net sekilde kaydedilir
- acik kalan isler bir sonraki oturum bolumune tasinir
- gun sonu kontrolunde dosya tekrar taranir ve gerekiyorsa guncellenir

Kullanim kurali:

1. high once o gunun degisikliklerini dosya ustunden tarar
2. sonra gunlugu olusturur veya gunceller
3. birinci oturumda dogrulanmis isleri kaydeder
4. ikinci oturumda planlanan acik maddeleri ayri yazar

Sinir:

- gunluk, ham sohbet ozeti degildir
- gunluk, teknik referans dosyasidir
- bu nedenle yalnizca o gunun isini, nedenini ve acik kalan planini tasir
