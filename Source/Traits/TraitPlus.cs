using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace OMW_Samhaphage
{
    public class TraitPlus
    {
        public Trait trait;
        public float value = 0f;
        public Pawn pawn;
        public string destinationConflictStr = "";
        public string blockedReason = "";

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

            if (this.trait.def.degreeDatas != null && this.trait.def.degreeDatas.Count > 1)
            {
                tip += $"\n\nThis is degree {this.trait.Degree} of {this.trait.def.degreeDatas.Count}";
            }

            if (!blockedReason.NullOrEmpty())
            {
                tip += $"\n\n<color=#ffcc00>(Blocked: {blockedReason})</color>";
            }

            if (OMW_BlacklistTraits.BlacklistedTraits.Any(x => x.traitDef == this.trait.def))
            {
                // get the reason why it's blacklisted
                BlacklistTrait bl = OMW_BlacklistTraits.BlacklistedTraits.FirstOrDefault(x => x.traitDef == this.trait.def);
                if (bl != null)
                {
                    tip += $"\n\n<color=#ffcc00>(Blacklisted: {bl.blacklistReason})</color>";
                }
            }


            if (this.trait.Suppressed)
            {
                if (this.trait.suppressedByTrait)
                {
                    tip +=
                        $"\n\n<color=#999999>(This trait is suppressed another trait)</color>";
                }
                if (this.trait.suppressedByGene != null)
                {
                    tip +=
                        $"\n\n<color=#999999>(This trait is suppressed by gene {this.trait.suppressedByGene.def.defName})</color>";
                }
            }
           
            if (!this.trait.def.disabledWorkTypes.NullOrEmpty())
            {
                tip +=
                    $"\n\n<color=#999999>Disabled WorkTypes: {string.Join(", ", this.trait.def.disabledWorkTypes.Select(w => w.defName))}</color>";
            }

            if (!this.trait.def.requiredWorkTypes.NullOrEmpty())
            {
                tip +=
                    $"\n\n<color=#999999>Required WorkTypes: {string.Join(", ", this.trait.def.requiredWorkTypes.Select(w => w.defName))}</color>";
            }

            if (!this.trait.def.conflictingTraits.NullOrEmpty())                
            {
                tip +=
                    $"\n\n<color=#999999>Conflicting Traits: {string.Join(", ", this.trait.def.conflictingTraits.Select(t =>  t.defName))}</color>";
            }

            if (!this.trait.def.conflictingPassions.NullOrEmpty())
            {
                tip +=
                    $"\n\n<color=#999999>Conflicting Skills: {string.Join(", ", this.trait.def.conflictingPassions.Select(s =>  s.defName))}</color>";
            }
            
            if (destinationConflictStr != "")
            {
                // Adds a red warning with the specific gene name
                tip += $"\n\n<color=#ff6666>(This trait conflicts with {destinationConflictStr})</color>";
            }

            return tip;
        }
    }
}