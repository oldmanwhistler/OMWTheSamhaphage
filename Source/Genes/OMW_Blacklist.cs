using Verse;
using System.Collections.Generic;

namespace OMW_Samhaphage
{
    public static class OMW_BlacklistGenes
    {
        // This is a list of gene defs that are blacklisted from being added by the "Random Gene" button in the debug menu.
        // This is to prevent certain genes that cause crashes or are otherwise undesirable from being added to pawns via this method.
        public static HashSet<GeneDef> BlacklistedGenes = new HashSet<GeneDef>
        {
            OMW_GeneDefOf.OMW_Resonance,
            OMW_GeneDefOf.OMW_Chimera_SovereignStillness_Limit,
            OMW_GeneDefOf.OMW_NullThrum,
            OMW_GeneDefOf.OMW_ScouredMind,
            OMW_GeneDefOf.OMW_UnstableMutationMinor,
            OMW_GeneDefOf.OMW_UnstableMutationMajor,
            OMW_GeneDefOf.OMW_UnstableMutation,
            OMW_GeneDefOf.OMW_UnstableMutationCatastrophic,
            OMW_GeneDefOf.OMW_UnstableMutationPositive,
            OMW_GeneDefOf.OMW_Cradlemold_Frame,
            OMW_GeneDefOf.OMW_Fluxspawn_Nudist,
            OMW_GeneDefOf.OMW_Fluxspawn_Ferility,
            OMW_GeneDefOf.OMW_FluxSpawn_Hiveling_Frame,
            OMW_GeneDefOf.OMW_FluxSpawn_Brute_Frame,
            OMW_GeneDefOf.OMW_FluxSpawn_Flicker_Frame,
            OMW_GeneDefOf.OMW_Hallowbound_Frame,
            OMW_GeneDefOf.OMW_Hallowbound_EntropyWell,
            OMW_GeneDefOf.OMW_Samhaphage_Frame,
            OMW_GeneDefOf.OMW_SovereignStillness_Frame,
            OMW_GeneDefOf.OMW_PsychicAbility_SovereignStillness
        };
    }
}