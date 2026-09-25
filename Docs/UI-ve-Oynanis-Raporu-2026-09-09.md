# Endless Hunt — UI ve oynanış revizyonu

Bu çalışma, projedeki Vector UI Pack demo tasarımları ve son gönderdiğin istekler üzerinden yapıldı. Mağaza ve sıralama, seçtiğin şekilde **yerel kayıt ve test satın alma** olarak çalışır.

## Korunan düzenlemelerin

- Elle hazırladığın top barın konumları, boyutları, ölçekleri ve sprite bağlantıları korundu. Değişiklik öncesi/sonrası karşılaştırması `Logs/topbar-preservation.txt` dosyasında.
- Koyduğun alt bar düğmeleri kullanıldı; normal ve basılı durumları paketin kendi sprite'larına bağlandı. Seçili düğme DOTween ile 24 referans piksel yukarı çıkar.
- Ana ekrana kaldırdığın oyun adı geri eklenmedi.
- İlk harita açık, sonraki haritalar kilitli. Ok düğmeleri gizlendi; harita kartı parmakla kaydırılır.
- Mevcut sonsuz haritanın 35 parçası korundu.
- Sildiğin `DUSMAN_ASSETS2` klasörü geri getirilmedi.
- Başlangıçta kıyafetsiz karakter ve başlangıç silahı korunur. Önceden seçilmiş ekipmanlar silinmez. Ekipman kataloğunun tamamı denemeye açıktır.

## Font ve menü tasarımı

Tüm görünür metinler `Assets/Fonts/myriad-pro/MYRIADPRO-BOLD SDF.asset` kullanır. Beyaz metinlerde koyu yuvarlak kontur ve aşağı yönlü gölge; krem panellerde koyu mürekkep rengi kullanılır. Türkçe karakterler için dinamik SDF üretimi açık. Ana sahne ve oyun HUD metinleri Edit Mode'da da bu fontla görünür.

Eski scriptlerin metin bağlantılarını bozmamak için `DemoTypography`, mevcut Text değerlerini TMP metinlerine aktarır. Görüntüyü TMP çizer; eski Text bileşenleri çizim yapmaz.

