# Dollar lottery

Current update: Auto Hammer controls and automatic strikes are disabled, including when an old save has automation enabled. Saved ownership remains unchanged. Manual-only validation passed 50,363 checks, plus portrait Play mode checks recorded in `Manual-Hammer-Checks.json`.

The visible catalogue contains only Classic phone (ID 10) and Classic tablet (ID 11). Legacy IDs remain stored for save migration, and cannot be purchased. There is one free Hammer with power 1 and no critical hits.

| Device | Purchase | HP per layer | Win chance | Win payout | Loss payout |
| --- | ---: | ---: | ---: | ---: | ---: |
| Classic phone | $0 | 100 | 100% | $1 | $0 |
| Classic tablet | $499 | 1,000 | 70% | $800 | $0 |

Each device has three layers. The final hammer contact rolls once and stores the dollar result. The bench displays the amount earned. Collect credits that result once, including a zero-dollar Continue path on failure. Reloading never rerolls. There is no motherboard inventory or sales page. The physical component art is preserved.

Save schema 6 retains the existing primary/backup keys. Older inventory converts at $60 per motherboard; pending rewards become pending dollars. Paid removed hammers are refunded. An unfinished retired device becomes a Classic phone with proportional HP and a purchase refund. Classic phone/tablet HP is rescaled proportionally. Existing crafting migrations, automation ownership, sound and pending currency remain supported. Migration credits do not repeat.

Validation passed **50,672 checks** for exact hit counts, every layer's HP, final-only lottery rolls, 70% threshold boundaries, guaranteed phone rewards, deterministic 700/1000 winning rolls, zero-dollar failures, saved outcomes, duplicate collection prevention, Classic-only purchases, free recovery, automation and versions 1–5 migration. Both original local primary and backup saves were separately decoded successfully and restored byte-for-byte after tests.

Play mode passed **58 checks** across 450×800, 360×640 and 390×844: exactly two cards, one hammer, no motherboard UI, final-contact rewards, amount-earned display, persistent win/loss results, collection, cancellation when opening menus, and free phone payout. The temporary helper is recorded in `Lottery-Play-Checks.cs.txt`, excluded from player compilation. Screenshots are `Lottery-{Shop,Hammer,Win,Loss,Menu,Phone}-{450,360,390}.png`.

WebGL and Android exports succeeded. The APK signature was verified; the package identifier remains `studio.pocketwreckshop.game`.
