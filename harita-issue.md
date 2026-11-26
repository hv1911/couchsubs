**Sürüm:** 0.1.0-beta.104 · Mac (Apple Silicon) · Samsung TV (Tizen), TV uygulaması 0.9.115 · DeepL · paketler açık
**Oyun:** GTA VI › Harita › STORY (görev listesi, görev kartı, sağ alttaki tuş çubuğu)
**İlgili:** #1359, #1360, #1362

## Özet
Harita ekranında seçim her göreve geçtiğinde çizim baştan farklı kuruluyor. Aynı satır bir karede yanına, bir karede üstüne, bir karede haritaya taşan bir kutuya yazılıyor ya da hiç çevrilmiyor; görev simgeleri örtülüyor, hatta yazı sanılıp çeviriye giriyor; görev kartı her seferinde başka biçimde çıkıyor.

Günlükte kök neden belli: ekran menü sayılmadığı için üç ayrı alan tarafından bölünerek okunuyor, seçim her değiştiğinde de bütün kutular silinip iki okuma bekleniyor. Çeviri hızı sorun değil (paket çevirisi neredeyse anlık); takılmayı yeniden çizme kuralı yaratıyor.

Bu issue'da sırasıyla: **1)** gözlenen hatalar, **2)** kök nedenler, **3)** hız ve akıcılık hedefi, **4)** bu ekran için net kurallar ve Ayarlar'dan seçilebilen iki görünüm biçimi (A: yanına yaz, B: üstünü ört).

---

## 1. Gözlenen hatalar
Altı fotoğraf, seçim sırasıyla: **1** Rob Vicki's Boss · **2** Search for the Skunk Ape · **3** Collect Chemicals for Kyle · **4** Check out the Mud Club · **5** Hit Kingpin Bowling · **6** Save for the Future

<details><summary>Fotoğraflar (6)</summary>

**1** · Rob Vicki's Boss
<!--F1-->

**2** · Search for the Skunk Ape
<!--F2-->

**3** · Collect Chemicals for Kyle
<!--F3-->

**4** · Check out the Mud Club
<!--F4-->

**5** · Hit Kingpin Bowling
<!--F5-->

**6** · Save for the Future
<!--F6-->

</details>