Paket referansı: [Vector UI Pack — dobo_ui](https://dobo-ui.itch.io/vector-ui-pack). Paket açıklamasında demo fontu Myriad Pro olarak belirtiliyor.

### Demo pencereleri

- **Haftalık ödül:** pembe defter başlığı, 3×2 günlük kart, geniş 7. gün kartı ve aşağıdaki yuvarlak kapatma düğmesi. Alınabilir gün yeşil, alınmış gün işaretlidir.
- **Çıkış:** koyu başlık, krem gövde, pembe Çık ve camgöbeği Vazgeç düğmesi.
- **Ödüller:** koyu kompakt panel, camgöbeği kurdele başlık, kazanılan para/enerji ikonları ve miktarları.
- Açılışlarda kısa ölçek ve saydamlık geçişleri kullanılır. Sürekli parlama animasyonu yoktur.

### Alt bar ve mağaza

Alt bar: mağaza, ekipman, savaş, gelişim ve silah koleksiyonu. Mağaza para paketlerini, koleksiyon mevcut silahları gösterir.

| Kategori | Paket miktarları |
| --- | --- |
| Elmas | 80 / 500 / 1.200 / 2.500 / 6.500 / 14.000 |
| Altın | 500 / 1.500 / 5.000 / 12.000 / 30.000 / 80.000 |
| Enerji | 5 / 10 / 20 / 40 / 80 / 160 |

Satın alma düğmeleri test işlemi yapar ve miktarı aktif profilin bakiyesine ekler. Gerçek ödeme, IAP ürünü veya fiyatlandırma bağlantısı yoktur. Enerji paketleri normal dolum sınırının üzerine eklenebilir; otomatik dolum 20 enerjide durur.

Top bardaki enerji, altın ve elmas değerleri ilgili mağaza kategorisine gider. İsme veya avatara dokunmak profil penceresini açar. Top barın görsel yerleşimi bu işlemler için değiştirilmedi.

### Ekipman slotları

Yeni `DEMO_Fantasy_RPG_UI_doboui/Item_slots` klasöründeki gri, yeşil ve gök mavisi slotlar ile seçili sürümleri kullanılır. Mevcut katalogda nadirlik alanı bulunmadığı için fiyat basamakları nadirlik eşlemesi olarak kullanıldı: 300 altı gri, 300–649 yeşil, 650 ve üzeri mavi. Kıyafet, silah envanteri ve koleksiyon aynı eşlemeyi kullanır.

### Yetenek ağacı

Üç bağımsız dalda toplam **72 kalıcı düğüm** var. Her düğüm kendi dalındaki önceki düğüme bağlıdır. Yukarı kaydırdıkça devamı görünür. Açılabilir düğüme dokunmak yeterlidir; yanında altınla satın alma düğmesi yoktur.

Tekrarlanan geliştirmeler:

- Azami can +10.
- Hasar +%4.
- Hareket hızı +%2.
- Toplama alanı +%5.
- Alınan hasar −%2; toplam azaltma %65 ile sınırlı.
- Her 10 saniyede +1 can yenilenmesi.

Bunlar yeni koşunun başlangıç değerlerine uygulanır. Ekipman ekranındaki can ve saldırı değerleri de aynı kalıcı yetenekleri hesaba katar.

## Profil, ayarlar ve yerel kayıt

- İsim değiştirme ve beş hazır avatar seçimi.
- Aynı cihazda en fazla sekiz bağımsız profil; yeni kayıt oluşturma ve profil değiştirme.
- Profillerin bakiyeleri, ekipmanları, harita ilerlemesi ve rekorları ayrı tutulur. İlk mevcut profilin önceki kaydı korunur.
- Liderlik tablosu ana ekranın sağ üst simgesinden açılır. Bu cihazdaki profilleri en yüksek skora, eşitlikte en yüksek dalgaya göre sıralar. Örnek/sahte çevrim içi oyuncu eklenmedi.
- Seçenekler menüsünde Ayarlar, Mesajlar, Profil, Haftalık Ödül ve Çıkış bulunur.
- Ayarlar: genel ses seviyesi, titreşim, hasar sayıları ve 30/60 FPS tercihi.
- Mesajlar ekranında yerel karşılama ve oynanış bilgisi var; çevrim içi mesajlaşma servisi değildir.
- Haftalık ödül günde bir kez alınır; gün kontrolü UTC tarihine göre yapılır. Yedinci ödülden sonra döngü başa döner.

Profil/ayar/yetenek/sıralama verisi `Application.persistentDataPath/hunters-v1.json` içinde tutulur. Yazım geçici dosya üzerinden yapılır ve `.bak` yedeği saklanır. Bakiyeler ve mevcut ekipman ilerlemesi profil kimliğiyle ayrılmış PlayerPrefs anahtarlarını kullanır.

## Oyun içi değişiklikler

### HUD ve kontrol

- Paketin gerçek bar sprite'larıyla can, seviye deneyimi ve elmas ödül çubuğu.
- Can önce azalır; sarı hasar izi gecikmeli yetişir. Hasarda kısa kırmızı ekran etkisi ve isteğe bağlı hasar sayıları vardır.
- Elmas ödül çubuğu dolunca üç seçenek çıkar. Seçimler silah ve can/hasar/hız/menzil/toplama geliştirmelerini birlikte sunar.
- Silahlar ve seviyeleri küçük bir şeritte görünür. Bu şerit açılır pencerelerin arkasında kalır.
- Dalga numarası, süre ve skor gösterilir. Yeni dalgaya beş saniye kala geri sayım, başlangıcında dalga bildirimi çıkar.
- Duraklatma ve çıkış pencereleri ortada kompakt kartlardır; oyun arkada kararır.
- Joystick başlangıçta gizlidir; oyun alanında dokunulan noktada açılır. Parmağı bırakınca hareket sıfırlanır.
- Uygulama arka plana alındığında aktif koşu duraklatılır.

### Goblinler ve elmaslar

`DUSMAN_ASSETS` içindeki Male Goblin, Female Goblin ve Chief Goblin kullanılır. Üç tür için toplam 162 seçilmiş kare, üç mobil atlasa toplandı: yönlü koşu, saldırı, hasar alma ve ölüm animasyonları. Kaynak paket dosyaları korunur; atlasları yeniden üretmek için `Tools/pack_goblins.py` kullanılabilir.

- Dalga 1–2: Male Goblin.
- Dalga 3–4: Male/Female karışımı.
- Dalga 5 ve sonrası: üç türün karışımı; her beşinci dalgada güçlü şef.
- Üç türün görünür yüksekliği de karakterin 1,8 dünya birimlik yüksekliğine göre ayarlanır.
- Vuruşta kısa geri itme, kırmızı renk geçişi ve ölçek tepkisi; ölümde ayrı animasyon.
- Ölümlerden çıkan elmaslar zeminin üzerinde, okunabilir boyutta görünür. Kısa ortaya çıkma animasyonu ve yakınlaşınca karaktere çekilme davranışı vardır.
- Elmas havuzu dolarsa yeni deneyim kaybolmaz; yakındaki mevcut elmasa eklenir.
- Koşu sonu veya menüye dönüşte skor sıralamaya kaydedilir. Her beş toplanan koşu elması, bir mağaza elması kazandırır.

### 11 ödül nesnesi

Görseller doğrudan `gear_left_c_11`–`gear_left_c_21` aralığındandır. Saydam kenar boşlukları dikkate alınarak atılan nesnelerin görünür boyutu ayarlandı. Arkalarında sarı/turuncu, incelerek sönen bir iz bulunur.

| Sprite | Nesne | Davranış | İlk bekleme süresi | En erken koşu seviyesi |
| --- | --- | --- | --- | --- |
| 11 | Güdümlü Top | Tek hedefi takip eder | 2,4 sn | 2 |
| 12 | Delici Şut | Aynı doğrultudaki 3 düşmanı deler | 3,6 sn | 3 |
| 13 | Seken Karpuz | 3 farklı hedef arasında seker | 4,2 sn | 4 |
| 14 | Patlayıcı Pas | Yay çizerek atılır, alanda patlar | 5,5 sn | 5 |
| 15 | Buzlu Gazoz | Hedefi 2 saniye yavaşlatır | 3,2 sn | 6 |
| 16 | Zehirli Havuç | İlk darbeden sonra zamanla hasar verir | 4,5 sn | 7 |
| 17 | Dönen Ördek | İleri gider, avcıya döner | 4,8 sn | 8 |
| 18 | Üçlü Beyzbol | Üç ayrı hedefe eşzamanlı atılır | 5,2 sn | 9 |
| 19 | Yıldırım Taşı | 4 hedef arasında sıçrar | 6,2 sn | 10 |
| 20 | Parça Bombası | Ana patlamaya çevrede üç alan darbesi ekler | 7,4 sn | 12 |
| 21 | Muz Meteoru | Yüksek yay ve geniş, güçlü alan hasarı | 9 sn | 14 |

Her nesne koşu içinde en fazla beşinci seviyeye yükselir. Seviye arttıkça hasarı yükselir ve bekleme süresi kısalır; en hızlısı da saniyede bir atışın altında kalır. Güçlü nesneler başlangıçta sunulmaz.

## Mobil uyumluluk ve doğrulama

Dikey ekran, 1080×1920 referans Canvas ölçeği, genişliğe göre uyarlama ve güvenli alan desteği kullanılır. 9:16, uzun 9:20, 3:4 tablet oranları ve çentik benzetimiyle görüntüler kontrol edildi. Farklı oranlarda daha fazla/daha az dikey alan görünür; içerik kaydırma ve sabit dokunma boyutlarıyla uyarlanır.

Otomatik Play Mode raporu: `Logs/demo-verification.txt`. Android C# derleme raporu: `Logs/android-script-check.txt`. Görsel çıktılar: `Logs/DemoPreviews/`.

Sonuç: **61 kontrol geçti, 0 oyun çalışma hatası. Android oyuncu C# derlemesi başarılı.** Doğrulama için ayrı proje kopyası ve ayrı PlayerPrefs kimliği kullanıldı; test bakiyeleri ve test profilleri ana oyunun kaydına aktarılmadı. Kopyadaki Unity Search indeks hatası editör tanısı olarak ayrıca kaydedildi; oyun hataları bu istisnaya dahil edilmedi.

Kontroller; menü geçişi, kilitli harita kaydırması, top bar dokunmaları, test satın alma, bağımsız profil bakiyeleri, JSON kayıt yükleme, günlük ödülün tekrar alınamaması, yetenek bağımlılıkları, joystick, hasar izi, dalgalar ve 11 nesnenin gerçek hedef/hasar davranışlarını kapsar.

Gerçek telefonda performans ve dokunma testi ile APK/AAB kurulum testi bu raporun kapsamına dahil değildir. Android C# derlemesi, tam mağaza yayın derlemesi değildir.

## Dosyalar ve bakım

- `Assets/Scenes/MainMenu.unity`, `GameScene.unity`, `GameScene2.unity` ve `Assets/EndlessHunt/UI/Screens/UI_MobileMenu.prefab` güncellendi.
- `Assets/EndlessHunt/UI/DemoMenu.cs`: mağaza, ağaç, profil, seçenekler ve demo pencereleri. Bu ek ekranların içerikleri Play Mode'da oluşturulur.
- `DemoTheme.cs` ve `Assets/EndlessHunt/Resources/DemoTheme.asset`: ortak sprite/font/material bağlantıları.
- `DemoUI.cs`, `DemoTypography.cs`, `DemoBattle.cs`: ortak UI üretimi, yazı stili ve oyun içi ek göstergeler.
- `LocalPlayerStore.cs`: yerel profil, sıralama, günlük ödül ve yetenek kaydı.
- `RunArsenal.cs`, `HuntProjectile.cs`, `GoblinSet.cs`: ödül nesneleri, saldırı davranışları ve goblin animasyonları.
- `Assets/EndlessHunt/Art/Goblins/`: üç atlas ve kaynak kare manifesti.
- `Assets/Editor/EndlessHuntDemoRevision.cs`: mevcut top barı koruyarak sahnelere bu revizyonu uygular. Eski menüyü sıfırdan kuran MobilePolish komutu bu revizyon için kullanılmamalıdır.
- Bu tur öncesindeki sahne/prefab yedeği: `Logs/DemoRevisionBackup-20260909/`.

Çevrim içi hesap, sunucu liderlik tablosu ve gerçek ödeme bağlantıları eklenmedi; bunlar seçtiğin yerel/test kapsamının dışındadır.
