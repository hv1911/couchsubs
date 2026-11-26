// SPDX-License-Identifier: AGPL-3.0-only

namespace CouchSubs.Core;

/// <summary>
/// Menü ekranında çevirinin biçimi ayardan gelir, her okumada yeniden seçilmez (#1365, Ayarlar › Ekran yazıları ›
/// "Menülerde İngilizcenin üstünü ört"). beta.105 testinde GTA 6 haritasında aynı satır art arda okumalarda örtü, yan,
/// üst/alt ya da haritada yüzen ikiz kutu oluyordu (5 dakikada 237 yan, 139 örtü, 37 ikiz): <see cref="TextPainter"/> kipi
/// satırın çevresindeki boş alana ve zemin desenine göre seçiyor, kayan harita ve seçim çubuğu bunları her karede
/// değiştiriyordu.
/// </summary>
/// <remarks>
/// Menüdeki otomatik alanın çizimleri <see cref="TextPainter"/>'ın ölçtüğü örtü kutusu, zemin ve yazı rengiyle kalır; yalnız
/// yerleşim kilitlenir. Yerleşim yalnız yazıların kutularından çıkar (kare noktalarına bakılmaz): aynı ekran her okumada
/// aynı çizilir.
/// <list type="bullet">
/// <item>B, üstünü ört (<c>cover</c>): her yazı kendi yerinde, ölçülen zemin renginde örtülür; kartın başlığı ve metni ayrı
/// ayrı örtülür, kişi resmi ve simgeler açıkta kalır.</item>
/// <item>A, yanına yaz (varsayılan): her satırın Türkçesi İngilizcesinin hemen yanına, koyu zeminli etikete (<c>label</c>)
/// yazılır: sola yaslı satırda sağına, sağa yaslı satırda (tuş çubuğu) soluna; satırın simgesi ve komşu yazılar örtülmez.
/// Yanına sığmayan etiket (sekme şeridi, sıkışık tuş çubuğu) altına, olmazsa üstüne yazılır; şeridin bir sekmesi altına
/// yazıldıysa yakınındaki öbür sekmeler de altına yazılır (şerit bir bütün). Hiçbir yere sığmayan satır örtülür.
/// Haritanın görev kartının (<c>cards</c>) çevirisi kartın hemen altına (yer yoksa üstüne), kartın zemininde, kişi
/// resmiyle ikiz kart olarak gelir; ikisine de yer yoksa ya da kart bir listenin parçasıysa (Görevler ekranı: sol kenarı
/// kartla hizalı başka yazılar) kartın satırları da yanına yazılır.</item>
/// <item>İki biçimde de satırın üstüne/altına küçük yazma, haritaya uzak ikiz kutu ve iki satırı tek kutuda birleştirme
/// olmaz; etiketler başka yazının ya da ikiz kartın üstüne binmez.</item>
/// </list>
/// Kutular kareye oranla (0…1); <c>aspect</c>: karenin genişlik/yükseklik oranı. Mac (MenuLayout.swift) ve Android
/// (MenuLayout.kt) aynı kuralı, aynı eşikleri kullanır; ortak deneme dosyası Fixtures/menu-layout.json.
/// </remarks>
public static class MenuLayout
{
    /// <summary>Yanına yazılan Türkçenin etiket zemini (TV yarı saydam çizer, yazı açık renk).</summary>
    public const string LabelColor = "#121419";
    /// <summary>Türkçe ile İngilizce (ya da satırın simgesi) arası, satır yüksekliğinin katı.</summary>
    public const double GapLines = 0.4;
    /// <summary>Satırın yanındaki simge okunan kutuda değilse simge için bırakılan yer, satır yüksekliğinin katı.</summary>
    public const double IconLines = 1.2;
    /// <summary>Yanına yazılan Türkçenin en çok uzayabileceği yer: İngilizcenin BesideMax katı artı bir satır, en az ReachLines.</summary>
    public const double BesideMax = 1.6;
    public const double ReachLines = 12.0;
    /// <summary>Yanında bu kadar yer (İngilizcenin genişliğinin payı) yoksa Türkçe altına/üstüne yazılır.</summary>
    public const double RoomShare = 0.6;
    /// <summary>Aynı satırda sayılmak için dikey örtüşme (kısa kutunun payı).</summary>
    public const double SameRow = 0.3;
    /// <summary>
    /// Etiket bir yazıya ancak etiketin boyunun bu payından çok binerse çakışır (sıkışık listede komşu satırların okunan
    /// kutuları birkaç nokta örtüşür).
    /// </summary>
    public const double TouchShare = 0.25;
    /// <summary>Aynı satırda bu kadar yakın (satır yüksekliğinin katı) iki yazı tek şerittir: biri altına/üstüne yazıldıysa öbürü de.</summary>
    public const double StripGapLines = 4.0;
    /// <summary>Ekran kenarından bırakılan pay, satır yüksekliğinin katı.</summary>
    public const double EdgeLines = 0.2;
    /// <summary>İkiz kartın kart yazılarının çevresindeki iç boşluğu ve kartla arası, satır yüksekliğinin katı.</summary>
    public const double CardPadLines = 0.5;
    public const double CardGapLines = 0.3;
    /// <summary>Kart dışı bir yazının sol kenarı kartınkine bu kadar yakınsa (satır yüksekliğinin katı) kart bir listenin parçasıdır.</summary>
    public const double ColumnLines = 1.0;

