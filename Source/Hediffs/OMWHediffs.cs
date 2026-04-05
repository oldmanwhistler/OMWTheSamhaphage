using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OMW_Samhaphage
{
    public static class OMWHediffs
    {
        private static bool debug = true;

        public static bool RemoveHediff(Pawn pawn, HediffDef hediffDef)
        {
            if (pawn == null) return false;
            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            if (firstHediffOfDef != null)
            {
                pawn.health.RemoveHediff(firstHediffOfDef);
                return true;
            }

            return false;
        }

        public static bool RemoveXenogermReplicating(Pawn pawn)
        {
            return RemoveHediff(pawn, HediffDefOf.XenogermReplicating);
        }
    }
}
