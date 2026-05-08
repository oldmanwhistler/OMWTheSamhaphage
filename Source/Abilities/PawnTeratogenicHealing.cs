using RimWorld;
using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    // And it’s teratogenic healing, baby
    // It’s teratogenic healing, it’s good for me
    // Teratogenic healing, baby
    // Makes me feel so... carcinogenicly free
    public class PawnTeratogenicHealing : NullThrumAbilityPawnOnly
    {
        public override string AbilityName => "Teratogenic Healing";
        public override string AbilityDescription(Pawn victim, Pawn caster)
        {
            return $"{caster.LabelShort} will use their carcinomas to heal {victim.LabelShort}.";
        }
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/TeratogenicHealing");
        public override bool ApplyPawn(Pawn pawn, Pawn caster)
        {
            HediffDef hediffDef = HediffDefOf.Carcinoma;

            List<Hediff> carcinomas = new List<Hediff>();
            foreach (Hediff hediffToCheck in caster.health.hediffSet.hediffs)
            {
                if (hediffToCheck.def == hediffDef)
                {
                    carcinomas.Add(hediffToCheck);
                }
            }

            if (carcinomas.Count == 0)
            {
                Log.Message($"{caster.LabelShort} doesn't have any carcinomas to heal with.");
                return false;
            }

            List<Hediff_Injury> injuries = new List<Hediff_Injury>();
            foreach (Hediff possibleInjuryHediff in pawn.health.hediffSet.hediffs)
            {
                if (possibleInjuryHediff as Hediff_Injury != null)
                {
                    injuries.Add((Hediff_Injury)possibleInjuryHediff);
                }
            }

            int removedCarcinomasCount = 0;
            int max = carcinomas.Count;
            for (int ii = 0; ii < max; ii++)
            {
                if (injuries.Count == 0) break;                

                Hediff_Injury injury = injuries.RandomElement();
                injury.Severity -= 20f;
                if (injury.Severity == 0f)
                {
                    injuries.Remove(injury);
                }

                Hediff carcinoma = carcinomas.RandomElement();
                caster.health.RemoveHediff(carcinoma);
                carcinomas.Remove(carcinoma);
                removedCarcinomasCount++;
            }

            Log.Message($"{caster.LabelShort} removed ${removedCarcinomasCount} carcinomas.");
            return (bool)(removedCarcinomasCount > 0);
        }

        public override bool CanApplyOnPawn(Pawn victim, Pawn caster, out string reason)
        {
            reason = "unknown reason";

            if (victim == null)
            {
                reason = "Target is null.";
                return false;
            }

            // Check if target is a not already Retune
            if (!victim.RaceProps.Humanlike)
            {
                reason = $"{victim.LabelShort} is not humanlike.";
                return false;
            }

            if (!OMWGenes.HasNullThrum(caster))
            {
                reason = $"{caster.LabelShort} is not part of the harmony of the Null-Thrum.";
                return false;
            }

            if (!caster.genes.HasActiveGene(OMW_GeneDefOf.AG_Teratogenesis))
            {
                reason = $"{caster.LabelShort} is does not possess Teratogenesis.";
                return false;
            }

            if (caster.health.hediffSet.HasHediff(OMW_HediffDefOf.OMW_GeneticDissonance))
            {
                reason = $"{caster.LabelShort} is affected by Genetic Dissonance.";
                return false;
            }

            if (PawnTeratogenics.CarcinomaCount(caster) == 0)
            {
                reason = $"{caster.LabelShort} doesn't have any carcinomas.";
                return false;
            }

            List<Hediff_Injury> injuries = new List<Hediff_Injury>();
            foreach (Hediff possibleInjuryHediff in victim.health.hediffSet.hediffs)
            {
                if (possibleInjuryHediff as Hediff_Injury != null)
                {
                    injuries.Add((Hediff_Injury)possibleInjuryHediff);
                }
            }  
            if (injuries.Count == 0)
            {
                reason = $"{victim.LabelShort} does not have any injuries.";
                return false;
            }            

            return true;
        }      
    }
}
