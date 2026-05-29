using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;
using System.Linq; // Added for LINQ operations

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
                Log.Debug($"{pawn.LabelShort} has {resonance.Value} available resonance, checking if they have {requiredAmount}.");
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
                    $"{pawn.LabelShort} has {resonance.Value} available resonance, incrementing by {amount}.");
                resonance.Value += amount;
                Log.Debug(
                    $"{pawn.LabelShort} now has {resonance.Value} available resonance.");                
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
                    $"{pawn.LabelShort} has {resonance.Value} available resonance, decrementing by {amount}.");
                resonance.Value -= amount;
                Log.Debug(
                    $"{pawn.LabelShort} now has {resonance.Value} available resonance.");
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

        public static float CalculateGenePowerValue(GeneDef geneDef)
        {
            // Accessing RimWorld's internal biostats
            float comp = geneDef.biostatCpx;
            float meta = geneDef.biostatMet;
            float arch = geneDef.biostatArc;

            // Compute the power scale
            float rawValue = 2f + (((comp * 4f) - (meta * 2f) + (arch * 8f)) * Mathf.Sqrt(geneDef.marketValueFactor) / 2.5f);

            return rawValue;
        }

        public static float GeneResonanceValue(GeneDef geneDef)
        {
            if (geneDef == null) return 0.1f;
            float pv = CalculateGenePowerValue(geneDef);
            if (pv < 0f)
            {
                pv = 0.1f;
            }
            return pv;
        }

        public static float TraitNormalize(float value)
        {
            float factor;
            float absVal = Mathf.Abs(value);
            if (absVal == 0f) factor = 1f;
            else if (absVal <= 0.0001f) factor = 100000f;
            else if (absVal <= 0.001f) factor = 10000f;
            else if (absVal <= 0.01f) factor = 1000f;
            else if (absVal <= 0.1f) factor = 100f;
            else if (absVal <= 1f) factor = 10f;
            else if (absVal <= 10f) factor = 1f;
            else if (absVal <= 100f) factor = 0.1f;
            else if (absVal <= 1000f) factor = 0.01f;
            else if (absVal <= 10000f) factor = 0.001f;
            else if (absVal <= 100000f) factor = 0.0001f;
            else if (absVal <= 1000000f) factor = 0.00001f;
            else factor = 0.000001f;
            return value * factor;
        }
        public static float CalculateTraitOffsetSum(TraitDegreeData degree)
        {
            if (degree.statOffsets.NullOrEmpty()) return 0f;
            float total = 0f;
            foreach (StatModifier offset in degree.statOffsets)
            {
                total += TraitNormalize(offset.value);
            }
            return total;
        }

        public static float CalculateTraitFactorSum(TraitDegreeData degree)
        {
            if (degree.statFactors.NullOrEmpty()) return 0f;
            float total = 0f;
            foreach (StatModifier offset in degree.statFactors)
            {
                total += TraitNormalize(offset.value);
            }
            return total;
        }        

        public static float CalculateTraitPowerValue(TraitDegreeData degree)
        {
            float marketValue = degree.marketValueFactorOffset; // MVO in csv file
            float offsetSum = CalculateTraitOffsetSum(degree); // OFFSET_SUM in csv file
            float factorSum = CalculateTraitFactorSum(degree); // FACTOR_SUM in csv file
            int skillSum = degree.skillGains.NullOrEmpty() ? 0 : degree.skillGains.Sum(s => 10*s.amount); // SKILL_SUM in csv file
            float hungerFactor = TraitNormalize(degree.hungerRateFactor); // HUNGER in csv file
            float painOffset = TraitNormalize(degree.painOffset); // PAIN_OFFSET in csv file
            float painFactor = TraitNormalize(degree.painFactor); // PAIN_FACTOR in csv file           
            float hungerImpact = (hungerFactor - 10f) * -50f; // Penalizes hunger > 10, rewards hunger < 10
            float painImpact = (painOffset * -100f) + ((painFactor - 10f) * -40f);
            float rawPower = (marketValue * 300f) + (offsetSum * 50f) + (skillSum * 50f) + (factorSum * 50) +
                             hungerImpact + painImpact;
            

            // 1. Define Pivot Points
            // The Pivot is where the formula starts compressing (roughly rawPower = 5000, PV = 25.35)
            float pivotRaw = 5000f;
            float slope1 = 0.004269513f;
            float intercept = 4.0f;

            // Calculate the Pivot PV based on the original linear formula
            float pivotPV = (slope1 * pivotRaw) + intercept;

            // 2. Define the Target Cap
            // We want rawPower 24000 to equal 60
            float targetMax = 60.0f;
            float rawMax = 24000.0f;
            float slope2 = (targetMax - pivotPV) / (rawMax - pivotRaw);

            // 3. Piecewise Calculation
            float pv;
            if (rawPower <= pivotRaw)
            {
                pv = (slope1 * rawPower) + intercept;
            }
            else
            {
                pv = pivotPV + (slope2 * (rawPower - pivotRaw));
            }
            return pv;
        }        

        public static float TraitResonanceValue(Trait trait)
        {
            if (trait == null || trait.CurrentData == null) return 0.1f;
            return TraitResonanceValue(trait.CurrentData);
        }

        public static float TraitResonanceValue(TraitDegreeData data)
        {
            float pv = CalculateTraitPowerValue(data);
            // set the negative values to positive so everything is positive
            if (pv < 0f)
            {
                pv = 0.1f;
            }
            return pv;
        }
    }
}