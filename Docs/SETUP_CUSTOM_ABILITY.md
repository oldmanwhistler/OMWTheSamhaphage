# OMW Samhaphage - Custom Ability Setup Guide

## Overview
This mod creates a custom ability called **"Implant Fluxspawn Hiveling"** that allows the `omw_samhaphage` xenotype to implant their genetic template into target humanoids, causing them to gestate and birth new Fluxspawn hivelings into your faction.

## Components Created

### XML Definitions
- **Defs/1.6/FactionDefs/FactionDefs.xml** - NEW
  - **omw_perfect_silence**: The hivemind faction for Samhaphage offspring
- **Defs/1.6/GenDefs/GeneDefsImplantation.xml** - NEW
  - **OMW_HivelingImplanter**: Gene that grants the implantation ability
  - **OMW_ImplantFluxspawnHiveling**: The ability definition with 3.9-unit range and cooldown
  - **OMW_FluxspawnImplantation**: The hediff that tracks gestation progress

### C# Source Code (Source/)
1. **CompAbilityFluxspawnImplant.cs** - Ability component that:
   - Deals damage to the target
   - Applies the implantation hediff
   - Makes the target's faction hostile

2. **HediffComp_FluxspawnImplant.cs** - Hediff component that:
   - Tracks gestation progress over 12 days
   - Generates offspring as `omw_fluxspawn_hiveling` xenotype
   - Places offspring in the caster's faction
   - Sends notification when birth occurs

## Setup Instructions

### Step 1: Install .NET SDK
The mod requires .NET 6 SDK or later to compile. 

**Windows:**
```powershell
# Using winget (Windows Package Manager)
winget install Microsoft.DotNet.SDK.8

# Or download from: https://dotnet.microsoft.com/download
```

### Step 2: Compile the Assembly

**Using the Batch File (Windows):**
```cmd
cd "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\[OMW] The Samhaphage\Source\"
Build.bat
```

**OR Manually with PowerShell:**
```powershell
cd "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\[OMW] The Samhaphage\Source\"
dotnet build OMWSamhaphage.csproj -c Release -o ..\Assemblies
```

**OR Manually with Command Prompt:**
```cmd
cd "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\[OMW] The Samhaphage\Source\"
dotnet build OMWSamhaphage.csproj -c Release -o ..\Assemblies
```

### Step 3: Verify the Build
After compilation, you should see:
```
Assemblies/
├── OMWSamhaphage.dll
├── OMWSamhaphage.pdb (optional debug symbols)
└── ...
```

### Step 4: Set the About.xml Load Order
Ensure your mod loads **after** Biotech and its dependencies. Edit `About/About.xml`:

```xml
<loadAfter>
  <li>Ludeon.RimWorld.Biotech</li>
  <li>sarg.alphagenes</li>
  <li>redmattis.bigsmall.core</li>
  <!-- other mod dependencies -->
</loadAfter>
```

## How It Works

### Ability Mechanics
1. **Cast**: `omw_samhaphage` humanoid targets another humanoid within 3.9 tiles
2. **Damage**: Target takes 25 cutting damage and becomes wounded
3. **Implantation**: A hediff `OMW_FluxspawnImplantation` is applied, causing pain over 12 days
4. **Gestation**: The hediff tracks progression through three stages
5. **Birth**: After 12 days, a new `omw_fluxspawn_hiveling` is born next to the host
6. **Faction**: The offspring is born into the caster's faction

### Pain Progression
- **Stage 1** (0-25% severity): Initial infection - 0.5x pain
- **Stage 2** (25-75% severity): Active gestation - 1.0x pain  
- **Stage 3** (75-100% severity): Severe gestation - 2.0x pain

### Cooldown
- 2500-3500 ticks (approximately 1 minute of in-game time) between casts

## Configuration

Edit `GenDefsImplantation.xml` to customize:

```xml
<!-- Adjust gestation duration (in days) -->
<gestationDays>12</gestationDays>

<!-- Adjust damage dealt on implantation -->
<implantationDamage>25</implantationDamage>

<!-- Adjust ability cooldown (ticks) -->
<cooldownTicksRange>2500~3500</cooldownTicksRange>

<!-- Adjust ability range (tiles) -->
<range>3.9</range>

<!-- Adjust biostat costs -->
<biostatCpx>4</biostatCpx>
<biostatMet>2</biostatMet>
```

## Troubleshooting

### XML Errors: "doesn't correspond to any field in type"
- **Cause**: Using deprecated or incorrect field names for RimWorld 1.6
- **Solution**: Ensure all XML fields match RimWorld 1.6 FactionDef, AbilityDef, and HediffDef specs
- **Common mistakes**:
  - `requiredCapacity` is not valid in AbilityDef
  - `lintsHumans` should be removed from targetParams
  - FactionDef doesn't support `mustMaintainGoodwill`, `pawnsCanTalk`, `baseGoodwill`, `hostileToFactionless`, etc. in 1.6
  - Check vanilla RimWorld 1.6 Defs folder for correct field names

### Assembly doesn't load
- Check RimWorld's debug log: `%APPDATA%\Ludeon Studios\RimWorld\Log.txt`
- Ensure `OMWSamhaphage.dll` is in the `Assemblies/` folder
- Verify the mod loads after Biotech
- Check that the comp class path is fully qualified: `Class="OMWSamhaphage.CompProperties_AbilityFluxspawnImplant"`

### Ability doesn't appear
- Confirm the gene `OMW_HivelingImplanter` was added to `omw_samhaphage` xenotype
- Check that the XML comp class path is correct and fully qualified with namespace
- Verify no gene load order conflicts (genes should load after ability definitions)
- Check the About.xml mod dependencies are correct

### Compilation errors
- **"no suitable method found to override"**: Wrong Apply method signature - should be `Apply(LocalTargetInfo target, LocalTargetInfo dest)`
- **"does not contain a definition for"**: Using methods that don't exist in RimWorld 1.6 assemblies
- Update .NET SDK: `dotnet sdk check`
- Delete `bin/` and `obj/` folders and rebuild
- Verify RimWorld 1.6 assembly paths are correct in `.csproj`
- Use `Find.FactionManager.FirstFactionOfDef()` to get faction instances, not static faction references

## Future Enhancements

Potential improvements:
- Add customizable offspring generation (randomized traits)
- Add psychic feedback to the caster
- Create variant abilities (eggs, direct conversion, etc.)
- Add mod settings for balance tweaking
- Create alternative implantation methods (peaceful vs aggressive)

## Support

For issues or questions, check your mod folder structure:
```
[OMW] The Samhaphage/
├── About/
├── Assemblies/
│   └── OMWSamhaphage.dll  ← Compiled assembly (generated)
├── Defs/
│   └── 1.6/
│       ├── FactionDefs/
│       │   └── FactionDefs.xml  ← NEW (faction for offspring)
│       ├── GenDefs/
│       │   ├── GenDefsFluxspawn.xml
│       │   ├── GeneDefsImplantation.xml  ← NEW (genes, ability, hediff)
│       │   └── ...
│       ├── XenotypeDefs/
│       │   ├── samhaphage.xml  ← UPDATED (added OMW_HivelingImplanter)
│       │   └── fluxspawnhiveling.xml
│       └── ...
├── Source/
│   ├── Build.bat  ← NEW (compilation script)
│   ├── OMWSamhaphage.csproj  ← NEW (project file)
│   ├── CompAbilityFluxspawnImplant.cs  ← NEW (ability component)
│   ├── HediffComp_FluxspawnImplant.cs  ← NEW (hediff component)
│   └── ...
└── SETUP_CUSTOM_ABILITY.md  ← This file
```
