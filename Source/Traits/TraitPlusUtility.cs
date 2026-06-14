using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace OMW_Samhaphage
{
    public static class TraitPlusUtility
    {
        public static List<TraitPlus> ConvertToTraitPlus(Pawn pawn, List<Trait> traits, List<TraitDef> destConflicts = null)
        {
            List<TraitPlus> traitList = new List<TraitPlus>();

            foreach (Trait trait in traits)
            {
                TraitPlus plus = new TraitPlus(trait, pawn);

                List<string> tmpDestConflicts = [];
                if (destConflicts != null)
                {
                    foreach (TraitDef possible in destConflicts)
                    {
                        if (trait.def == possible || trait.def.ConflictsWith(possible))
                        {
                            tmpDestConflicts.Add(possible.LabelCap);
                        }
                    }
                }
                plus.destinationConflictStr = string.Join(", ", tmpDestConflicts);

                traitList.Add(plus);
            }

            return traitList;
        }
    }
}