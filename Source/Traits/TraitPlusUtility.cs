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

                if (destConflicts != null)
                {
                    foreach (TraitDef possible in destConflicts)
                    {
                        if (trait.def == possible || trait.def.ConflictsWith(possible))
                        {
                            plus.destinationConflicts.Add(possible);
                        }
                    }
                }

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
                if (other.def == trait.def)
                {
                    duplicates.Add(other);
                }
            }
            // This needs to be lowest to highest
            duplicates.Sort((a, b) => a.Degree.CompareTo(b.Degree));
            foreach (Trait traitDef in duplicates)
            {
                Log.Message(
                    $"GetDuplicateSpectrumTraits({pawn.LabelCap}, {trait.LabelCap}) has def: {trait.def.defName}, degree: {trait.Degree}");
            }
            return duplicates;
        }
    }
}