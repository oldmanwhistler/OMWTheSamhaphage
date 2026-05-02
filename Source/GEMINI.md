 Gemini Instructions: The Samhaphage Mod Development

## 1. Project Context

* **Game:** RimWorld (Targeting Version 1.6)
* **Mod Name:** The Samhaphage (Internal Namespace: `OMW_Samhaphage`)
* **Theme:** Horror, biological evolution, "Gotta Catch 'Em All" gene collection, Isekai/Litrpg.
* **Key Faction:** The Perfect Stillness (The Hivemind).
* **Core Loop:** Harvesting genes from the dead/living to evolve from weak Fluxspawn into the Sovereign Stillness.

## 2. XML Coding Standards (Strict)

* **Nomenclature:** Always use `OMW_` prefix for `defName` to avoid mod conflicts.
* **Biostats:** Use `<biostatMet>`, `<biostatCpx>`, and `<biostatArc>`. DO NOT use `<biostats><metabolism>`.
* **Categories:** Genes should generally belong to `<displayCategory>OMW_PerfectSilence</displayCategory>`.
* **Version Sensitivity:** Ensure tags are compatible with RimWorld 1.6. If a tag is from a DLC, include `MayRequire="Ludeon.RimWorld.Biotech"`.
* **Validation:** Before providing XML, double-check that every tag exists in the vanilla 1.6 source code.

## 3. C# Scripting Standards

* **Namespace:** `OMW_Samhaphage`.
* **API References:** 
  * Use `Verse` and `RimWorld` namespaces.
  * For Hediffs, inherit from `HediffWithComps` and use `HediffComp`.
  * For Abilities, use `CompAbilityEffect`.
* **Performance:**
  * Cache `Def` lookups. Do not use `DefDatabase<T>.GetNamed` inside `Tick()` or `CompPostTick`.
  * Use `IsHashIntervalTick` for recurring logic to save UPS.
* **Mod Interop:** 
  * We use `AlphaGenes`, `WVC`, and `Big and Small`. 
  * When referencing `AlphaGenes`, use the `AlphaGenes` namespace.
  * Handle null checks for mod-specific components gracefully (e.g., check if the mod is loaded before accessing its Defs).

## 4. Lore & Nomenclature Rules

* **The Cycle:** Fluxspawn (Brood) -> Hallowbound (Workers) -> Samhaphage (Elite) -> Sovereign Stillness (Apex).
* **Resources:** "Resonance" is the primary resource. Carcinomas/Cancers are treated as a biological fuel for evolution.
* **Terminology:** Use words like "Harrowing," "Stillness," "Echo," "Thrum," and "Frequency." Avoid generic "Zombie" or "Alien" terms.

## 5. Specific Fixes for AI Hallucinations

* ✅ Abilities: RimWorld 1.6 `AbilityDef` uses `CompProperties_AbilityEffect` for logic. Do not invent `requiredCapacity` or `lintsHumans` tags.
* ✅ Gene Removal: When removing genes via C#, check if they are `Xenogenes` vs `Endogenes` as per the mod’s "Retention" mechanic.
* ✅ Hediffs: Use `severityAdjustment` correctly in `CompPostTick`.
* ✅ Use only 1.6 FactionDef fields: `defName`, `label`, `description`, `categoryTag`, `isPlayer`, `hidden`, `autoFlee`, `permanentEnemy`, `canMakeWarWith`, `leaderTitle`, `colorSpectrum`
* ✅ Use 1.6 AbilityDef valid fields only (check vanilla for reference)
* ✅ Use 1.6 GeneDef fields: `biostatCpx`, `biostatMet`, `biostatArc`, `displayOrderInCategory`, `exclusionTags`, `statOffsets`, `statFactors`
* ✅ Use 1.6 HediffDef stages system for progression
* ❌ Avoid deprecated 1.5 and earlier fields
* ❌ Do not use past-version XML tags (test against vanilla RimWorld 1.6 Data folder)

## 6. Workflow Instructions

* When asked to create a new ability, provide both the **C# Class** and the **XML Def**.
* If the user asks for "vibe coding," prioritize logic based on existing examples from `AlphaGenes` or `WVC`.
* Always check if a suggested method or field was deprecated in 1.6.

## 7. Memory & References

* **Lore Reference:** `Docs/LORE.md`.
* **Design Rules:** `Docs/DESIGN_RULES.md`.

---

## RIM WORLD 1.6 TECHNICAL FRAMEWORK

### C# Compilation Standards

* Target Framework: .NET 4.8 (RimWorld standard)
* Namespace: `OMW_Samhaphage`
* Referenced Assemblies: RimWorld core (Assembly-CSharp), and AlphaGenes
* Method Signatures: Must match RimWorld 1.6 base classes (e.g., `Apply(LocalTargetInfo target, LocalTargetInfo dest)`)
* No use of deprecated RimWorld APIs