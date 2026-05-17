using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public static class ResonanceUtility
    {
        static Logger Log = new Logger("Resonance");
        public static bool HasGene(Pawn pawn)
        {
            if (pawn == null) return false;
            if (pawn.genes == null) return false;
            if (pawn.genes.HasActiveGene(OMW_GeneDefOf.OMW_Resonance)) return true;
            return false;
        }

        public static bool HasAvailable(Pawn pawn, float requiredAmount = 1f)
        {
            if (HasGene(pawn))
            {
                Gene_Resource resonance = pawn.genes.GetGene(OMW_GeneDefOf.OMW_Resonance) as Gene_Resource;
                Log.Debug($"[Resonance] {pawn.LabelShort} has {resonance.Value} available resonance, checking if they have {requiredAmount}.");
                return resonance.Value >= requiredAmount;
            }
            return false;
        }

        public static bool Incr(string reason, Pawn pawn, float amount = 1f)
        {
            if (HasGene(pawn))
            {
                Gene_Resource resonance = pawn.genes.GetGene(OMW_GeneDefOf.OMW_Resonance) as Gene_Resource;
                Log.Debug(
                    $"[Resonance] {pawn.LabelShort} has {resonance.Value} available resonance, incrementing by {amount}.");
                resonance.Value += amount;
                Log.Debug(
                    $"[Resonance] {pawn.LabelShort} now has {resonance.Value} available resonance.");                
                return true;
            }
            return false;
        }

        public static bool Decr(Pawn pawn, float amount = 1f)
        {
            if (HasGene(pawn))
            {
                Gene_Resource resonance = pawn.genes.GetGene(OMW_GeneDefOf.OMW_Resonance) as Gene_Resource;
                Log.Debug(
                    $"[Resonance] {pawn.LabelShort} has {resonance.Value} available resonance, decrementing by {amount}.");
                resonance.Value -= amount;
                Log.Debug(
                    $"[Resonance] {pawn.LabelShort} now has {resonance.Value} available resonance.");
                if (resonance.Value < 0f) resonance.Value = 0f;
                return true;
            }

            return false;
        }

        public static int Total(Pawn pawn, float multiplier = 1f)
        {
            if (HasGene(pawn))
            {
                Gene_Resource resonance = pawn.genes.GetGene(OMW_GeneDefOf.OMW_Resonance) as Gene_Resource;
                return Mathf.RoundToInt(resonance.Value * multiplier);
            }
            return 0;
        }

        public static float GeneResonanceValueArchite(GeneDef geneDef)
        {
            return geneDef.biostatArc * 10f;
        }

        public static float GeneResonanceValueComplexity(GeneDef geneDef)
        {
            return geneDef.biostatCpx * 2f;
        }

        public static float GeneResonanceValueMetabolism(GeneDef geneDef)
        {
            return ((geneDef.biostatMet < 0) ? (geneDef.biostatMet * -1.5f) : (geneDef.biostatMet * -1f));
        }

        public static float GeneResonanceValue(GeneDef geneDef)
        {
            // Archite is the primary anchor (Max 30)
            float arcWeight = GeneResonanceValueArchite(geneDef);
            // Complexity is the signal density (Max 10)
            float cpxWeight = GeneResonanceValueComplexity(geneDef);
            // Metabolism is the entropic stability. Negative (high power) costs more to stabilize.
            float metWeight = GeneResonanceValueMetabolism(geneDef);

            // negative values become 1
            return Mathf.Max(arcWeight + cpxWeight + metWeight, 1f);
        }

    }
}