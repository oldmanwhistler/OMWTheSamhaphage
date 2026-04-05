using RimWorld;
using Verse;

namespace OMW_Samhaphage
{
    public class ThoughtWorker_ScouredMind : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
            {
                return false;
            }

            if (pawn.genes?.GetGene(OMW_GeneDefOf.OMW_ScouredMind)?.Active == false)
            {
                return false;
            }

            // No double-dipping
            if (pawn.genes?.GetGene(OMW_GeneDefOf.OMW_NullThrum)?.Active == true)
            {
                return false;
            }


            if (other.genes?.Xenotype == OMW_XenotypeDefOf.omw_fluxspawn_brute)
            {
                return ThoughtState.ActiveAtStage(1);
            }
            else if (other.genes?.Xenotype == OMW_XenotypeDefOf.omw_fluxspawn_flicker)
            {
                return ThoughtState.ActiveAtStage(1);
            }
            else if (other.genes?.Xenotype == OMW_XenotypeDefOf.omw_echovessel)
            {
                return ThoughtState.ActiveAtStage(1);
            }
            else if (other.genes?.Xenotype == OMW_XenotypeDefOf.omw_fluxspawn_hiveling)
            {
                return ThoughtState.ActiveAtStage(2);
            }
            else if (other.genes?.Xenotype == OMW_XenotypeDefOf.omw_cradlemold)
            {
                return ThoughtState.ActiveAtStage(2);
            }
            else if (other.genes?.Xenotype == OMW_XenotypeDefOf.omw_hallowbound)
            {
                return ThoughtState.ActiveAtStage(3);
            }
            else if (other.genes?.Xenotype == OMW_XenotypeDefOf.omw_samhaphage)
            {
                return ThoughtState.ActiveAtStage(4);
            }
            else if (other.genes?.Xenotype == OMW_XenotypeDefOf.omw_sovereign_stillness)
            {
                return ThoughtState.ActiveAtStage(5);
            }
            else if (other.genes?.GetGene(OMW_GeneDefOf.OMW_NullThrum)?.Active == true)
            {
                // hybrids
                return ThoughtState.ActiveAtStage(0);
            }
            else
            {
                // normies
                return false;
            }
        }
    }
}