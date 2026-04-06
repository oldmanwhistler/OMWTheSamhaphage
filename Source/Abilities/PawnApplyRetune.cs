using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OMW_Samhaphage
{
    public static class PawnApplyRetune
    {

        public static bool RemoveCarcinomas(Pawn victim, Pawn caster)
        {
            HediffDef hediffDef = HediffDefOf.Carcinoma;

            List<Hediff> carcinomas = new List<Hediff>();
            foreach (Hediff hediffToCheck in victim.health.hediffSet.hediffs)
            {
                if (hediffToCheck.def == hediffDef)
                {
                    carcinomas.Add(hediffToCheck);
                }
            }

            if (carcinomas.Count == 0)
            {
                Log.Message($"{victim.LabelShort} doesn't have any carcinomas to remove.");
                return false;
            }

            ResonanceUtility.Incr(caster, carcinomas.Count);

            foreach (Hediff carcinoma in carcinomas)
            {
                victim.health.RemoveHediff(carcinoma);
            }

            return true;
        }

        public static bool Apply(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return false;
            
            if (ResonanceUtility.HasAvailable(caster))
            {
                Log.Message($"{caster.LabelShort} has enough available resonance to Retune {victim.LabelShort}.");
            }
            else
            {
                Log.Message($"{caster.LabelShort} does not have enough available resonance to Retune {victim.LabelShort}.");
                return false;
            }

            // GeneticDissonance prevents repeated calls to Retune
            Hediff hediff_Retune = HediffMaker.MakeHediff(OMW_HediffDefOf.OMW_GeneticDissonance, caster);            
            victim.health.AddHediff(hediff_Retune);

            RemoveCarcinomas(victim, caster);

            OMWHediffs.RemoveHediff(victim, HediffDefOf.XenogermReplicating);
            OMWHediffs.RemoveHediff(victim, HediffDefOf.XenogermLossShock);
            OMWHediffs.RemoveHediff(victim, HediffDefOf.XenogerminationComa);

            OMWGenes.RemoveDisabledGenes(victim);
            OMWGenes.XenogenesToEndogenes(victim);
            OMWGenes.RemoveDisabledGenes(victim);
            OMWGenes.Refresh(victim);

            ResonanceUtility.Decr(caster);
            return true;
        }

        public static bool CanApplyOn(Pawn p, out string reason)
        {
            reason = "unknown reason";

            if (p == null) 
            {
                reason = "Target is null.";
                return false;
            }            
            // Check if target is a not already Retune
            if (!p.RaceProps.Humanlike)
            {
                reason = $"{p.LabelShort} is not humanlike.";
                return false;
            }

            if (!OMWGenes.HasScouredMind(p))
            {
                reason = $"{p.LabelShort} has not had their mind scoured to prepare them for genetic manipulation.";
                return false;
            }

            if (!OMWGenes.HasNullThrum(p))
            {            
                reason = $"{p.LabelShort} is not part of the harmony of the Null-Thrum.";
                return false;
            }

            if (p.health.hediffSet.HasHediff(OMW_HediffDefOf.OMW_SilentServitude))
            {
                reason = $"{p.LabelShort} is affected by Silent Servitude.";
                return false;
            }

            if (p.health.hediffSet.HasHediff(OMW_HediffDefOf.OMW_GeneticDissonance))
            {
                reason = $"{p.LabelShort} is affected by Genetic Dissonance";
                return false;
            }

            return true;
        }
    }
}