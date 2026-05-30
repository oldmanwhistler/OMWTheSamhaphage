using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OMW_Samhaphage
{
    public class TraitPlus
    {
        public Trait trait;
        public float value = 0f;
        public Pawn pawn;
        public string destinationConflictStr = "";

        public TraitPlus(Trait trait, Pawn pawn)
        {
            this.trait = trait;
            this.pawn = pawn;
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
            var tip = $"{this.trait.LabelCap}\n\n{this.trait.TipString(this.pawn)}";
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