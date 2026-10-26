# Pocket Wreckshop agent rules

Read `Documentation/DEVICE_MODEL_RULES.md` before adding or changing device art or models.
Read `README.md` for project setup and build commands.

- Use original stylized pixel art informed by recognizable real-device silhouettes. No photographic destruction textures, burnt/swollen batteries, organic-looking components, generic five-piece end cards, or side decorations.
- Keep the whole device identifiable in its final wrecked frame.
- Every device has exactly THREE destruction steps. Motherboards are the ONLY salvage item. Preserve this gameplay contract when adding models.
- The loop is buy used devices → break → collect motherboards → sell for dollars. Dollars are the only currency. There is no crafting, building, recipe system or cash bonus for breaking. Each paid device must return more dollars in motherboard sales than its purchase price. Keep the $0 scrap phone for recovery.
- Do not add health bars, phase counters, damage statistics, or promotional copy to the bench. Show the current hammer simply. Keep wallet, navigation, and necessary collection actions usable. Navigation uses an open/close side drawer, with no bottom tabs.
- Preserve save IDs, catalogue order and the existing package identifier. Append new device IDs; never reorder the first four legacy phone IDs. Add migrations for schema changes and preserve pending rewards and currency. Older crafted phones are converted to dollars during migration.
- Motion changes must keep damage and sound at the hammer contact frame. Hard devices should recoil slightly, not wobble or stretch. Debris skitters on the desk rather than falling through it.
- Each model uses persistent damage states (currently 19 assets) and 12 matching fragments. Small cracks, missing glass sections, case chips and exposed internals accumulate; changing steps must never replace the whole surface at once. These are physical damage details selected by health, not a timed animation. Use Art/Effects flipbooks/chips for hits. Automation only strikes the current bench device, pauses for menus/help/background and ends at completion; no automatic purchases, sales or offline rewards.
- Use the sprite atlas for every world sprite. Separate textures caused incorrect sprite sampling in this project's rendering setup.
- Regenerate only the intended assets. Do not run retired generators or recreate removed photographic/side-tray art.
- Run `Pocket Wreckshop > Validate gameplay & saves` after gameplay/catalogue changes. Check all new frames in Play mode and test on portrait viewports before final exports.