    /// <summary>
    /// Menüdeki çizimler ayardaki biçime kilitlenir. <paramref name="cards"/>: görev kartlarının parçaları (her kart için
    /// <paramref name="paints"/> sıraları); <paramref name="cover"/>: B (üstünü ört), false: A (yanına yaz).
    /// <paramref name="portrait"/>: ikiz kurulacak kartın parçalarında kişi resmi yoksa kartın parçaları ve zemini verilip resim
    /// (kutusu ve kesilmiş görüntüsü) kareden aranır (<see cref="TextPainter.CardPortrait(IReadOnlyList{TextBox}, IReadOnlyList{int}, string, double, Frame)"/>;
    /// ikiz kart resimsiz kalmasın).
    /// </summary>
    public static List<TextPaint> Locked(IReadOnlyList<TextPaint> paints, IReadOnlyList<TextPainter.Target> targets,
                                         IReadOnlyList<IReadOnlyList<int>> cards, bool cover, double aspect,
                                         Func<IReadOnlyList<int>, string, (TextBox Box, string? Image)?>? portrait = null)
    {
        if (paints.Count != targets.Count || paints.Count == 0) return paints.ToList();
        var result = paints.Select(Plain).ToList();
        if (cover) return result;
        var taken = Enumerable.Range(0, paints.Count).Select(i => paints[i].Cover.Union(targets[i].Box)).ToList();
        var done = new HashSet<int>();
        var twins = new List<TextBox>();
        foreach (var raw in cards)
        {
            var card = raw.Where(m => m >= 0 && m < paints.Count && !done.Contains(m)).ToList();
            if (card.Count == 0) continue;
            var made = CardTwin(card, paints, targets, taken, twins, aspect, portrait);
            if (made is null) continue;
            foreach (var (m, paint) in made) result[m] = paint;
            done.UnionWith(card);
            twins.Add(made[0].Paint.Twin!.Value);
        }
        var vertical = new HashSet<int>();
        var basis = result.ToList();
        for (var i = 0; i < paints.Count; i++)
        {
            if (done.Contains(i)) continue;
            var (paint, below) = Beside(i, paints, targets, taken, twins, aspect, basis[i], side: true);
            result[i] = paint;
            if (below) vertical.Add(i);
        }
        // Şerit bir bütün: aynı satırda yakınındaki bir yazı altına/üstüne yazıldıysa yanına yazılan da oraya yazılır
        // (sekme şeridinde son sekmenin Türkçesi yanında, öbürleri altında kalıyordu).
        for (var i = 0; i < paints.Count; i++)
        {
            if (done.Contains(i) || vertical.Contains(i) || result[i].Mode != PaintMode.Beside) continue;
            var c = paints[i].Cover;
            var lineX = Math.Max(1e-6, targets[i].LineHeight) / Math.Max(0.1, aspect);
            var strip = vertical.Any(j =>
            {
                var o = paints[j].Cover;
                return OverlapY(c, o) >= SameRow * Math.Min(c.Bottom - c.Top, o.Bottom - o.Top)
                    && Math.Max(0, Math.Max(taken[i].Left, taken[j].Left) - Math.Min(taken[i].Right, taken[j].Right)) <= StripGapLines * lineX;
            });
            if (!strip) continue;
            var (paint, below) = Beside(i, paints, targets, taken, twins, aspect, basis[i], side: false);
            if (below) result[i] = paint;
        }
        return result;
    }

    /// <summary>Ölçülen örtü, zemin ve yazı rengi kalır; kip örtmeye döner, yanına yazma ve ikiz alanları silinir.</summary>
    internal static TextPaint Plain(TextPaint paint) => paint with
    {
        Mode = PaintMode.Cover, Spot = null, SpotRoom = null, Twin = null, Radius = 0, Scale = 1, Tail = null,
        Portrait = null, PortraitTwin = null, PortraitImage = null, PortraitSquare = false, Edge = null, Label = null,
    };

