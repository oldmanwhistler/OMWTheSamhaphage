using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OMW_Samhaphage
{
    public class PawnClearXenogenes
    {

        public static bool Apply(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return false;

            OMWGenes.RemoveXenogenes(victim);
            OMWGenes.Refresh(victim);

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
            
            if (!p.RaceProps.Humanlike)
            {
                reason = $"{p.LabelShort} is not humanlike.";
                return false;
            }

            int xenogenes = OMWGenes.CountXenogenes(p);

            if (xenogenes == 0)
            {
                reason = $"{p.LabelShort} doesn't have any xenogenes.";
                return false;
            }

            // Check if target is a not already Flatten
            if (p.health.hediffSet.HasHediff(OMW_HediffDefOf.OMW_GeneticDissonance))
            {
                reason = $"{p.LabelShort} is affected by Genetic Dissonance";
                return false;
            }

            return true;
        }
    }
}