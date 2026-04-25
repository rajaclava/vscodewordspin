# FOUNDATION-FIRST EXECUTION PLAN (Demo 25 Level, Pre-Gameplay UI)

## Ozet
Bu planin amaci, tasarim uretimine geri donmeden once veri dayanikliligi, telemetry iskeleti ve editor otomasyon davranisini karar-komple hale getirmektir. Plan, gameplay ekranini bilerek en sona birakarak mekanik/slot-zorluk eksenindeki acik kararlari erken kilitlememeyi esas alir. Bu dokuman hem Codex hem baska model tarafindan tek basina takip edilebilir olacak sekilde faz, cikti, karar kapilari ve ongoru referanslari icerir.

Hedef dokuman yolu: `Assets/WordSpinAlpha/Docs/FOUNDATION_FIRST_EXECUTION_PLAN.md`

## Fazli Uygulama Plani

### Faz 0: Karar Kilidi ve Is Akisi Protokolu
1. Bu fazda ekip protokolu yazili kilitlenir: Codex kod-gercekligi raporu uretir, dis model (GPT-5.4/5.5) mimari trade-off karari verir, karar tekrar Codex ile uygulanabilirlik/risk filtresinden gecer.
2. Cikis dokumani: `DECISION_PROTOCOL.md` ve `OPEN_QUESTIONS.md`.
3. Etkilenmesi muhtemel referans noktalari: `SceneBootstrap.cs`, `WordSpinAlphaSceneBuilder.cs`, `TelemetryService.cs`.

### Faz 1: Save ve Telemetry Dayaniklilik Sertlestirmesi
1. `SaveManager` ve `TelemetryService` yazimlari atomik write pattern'e gecirilir: temp dosyaya yaz, dogrula, replace et; hata halinde onceki dosya korunur.
2. I/O hata davranisi tek standartta toplanir: sessiz yutma yok, kontrollu uyari ve fallback.
3. Flush stratejisi korunur ama deterministik hale getirilir: pause/quit force flush, normal akis throttled flush.
4. Cikis dokumani: `PERSISTENCE_HARDENING_NOTES.md` ve senaryo bazli dogrulama ciktisi.
5. Etkilenmesi muhtemel referans noktalari: `SaveManager.cs`, `TelemetryService.cs`, `PlayerSaveModels.cs`.

### Faz 2: Metric Sozlesmesi Cekirdek Iskeleti (Detay Kilitlemeden)
1. Bu fazda gameplay detay metrikleri kilitlenmez; sadece degismeyecek cekirdek event envelope sabitlenir.
2. Cekirdek alanlar: `event_name`, `event_version`, `timestamp_utc`, `session_id`, `level_id`, `language_code`, `app_build`, `payload`.
3. Event adlandirma standardi tek kurala baglanir ve string birlestirme yerine merkezi helper ile uretilir.
4. Gameplay'e ozgu detay alanlar `DEFERRED_METRICS_BACKLOG.md` dosyasina alinir.
5. Etkilenmesi muhtemel referans noktalari: `GameEvents.cs`, `MetricLogger.cs`, `GameManager.cs`.

### Faz 3: Editor Otomasyon Davranisinin Merkezilestirilmesi
1. Daginik `InitializeOnLoad`, `delayCall`, scene hook davranislari tek bir `EditorAutomationConfig` kaynagina baglanir.
2. Gecici `return` ile kapatilmis davranislar config flag'e tasinir; gizli davranis yerine acik policy uygulanir.
3. `Ctrl+S ne ise o kalir` prensibi HubPreview icin standart hale getirilir.
4. Cikis dokumani: `EDITOR_AUTOMATION_POLICY.md`.
5. Etkilenmesi muhtemel referans noktalari: `HubPreviewSceneNormalizer.cs`, `HubPreviewLayoutTuningProfileEditor.cs`, `WordSpinAlphaSceneBuilder.cs`.

### Faz 4: UI Tasarim Uretimine Donus (Gameplay Haric)
1. HubPreview ve diger sayfa tasarimlarina geri donulur; pipeline “asset at, sahnede gor, surukle-birak, Ctrl+S” akisinda calisir.
2. `TopBarWidget`, `Alttas`, `BottomPageNav` gibi kalici UI katmanlari prefab butunluguyle korunur.
3. Cikis dokumani: `UI_PRODUCTION_CHECKLIST.md`.
4. Etkilenmesi muhtemel referans noktalari: `LevelHubPreviewController.cs`, `HubPreviewTopBarAutoSync.cs`, `LevelHubPreview.prefab`.

### Faz 5: Gameplay Ekrani Finalizasyonu (Bilincli Olarak En Son)
1. Zorluk kurgusu, slot sekil dili ve gorsel uyum karari netlesmeden gameplay ekran tasarimi baslamaz.
2. Gameplay kararlari netlesince detay metric sozlesmesi finalize edilir.
3. Bu faz ayri karar kapisiyla acilir; onceki fazlar tamamlanmadan baslatilmaz.
4. Etkilenmesi muhtemel referans noktalari: `LevelFlowController.cs`, `GameManager.cs`, `TelemetryModels.cs`.

## Arayuz/Sozlesme Degisiklikleri
1. Persistence davranisi icin tek teknik kural: atomik write + kontrollu fallback.
2. Telemetry icin `EventEnvelope` cekirdek sozlesmesi: event adi ve payload versiyonlamasi zorunlu.
3. Editor otomasyonlari icin tek config kapisi: hook davranislari runtime yerine policy ile ac/kapa.

## Test ve Kabul Kriterleri
1. Save dayaniklilik testi: hizli art arda save, pause, quit, yeniden acilis senaryolarinda veri kaybi olmamali.
2. Telemetry dayaniklilik testi: queue/snapshot dosyasi bozuldugunda servis fallback ile ayaga kalkmali.
3. Metric cekirdek testleri: ayni event farkli modulden uretildiginde sema uyumlu olmali.
4. Editor otomasyon testleri: HubPreview’de manuel yerlesim `Ctrl+S` sonrasi geri yazilmamali.
5. UI donus testi: tasarim PNG import sonrasi sahnede otomatik gorunurluk ve play yansimasi korunmali.

## Karar Kapilari ve Sira Kilidi
1. Faz 1 tamamlanmadan Faz 2 baslayamaz.
2. Faz 2 cekirdek iskeleti tamamlanmadan Faz 3'e gecilmez.
3. Faz 3 policy dogrulanmadan UI uretim fazi acilmaz.
4. Gameplay ekran fazi yalnizca urun kararlari kilitlenince acilir.

## Varsayimlar ve Varsayilan Secimler
1. Demo kapsami 25 level ve gameplay temel dongusu muhurlu kabul edilir.
2. Gameplay detay metrikleri erken kilitlenmeyecek, backlog’da tasinacaktir.
3. HubPreview uretim sahnesi olarak kullanilmaya devam eder.
4. Kod degisiklikleri “az ama deterministik” prensibiyle yapilir; buyuk refactor bu planin disinda tutulur.
5. Bu plan karar-kompledir; implementer ek mimari karar uretmeden dogrudan uygulayabilir.
