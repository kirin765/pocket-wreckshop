# Device model contract for future agents

This is an asset/gameplay specification for adding models to this Unity project.

## Art

1. Start from the real device's proportions and recognizable silhouette: e.g. the front camera island and flat rounded phone frame, a tablet's even bezels, console side plates, a controller's grips and sticks. Make original simplified art; do not paste product photographs or logos.
2. Draw on a transparent 192 × 224 working canvas in the existing muted pixel palette. Use crisp contours, small stepped shades and material-specific details: machined-metal edges, restrained glass reflections, molded plastic, rubber recesses and legible fabric weave. Prefer a consistent upper-left light source and solid palette colors. Do not use photographic noise or copied textures. Use `Tools/generate_device_sprites.py` as the source of truth for examples and reusable shapes.
3. Batteries are sealed, flat modules. Never draw inflated, wrinkled, burnt or organic-looking materials. Avoid photographic noise and excessively graphic internals.
4. Export NINETEEN aligned frames. States 00–05 accumulate surface cracks/chips, 06–11 accumulate casing scuffs/dents, and 12–17 accumulate localized internal damage; 18 is the completed wreck. Frames 00, 06 and 12 are complete opaque layer bases. Replace only the current complete layer at 05→06 and 11→12, with no alpha/color crossfade, fade-out or partial peeling mask. Never reveal all layers in one hit. Use health to select a persistent state, not an animation clock. Frame 18 must show the whole model broken in place, including its own cracked/displaced case and modules. Do not replace it with five generic resource icons.
5. All frames in a family must share one transparent crop rectangle, scale, center and pivot. Keep the same opaque silhouette and negative-space cutouts through frames 00–17; mount exposed modules on a shared backing silhouette so they never float when a layer changes. Frame changes must not make the model jump. Work out the union bounds across the sequence, add a small margin, then apply that crop to every frame.
6. Save as `Assets/Wreckshop/Art/Devices/<kind>_00.png` through `<kind>_18.png`. Import as Sprite/Single, 100 pixels per unit, Point filtering, no mipmaps, and no texture compression. Include `.meta` files in deliverables.
7. Export twelve model-specific fragment sprites as `Devices/Fragments/<kind>_<phase>_<0..3>.png`, cut from that model’s glass, casing and modules. Use the same palette and the common atlas. Impact flipbooks and material chips live in Art/Effects. They are transient visual effects, not collectible inventory.
8. Keep the desk clear. Do not add trays, bulletins, badges, posters or decorative side props.
9. For user-supplied layered PNGs, use the original files directly instead of generating replacement artwork. `phoneN.png` is layer N; `phoneN-partsM.png` is mounted part M belonging to layer N. Preserve their source-canvas alignment and image pixels. Parts stay mounted and fully opaque during ordinary hits, vibrating independently and settling exactly back onto their mounts. Only the contact that clears their owner layer releases them; the released sprites briefly slide/rotate away and fade out over 0.28 seconds. Do not leave cleared pieces overlapping the next layer or restore them from saves. Keep the complete backing and incoming layer fully opaque; fading applies only to released part sprites. For this supplied layered phone, do not spawn impact, dust, chip or sampled fragment particle substitutes. Keep the whole backing recognizable. They are separate from flying debris and salvage. The Classic phone follows this convention and reuses its three supplied layers across the existing health slots. For `phone2.aseprite`, import the source directly as individual layers: `Layer` is the backing and `Layer 1`/`Layer 2` are mounted components on gameplay layer 2. Preserve Aseprite canvas pivots rather than fitting components by hand, disable generated animation/private atlases, and pack native sprites into the common atlas. Its front glass has persistent pixel crack geometry overlaid from health; keep the supplied image unchanged and remove the crack overlay when layer 0 is removed.

10. The supplied `LayeredTablet/ipad.aseprite` follows the same direct-import exception: Layer 6 is the front, Layer 1 is the full backing, and Layers 2–5 are individually mounted components. Keep native canvas pivots. Use three gameplay steps: front, all four components on their backing, then bare backing. Components vibrate during hits and release/fade only on clearing the component step; no particle substitutes. Preserve the source file unchanged and pack every layer into the common sprite atlas.

## Catalogue

- Add a `DeviceKind` only when it is a new family; a variant of an existing family can reuse its kind.
- Add the definition through `WorkshopBuilder` and assign nineteen sprites and THREE named steps. Keep existing indices stable, especially phone IDs 0–3. Append new IDs.
- The dollar lottery supersedes the retired salvage economy. Only IDs 10 and 11 are visible/purchasable: Classic phone costs $0, has 100 HP per layer and pays $1 with 100% success; Classic tablet costs $499, has 1000 HP per layer and pays $800 with 70% success or $0 with 30% failure. There is exactly one hammer with power 1 and no critical hits. Keep legacy catalogue IDs for migration.
- Add its catalogue entry to the device shop. Keep the 9:16 layout readable; use pagination or scrolling if the catalogue no longer fits. Do not shrink controls to fit more items.
- Pack the new model with `DeviceAtlasBuilder`; every world sprite must be in the common atlas.
- No inventory, sales, crafting, recipes or Build screen. Roll the dollar payout once at the final contact, show the amount earned, and let the user collect it once. Persist the result without rerolling. Keep the free Classic phone available.

## Animation and UI

- Contact, visible damage, recoil, audio and debris happen together at 90 ms into the swing. Preserve the brief contact hold and rebound. Do not apply damage on pointer-down before contact.
- Auto Hammer controls and automatic strikes are disabled. Only manual power-1 hits are available. Preserve old automation ownership fields for save compatibility, without exposing upgrades or changing saved ownership. Never auto-buy, collect, sell, or simulate offline rewards.
- Keep devices rigid. Small damped translation/rotation is enough; no rubber-like squash or rapid jitter. Use zero-gravity, damped debris to simulate pieces skittering over a tabletop.
- Keep the current hammer display simple. Do not add bench status readouts, health/phase indicators or marketing slogans. A model should communicate damage visually.
- User-facing text should be plain and brief. Use dollars for prices. Navigation lives in a collapsible side menu; never reintroduce bottom tabs.

## Verify

Run the catalogue/gameplay validator in Unity. Verify frame alignment, three-step completion, deterministic lottery boundaries, dollar collection, failed payouts, exact HP and power, old-save compatibility and rapid taps/navigation during swings. Inspect the pristine and final-wreck frames at 450×800, then check 360×640 and 390×844 viewports. Rebuild both WebGL and Android for a deliverable change.
