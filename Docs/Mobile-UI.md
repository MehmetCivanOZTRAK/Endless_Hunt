# Mobil arayüz ve savaş düzenlemesi

Başlangıç sahnesi: `Assets/Scenes/MainMenu.unity`.
Menü prefabı: `Assets/EndlessHunt/UI/Screens/UI_MobileMenu.prefab`.

- Bölüm kartı yatay sürüklenebilir; oklar da çalışır. Yeni profilde yalnızca ilk bölüm açıktır. Önceki bölümde üç dakika hayatta kalıp avı tamamlamak sonraki bölümü açar.
- Alt sekme seçimi DOTween ile 24 referans piksel yükselir; parıltı kullanılmaz.
- Başlangıçta yalnızca ilk silah takılıdır. Envanterde eşyaya dokunmak takar; takılı eşyanın yuvasına veya aynı envanter kartına dokunmak çıkarır. Silah elde kalır.
- 40 silah ve 230 kıyafet/saç seçeneği ile boş baş seçeneği mevcuttur. Kategori filtreleri silah, üst, alt, baş, saç ve botları ayırır.
- Deneme için `GameProfile.UnlockAllEquipment` açıktır. Yayın ekonomisi hazırlanırken kapatılabilir. Kıyafetlerin can ve botların hareket bonusları savaşta uygulanır; kalıcı karakter gelişimi ayrı sayfadadır.
- Goblinin tam yürüyüş sprite dizisi dört yönde oynar. Boyut hesabı şeffaf kenar boşluklarını dışarıda bırakır. Hasar anında kısa geri tepme, renk tepkisi ve havaya yükselen hasar sayısı kullanılır.
- Canın gerçek değeri hemen değişir; görsel bar kısa sürede azalır, sarı kayıp göstergesi gecikerek takip eder. Hasarda kısa kırmızı ekran tepkisi bulunur.
- Seviye seçimindeki üç kart farklıdır. Menzil kartı gerçekten saldırı menzilini artırır. Duraklatılmış ekran geçişleri ölçeklenmemiş zaman kullanır.

## Mobil ölçekleme

Canvas: Scale With Screen Size, 1080 × 1920, genişlik eşleştirme. Üst/alt barlar referans birimlerle sabit, orta içerik esnektir. SafeAreaController çentik ve sistem hareket alanlarını ayırır. Oyun portre yönündedir; kamera görüş genişliği ekran oranına göre korunur. Ekranların fiziksel ölçüleri farklı olduğu için amaç aynı fiziksel santimetre değil, tutarlı yerleşim ve oranlardır.

## Bakım ve doğrulama

`Endless Hunt > UI > Apply Mobile Survivor Layout` menü prefabını ve sahnelerin HUD bölümünü yeniden üretir. Mevcut sonsuz harita, oyuncu ve havuzlar korunur. Önceki sahneler `Logs/MobilePolishBackup` altına bir kez yedeklenir. Bu işlemden önce elle yapılmış sahne değişikliklerini kaydedin.

`EndlessHuntMobileVerification.ApplyAndRun` bağımsız Unity batch kopyasında derleme/Play Mode kontrolü ve ekran görüntüleri üretir. Testte değiştirilen profil alanları sonrasında geri yüklenir. Çıktılar `Logs/mobile-verification.txt` ve `Logs/MobilePreviews` altındadır. Fiziksel Android/iOS cihaz testi ayrıca gerekir; bu düzenleme APK/IPA üretmez.
