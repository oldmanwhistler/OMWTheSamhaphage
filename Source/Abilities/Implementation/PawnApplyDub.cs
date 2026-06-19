using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class NullThrumSelectionGeneDub : NullThrumSelectionGene
    {
        public NullThrumSelectionGeneDub(Pawn caster, Pawn source, Pawn dest) : base(caster, source, dest) { }

        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.dub;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        protected override float ResonanceTotalMultiplier => AbilityProp.value;

        // note: source is caster and dest is victim
        protected override NullThrumSelectionGeneBlocked GenesBlockedFromSelection(Pawn source, Pawn dest)
        {
            NullThrumSelectionGeneBlocked blocked = new();
            if (source?.genes == null) return blocked;

            foreach (Gene gene in source.genes.GenesListForReading)
            {
                bool isBlocked = false;
                if (OMW_BlacklistGenes.BlacklistedGenesDontRemove.Contains(gene.def))
                {
                    Log.Debug($"blocking {gene.def} because don't remove");
                    blocked.Append(gene.def, "Don't Remove");
                    isBlocked = true;
                }
                else if (!gene.Overridden)
                {
                    Log.Debug($"blocking {gene.def} because active gene");
                    blocked.Append(gene.def, "Active Gene");
                    isBlocked = true;
                }
                else if (gene.def.biostatMet > 0)
                {
                    Log.Debug($"blocking {gene.def} because positive metabolism");
                    blocked.Append(gene.def, "Positive Metabolism");
                    isBlocked = true;
                }
                else if (gene.def.biostatMet == 0 && gene.def.biostatCpx <= 0 && gene.def.biostatArc <= 0)
                {
                    Log.Debug($"blocking {gene.def} because zero metabolism");
                    blocked.Append(gene.def, "Zero Metabolism");
                    isBlocked = true;
                }

                if (!isBlocked)
                {
                    Log.Debug($"not blocking {gene.def}: {gene.Label}");
                }
            }

            return blocked;
        }

        // note: source is caster and dest is victim
        protected override List<Gene> GenesToSelectFrom(Pawn source, Pawn dest, NullThrumSelectionGeneBlocked blocked)
        {
            return source.genes.GenesListForReading
                .Where(g => !blocked.Has(g.def))
                .ToList();
        }

        protected override List<GeneDef> ConflictGeneDefs(Pawn source, Pawn dest)
        {
            if (dest == null || dest.genes == null || dest.genes.GenesListForReading == null)
                return new List<GeneDef>();

            return dest.genes.GenesListForReading.Where(g => !g.Overridden)
                .Select(g => g.def).ToList();
        }
    }

    /// <summary>
    /// Implementation of the Dub ability: Transfer an overridden gene from the caster to another pawn.
    /// </summary>
    public class PawnApplyDub : NullThrumAbilityPawnOnly
    {
        private NullThrumSelectionGeneDub selectorDub;
        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.dub;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;

        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"Sever a suppressed genetic frequency from {caster.LabelShort} and imprint it onto {victim.LabelShort}.";
        }

        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Dub", false) ??
                                          BaseContent.BadTex;

        public override void ApplyPawn(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return;

            // Refresh target to ensure current gene state is known
            OMWGenes.Refresh(victim);

            if (selectorDub == null) selectorDub = new NullThrumSelectionGeneDub(caster, caster, victim);

            Find.WindowStack.Add(new WindowSelectGenesForNullThrumAbility(selectorDub, onCompleteAction(),
                selectedList =>
                {
                    bool activated = false;
                    foreach (GenePlus plus in selectedList)
                    {
                        if (plus.gene != null && caster.genes.GenesListForReading.Contains(plus.gene) && selectorDub.ResonanceDebit(plus))
                        {
                            caster.genes.RemoveGene(plus.gene);
                            victim.genes.AddGene(plus.gene.def, true);                            
                            Log.Debug($"Dubbed {plus.gene.LabelCap} from {caster.LabelShort} to {victim.LabelShort}");
                            activated = true;
                        }
                    }

                    if (activated)
                    {
                        OMWGenes.Refresh(victim);
                        OMWGenes.Refresh(caster);
                    }

                    doOnComplete();
                }));
        }


        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown";
            if (victim == null || caster == null) return false;

            if (victim == caster)
            {
                reason = "Cannot dub onto yourself; the frequency requires a separate vessel.";
                return false;
            }

            if (victim.Dead)
            {
                reason = "Target is dead.";
                return false;
            }

            if (!ResonanceUtility.HasAvailable(caster))
            {
                reason = $"{caster.LabelShort} does not have enough resonance.";
                return false;
            }

            if (!OMWGenes.HasNullThrum(victim))
            {
                reason = $"{victim.LabelShort} is not part of the harmony of the Null-Thrum.";
                return false;
            }

            selectorDub = new NullThrumSelectionGeneDub(caster, caster, victim);
            if (selectorDub.genes.Count == 0)
            {
                reason = $"{caster.LabelShort} has no overridden genes to dub on to {victim.LabelShort}.";
                return false;
            }

            return true;
        }
    }
}