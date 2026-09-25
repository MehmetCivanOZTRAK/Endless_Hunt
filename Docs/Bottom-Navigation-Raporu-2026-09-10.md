# Bottom navigation düzenlemesi — 10 Eylül 2026

Başlangıçta Battle seçili. Başka bir sekmeye dokununca o düğmenin editörde hazırlanan basılı görseli açılır, önceki düğme normal görünümüne döner. Komşu düğmeler, basılı görselin kapladığı alana göre yatayda kayar.

## Yapılan değişiklikler

- Mevcut beş normal düğme ve bunların basılı alt nesneleri bağlandı. `Kilitlibolge` düğmesinin boş kalan bağlantısı mevcut koleksiyon/cephanelik sayfasına bağlandı.
- DOTween ile 0,22 saniyelik `OutCubic` yatay geçiş eklendi. Süre, `SurvivorMenu > Navigation Duration` alanından değiştirilebilir.
- Hesaplama, editördeki genişlikleri, basılı görsellerin sağ/sol ofsetlerini ve düğme aralıklarını kullanıyor. Sekmelerin görsel sırası, sayfa indekslerinden bağımsız.
- Aynı sekmeye tekrar dokunmak geçişi yeniden başlatmıyor. Hızlı tıklamalar önceki hareketi kesip son seçime gidiyor; Battle'a dönüşte başlangıç konumları korunuyor.
- Sabit 24 birim yukarı taşıma, temadan navigation sprite atama ve buton rengini/transition ayarını başlangıçta değiştirme kaldırıldı.
- Envanter butonlarını açılışta tekrar kaplayan işlem kaldırıldı. Mevcut TMP yazıların editörde seçilen font ve materyallerinin topluca yeniden atanması kaldırıldı.

## Korunan tasarım ve oyun davranışı

`MainMenu.unity`, navigation bağlantıları ve geçiş süresi çıkarıldığında düzenleme öncesi yedekle **bayt düzeyinde aynı**. Top bar, sprite, renk, font, boyut, anchor, pivot, ölçek ve hiyerarşi alanlarına müdahale edilmedi. Basılı görsellerin dikey konumları editördeki değerlerden geliyor.

Karakterin ve ekipmanların gerçek durumu kayıt sisteminden gelmeye devam ediyor. Editörde görünen deneme kıyafetleri veya slot içerikleri başlangıç ekipmanı olarak kaydedilmedi; mevcut başlangıç silahı ve boş kıyafet davranışı korunuyor.

## Doğrulama

- Unity Play Mode: **37 başarılı kontrol, 0 oyun hatası**.
- Beş sekmenin dokunma alanları ve ekipmanın basılı görseline tekrar dokunma doğrulandı.
- Normal/basılı görünüm, sayfa seçimi, yerleşim ve tasarım değerlerinin korunması kontrol edildi.
- Hızlı sekme değişimleri ve `Time.timeScale = 0` sırasında geçişin tamamlanması doğrulandı.
- Android oyuncu C# derlemesi başarılı. Bu kontrolde APK üretilmedi ve fiziksel telefonda test yapılmadı.
- 540×1110, 540×1200 ve 768×1024 görüntüleri incelendi. Testler ayrı doğrulama projesinde çalıştırıldı; teslim edilen dosyalar test edilen kopyayla aynı.
- Doğrulama kopyasında Unity'nin arama indeksine ait bir editör tanısı görüldü; oyun kodundan kaynaklanan hata bulunmadı.

Kanıtlar: `Logs/navigation-verification.txt`, `Logs/navigation-preservation.txt`, `Logs/navigation-android-script-check.txt`.

Görüntüler: `Logs/DemoPreviews/nav-01-battle.png`, `nav-02-equipment.png`, `nav-03-shop.png`, `nav-04-upgrades.png`, `nav-05-arsenal.png`, `nav-06-tablet.png`, `nav-07-tall.png`.

Sahne yedeği: `Logs/NavigationBackup-20260910/MainMenu.unity`.

Ana kod: `Assets/EndlessHunt/UI/AuthoredBottomNavigation.cs`. Referanslar: `SurvivorMenu > Tab Pressed Visuals`, sırasıyla Shop / Equipment / Battle / Upgrades / Arsenal.
