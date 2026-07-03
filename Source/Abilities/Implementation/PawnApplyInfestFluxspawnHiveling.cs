using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using AlphaGenes;

// Based onAlphaGenes' Parasite hediff comp (c)2021 juanosarg. License CC-BY-NC-ND.
// See original at: https://github.com/juanosarg/AlphaGenes/blob/d6f14ee6106ce01351c86eb369703edde65bce66/1.6/Source/AlphaGenes/AlphaGenes/HediffComps/HediffComp_Parasites.cs

// The differences from AlphaGenes
// - has a targetXenotype that can be neither the mother nor the father, since multiple xenotypes can be used to create fluxspawn through infestation.
// - has a min/max number of babies that can be spawned, since the fluxspawn are born via litter.
namespace OMW_Samhaphage
{
    public class PawnApplyInfestFluxspawnHiveling : NullThrumAbilityPawnOnly
    {
        protected int complexityCost;
        public override NullThrumAbilityProps AbilityProp => OMW_Mod.settings.abilityValue.infest;
        public override NullThrumAbilityType AbilityType => AbilityProp.abilityType;
        public override string AbilityDescription(Pawn victim, Pawn caster) => $"Infest {victim.LabelShort} with Fluxspawn embryos.\nThe process is lethal and will birth a litter of Fluxspawn.";
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/Infest", false) ??
                                          BaseContent.BadTex;
        public override bool IsLethal => true;
        
        public virtual HediffDef TargetHediffDef => OMW_HediffDefOf.OMW_ParasiticImplantation;
        public virtual XenotypeDef TargetXenotype => OMW_XenotypeDefOf.omw_fluxspawn_hiveling;        

        public override void ApplyPawn(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return;

            if (!ResonanceUtility.Decr(caster, complexityCost))
            {
                Log.Error(
                    $"[OMW_Samhaphage] Failed to decrement resonance for {caster.LabelShort} during {AbilityType}. This indicates a logic error where CanApplyOnPawn did not prevent the ability.");
                doOnComplete();
                return;
            }

            string msg = $"{victim.LabelShort} has been implanted by {caster.LabelShort} and will die when the egg(s) hatch.";
            // We define the lethal logic as an Action
            System.Action sacrificeAction = () =>
            {
                // I would have preferred to use the existing parasiticStinger ability, but I needed to tweak it to behave differently for lore reasons and to fit in with how I did the NullThrumAbilities menu.

                HealthUtility.DamageUntilDowned(victim);

                victim.health.AddHediff(TargetHediffDef);
                Hediff hediff = victim.health.hediffSet.GetFirstHediffOfDef(TargetHediffDef);

                // This HediffComp passes info to the eggs
                HediffComp_ParasitesXenotype comp = hediff.TryGetComp<HediffComp_ParasitesXenotype>();
                comp.motherDef = caster.kindDef;
                comp.mother = caster;
                comp.motherFaction = caster.Faction;
                comp.motherXenotypeDef = TargetXenotype;
                int maxFluxspawn = ColonyUtility.MaxPossibleXenotypeIncrease(TargetXenotype);
                comp.numBabiesMin = Math.Min(2, maxFluxspawn);
                comp.numBabiesMax = Math.Min(5, maxFluxspawn);

                FleckMaker.AttachedOverlay(victim, FleckDefOf.FlashHollow, new Vector3(0f, 0f, 0.26f));

                victim.needs?.mood?.thoughts?.memories?.TryGainMemory(
                    (Thought_Memory)ThoughtMaker.MakeThought(InternalDefOf.AG_Parasite), caster);

                victim.needs?.mood?.thoughts?.memories?.TryGainMemory(
                    (Thought_Memory)ThoughtMaker.MakeThought(InternalDefOf.AG_Parasite_Social), caster);

                for (int i = 0; i < 20; i++)
                {
                    IntVec3 c;
                    CellFinder.TryFindRandomReachableCellNearPosition(victim.Position, victim.Position, victim.Map, 2,
                    TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c);

                    FilthMaker.TryMakeFilth(c, victim.Map, ThingDefOf.Filth_Blood);
                }
                Messages.Message(msg, MessageTypeDefOf.NegativeEvent);                
            };

            // Open the confirmation dialog
            ShowLethalConfirmation(victim, sacrificeAction);
        }




        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";
            if (victim == null)
            {
                reason = "Target is null.";
                return false;
            }

            if (victim.HostileTo(caster))
            {
                reason = $"{victim.LabelShort} is hostile.";
                return false;
            }

            if (victim.HasActiveGene(GeneDefOf.Deathless))
            {
                reason = $"{victim.LabelShort} is deathless.";
                return false;
            }

            if (OMWGenes.HasNullThrum(victim))
            {
                reason = $"{victim.LabelShort} is part of the Null-Thrum.";
                return false;
            }

            complexityCost = OMWGenes.CalculateComplexity(victim, true, caster.genes.Xenotype);
            if (!ResonanceUtility.HasAvailable(caster, complexityCost))
            {
                reason =
                    $"{caster.LabelShort} does not have enough resonance to infect {victim.LabelShort}. Requires {complexityCost} resonance.";
                return false;
            }            

            return CanApplyLimitXenotype(TargetXenotype, out reason);
        }
    }
}