    internal static double OverlapY(TextBox a, TextBox b) => Math.Max(0, Math.Min(a.Bottom, b.Bottom) - Math.Max(a.Top, b.Top));

    internal static bool Overlaps(TextBox a, TextBox b) =>
        Math.Min(a.Right, b.Right) > Math.Max(a.Left, b.Left) && Math.Min(a.Bottom, b.Bottom) > Math.Max(a.Top, b.Top);

    internal static TextBox Shifted(TextBox box, double dy) => box with { Top = box.Top + dy, Bottom = box.Bottom + dy };

    /// <summary>
    /// A: satırın Türkçesi yanında (yer yoksa altında ya da üstünde), koyu etikette; hiçbir yere sığmazsa örtü
    /// (<paramref name="basis"/>). <paramref name="side"/>: false ise yanı denenmez (şeridin öbür sekmeleri). Below: Türkçe
    /// altına/üstüne yazıldı.
    /// </summary>
    internal static (TextPaint Paint, bool Below) Beside(int i, IReadOnlyList<TextPaint> paints, IReadOnlyList<TextPainter.Target> targets,
                                                         IReadOnlyList<TextBox> taken, IReadOnlyList<TextBox> twins, double aspect,
                                                         TextPaint basis, bool side)
    {
        var c = paints[i].Cover;
        var lineY = Math.Max(1e-6, targets[i].LineHeight);
        var lineX = lineY / Math.Max(0.1, aspect);
        double gap = GapLines * lineX, edge = EdgeLines * lineX, eps = 1e-9;
        double w = c.Right - c.Left, h = c.Bottom - c.Top;
        var reach = Math.Max(BesideMax * w + lineX, ReachLines * lineX);
        // Satırın kendi kutusu simgesiyle birlikte (okunan kutu); simge okunmadıysa simge tarafına simge kadar yer.
        var own = taken[i];
        if (paints[i].IconSide == "left" && c.Left - own.Left < 0.5 * IconLines * lineX) own = own with { Left = c.Left - IconLines * lineX };
        if (paints[i].IconSide == "right" && own.Right - c.Right < 0.5 * IconLines * lineX) own = own with { Right = c.Right + IconLines * lineX };
        var others = Enumerable.Range(0, paints.Count).Where(j => j != i).Select(j => taken[j]).Concat(twins).ToList();
        var p = basis with { Mode = PaintMode.Beside, Label = LabelColor };
        var touch = TouchShare * h;
        if (side && targets[i].Align != "right")
        {
            var left = own.Right + gap;
            var limit = Math.Min(1 - edge, left + reach);
            foreach (var o in others)
            {
                if (OverlapY(o, c) > touch && o.Right > left) limit = Math.Min(limit, o.Left - gap);
            }
            if (limit - left >= RoomShare * Math.Min(w, reach))
            {
                return (p with
                {
                    Spot = new TextBox(left, c.Top, Math.Min(limit, left + w), c.Bottom),
                    SpotRoom = new TextBox(left, c.Top, limit, c.Bottom),
                }, false);
            }
        }
        else if (side)
        {
            var right = own.Left - gap;
            var limit = Math.Max(edge, right - reach);
            foreach (var o in others)
            {
                if (OverlapY(o, c) > touch && o.Left < right) limit = Math.Max(limit, o.Right + gap);
            }
            if (right - limit >= RoomShare * Math.Min(w, reach))
            {
                return (p with
                {
                    Spot = new TextBox(Math.Max(limit, right - w), c.Top, right, c.Bottom),
                    SpotRoom = new TextBox(limit, c.Top, right, c.Bottom),
                }, false);
            }
        }
        // Yanına sığmayan etiket (sekme şeridinin sekmesi, sıkışık tuş çubuğu) altına, olmazsa üstüne; aynı satırdaki
        // komşularla arasının ortasını geçmez (komşunun etiketiyle arada gap kalır), başka yazının üstüne binmez.
        var mates = Enumerable.Range(0, paints.Count).Where(j =>
            j != i && OverlapY(c, paints[j].Cover) >= SameRow * Math.Min(h, paints[j].Cover.Bottom - paints[j].Cover.Top));
        double l = Math.Max(edge, own.Left - 0.5 * w), r = Math.Min(1 - edge, own.Right + 0.5 * w);
        foreach (var j in mates)
        {
            var o = taken[j];
            if (o.Left >= own.Right - eps) r = Math.Min(r, (own.Right + o.Left - gap) / 2);
            else if (o.Right <= own.Left + eps) l = Math.Max(l, (o.Right + own.Left + gap) / 2);
        }
        l = Math.Min(l, c.Left);
        r = Math.Max(r, c.Right);
        var step = GapLines * lineY;
        foreach (var top in new[] { own.Bottom + step, own.Top - step - h })
        {
            var spot = new TextBox(c.Left, top, c.Right, top + h);
            var hit = others.Any(o => Math.Min(o.Right, spot.Right) > Math.Max(o.Left, spot.Left) && OverlapY(o, spot) > touch);
            if (spot.Top < 0 || spot.Bottom > 1 || hit) continue;
            return (p with { Spot = spot, SpotRoom = new TextBox(l, top, r, top + h) }, true);
        }
        return (basis, false);
    }

