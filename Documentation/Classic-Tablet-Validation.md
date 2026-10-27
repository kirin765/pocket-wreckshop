# Classic tablet

The new device is appended at catalogue ID 11. It costs $70 and returns three motherboards worth $180 when sold. Existing IDs and saves remain unchanged.

`Assets/Wreckshop/Art/Devices/LayeredTablet/ipad.aseprite` is an unchanged copy of the supplied Downloads file (SHA-256 `7296f21388e802dedbf065ec5a58d800cb125bd4d72423e5ef75975da526a47d`). Native individual-layer import preserves its canvas registration and original pixels. All six sprites use the common atlas; no replacement artwork was generated.

- Layer 6: front, health slots 00–05, with persistent screen cracks.
- Layer 1: full backing, health slots 06–18.
- Layers 2–5: four separately mounted components visible during the second step. Each vibrates independently on contact and settles onto its authored mount. Clearing that step releases and fades all four over 0.28 seconds. They stay hidden after reload. The backing remains opaque and identifiable through the final step.

There are exactly three gameplay steps and no particle substitutes. Existing hammer contact timing, rewards, collection, currency and automation behavior are preserved.

Validation: **2,662 gameplay/save checks passed**. Play mode: **239 checks passed**, including contact timing, four independent vibrations, exact settling, one-HP attachment, clearing-only fade, no stale overlays/particles, and all nineteen health-selected frames at 450×800, 360×640 and 390×844. Portrait captures preserve the game's safe-area letterboxing. The original local primary and backup saves were restored after testing. `Tablet-Play-Checks.cs.txt` records the temporary verification helper; it is excluded from the game.

Preview: `Classic-Tablet.png` and `Classic-Tablet.gif`. Detailed evidence: `Tablet-Play-Checks.json` and `Tablet-Frame*.png`.

Both WebGL and Android exports succeeded. The APK signature was verified.
