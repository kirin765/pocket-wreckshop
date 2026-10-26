# Device model contract for future agents

This is an asset/gameplay specification for adding models to this Unity project.

## Art

1. Start from the real device's proportions and recognizable silhouette: e.g. the front camera island and flat rounded phone frame, a tablet's even bezels, console side plates, a controller's grips and sticks. Make original simplified art; do not paste product photographs or logos.
2. Draw on a transparent 192 × 224 working canvas in the existing muted pixel palette. Use crisp contours and small flat color groups. Use `Tools/generate_device_sprites.py` as the source of truth for examples and reusable shapes.
3. Batteries are sealed, flat modules. Never draw inflated, wrinkled, burnt or organic-looking materials. Avoid photographic noise and excessively graphic internals.
4. Export NINETEEN aligned frames. States 00–05 add small local cracks/chips, 06–11 remove individual pieces of the damaged surface to reveal casing, 12–17 expose internal sections gradually, and 18 is the completed wreck. Keep previous damage when crossing step boundaries. Never replace the entire screen with a bare panel or reveal all internals in one hit. Use health to select a persistent state, not an animation clock. Frame 18 must show the whole model broken in place, including its own cracked/displaced case and modules. Do not replace it with five generic resource icons.
5. All frames in a family must share one transparent crop rectangle, scale, center and pivot. Frame changes must not make the model jump. Work out the union bounds across the sequence, add a small margin, then apply that crop to every frame.
6. Save as `Assets/Wreckshop/Art/Devices/<kind>_00.png` through `<kind>_18.png`. Import as Sprite/Single, 100 pixels per unit, Point filtering, no mipmaps, and no texture compression. Include `.meta` files in deliverables.
7. Export twelve model-specific fragment sprites as `Devices/Fragments/<kind>_<phase>_<0..3>.png`, cut from that model’s glass, casing and modules. Use the same palette and the common atlas. Impact flipbooks and material chips live in Art/Effects. They are transient visual effects, not collectible inventory.
8. Keep the desk clear. Do not add trays, bulletins, badges, posters or decorative side props.

## Catalogue

- Add a `DeviceKind` only when it is a new family; a variant of an existing family can reuse its kind.
- Add the definition through `WorkshopBuilder` and assign nineteen sprites and THREE named steps. Keep existing indices stable, especially phone IDs 0–3. Append new IDs.
- Set health, purchase price, motherboard yield (`partsPerType`), and zero scrap cash explicitly. Motherboards remain the sole salvage item and are sold for dollars. Dollars are the only currency; do not add per-model currencies or component inventories. Each paid device must yield more motherboard sale value than its purchase cost.
- Add its catalogue entry to the device shop. Keep the 9:16 layout readable; use pagination or scrolling if the catalogue no longer fits. Do not shrink controls to fit more items.
- Pack the new model with `DeviceAtlasBuilder`; every world sprite must be in the common atlas.
- Do not add crafting, recipes or a Build screen. Breaking yields motherboards, with no new cash bonus; players earn dollars by selling their inventory. Keep the zero-dollar recovery device available.

## Animation and UI

- Contact, visible damage, recoil, audio and debris happen together at 90 ms into the swing. Preserve the brief contact hold and rebound. Do not apply damage on pointer-down before contact.
- Auto Hammer is purchased/upgraded with dollars in Hammers and has an on/off switch. It only hits the current device on the visible bench, pauses in menus/help or app background, and stops at completion. Never auto-buy, collect, sell, or simulate offline rewards.
- Keep devices rigid. Small damped translation/rotation is enough; no rubber-like squash or rapid jitter. Use zero-gravity, damped debris to simulate pieces skittering over a tabletop.
- Keep the current hammer display simple. Do not add bench status readouts, health/phase indicators or marketing slogans. A model should communicate damage visually.
- User-facing text should be plain and brief. Use dollars for prices. Navigation lives in a collapsible side menu; never reintroduce bottom tabs.

## Verify

Run the catalogue/gameplay validator in Unity. Verify frame alignment, three-step completion, motherboard rewards, motherboard sales and profitable device purchase/sale, old-save compatibility and rapid taps/navigation during swings. Inspect the pristine and final-wreck frames at 450×800, then check 360×640 and 390×844 viewports. Rebuild both WebGL and Android for a deliverable change.