    /// <summary>
    /// A: görev kartının ikizi kartın hemen altında (yer yoksa üstünde), kartın zemininde; başka yazının ya da ikizin
    /// üstüne binmez. İkisine de yer yoksa ya da kart bir listenin parçasıysa null (kartın satırları yanına yazılır; Görevler
    /// ekranında son görev ikiz kart, öbürleri yanında çiziliyordu).
    /// </summary>
    internal static List<(int Index, TextPaint Paint)>? CardTwin(IReadOnlyList<int> card, IReadOnlyList<TextPaint> paints,
                                                                IReadOnlyList<TextPainter.Target> targets, IReadOnlyList<TextBox> taken,
                                                                IReadOnlyList<TextBox> twins, double aspect,
                                                                Func<IReadOnlyList<int>, string, (TextBox Box, string? Image)?>? portrait = null)
    {
        var heights = card.Select(m => targets[m].LineHeight).OrderBy(x => x).ToList();
        var line = Math.Max(1e-6, heights[heights.Count / 2]);
        double padY = CardPadLines * line, padX = padY / Math.Max(0.1, aspect);
        var left = card.Min(m => taken[m].Left);
        if (Enumerable.Range(0, paints.Count).Any(j => !card.Contains(j) && Math.Abs(taken[j].Left - left) <= ColumnLines * line / Math.Max(0.1, aspect)))
            return null;
        // Kartın zemini: en büyük parçanın (açıklama) ölçülen zemini.
        static double Area(TextBox b) => (b.Right - b.Left) * (b.Bottom - b.Top);
        var largest = card.Aggregate((a, b) => Area(paints[b].Cover) > Area(paints[a].Cover) ? b : a);
        var fill = paints[largest].Background;
        // Kişi resmi: ölçümün bulduğu ya da kareden aranan; ikizde kartın bütün parçalarında aynı (TV ilkinden çizer).
        var face = card.Select(m => paints[m].Portrait).FirstOrDefault(f => f is not null);
        var image = card.Select(m => paints[m].PortraitImage).FirstOrDefault(i => i is not null);
        if (face is null && portrait?.Invoke(card, fill) is { } found)
        {
            face = found.Box;
            image = found.Image;
        }
        var union = taken[card[0]];
        foreach (var m in card) union = union.Union(taken[m]);
        if (face is { } seen) union = union.Union(seen);
        var box = new TextBox(union.Left - padX, union.Top - padY, union.Right + padX, union.Bottom + padY);
        var step = (box.Bottom - box.Top) + CardGapLines * line;
        var blockers = Enumerable.Range(0, paints.Count).Where(j => !card.Contains(j)).Select(j => taken[j]).Concat(twins).ToList();
        bool Free(TextBox b) => b.Top >= 0 && b.Bottom <= 1 && b.Left >= 0 && b.Right <= 1 && !blockers.Any(o => Overlaps(o, b));
        double dy;
        if (Free(Shifted(box, step))) dy = step;
        else if (Free(Shifted(box, -step))) dy = -step;
        else return null;
        var twin = Shifted(box, dy);
        return card.Select(m =>
        {
            var spot = Shifted(paints[m].Cover, dy);
            var p = Plain(paints[m]) with
            {
                Mode = PaintMode.Twin, Twin = twin, Radius = paints[m].Radius, Background = fill, Background2 = null,
                Direction = null, Patterned = false, Spot = spot,
                SpotRoom = new TextBox(spot.Left, spot.Top, Math.Max(spot.Right, twin.Right - padX), spot.Bottom),
                Edge = paints[m].Edge,
            };
            if (face is { } picture)
            {
                p = p with { Portrait = picture, PortraitTwin = Shifted(picture, dy), PortraitImage = image, PortraitSquare = true };
            }
            return (m, p);
        }).ToList();
    }
}
