# Pocket Wreckshop agent rules

Read `Documentation/DEVICE_MODEL_RULES.md` before adding or changing device art or models.
Read `README.md` for project setup and build commands.

- Use original stylized pixel art informed by recognizable real-device silhouettes. No photographic destruction textures, burnt/swollen batteries, organic-looking components, generic five-piece end cards, or side decorations.
- Keep the whole device identifiable in its final wrecked frame.
- Every device has exactly THREE destruction steps. Rewards are dollar lottery payouts only. Preserve this gameplay contract when adding models.
- The loop is buy → break three layers → reveal payout → collect dollars. No motherboard inventory, sales, crafting or recipes. Show only Classic phone (ID 10: free, 100 HP/layer, guaranteed $1) and Classic tablet (ID 11: $499, 1000 HP/layer, 70% $800 / 30% $0). One manual hammer, power 1; no critical hits. Auto Hammer controls and automatic strikes are disabled. Keep the free Classic phone for recovery. Lottery outcomes are fixed at the final contact and persist until collected.
- Do not add health bars, phase counters, damage statistics, or promotional copy to the bench. Show the current hammer simply. Keep wallet, navigation, and necessary collection actions usable. Navigation uses an open/close side drawer, with no bottom tabs.
- Preserve save IDs, catalogue order and the existing package identifier. Append new device IDs; never reorder the first four legacy phone IDs. Add migrations for schema changes and preserve pending rewards and currency. Older crafted phones are converted to dollars during migration.
- Motion changes must keep damage and sound at the hammer contact frame. Hard devices should recoil slightly, not wobble or stretch. Debris skitters on the desk rather than falling through it.
- Each model uses persistent damage states (currently 19 assets) and 12 matching fragments. States 00–05 accumulate surface cracks, 06–11 accumulate casing damage, and 12–17 accumulate internal damage; 18 is the recognizable whole-device wreck. Replace the complete current layer at 06 and 12, one layer per phase boundary, with fully opaque pixels and no crossfade, fade-out, translucent ghosting or irregular peeling mask. Keep the outer silhouette aligned and mount exposed modules on the shared backing silhouette. These persistent states are selected by health, not a timed animation. Use Art/Effects flipbooks/chips for hits. No automatic strikes, purchases, sales or offline rewards. Preserve retired automation ownership fields for save compatibility.
- Use the sprite atlas for every world sprite. Separate textures caused incorrect sprite sampling in this project's rendering setup.
- Regenerate only the intended assets. Do not run retired generators or recreate removed photographic/side-tray art.
- Run `Pocket Wreckshop > Validate gameplay & saves` after gameplay/catalogue changes. Check all new frames in Play mode and test on portrait viewports before final exports.
