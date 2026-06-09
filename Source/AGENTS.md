# Purpose

C# codebase for the Samhaphage mod. Handles custom job drivers, ability effects, UI windows, and mod compatibility (Alpha Genes, WVC).

# Local Contracts

- **RimWorld Version**: 1.6 (Strict).
- **Assembly**: `OMW_Samhaphage` namespace.
- **Stability**: Use `bool selectionMade` flags for UI windows to prevent double-callback bugs.

# Work Guidance

- **Compatibility**: Use the `Blacklist/` logic to prevent game-breaking trait/gene combinations.
- **UI**: Leverage `BetterFloatMenu` for ability selection.
- **API**: Inherit from `NullThrumAbilityBase` for all custom abilities.

# Child DOX Index

- Blacklist/ - Logic for filtering problematic genes and traits.
- Defs/ - Centralized `DefOf` static classes.
- ModSettings/ - Configuration and balance reporting tools.
- Window/ - Custom UI components.
- DLC/ - Ideology and specific DLC integration.

# Verification

- **Static Check**: Ensure `DefOf` classes are initialized with `DefOfHelper.EnsureInitializedInCtor`.
- **Code Quality**: Reference `GEMINI.md` in this directory for specific C# scripting standards and hallucination fixes.
