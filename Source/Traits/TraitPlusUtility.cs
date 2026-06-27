using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace OMW_Samhaphage
{
    public static class TraitPlusUtility
    {
        public static List<TraitPlus> ConvertToTraitPlus(Pawn pawn, List<Trait> traits,
            NullThrumSelectionTraitBlocked blocked, List<TraitDef> destConflicts = null)
        {
            List<TraitPlus> traitList = new List<TraitPlus>();

            foreach (Trait trait in traits)
            {
                TraitPlus plus = new TraitPlus(trait, pawn);
                plus.blockedReason = blocked.Str(trait.def);

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

        public static int CountTraits(Pawn pawn)
        {
            int count = 0;
            foreach (Trait trait in pawn.story.traits.allTraits)
            {
                if (!OMW_BlacklistTraits.BlacklistedTraitsDontCount.Contains(trait.def))
                {
                    count++;
                }
            }

            return count;
        }

        public static List<Trait> GetDuplicateSpectrumTraits(Pawn pawn, Trait trait)
        {
            List<Trait> duplicates = new List<Trait>();
            foreach (Trait other in pawn.story.traits.allTraits)
            {
                if (other.def == trait.def && other.Degree != trait.Degree)
                {
                    duplicates.Add(other);
                }
            }
            duplicates.Sort((a, b) => a.Degree.CompareTo(b.Degree));
            return duplicates;
        }
    }
}