# Pocket Wreckshop · Unity 2D

An electronics-smashing game for 9:16 portrait screens. Open with Unity **6000.6.6f1**, load `Assets/Wreckshop/Scenes/Wreckshop.unity`, and press Play.

To edit the sprites, damage details, hammer motion or hit effects yourself, start with [the editing guide](Documentation/EDITING_ART_AND_ANIMATION.md). After replacing an existing PNG, use **Pocket Wreckshop → Refresh art only** to import and repack it while preserving the scene and catalogue.

## Game loop

Buy a device → break all three layers → reveal the dollar result → collect. There is no inventory or sales step. Only **Classic phone** and **Classic tablet** appear in the shop, and there is one free **Hammer** with power **1** (no critical hits).

| Device | Price | HP per layer | Success / failure | Payout on success / failure |
| --- | ---: | ---: | --- | --- |
| Classic phone | $0 | 100 | 100% / 0% | $1 / $0 |
| Classic tablet | $499 | 1,000 | 70% / 30% | $800 / $0 |

The final contact rolls the result once. The bench shows how much was earned, and collection credits that exact amount once. A saved pending result never rerolls. The free phone remains available at $0. New games start with $100 and a Classic phone on the bench.

Tap a device or press Space. Each has three destruction steps with nineteen aligned pixel sprites, ending in a whole-device wreck. Hammer contact, damage, audio, recoil and chips occur together after a 90 ms wind-up, followed by a rebound. Chips skitter across the desk. One tap can be buffered; opening the menu, help or another page cancels an unfinished swing.

## Hammer and hit effects

Only the manual power-1 Hammer is available. Auto Hammer controls are hidden and automatic strikes are disabled, including for saves that previously owned and enabled automation. Old automation ownership stays in the save for compatibility; no upgrade charges or settings changes occur.

Damage begins with short local cracks and chipped glass. Small cracks, scuffs and chips accumulate within each layer's six persistent states. Frame 06 replaces the complete damaged surface with the casing, and frame 12 replaces the casing with the internal components. Layers stay fully opaque: there is no crossfade, fade-out, translucent ghosting or irregular peeling mask. Only one layer changes at a phase boundary, and the outer device remains aligned. States are selected by health and remain until another hit; they do not play as a timed damage animation.

The existing transient impact effects remain. No additional destruction steps or animation frames were added for this refinement.

## Controls

The top-left menu button opens a sliding side drawer: **Workbench**, **Used devices**, and **Hammer**. Select a page, tap X, tap outside, or press Escape to close it. Escape/Android Back also closes help. There are no bottom tabs. The bench shows the current hammer and, after completion, the dollar payout.

Legacy catalogue IDs remain stored for migration, with the visible Classic phone at ID 10 and Classic tablet at ID 11. The Classic phone uses the supplied PNG/Aseprite layers; the tablet directly imports `ipad.aseprite` with Layer 6 as front, Layer 1 as backing, and Layers 2–5 as parts. Mounted parts vibrate separately and fade away only when their layer clears.

## Adding models

Read **AGENTS.md** and **Documentation/DEVICE_MODEL_RULES.md** before changing models. They define the original pixel style, real-device silhouette references, 192×224 working canvas, nineteen aligned frames and twelve matching fragments, three steps, whole-device final wreck, common sprite atlas, dollar lottery economy, side-menu navigation and verification.

- `Assets/Wreckshop/Runtime/GameSession.cs`: purchases, destruction, lottery results and dollar collection.
- `Assets/Wreckshop/Runtime/GameCatalog.cs`: devices, hammer and lottery terms.
- `Assets/Wreckshop/GameCatalog.asset`: Inspector-editable catalogue.
- `Assets/Wreckshop/Runtime/SaveStore.cs`: schema-v6 saves and migrations.
- `Assets/Wreckshop/Runtime/WorkshopUI.cs`: side drawer, bench, used devices, tools and sales.
- `Assets/Wreckshop/Runtime/WorkshopGame.cs`: input, contact timing, cancellation and audio.
- `Assets/Wreckshop/Runtime/PhoneVisual.cs`: rigid recoil, hammer animation and debris.
- `Assets/Wreckshop/Runtime/BreakEffect.cs`: transient sprite flipbooks and damped debris.
- `Tools/generate_impact_sprites.py`: original pixel impact bursts, dust and chips.
- `Tools/generate_device_sprites.py`: original pixel art.
- `Assets/Wreckshop/Art/Devices/ClassicPhone/`: unchanged supplied layer PNGs. `ClassicPhoneAssets.cs` defines aligned sprite slices and discovers mounted parts using `phoneN-partsM.png` filenames. Use **Pocket Wreckshop → Add Classic phone** to import/update this device without rebuilding the scene. For phone layer 2, `phone2.aseprite` is imported directly: its `Layer` backing and `Layer 1`/`Layer 2` component sprites share the authored canvas pivot. Animation and private atlas generation are disabled; all sprites use the common atlas. Actual components release and fade away over 0.28 seconds only when their layer clears, without particle bursts. Cleared pieces stay hidden after reload, and the next layer remains opaque. The nineteen health slots reuse the three supplied device layers. Persistent pixel crack geometry overlays front glass as health decreases; no replacement damage images are used.
- `Assets/Wreckshop/Editor/WorkshopBuilder.cs`: catalogue/scene generation and platform builds.

The scene builder overwrites the default scene/catalogue. Dynamic page contents are created at runtime. Regenerate art only when intended, then use **Pocket Wreckshop → Create or rebuild scene** to import and pack it. All world sprites must share `DeviceSprites.spriteatlas`.

## Builds and validation

- **Pocket Wreckshop → Validate gameplay & saves** runs economy, destruction and migration checks.
- **Pocket Wreckshop → Build → WebGL** exports `Builds/WebGL/`.
- **Pocket Wreckshop → Build → Android APK** exports `Builds/Android/PocketWreckshop.apk`.

```sh
unity command script eval --project-path . -- --code 'return PocketWreckshop.Editor.WorkshopValidation.Run();'
python3 -m http.server 8078 --directory Builds/WebGL
```

Android uses ARM64/IL2CPP, portrait, minimum API 26. The APK is for local testing; physical-device testing and release signing remain pending.

## Saves

PlayerPrefs retains the key `PocketWreckshop.Unity.Save.v1` and backup key. The internal schema is version 6. Version 1–5 inventories convert immediately to dollars at their original $60 motherboard sale value; pending boards and cash become a pending dollar payout. Removed paid hammers are refunded. An in-progress retired device becomes a Classic phone with proportional layer progress and its purchase refunded. Existing Classic layer HP scales proportionally to 100 / 1,000. Old crafting refunds and finished-phone dollar conversion remain supported. Migration credits apply once. Automation ownership, sound, currency and pending rewards survive. Current saves contain no motherboard inventory or crafting state.

Font licenses, design references, model rules, build reports and verification screenshots are included.