| # | Hata | Fotoğraf |
|---|---|---|
| 1 | Aynı satır her karede başka biçimde: "Vicki'nin Patronunu Soy" 2, 5, 6'da satırın **üstünde**, 3, 4'te **yanında**, 1'de **örtü** | 1–6 |
| 2 | Haritaya taşan büyük, boş ikiz kutular: İSTEĞE BAĞLI (+ Skunk Ape'i Ara), Gelecek İçin Biriktir; 6'da kartın solunda dev bir kutu ve "GOAL" → "GOL." | 1, 2, 3, 5, 6 |
| 3 | **Görev simgesi örtülüyor:** 3'te KM simgesi beyaz kutunun altında; 6'da simge yazı sanılıp çeviriye giriyor: "KM, Kyle için kimyasalları topluyor" | 3, 6 |
| 4 | Bozuk okumadan saçma çeviri: "Onur ape'de arama yap", "Ape'nin onur listesindeki bir isim" | 3, 5 |
| 5 | Hit Kingpin Bowling hiç çevrilmiyor (liste ve kart başlığı) | 1–6 |
| 6 | Görev kartı her seferinde başka: başlık kartın üstünde (2), solunda ve kişi resminin üstüne binerek (3), başlık satırında (4), hiç yok (5) | 2–5 |
| 7 | Kart metni iki kez çevriliyor: üstte yarım ve yanlış bir örtü, altta ikinci bir Türkçe; aradaki İngilizce açıkta | 2 |
| 8 | Kart metni ortalanmış (aslı sola yaslı) | 4 |
| 9 | Tuş çubuğu tutarsız: çoğu karede yalnız ORTALA; 4'te beşi birden; 3'te İŞARET EKLE yok | 1–6 |
| 10 | Seçili satırda açık zemine açık gri yazı; örtü kutusu şeritten kısa | 2, 4, 6 |
| 11 | Yanına yazılan Türkçe zeminsiz, panel kenarını geçip haritanın üstüne taşıyor | 3, 4 |

---

## 2. Kök nedenler (altyazi.log)

### 2.1 Harita ekranı menü sayılmıyor
Menü durumu için bileşen dışındaki karoların en az %60'ı sakin olmalı (`ScreenTextDetector.menuQuietShare = 0.6`). Haritanın temiz bir PS5 karesinde bu oran **%55** (32×18 karo, karo başına 24×24 örnek, sapma < 18). Ekran eşiğin hemen altında olduğu için menü durumu gidip geliyor:
```
15:20:22.739 ekran menüsü açık: altyazı bekletiliyor
15:20:24.762 ekran menüsü kapandı: altyazı gösteriliyor (2023 ms)
```
Menü sayılmayınca ekranı üç alan bölüşüyor ve her biri kendi kuralıyla çiziyor:
```
15:49:46.206 ek alan Help: "STORY Rob Vicki's Boss OPTIONAL Search for the Skunk Ape"
             çizim yan 6.0×4.2, örtü 10.7×2.7, ikiz 21.9×11.6 (yazı 5.9×3.1), ikiz 21.9×11.6 (yazı 16.8×3.1)
15:49:46.647 ek alan Messages: "and split the RECENTER"
             çizim örtü 9.2×2.9, örtü 5.7×3.0
15:49:47.180 ek alan Screen text: "Collect Chemicals for Kyle Check out the Mud Club Hit Kingpin Bowling Save for the Future JASON ONLY Meet Bret"
             çizim örtü 17.6×3.7, örtü 16.6×2.5, örtü 12.7×3.7, ikiz 20.8×5.8 (yazı 12.7×2.5), yan 7.5×3.0, yan 6.8×2.3
```
Liste tek parça okunduğunda (fotoğraf 4) çizim en düzgün çıkıyor ve tuşların beşi de çevriliyor; yönün doğru olduğunu gösteriyor:
```
16:26:36.797 ek alan Screen text: "STORY Rob Vicki's Boss OPTIONAL Search for the Skunk Ape Collect Chemicals for Kyle …"
             çizim yan 6.1×4.3, yan 10.5×2.7, yan 6.0×3.0, yan 16.7×2.7, yan 17.3×2.7, örtü 16.2×2.5, örtü 12.6×3.1, örtü 12.8×2.9, yan 7.5×3.0, yan 6.8×2.6
16:26:36.800 ek alan Screen text: "RECENTER 2D VIEW ADD MARKER SET DESTINATION CLOSE MENU"
             çizim yan 5.8×3.0, yan 4.6×2.9, yan 7.3×3.0, yan 9.3×2.9, yan 8.0×2.6
```

### 2.2 Hazır alanlar listeyi ve kartı bölüyor
`RegionWatcher.yieldingPresets` hazır alanları yalnız bir menü alanı varken geri çekiyor. Menü sayılmayınca Help köşesi listenin üst kısmını kutu sanıp ikiz açıyor (yukarıda), Messages alanı da kartın sağ yarısından parça kapıp çeviriyor:
```
16:26:14.338 ek alan Messages: "Boon Kirk asked ething called Pelican River. RECENTER" → hazır çeviri+motor
             çizim örtü 4.5×2.7, örtü 6.7×2.6, örtü 8.6×3.1, örtü 8.8×3.1, örtü 6.0×3.0
16:26:29.823 ek alan Messages: "ON NILL emicals to ops. Head to ed. RECENTER" → hazır çeviri+motor
```

### 2.3 Seçim değişince bütün kutular siliniyor (#1359)
Seçim her yer değiştirdiğinde alanın bütün kutuları kalkıyor ve yeni çizim için iki aynı okuma bekleniyor; ayrıntı #1359'da.

### 2.4 Görev simgesi yazı olarak okunuyor
```
16:26:48.884 ek alan Screen text: "KM Collect Chemicals for Kyle Check out the Mud Club …" → hazır çeviri+motor
```

### 2.5 Bozuk okuma çeviri motoruna gidiyor
"Search for the Skunk Ape" satırı (simgesinde küçük saat rozeti var) sürekli yanlış okunuyor; paket eşleşmeyince motor bozuk metni çeviriyor:
```
16:26:14.138 "scarch rol the onuin Ape …" → hazır çeviri+motor
16:26:30.103 "Search rot the onur ape …"  → hazır çeviri+motor
16:26:42.978 "oedict rol the onurn Ape …" → hazır çeviri+motor
16:26:30.118 "AUD MARNER SET DESTINATION CLOSE MENU" → hazır çeviri+motor
```

### 2.6 Paket
Hit Kingpin Bowling aslıyla aynı kalıyor; Save for the Future kartının "GOAL" satırı "GOL." oluyor (16:26:48.888, `ikiz 19.6×31.2`).

---

## 3. Hız ve akıcılık
Gecikmeyi çeviri yaratmıyor: bu ekrandaki metinlerin neredeyse hepsi paketten geliyor ("hazır çeviri"), okuma ortalama ~53 ms, çizim ~2 ms.
```
15:18:32.544 aşama süreleri (30 sn): okuma 47× ort 53,0 en çok 142,0 ms; … çizim 47× ort 2,3 en çok 9,0 ms
```
Takılmayı, seçim değişince bütün kutuların silinip iki okuma beklenmesi yaratıyor:
```
16:26:28.968 seçim kaydı, eski kutular kaldırıldı  →  16:26:30.103 yeniden çizim   (1,1 sn)
15:18:53.365 seçim kaydı, eski kutular kaldırıldı  →  15:18:57.073 yeniden çizim   (3,7 sn)
```

**Hedef:** paketteki menü ve görev metinlerinde seçim değişimi **tek okuma turunda (~100–150 ms)** doğru çizilsin, değişmeyen satırlara hiç dokunulmasın. Paket çevirisi neredeyse anlık olduğu için iki biçimde de görevden göreve geçiş gözle fark edilmemeli.

**Öneriler**
- **Onaysız çizim:** okunan satırların hepsi pakette varsa ve liste kaymadıysa (yalnız seçim yer değiştirdi), yeni okuma onay beklemeden çizilsin; yalnız zemini dönen iki satır güncellensin.
- **Pakete oturtma:** menüde paketle eşleşmeyen satır önce paketin bulanık imzalarıyla eşleştirilsin ("scarch rol the onuin Ape" → "Search for the Skunk Ape"); yine eşleşmezse motora gönderilmesin, satırın önceki çevirisi kalsın.
- **Arayüz sözlüğü:** harita ve menü terimleri (RUN, MOVE, GOAL, 2D VIEW, ADD MARKER…) için pakete ya da `tools/packs/glossary/gta6.json`'a arayüz girdileri. İsterseniz bu listeyi biz hazırlayabiliriz.

---

## 4. Önerilen kurallar ve iki görünüm biçimi
**Ayarlar › Ekran yazısı › Çeviri biçimi:** `Yanına yaz (A)` · `Üstünü ört (B)`

Menülerde örtmeyi bilerek kaldırdığınızı biliyoruz (#1320); B bu yüzden isteğe bağlı bir seçenek olarak öneriliyor, varsayılan değişmeyebilir.

### A — Yanına yaz
<!--A-->

- **Liste satırları ve başlıklar:** Türkçe, kendi İngilizcesinin hemen sağında, koyu zeminli bir etikette. Bütün satırlar aynı boyda ve aynı taban çizgisinde; hiçbiri satırın üstüne, altına ya da ikiz kutuya taşınmaz.
- **Görev kartı:** altında ikiz kart: aynı zemin, pembe üst çizgi, kareden kesilmiş kişi resmi, aynı başlık ve metin stili.
- **Tuş çubuğu:** Türkçe İngilizcenin solunda, koyu zeminli etikette, sağ kenarları hizalı; beşi de her zaman çevrili.

### B — Üstünü ört
<!--B-->

- **Liste satırları ve başlıklar:** yazı, kareden ölçülen zemin renginde (gerekirse geçişli) bir kutuyla örtülür; Türkçe aynı yer, boy ve renkte. Satır simgeleri asla örtülmez: uzun Türkçe simgeden uzağa uzar, sığmazsa küçülür.
- **Seçili satır:** kutu şeridin boyunda ve geçişli zemininde, yazı koyu.
- **Görev kartı:** kartın tamamı tek kutuyla örtülüp baştan çizilir: kart zemini, pembe üst çizgi, kareden kesilmiş kişi resmi, başlık ve metin. Parça parça örtüden daha az hata çıkarır.
- **Tuş çubuğu:** yalnız yazılar örtülür, tuş simgeleri açık kalır; hepsi aynı renkte.

### İki biçimde de geçerli kabul şartları
1. Aynı satır her karede aynı biçimde çizilir; seçim değişince biçim, boy ve yer değişmez.
2. Görev simgeleri, tuş simgeleri ve kişi resmi asla örtülmez, yazı olarak da okunmaz.
3. Gamepad ile gezerken değişmeyen satırların çevirisi yerinde kalır; değişen satırlar tek okuma turunda güncellenir; boş kare ya da titreme olmaz.
4. Liste, görev kartı ve tuş çubuğu ayrı alanlara bölünmez; menüde hazır alanlar yer verir.
5. Bozuk okunan satır çeviri motoruna gitmez: pakete oturtulur ya da önceki çevirisi kalır.
6. Haritanın üstüne taşan ikiz kutu açılmaz.
7. Liste ve kart metinleri için oyunun yazısına yakın, gömülü bir yazı tipi kullanılır (başlık ve tuşlar için mevcut Barlow Condensed).

---

## 5. Notlar
- Taslaklar temiz bir PS5 karesinin üstüne çizildi. Oyunun yazı tipi telifli olduğu için Barlow Condensed ve DM Sans kullanıldı. B taslağı, CouchSubs'ın mevcut örtme yöntemiyle (zemin ve yazı rengi kareden ölçülerek) çizildi.
- Test karesi olarak temiz PS5 ekran görüntüleri (seçim farklı satırlardayken) gönderebiliriz.
- Bu işin bir bölümünü (örneğin haritanın menü sayılması ve 2.3) PR olarak hazırlamamızı isterseniz memnuniyetle yaparız. Nasıl tercih edersiniz?
