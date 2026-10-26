# Pocket Wreckshop · Unity 2D

An electronics-smashing game for 9:16 portrait screens. Open with Unity **6000.6.6f1**, load `Assets/Wreckshop/Scenes/Wreckshop.unity`, and press Play.

To edit the sprites, damage details, hammer motion or hit effects yourself, start with [the editing guide](Documentation/EDITING_ART_AND_ANIMATION.md). After replacing an existing PNG, use **Pocket Wreckshop → Refresh art only** to import and repack it while preserving the scene and catalogue.

## Game loop

Buy a used device with dollars → break it → collect motherboards → sell them for dollars. Buy more used devices and stronger hammers. **Dollars are the only currency.** Motherboards are inventory items, sold for $60 each. There is no crafting, Build screen, recipe system or automatic cash reward for destruction. A starter phone is already on the bench. A $0 scrap phone remains available if you run out of money; it takes longer to break. Every paid device yields a net dollar profit when its motherboards are sold.

Tap a device or press Space. Each has three destruction steps with nineteen aligned pixel sprites, ending in a whole-device wreck. Hammer contact, damage, audio, recoil and chips occur together after a 90 ms wind-up, followed by a rebound. Chips skitter across the desk. One tap can be buffered; opening the menu, help or another page cancels an unfinished swing.

## Auto Hammer and hit effects

Open **Hammers** and unlock Auto Hammer for **$150**. It hits once every **1.2 seconds** using your equipped hammer. Speed upgrades cost **$300** (0.75 seconds) and **$600** (0.45 seconds). The on/off button and upgrades stay in the Hammers menu; the bench keeps its simple tool display.

Automation only hits the device currently on the visible bench. It pauses in menus, help, other pages, when the app loses focus or enters the background, and when the device is fully broken. Collection, device purchases and motherboard sales remain manual. There is no offline catch-up. Ownership, speed tier and on/off preference are saved; version-4 saves migrate with automation locked.

Damage now accumulates in small physical details: short local cracks, chipped glass, separate sections of the screen coming away, then small sections of casing revealing internals. Earlier damage remains visible as the next piece breaks. Crossing a destruction-step boundary makes a small change; it does not replace the whole front with an empty metal panel. The existing state assets are selected by health and remain on the device until another hit. They do not play as a timed damage animation.

The existing transient impact effects and Auto Hammer remain. No additional destruction steps or animation frames were added for this refinement.

## Controls

The top-left menu button opens a sliding side drawer: **Workbench**, **Used devices**, **Hammers**, and **Sell motherboards**. Select a page, tap X, tap outside, or press Escape to close it. Escape/Android Back also closes help. There are no bottom tabs. The bench displays the current hammer simply, without health bars, phase counters or damage statistics. Inventory offers Sell one and Sell all, with dollar values shown before sale.

Seven device families are included: phone, tablet, headphones, console, controller, speaker and monitor. Catalogue IDs 0–3 remain reserved for legacy phone products. New IDs must be appended.

## Adding models

Read **AGENTS.md** and **Documentation/DEVICE_MODEL_RULES.md** before changing models. They define the original pixel style, real-device silhouette references, 192×224 working canvas, nineteen aligned frames and twelve matching fragments, three steps, whole-device final wreck, common sprite atlas, motherboard-only salvage, dollar economy, side-menu navigation and verification.

- `Assets/Wreckshop/Runtime/GameSession.cs`: purchases, destruction, collection and sales.
- `Assets/Wreckshop/Runtime/GameCatalog.cs`: devices, hammers and board prices.
- `Assets/Wreckshop/GameCatalog.asset`: Inspector-editable catalogue.
- `Assets/Wreckshop/Runtime/SaveStore.cs`: schema-v5 saves and migrations.
- `Assets/Wreckshop/Runtime/WorkshopUI.cs`: side drawer, bench, used devices, tools and sales.
- `Assets/Wreckshop/Runtime/WorkshopGame.cs`: input, contact timing, cancellation and audio.
- `Assets/Wreckshop/Runtime/PhoneVisual.cs`: rigid recoil, hammer animation and debris.
- `Assets/Wreckshop/Runtime/BreakEffect.cs`: transient sprite flipbooks and damped debris.
- `Tools/generate_impact_sprites.py`: original pixel impact bursts, dust and chips.
- `Tools/generate_device_sprites.py`: original pixel art.
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

PlayerPrefs retains the key `PocketWreckshop.Unity.Save.v1` and backup key. The internal schema is version 5. Old inventories convert to motherboards, and pending rewards remain collectable. Removed paid recipes are refunded ($120/$260), and an already assembled unsold phone is converted to its saved dollar sale value. Migration credits apply once; new saves contain no crafting state. Currency, owned hammers and the active device are retained. Existing fourth-stage saves become collectable three-stage wrecks.

Font licenses, design references, model rules, build reports and verification screenshots are included.
