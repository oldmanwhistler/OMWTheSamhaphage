using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public class CompProperties_AbilityFluxspawnSelf : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityFluxspawnSelf()
        {
            this.compClass = typeof(CompAbilityEffect_AbilityFluxspawnSelf);
        }
    }

    public class CompAbilityEffect_AbilityFluxspawnSelf :  CompAbilityEffect_AbilityBase

    {
        public override void OpenMenu(LocalTargetInfo target, LocalTargetInfo dest)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            XenotypeDef xeno = parent.pawn.genes.Xenotype;
            string reason;
            NullThrumAbilityBase ability;

            options.Add(new FloatMenuOption(GetGeneStateDesc(parent.pawn), null) { Disabled = true });
            if ((xeno != OMW_XenotypeDefOf.omw_fluxspawn_hiveling) && (xeno != OMW_XenotypeDefOf.omw_fluxspawn_brute) &&
                (xeno != OMW_XenotypeDefOf.omw_fluxspawn_flicker))
            {
                // hybrids lose the ability
                reason = $"Xenotype {xeno} can't use this ability.";
                options.Add(new FloatMenuOption(reason, null)
                    { Disabled = true });
                OpenWindow(options);
                return;
            }
            else if (xeno == OMW_XenotypeDefOf.omw_fluxspawn_brute)
            {
                ability = new FluxspawnShiftHiveling();
                options.Add(ability.NewFloatMenuOption(target, parent.pawn));
                ability = new FluxspawnShiftFlicker();
                options.Add(ability.NewFloatMenuOption(target, parent.pawn));
            }
            else if (xeno == OMW_XenotypeDefOf.omw_fluxspawn_flicker)
            {
                ability = new FluxspawnShiftHiveling();
                options.Add(ability.NewFloatMenuOption(target, parent.pawn));
                ability = new FluxspawnShiftBrute();
                options.Add(ability.NewFloatMenuOption(target, parent.pawn));
            }
            else if (xeno == OMW_XenotypeDefOf.omw_fluxspawn_hiveling)
            {
                ability = new FluxspawnShiftBrute();
                options.Add(ability.NewFloatMenuOption(target, parent.pawn));
                ability = new FluxspawnShiftFlicker();
                options.Add(ability.NewFloatMenuOption(target, parent.pawn));
            }

            // Pop the menu at the mouse location
            if (options.Count > 0)
            {
                Find.WindowStack.Add(new Dialog_Options(options));
            }
        }

    }
}