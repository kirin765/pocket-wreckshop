# Current behavior: mounted vibration, release on clear, then fade

Parts remain mounted and fully opaque at every nonzero health value in their current layer. Ordinary hits cause independent rigid vibration that settles onto the authored mounts. Only the hammer contact that clears the layer releases its parts. Released sprites slide/rotate briefly and fade over 0.28 seconds, then hide; they do not remain over the next layer or reappear after reload. The backing and incoming layer stay fully opaque. Particle substitutes remain disabled for this phone. Save schema, three destruction steps, pending rewards and manual collection remain unchanged.

The descriptions below record earlier iterations; the behavior above supersedes progressive health-based scattering and persistent desk pieces.

# Current part behavior

For the supplied layered phone, only its actual PNG/Aseprite components separate. Impact flipbooks, dust, chip bursts and sampled fragment particles are disabled. Parts loosen and slide/rotate apart at staggered health thresholds; they remain fully opaque on the tabletop across layer changes and completion until manual collection. The complete backing remains recognizable. Detached parts have a stable desk parent and do not inherit later backing recoil. Existing health/phase/pending-reward data reconstructs their exact resting positions after reload; no save migration or new gameplay rewards are introduced.

Play checks passed contact timing, different detach thresholds, no particles, actual source sprites, persistent scatter, no fading, layer carry-over, exact save restoration and manual collection cleanup.

## Source artwork and earlier checks

Uses the supplied PNG files and native `phone2.aseprite` source directly. Project copies under `Assets/Wreckshop/Art/Devices/ClassicPhone/` are byte-for-byte identical to the originals. SHA-256 values are recorded in `Classic-Phone-Source-Checks.json`.

- `phone0.png`: front surface.
- `phone1.png`: casing/shield plate.
- `phone2.aseprite`, `Layer`: internal backing and completed state.
- `phone2.aseprite`, `Layer 1`: battery mounted on phone layer 2.
- `phone2.aseprite`, `Layer 2`: motherboard mounted on phone layer 2.
- `phone2.png`: retained for internal fragment slices.
- `phone1-parts0.png`: ribbon mounted on layer 1.
- `phone1-parts1.png`: plate mounted beneath the ribbon on layer 1.
- `phone2-parts0.png`: retained source reference; superseded on the bench by the Aseprite motherboard.

All four mounted parts shake at hammer contact and settle exactly back to their individual mounts; the source PNGs remain unchanged.

The generated replacement art and its generator have been removed. There is no replacement image synthesis, recoloring or generated wreck frame. Front-glass damage is a separate pixel mesh over the original image; it never rewrites a supplied PNG. PNG sprite rectangles register the first two device layers on 73×150 canvases. The Aseprite source is 228×168: its backing and both components use a shared canvas pivot at source pixel (112.5, 81), 100 PPU and scale 1, preserving authored offsets despite tightly cropped native sprite rectangles. Native individual-layer import uses no generated animation or private sprite atlas. The original PNG pixels and transparency are preserved. Twelve flying chip sprites are metadata slices of the supplied glass, casing and internals. Mounted `phoneN-partsM.png` images are independent of flying chips: they belong to layer N, appear whole at their original canvas positions, and shake briefly at hammer contact before settling exactly back on their mounts. They hide when their owning layer is removed. Both mounted parts and chips use the shared sprite atlas; neither changes salvage inventory.

The existing nineteen health-selected slots reuse the supplied layers: 00–05 front, 06–11 casing, 12–18 internals. Layer switches happen at the two existing phase boundaries. Hits retain the existing impact effects, audio and rigid recoil. On the front layer, five increasing fracture levels are rebuilt deterministically from the existing health-selected state. Cracks start at hammer contact, persist after transient effects and save/reload, remain inside the glass, and hide when the screen layer is removed. Pristine devices have no crack overlay. The material renders opaque colored pixel geometry; no extra world sprite or separate sprite texture was added. This device intentionally uses supplied artwork rather than the generated per-hit damage-state workflow used by the other models.

Catalogue ID 10, $45 price, two-motherboard reward, dollar-only economy and three destruction steps are preserved. The first ten IDs, save schema and package identifier remain unchanged.

Unity compilation passed with zero errors/warnings. Gameplay/save validation passed 2,428 checks, including direct source asset paths for all nineteen slots and the mounted layer-1 ribbon. Play mode checked every health slot against its assigned packed sprite, contact timing, navigation cancellation and purchases at 450×800, 360×640 and 390×844. Portrait screenshots show the supplied front/internal layers and the scrolled shop. Additional Play checks verified owner-layer visibility, atlas packing, no wind-up movement, contact-triggered motion, settling, cancelled swings, layer removal and empty-bench hiding. The shake screenshot samples 25 ms after contact deterministically to avoid Editor capture stalls. Mounted-part portrait checks also passed at 450×800, 360×640 and 390×844; the visible source pixels fit all three viewports. The original Editor save and backup were restored after checks.

Corrected WebGL and Android ARM64 builds succeeded. APK signature verification passed. Exports are in `Builds/WebGL/` and `Builds/Android/PocketWreckshop.apk`. Physical Android handset testing remains pending.

Front-glass Play checks passed: pristine state, contact timing, persistence, identical geometry after save restoration, five growing fracture levels, glass clipping, cancelled swings, layer changes, new devices and empty bench. Screenshots: `Classic-Phone-Cracks-First-Hit.png` and `Classic-Phone-Cracks-Heavy.png`.

The native file has one frame and three saved layer names: `Layer` (backing), `Layer 1` (battery), `Layer 2` (motherboard). Both isolated component layers are mounted on gameplay layer 2. Play checks verified native source references, authored positions without manual fitting, common-atlas packing, contact shake, exact settle, correct owner-layer visibility and empty-bench hiding.

Mounted parts now have separate motion clocks, contact-relative impulse directions, frequencies, damping, amplitudes and alternating rotation directions. Rotation is compensated around each visible component center, preserving its authored Aseprite/PNG canvas pivot at rest. Scale remains fixed. Play checks verify different simultaneous translations/rotations, one part settling while another remains in motion, owner-layer visibility, contact timing and exact independent settling.
