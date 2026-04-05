using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public abstract class CompAbilityEffect_AbilityBase : CompAbilityEffect
    {
        private void OpenSelfMenu() => Log.Message("Self menu opened");

        private void OpenGeneSelection(Pawn p) {
            Find.WindowStack.Add(new Dialog_GeneSelection(parent.pawn, p.genes.GenesListForReading));
        }

        public string GetGeneStateDesc(Pawn pawn)
        {
            return $"Endo: {OMWGenes.CountEndogenes(pawn).ToString()}, Xeno: {OMWGenes.CountXenogenes(pawn).ToString()}, Complex: {OMWGenes.CalculateComplexity(pawn).ToString()}, Metab: {OMWGenes.CalculateMetabolism(pawn).ToString()}";
        }

        public void OpenWindow(List<FloatMenuOption> options)
        {
            // Pop the menu at the mouse location
            if (options.Count > 0)
            {
                Find.WindowStack.Add(new Dialog_Options(options));
            }
        }       
    }
}