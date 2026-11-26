// SPDX-License-Identifier: AGPL-3.0-only
package com.couchsubs.mobile.core

import org.json.JSONArray
import org.json.JSONObject
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotNull
import org.junit.Assert.assertNull
import org.junit.Assert.assertTrue
import org.junit.Test
import kotlin.math.abs

/**
 * Menüde çevirinin biçimi ayardan gelir (#1365, [MenuLayout]): B üstünü örter, A Türkçeyi yanına koyu etikete yazar
 * (yanına sığmazsa altına/üstüne, şerit bir bütün), görev kartının ikizi kartın altında. Ortak deneme dosyası
 * fixtures/menu-layout.json (Mac MenuLayoutTests.swift, Windows MenuLayoutTests.cs aynı vakalar).
 */
class MenuLayoutTest {
    private val fixture = JSONObject(Fixtures.text("menu-layout.json"))

    private fun box(array: JSONArray) = TextBox(array.getDouble(0), array.getDouble(1), array.getDouble(2), array.getDouble(3))

    private fun near(expected: TextBox, actual: TextBox?, what: String) {
        assertNotNull("$what: kutu yok", actual)
        val a = actual!!
        val ok = abs(a.left - expected.left) < 1e-6 && abs(a.top - expected.top) < 1e-6 &&
            abs(a.right - expected.right) < 1e-6 && abs(a.bottom - expected.bottom) < 1e-6
        assertTrue("$what: beklenen $expected, çıkan $a", ok)
    }

    @Test
    fun sharedCases() {
        assertEquals(MenuLayout.LABEL_COLOR, fixture.getString("labelColor"))
        val cases = fixture.getJSONArray("cases")
        for (n in 0 until cases.length()) {
            val c = cases.getJSONObject(n)
            val id = c.getString("id")
            val items = c.getJSONArray("items").let { a -> (0 until a.length()).map { a.getJSONObject(it) } }
            val paints = items.map { i ->
                val cover = box(i.getJSONArray("cover"))
                // Ölçümün seçtiği kip ne olursa olsun kilitlenir.
                TextPaint(i.getString("bg"), i.getDouble("line"), cover, iconSide = i.optString("icon").ifEmpty { null },
                    mode = TextPaint.MODE_TWIN, spot = cover, spotRoom = cover, twin = TextBox(0.0, 0.0, 1.0, 1.0))
            }
            val targets = items.map { i ->
                TextPainter.Target(box(i.getJSONArray("box")), i.getDouble("line"), 1, i.getString("bg"), "#F0F0F0", false,
                    i.getString("align"))
            }
            val cards = c.getJSONArray("cards").let { a ->
                (0 until a.length()).map { k -> a.getJSONArray(k).let { card -> (0 until card.length()).map { card.getInt(it) } } }
            }
            val result = MenuLayout.locked(paints, targets, cards, c.getBoolean("cover"), c.getDouble("aspect"))
            val expect = c.getJSONArray("expect")
            assertEquals(id, expect.length(), result.size)
            for (k in 0 until expect.length()) {
                val e = expect.getJSONObject(k)
                val p = result[k]
                val what = "$id / ${items[k].getString("name")}"
                assertEquals("$what: kip", e.getString("mode"), p.mode)
                near(paints[k].cover, p.cover, "$what örtü")
                if (p.mode == TextPaint.MODE_COVER) {
                    assertNull(what, p.spot)
                    assertNull(what, p.twin)
                    assertNull(what, p.label)
                    continue
                }
                near(box(e.getJSONArray("spot")), p.spot, "$what spot")
                near(box(e.getJSONArray("room")), p.spotRoom, "$what spotRoom")
                if (p.mode == TextPaint.MODE_BESIDE) {
                    assertEquals(what, e.getString("label"), p.label)
                    assertNull(what, p.twin)
                } else {
                    near(box(e.getJSONArray("twin")), p.twin, "$what twin")
                    assertEquals(what, e.getString("bg"), p.background)
                    assertNull(what, p.label)
                }
            }
        }
    }
}
