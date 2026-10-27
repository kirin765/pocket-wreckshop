# Edit the artwork and breaking behavior yourself

Open this repository's root folder in Unity **6000.6.6f1**. Open `Assets/Wreckshop/Scenes/Wreckshop.unity` and press Play to try your changes. Exit Play mode before editing or refreshing assets.

## Start with one sprite

1. Open `Assets/Wreckshop/Art/Devices/phone_01.png` in your pixel editor.
2. Change the crack or chipped area. Keep the existing image dimensions, transparent background and device position.
3. Save over the same PNG. Keep its `.meta` file; it contains Unity's asset identity.
4. Return to Unity and choose **Pocket Wreckshop → Refresh art only**.
5. Press Play and hit a phone. The first damaged appearance should include your edit.

The refresh command imports sprites and packs the shared atlas. It preserves PNG contents, the scene and catalogue values. You do not need to run the sprite generators or rebuild the scene for this workflow.

## Which files control what?

| What you want to change | File or folder |
| --- | --- |
| Permanent phone damage | `Assets/Wreckshop/Art/Devices/phone_00.png` through `phone_18.png` |
| Other device families | Same folder, using `tablet`, `headphones`, `console`, `controller`, `speaker` or `monitor` |
| Pieces that fly off | `Assets/Wreckshop/Art/Devices/Fragments/<kind>_<step>_<piece>.png` |
| Hammer appearance | `Assets/Wreckshop/Art/hammer.png`, `hammer_1.png`, `hammer_2.png`, `hammer_3.png` |
| Impact flash, chips and dust | `Assets/Wreckshop/Art/Effects/` |
| Hammer arc, angle, recoil and debris | [PhoneVisual.cs](../Assets/Wreckshop/Runtime/PhoneVisual.cs) |
| When damage and sound happen | [WorkshopGame.cs](../Assets/Wreckshop/Runtime/WorkshopGame.cs), `Attack()` |
| How health selects a damage appearance | [GameCatalog.cs](../Assets/Wreckshop/Runtime/GameCatalog.cs), `DamageState()` |
| Effect playback, drift and fading | [BreakEffect.cs](../Assets/Wreckshop/Runtime/BreakEffect.cs) |
| Device health, hammer strength and Auto Hammer speed | `Assets/Wreckshop/GameCatalog.asset` in Unity's Inspector |

## Permanent damage details

The numbered device images represent damage that stays after a hit. They do not play in a timed sequence.

| Images | Purpose |
| --- | --- |
| `00` | Intact device |
| `01`–`05` | Small cracks and chips accumulating on the surface |
| `06`–`11` | Complete opaque casing, accumulating scuffs and dents |
| `12`–`17` | Complete opaque internal layer, accumulating localized damage |
| `18` | The whole device in its final wrecked state |

Keep each family's images the same size and aligned, including the opaque silhouette and negative-space cutouts through frame 17. At 05→06, replace the damaged surface with the complete casing; at 11→12, replace the casing with the internal components. Do not crossfade, fade out, use translucent ghosting or reveal irregular peeled pieces. Each boundary changes only one layer. Start by editing existing images rather than changing their count. Unity imports them with Point filtering, no compression and 100 pixels per unit.

For a new family, use the 192×224 working canvas and uniform crop described in [DEVICE_MODEL_RULES.md](DEVICE_MODEL_RULES.md). Existing exported PNGs are already cropped, so edit them at their current dimensions.

## Hammer timing and movement

The current hammer motion is coded in `PhoneVisual.cs`; it does not use an Animator Controller or an `.anim` clip.

| Moment | Current time from start of swing |
| --- | --- |
| Contact, damage and sound | 0.09 seconds |
| End of contact hold | 0.125 seconds |
| End of rebound | 0.26 seconds |

In `PhoneVisual.Update()`, the angles move from 115° to 205°, then rebound to 140°. `GripLocal` and `HeadLocal` determine the pivot and striking point. Keep the hammer drawing in the same position on its 64×64 canvas unless you also adjust these points.

If you change the contact time, update both `PhoneVisual.Update()` and the first `WaitForSecondsRealtime` in `WorkshopGame.Attack()`. The second wait in `Attack()` covers the remaining hold/rebound. Damage, audio and debris should happen together when the hammer reaches the device. Animation duration and damage detail count are independent.

`PhoneVisual.Impact()` controls chip counts, sizes, speed and effect lifetimes. `BreakEffect.cs` plays the short impact/dust images and fades moving pieces. These temporary effects are separate from the permanent numbered damage images.

## Avoid overwriting your edits

- `Tools/generate_device_sprites.py` rewrites device PNGs and fragment PNGs. Running it after hand drawing will replace those edits.
- `Tools/generate_tool_sprites.py` and `Tools/generate_impact_sprites.py` rewrite their corresponding art.
- **Create or rebuild scene** rewrites the scene and default catalogue, including manually adjusted health, prices and automation values.
- Use **Refresh art only** after editing existing artwork. Commit PNGs and their `.meta` files together so you can restore earlier versions.

## Check and share your changes

Run **Pocket Wreckshop → Validate gameplay & saves**, then inspect the edited artwork in Play mode. For the current opaque-layer, alignment and first-crack constraints, you can also run `python3 Tools/verify_damage_details.py` with Pillow installed. Changing those visual constraints may require updating that art check.

When ready, export using **Pocket Wreckshop → Build → WebGL** or **Build → Android APK**. Build output and Unity caches are ignored by Git. Commit your source changes and push `main` to GitHub; publishing the playable website is a separate action.
