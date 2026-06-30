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
        public List<TraitDef> destinationConflicts = new List<TraitDef>();
        public string blockedReason = "";

        public TraitPlus(Trait trait, Pawn pawn)
        {
            this.trait = trait;
            this.pawn = pawn;
        }

        public bool HasConflict()
        {
            return this.destinationConflicts.Count > 0;
        }

        public Trait Copy()
        {
            return new Trait(this.trait.def, this.trait.Degree);
        }

        public override string ToString()
        {       
            var tip = $"{this.trait.LabelCap}\n\n{this.trait.TipString(this.pawn)}";

            if (this.trait.def.degreeDatas != null && this.trait.def.degreeDatas.Count > 1)
            {
                tip +=
                    $"\n\nSpectrum Trait:";
                int count = 1;
                foreach (TraitDegreeData data in trait.def.degreeDatas)
                {
                    tip += $"\n#{count} {data.LabelCap}, degree: {data.degree}";
                    if (trait.Degree == data.degree) tip += " (this trait)";
                    count++;
                }
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
                    $"\n\n<color=#999999>Disabled WorkTypes: {string.Join(", ", this.trait.def.disabledWorkTypes.Select(w => w.labelShort))}</color>";
            }

            if (!this.trait.def.requiredWorkTypes.NullOrEmpty())
            {
                tip +=
                    $"\n\n<color=#999999>Required WorkTypes: {string.Join(", ", this.trait.def.requiredWorkTypes.Select(w => w.labelShort))}</color>";
            }

            if (!this.trait.def.conflictingTraits.NullOrEmpty())
            {
                tip += $"\n\n<color=#999999>Conflicting Traits:";
                foreach (TraitDef traitDef in this.trait.def.conflictingTraits)
                {
                    if (traitDef.LabelCap == "") tip += $"\n{traitDef.defName}";
                    else tip += $"\n{traitDef.LabelCap}";
                }
                tip += "\n</color>";
            }

            if (!this.trait.def.conflictingPassions.NullOrEmpty())
            {
                tip +=
                    $"\n\n<color=#999999>Conflicting Skills: {string.Join(", ", this.trait.def.conflictingPassions.Select(s =>  s.LabelCap))}</color>";
            }

            if (destinationConflicts.Count > 0)
            {
                tip += $"\n\n<color=#ff6666>This trait could conflict with these traits:";
                foreach (TraitDef traitDef in destinationConflicts)
                {
                    foreach (TraitDegreeData data in traitDef.degreeDatas)
                    {
                        tip += $"\n{data.LabelCap}";
                    }
                }
                tip += "</color>";
            }

            return tip;
        }
    }
}