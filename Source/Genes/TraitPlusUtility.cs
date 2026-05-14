using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OMW_Samhaphage
{
    public class TraitPlus
    {
        public Trait trait;
        public float value = 1f;
        public string destinationConflictStr = "";

        public TraitPlus(Trait trait)
        {
            this.trait = trait;            
        }

        public bool HasConflict()
        {
            return !this.destinationConflictStr.NullOrEmpty();
        }

        public Trait Copy()
        {
            return new Trait(this.trait.def, this.trait.Degree);
        }

        public override string ToString()
        {
            //var stats = $"\nResonance Value: {this.value}";            
            var tip = $"{this.trait.LabelCap}\n\n{this.trait.def.description}";

            if (destinationConflictStr != "")
            {
                tip += $"\n\n<color=#ff6666>(This trait conflicts with {destinationConflictStr})</color>";
            }
            return tip;
        }
    }

    public static class TraitPlusUtility
    {
        public static List<TraitPlus> ConvertToTraitPlus(Pawn pawn, List<Trait> traits, List<TraitDef> destConflicts = null)
        {
            List<TraitPlus> traitList = new List<TraitPlus>();

            foreach (Trait trait in traits)
            {
                TraitPlus plus = new TraitPlus(trait);

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