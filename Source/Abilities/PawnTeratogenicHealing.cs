using RimWorld;
using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    // And it’s teratogenic healing, baby
    // It’s teratogenic healing, it’s good for me
    // Teratogenic healing, baby
    // Makes me feel so... carcinogenic - ally free
    public class PawnTeratogenicHealing : NullThrumAbilityPawnOnly
    {
        public override string VerbName => "Teratogenic Healing";
        public override string VerbDescription => "and heal them with their carcinomas.";   
        public override Texture2D Icon => BaseContent.BadTex;
        public override bool ApplyPawn(Pawn pawn, Pawn caster)
        {
            int healedCount = 0;
            HediffDef hediffDef = HediffDefOf.Carcinoma;

            List<Hediff> carcinomas = new List<Hediff>();
            foreach (Hediff hediffToCheck in pawn.health.hediffSet.hediffs)
            {
                if (hediffToCheck.def == hediffDef)
                {
                    carcinomas.Add(hediffToCheck);
                }
            }

            if (carcinomas.Count == 0)
            {
                Log.Message($"{pawn.LabelShort} doesn't have any carcinomas to heal with.");
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

            if (injuries.Count == 0)
            {
                foreach (Hediff possibleInjuryHediff in caster.health.hediffSet.hediffs)
                {
                    if (possibleInjuryHediff as Hediff_Injury != null)
                    {
                        injuries.Add((Hediff_Injury)possibleInjuryHediff);
                    }
                }
            }

            if (injuries.Count == 0)
            {
                Log.Message($"{pawn.LabelShort} and ${caster.LabelShort} both don't have any carcinomas to heal with.");
                return false;
            }

            if (injuries.Count > 0)
            {
                healedCount++;
                injuries.RandomElement().Severity -= 20f;
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
                pawn.health.RemoveHediff(carcinoma);
                carcinomas.Remove(carcinoma);
                removedCarcinomasCount++;
            }

            Log.Message($"{pawn.LabelShort} removed ${removedCarcinomasCount} carcinomas.");
            return (bool)(removedCarcinomasCount > 0);
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

            if (!p.genes.HasActiveGene(OMW_GeneDefOf.AG_Teratogenesis))
            {
                reason = $"{p.LabelShort} is does not possess Teratogenesis.";
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
    }
}
