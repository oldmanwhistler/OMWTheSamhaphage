using RimWorld;
using Verse;
using System.Collections.Generic;

namespace OMW_Samhaphage
{
    public abstract class NullThrumSelectionBase
    {
        protected Pawn caster;
       
        protected NullThrumSelectionBase(Pawn caster, Pawn source, Pawn dest)
        {
            // Need to store this for calculating resonance value and max selection
            this.caster = caster;
        }

        // Abstract methods
        public abstract NullThrumAbilityType AbilityType { get; }

        public string Name => NullThrumUtility.ToString(this.AbilityType);

        // Concrete methods

        // GeneticDissonance prevents repeatedly using the same abilities on the same pawn
        public void ApplyDissonance(Pawn victim, Pawn caster)
        {
            Hediff hediffDissonance = HediffMaker.MakeHediff(OMW_HediffDefOf.OMW_GeneticDissonance, caster);
            victim.health.AddHediff(hediffDissonance);
        }

        public float SelectionMaxCost()
        {
            return ResonanceUtility.Total(this.caster);
        }
    }
}