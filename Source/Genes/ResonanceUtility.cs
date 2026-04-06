using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public static class ResonanceUtility
    {
        public static bool HasGene(Pawn pawn)
        {
            if (pawn == null) return false;
            if (pawn.genes == null) return false;
            if (pawn.genes.HasActiveGene(OMW_GeneDefOf.OMW_Resonance)) return true;
            return false;
        }

        public static bool HasAvailable(Pawn pawn, int requiredAmount = 1)
        {
            if (HasGene(pawn))
            {
                Gene_Resource resonance = pawn.genes.GetGene(OMW_GeneDefOf.OMW_Resonance) as Gene_Resource;
                Log.Message($"[Resonance] {pawn.LabelShort} has {resonance.Value} available resonance, checking if they have {requiredAmount}.");
                return Mathf.RoundToInt(resonance.Value) >= requiredAmount;
            }
            return false;
        }

        public static bool Incr(string reason,Pawn pawn, int amount = 1)
        {
            if (HasGene(pawn))
            {
                Gene_Resource resonance = pawn.genes.GetGene(OMW_GeneDefOf.OMW_Resonance) as Gene_Resource;
                Log.Message(
                    $"[Resonance] {pawn.LabelShort} has {resonance.Value} available resonance, incrementing by {amount}.");
                resonance.Value += amount;
                Log.Message(
                    $"[Resonance] {pawn.LabelShort} now has {resonance.Value} available resonance.");                
                Messages.Message($"Gained {amount} resonance {reason}.", pawn, MessageTypeDefOf.PositiveEvent);
                return true;
            }
            return false;
        }

        public static bool Decr(Pawn pawn, int amount = 1)
        {
            if (HasGene(pawn))
            {
                Gene_Resource resonance = pawn.genes.GetGene(OMW_GeneDefOf.OMW_Resonance) as Gene_Resource;
                Log.Message(
                    $"[Resonance] {pawn.LabelShort} has {resonance.Value} available resonance, decrementing by {amount}.");
                resonance.Value -= amount;
                Log.Message(
                    $"[Resonance] {pawn.LabelShort} now has {resonance.Value} available resonance.");
                if (resonance.Value < 0) resonance.Value = 0f;
                return true;
            }

            return false;
        }

        public static int Total(Pawn pawn)
        {
            if (HasGene(pawn))
            {
                Gene_Resource resonance = pawn.genes.GetGene(OMW_GeneDefOf.OMW_Resonance) as Gene_Resource;
                return Mathf.RoundToInt(resonance.Value);
            }
            return 0;
        }
    }
}