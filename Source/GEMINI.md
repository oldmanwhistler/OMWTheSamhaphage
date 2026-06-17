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
* **Categories:** Genes should belong to `<displayCategory>OMW_PerfectSilence</displayCategory>`.
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
* **Resources:** "Resonance" is the primary resource. Carcinomas/Cancers are treated as a biological fuel for resonance.
* **Terminology:** Use words like "Harrowing," "Stillness," "Echo," "Thrum," and "Frequency." Avoid generic "Zombie" or "Alien" terms.

## 5. Specific Fixes for AI Hallucinations

* ✅ Abilities: RimWorld 1.6 `AbilityDef` uses `CompProperties_AbilityEffect` for logic. Do not invent `requiredCapacity` or `lintsHumans` tags.
* ✅ Gene Removal: When removing genes via C#, check if they are `Xenogenes` vs `Endogenes` as per the mod’s "Retention" mechanic.
* ✅ Hediffs: Use `severityAdjustment` correctly in `CompPostTick`.
* ✅ Use only 1.6 FactionDef fields: `defName`, `label`, `description`, `categoryTag`, `isPlayer`, `hidden`, `autoFlee`, `permanentEnemy`, `canMakeWarWith`, `leaderTitle`, `colorSpectrum`
* ✅ Use 1.6 AbilityDef valid fields only (check vanilla for reference)
* ✅ Use 1.6 GeneDef fields: `biostatCpx`, `biostatMet`, `biostatArc`, `displayOrderInCategory`, `exclusionTags`, `statOffsets`, `statFactors`
* ✅ Use 1.6 HediffDef stages system for progression
* ✅ `Pawn_StoryTracker`: Use public properties `Childhood` and `Adulthood`. The fields `childhood` and `adulthood` are private and inaccessible.
* ✅ `BackstoryDef`: Use the `workDisables` field for checked work tags. Do not confuse this with `disabledWorkTags` which is used by Traits and Genes.
* ❌ **STRICT RULE**: Before performing unsolicited structural refactors or "optimize" existing logic, show the user an example and ask if they want to proceed.
* ❌ Avoid deprecated 1.5 and earlier fields
* ❌ Do not use past-version XML tags (test against vanilla RimWorld 1.6 Data folder)
* `pawn.genes.CheckForOverrides()`: Internal method; not accessible to external assemblies.
* `pawn.genes.Notify_GenesChanged()`: Hallucinated member; does not exist on `Pawn_GeneTracker`.
* `pawn.Notify_GenesChanged()`: Flagged as not accessible in the current project context.
* **Double-Callback Bug:** When opening a window with both a selection lambda and a close action, the `onAbilityComplete` callback can be triggered twice. Always use a `bool selectionMade = false;` flag within the method to ensure the callback only fires once.
* **Premature Completion in Chained Abilities:** Abilities that chain (e.g., `Retune` calling `Scrub`) must pass the `onAbilityComplete` action down the line. If a helper ability (like `Flatten`) signals completion via `doOnComplete()`, it may end the pawn's job while the user is still interacting with a selection UI.
* **Lifecycle & State:** `PawnApply[Action]` classes (like `PawnApplyRetune`) are **short-lived**. They are instantiated via a static `DoAbility` method for a single execution context and do not persist state between different target pawns.
* **Initialization in CanApply:** Because instances are fresh per-use, initialization of selectors (e.g., `selectorRetune`) should happen inside `CanApplyOnPawn` to ensure validity before the UI attempt.

## 6. Workflow Instructions

* When asked to create a new ability, provide both the **C# Class** and the **XML Def**.
* Always check if a suggested method or field was deprecated in 1.6.

### 8. Adding a New NullThrumAbility

Abilities can apply on `Pawn`, `Corpse`, or `Thing`. Ensure correct target validation by asking the user's intent. Ensure the correct `NullThrumAbilityType` before modifying files.

1.  **Documentation**:
    *   Validate that `Docs/DESIGN_RULES.md` has the thematic concept and sensory description for the new `NullThrumAbilityType`.
    *   Validate that `Docs/XENOTYPES.md` has the a short description for `NullThrumAbilityType`.
    *   Add to `Docs/TESTS.md` to ensure the new ability is verified across target types (Pawn, Corpse, Self).

When adding a new value to the `NullThrumAbilityType` enum, the following files and structures must be updated:

1.  **`Source/Ability/Type/NullThrumAbilityType.cs`**: Add the new member to the `NullThrumAbilityType` enum.
2.  **`Source/Ability/Type/NullThrumAbility.cs`**: Create the `NullThrumAbilityProp` instance for the new enum member.
3.  **`Source/Abilities/Static/NullThrumAbilityUtility.cs`**: Update the necessary case statements to support the new enum member.
4.  **`Source/ModSettings/OMW_Settings.cs`**: Add the necessary mod setting for the `NullThrumAbilityType`
5.  **Implementation Class**: Create a new class (in `Source/Abilities/Implementations/`) inheriting from `NullThrumAbilityBase`.
6.  **XML Defs**: Do not manually create new `AbilityDef` files; ensure the logic integrates with the existing `OMW_NullThrumAbility` framework.
7.  **`Source/CompAbilityEffect/CompAbilityEffect` classes**: Are used the map the ability to specific xenotypes and targets. Leave this alone but remind the user to update it.

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