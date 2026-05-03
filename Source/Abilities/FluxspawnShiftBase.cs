using RimWorld;
using Verse;

namespace OMW_Samhaphage
{    
    public abstract class FluxspawnShiftBase: NullThrumAbilityPawnOnly
    {
        public abstract XenotypeDef TargetXenotype();

        public override bool ApplyPawn(Pawn pawn, Pawn caster = null)
        {
            if (PawnTeratogenics.CarcinomaCount(pawn) == 0)
            {
                Messages.Message($"{pawn.LabelShort} doesn't have any carcinomas.", MessageTypeDefOf.RejectInput);
                return false;
            }

            OMWGenes.ChangeEndotype(pawn, null, TargetXenotype());
            return PawnTeratogenics.RemoveRandomCarcinoma(pawn);
        }

        public override bool CanApplyOnPawn(Pawn p, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (p == null)
            {
                reason = "Target is null.";
                return false;
            }

            // Check if target is a not already Retune
            if (!p.RaceProps.Humanlike)
            {
                reason = $"{p.LabelShort} is not humanlike.";
                return false;
            }

            if (!OMWGenes.HasNullThrum(p))
            {
                reason = $"{p.LabelShort} is not part of the harmony of the Null-Thrum.";
                return false;
            }

            if ((p.genes.Xenotype != OMW_XenotypeDefOf.omw_fluxspawn_brute) && 
                (p.genes.Xenotype != OMW_XenotypeDefOf.omw_fluxspawn_hiveling) &&
                (p.genes.Xenotype != OMW_XenotypeDefOf.omw_fluxspawn_flicker))
            {
                reason = $"{p.LabelShort} is not a Fluxspawn.";
                return false;
            }

            if (p.health.hediffSet.HasHediff(OMW_HediffDefOf.OMW_GeneticDissonance))
            {
                reason = $"{p.LabelShort} is affected by Genetic Dissonance.";
                return false;
            }

            if (PawnTeratogenics.CarcinomaCount(p) == 0)
            {
                reason = $"{p.LabelShort} doesn't have any carcinomas.";
                return false;
            }

            return true;
        }

        public override MenuItemIcon NewMenuItemIconPawn(LocalTargetInfo targetInfo, Pawn pawn, Pawn caster = null)
        {
            XenotypeDef xeno = TargetXenotype();
            string reason;

            if (CanApplyOnPawn(pawn, caster, out reason))
            {
                return new MenuItemIcon(this.VerbName, $"Shift {pawn.LabelShort} to {xeno.descriptionShort}", this.Icon, () => Job(targetInfo, caster));
            }
            else
            {
                return NewMenuItemIconDisabled(pawn, $"Can't shift because {reason}");
            }
        }        
    }
}
