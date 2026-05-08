using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public static class OMW_UIHelpers
    {
        public static void ShowPlaceholder(string featureName = "This feature")
        {
            Find.WindowStack.Add(new Dialog_MessageBox(
                $"{featureName} is still being synthesized in the rift and is not implemented yet.",
                "Okay"
            ));
        }

        public static void ShowLethalConfirmation(Pawn pawn, System.Action sacrificeAction)
        {
            string msg = $"Warning: Activating this ability will kill {pawn.LabelShort}. Are you sure?";
            Dialog_MessageBox window = new Dialog_MessageBox(
                text: msg,
                buttonAText: "Confirm".Translate(),
                buttonAAction: sacrificeAction,
                buttonBText: "Cancel".Translate(),
                buttonBAction: null,
                buttonADestructive: true,
                title: "Lethal Ability".Translate()
            );

            Find.WindowStack.Add(window);
        }

        public static void ShowCorpseConfirmation(Pawn pawn, System.Action sacrificeAction)
        {
            string msg = $"Warning: Activating this ability will destroy {pawn.LabelShort}. Are you sure?";
            if (ModsConfig.AnomalyActive)
            {
                msg = $"Warning: Activating this ability could raise {pawn.LabelShort} as a shambler. Are you sure?";
            }
            Dialog_MessageBox window = new Dialog_MessageBox(
                text: msg,
                buttonAText: "Confirm".Translate(),
                buttonAAction: sacrificeAction,
                buttonBText: "Cancel".Translate(),
                buttonBAction: null,
                buttonADestructive: true,
                title: "Lethal Ability".Translate()
            );

            Find.WindowStack.Add(window);
        }


        public static void ShowXenogeneLossConfirmation(Pawn target, Pawn caster, System.Action sacrificeAction)
        {
            int xenoCountCaster = OMWGenes.CountXenogenes(caster);
            int xenoCountTarget = OMWGenes.CountXenogenes(target);
            string msg =
                $"Warning: {caster.LabelShort} will lose {xenoCountCaster} xenogenes if they steal {xenoCountTarget} xenogenes from {target.LabelShort}.";


            Dialog_MessageBox window = new Dialog_MessageBox(
                text: msg,
                buttonAText: "Confirm".Translate(),
                buttonAAction: sacrificeAction,
                buttonBText: "Cancel".Translate(),
                buttonBAction: null,
                buttonADestructive: true,
                title: "Xenogene Loss".Translate()
            );

            Find.WindowStack.Add(window);
        }        
    }
}
