using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace OMW_Samhaphage
{
    public static class OMWXenotypes
    {
        private static Pawn theSovereignStillness = null;

        private static Pawn GetSovereignStillness()
        {
            if (theSovereignStillness != null && theSovereignStillness.Spawned && !theSovereignStillness.Dead)
            {
                // The pawn is physically on the map and alive
                if (theSovereignStillness.genes?.Xenotype == OMW_XenotypeDefOf.omw_sovereign_stillness)
                {
                    return theSovereignStillness;
                }
            }

            // if we get here then we don't have a valid cached Sovereign Stillness
            theSovereignStillness = null;

            ThereCanOnlyBeOne();

            return theSovereignStillness;
        }

        public static void ThereCanOnlyBeOne()
        {
            IEnumerable<Pawn> playerPawns = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction;

            // 2. Filter for your specific xenotype
            // Use the '?' null-conditional operator to avoid crashing if a pawn has no genes
            List<Pawn> specificXenos = playerPawns.Where(p => p.genes?.Xenotype == OMW_XenotypeDefOf
                .omw_sovereign_stillness).ToList();

            int complexity = 0;
            Pawn choice = null;

            if (specificXenos.Count() == 0)
            {
                theSovereignStillness = null;
                return;
            }

            if (specificXenos.Count() == 1)
            {
                theSovereignStillness = specificXenos[0];
                return;
            }

            // find the "best" Sovereign
            foreach (Pawn pawn in specificXenos)
            {
                int currComplexity = OMWGenes.CalculateComplexity(pawn);
                if (currComplexity > complexity)
                {
                    complexity = currComplexity;
                    choice = pawn;
                }
            }

            theSovereignStillness = choice;

            // convert the rest to Samhaphage
            foreach (Pawn pawn in specificXenos)
            {
                if (pawn != choice)
                {
                    OMWGenes.ChangeEndotype(pawn, OMW_XenotypeDefOf.omw_samhaphage);
                    Find.LetterStack.ReceiveLetter($"{pawn.LabelShort} xenotype changed.", $"{pawn.LabelShort} lost the role of Sovereign Stillness and has returned to being a Samhaphage.", LetterDefOf.PositiveEvent,(TargetInfo)pawn);
                }
            }
        }

        public static bool IsSovereignStillnessInPlayerFaction()
        {
            if (null == GetSovereignStillness())
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}