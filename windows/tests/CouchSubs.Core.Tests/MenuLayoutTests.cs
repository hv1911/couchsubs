// SPDX-License-Identifier: AGPL-3.0-only

using System.Text.Json;
using CouchSubs.Core;

namespace CouchSubs.Core.Tests;

/// <summary>
/// Menüde çevirinin biçimi ayardan gelir (#1365, <see cref="MenuLayout"/>): B üstünü örter, A Türkçeyi yanına koyu etikete
/// yazar (yanına sığmazsa altına/üstüne, şerit bir bütün), görev kartının ikizi kartın altında. Ortak deneme dosyası
/// core/Tests/CouchSubsCoreTests/Fixtures/menu-layout.json (Mac MenuLayoutTests.swift, Android MenuLayoutTest.kt aynı).
/// </summary>
public class MenuLayoutTests
{
    private static TextBox Box(JsonElement e)
    {
        var v = e.EnumerateArray().Select(x => x.GetDouble()).ToArray();
        return new TextBox(v[0], v[1], v[2], v[3]);
    }

    private static void Near(TextBox expected, TextBox? actual, string what)
    {
        Assert.True(actual is not null, $"{what}: kutu yok");
        var a = actual!.Value;
        var ok = Math.Abs(a.Left - expected.Left) < 1e-6 && Math.Abs(a.Top - expected.Top) < 1e-6
            && Math.Abs(a.Right - expected.Right) < 1e-6 && Math.Abs(a.Bottom - expected.Bottom) < 1e-6;
        Assert.True(ok, $"{what}: beklenen {expected}, çıkan {a}");
    }

    [Fact]
    public void SharedCases()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(PackFixtures.Path("menu-layout.json")));
        Assert.Equal(MenuLayout.LabelColor, document.RootElement.GetProperty("labelColor").GetString());
        foreach (var c in document.RootElement.GetProperty("cases").EnumerateArray())
        {
            var id = c.GetProperty("id").GetString()!;
            var items = c.GetProperty("items").EnumerateArray().ToList();
            var paints = items.Select(i => new TextPaint(i.GetProperty("bg").GetString()!, i.GetProperty("line").GetDouble(),
                                                          Box(i.GetProperty("cover")))
            {
                IconSide = i.TryGetProperty("icon", out var icon) ? icon.GetString() : null,
                // Ölçümün seçtiği kip ne olursa olsun kilitlenir.
                Mode = PaintMode.Twin, Spot = Box(i.GetProperty("cover")), SpotRoom = Box(i.GetProperty("cover")),
                Twin = new TextBox(0, 0, 1, 1),
            }).ToList();
            var targets = items.Select(i => new TextPainter.Target(Box(i.GetProperty("box")), i.GetProperty("line").GetDouble(), 1,
                                                                   i.GetProperty("bg").GetString()!, "#F0F0F0", false,
                                                                   i.GetProperty("align").GetString()!)).ToList();
            var cards = c.GetProperty("cards").EnumerateArray()
                .Select(card => (IReadOnlyList<int>)card.EnumerateArray().Select(x => x.GetInt32()).ToList()).ToList();
            var result = MenuLayout.Locked(paints, targets, cards, c.GetProperty("cover").GetBoolean(), c.GetProperty("aspect").GetDouble());
            var expect = c.GetProperty("expect").EnumerateArray().ToList();
            Assert.Equal(expect.Count, result.Count);
            for (var k = 0; k < expect.Count; k++)
            {
                var e = expect[k];
                var p = result[k];
                var what = $"{id} / {items[k].GetProperty("name").GetString()}";
                Assert.True(e.GetProperty("mode").GetString() == p.Mode, $"{what}: kip {p.Mode}");
                Near(paints[k].Cover, p.Cover, what + " örtü");
                if (p.Mode == PaintMode.Cover)
                {
                    Assert.Null(p.Spot);
                    Assert.Null(p.Twin);
                    Assert.Null(p.Label);
                    continue;
                }
                Near(Box(e.GetProperty("spot")), p.Spot, what + " spot");
                Near(Box(e.GetProperty("room")), p.SpotRoom, what + " spotRoom");
                if (p.Mode == PaintMode.Beside)
                {
                    Assert.Equal(e.GetProperty("label").GetString(), p.Label);
                    Assert.Null(p.Twin);
                }
                else
                {
                    Near(Box(e.GetProperty("twin")), p.Twin, what + " twin");
                    Assert.Equal(e.GetProperty("bg").GetString(), p.Background);
                    Assert.Null(p.Label);
                }
            }
        }
    }

    /// <summary>Etiket TV'ye yalnız yanına yazmada gider (eski TV yok sayar).</summary>
    [Fact]
    public void LabelGoesToTheTvOnlyBeside()
    {
        var paint = new TextPaint("#202020", 0.04, new TextBox(0.1, 0.1, 0.2, 0.14))
        {
            Mode = PaintMode.Beside, Spot = new TextBox(0.21, 0.1, 0.31, 0.14), SpotRoom = new TextBox(0.21, 0.1, 0.5, 0.14),
            Label = MenuLayout.LabelColor,
        };
        var json = SubtitleServer.ItemJson(Item(paint), phone: false);
        Assert.Equal(MenuLayout.LabelColor, json["label"]);
        Assert.False(SubtitleServer.ItemJson(Item(MenuLayout.Plain(paint)), phone: false).ContainsKey("label"));
    }

    private static ScreenTranslation.Item Item(TextPaint paint) =>
        new("Menu", "Menü", paint.Cover, 0.04)
        {
            Style = new PhoneTextStyle("#202020", "#F0F0F0"), Look = new TextLook(false, false, "left", false, false), Paint = paint,
        };
}
