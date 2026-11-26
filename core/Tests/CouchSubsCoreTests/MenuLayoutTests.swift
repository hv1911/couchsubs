// SPDX-License-Identifier: AGPL-3.0-only

@testable import CouchSubsCore
import Foundation
import XCTest

/// Menüde çevirinin biçimi ayardan gelir (#1365, `MenuLayout`): B üstünü örter, A Türkçeyi yanına koyu etikete yazar
/// (yanına sığmazsa altına/üstüne, şerit bir bütün), görev kartının ikizi kartın altında. Ortak deneme dosyası
/// Fixtures/menu-layout.json (Windows MenuLayoutTests.cs, Android MenuLayoutTest.kt aynı vakalar).
final class MenuLayoutTests: XCTestCase {
    private struct Fixture: Decodable {
        struct Item: Decodable {
            let name: String
            let cover: [Double]
            let box: [Double]
            let line: Double
            let align: String
            let bg: String
            let icon: String?
        }
        struct Expect: Decodable {
            let mode: String
            let spot: [Double]?
            let room: [Double]?
            let twin: [Double]?
            let label: String?
            let bg: String?
        }
        struct Case: Decodable {
            let id: String
            let aspect: Double
            let cover: Bool
            let items: [Item]
            let cards: [[Int]]
            let expect: [Expect]
        }
        let labelColor: String
        let cases: [Case]
    }

    private func box(_ v: [Double]) -> TextBox { TextBox(left: v[0], top: v[1], right: v[2], bottom: v[3]) }

    private func near(_ expected: [Double]?, _ actual: TextBox?, _ what: String, file: StaticString = #filePath, line: UInt = #line) {
        guard let expected, let actual else { return XCTFail("\(what): kutu yok", file: file, line: line) }
        let e = box(expected)
        let ok = abs(actual.left - e.left) < 1e-6 && abs(actual.top - e.top) < 1e-6
            && abs(actual.right - e.right) < 1e-6 && abs(actual.bottom - e.bottom) < 1e-6
        XCTAssertTrue(ok, "\(what): beklenen \(e), çıkan \(actual)", file: file, line: line)
    }

    func testSharedCases() throws {
        let url = Bundle.module.url(forResource: "Fixtures", withExtension: nil)!.appendingPathComponent("menu-layout.json")
        let fixture = try JSONDecoder().decode(Fixture.self, from: Data(contentsOf: url))
        XCTAssertEqual(fixture.labelColor, MenuLayout.labelColor)
        for c in fixture.cases {
            let paints = c.items.map { item -> TextPaint in
                var paint = TextPaint(background: item.bg, pitch: item.line, cover: box(item.cover))
                paint.iconSide = item.icon.flatMap(TextLook.Align.init(rawValue:))
                // Ölçümün seçtiği kip ne olursa olsun kilitlenir.
                paint.mode = .twin
                paint.spot = box(item.cover)
                paint.spotRoom = box(item.cover)
                paint.twin = TextBox(left: 0, top: 0, right: 1, bottom: 1)
                return paint
            }
            let targets = c.items.map { item in
                TextPainter.Target(box: box(item.box), lineHeight: item.line, rows: 1, background: item.bg,
                                   foreground: "#F0F0F0", upper: false, align: TextLook.Align(rawValue: item.align)!)
            }
            let result = MenuLayout.locked(paints, targets: targets, cards: c.cards, cover: c.cover, aspect: c.aspect)
            XCTAssertEqual(result.count, c.expect.count, c.id)
            for (k, e) in c.expect.enumerated() where k < result.count {
                let p = result[k]
                let what = "\(c.id) / \(c.items[k].name)"
                XCTAssertEqual(p.mode.rawValue, e.mode, what)
                XCTAssertEqual(p.cover, paints[k].cover, what)
                if p.mode == .cover {
                    XCTAssertNil(p.spot, what)
                    XCTAssertNil(p.twin, what)
                    XCTAssertNil(p.label, what)
                    continue
                }
                near(e.spot, p.spot, what + " spot")
                near(e.room, p.spotRoom, what + " spotRoom")
                if p.mode == .beside {
                    XCTAssertEqual(p.label, e.label, what)
                    XCTAssertNil(p.twin, what)
                } else {
                    near(e.twin, p.twin, what + " twin")
                    XCTAssertEqual(p.background, e.bg, what)
                    XCTAssertNil(p.label, what)
                }
            }
        }
    }
}
