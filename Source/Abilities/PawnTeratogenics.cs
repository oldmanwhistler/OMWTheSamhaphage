using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OMW_Samhaphage
{
    public static class PawnTeratogenics
    {
        public static int CarcinomaCount(Pawn pawn)
        {
            HediffDef hediffDef = HediffDefOf.Carcinoma;

            List<Hediff> carcinomas = new List<Hediff>();
            foreach (Hediff hediffToCheck in pawn.health.hediffSet.hediffs)
            {
                if (hediffToCheck.def == hediffDef)
                {
                    carcinomas.Add(hediffToCheck);
                }
            }

            return carcinomas.Count;
        }

        public static bool RemoveRandomCarcinoma(Pawn caster)
        {
            HediffDef hediffDef = HediffDefOf.Carcinoma;

            List<Hediff> carcinomas = new List<Hediff>();
            foreach (Hediff hediffToCheck in caster.health.hediffSet.hediffs)
            {
                if (hediffToCheck.def == hediffDef) carcinomas.Add(hediffToCheck);
            }

            if (carcinomas.Count > 0)
            {
                Hediff carcinoma = carcinomas.RandomElement();
                caster.health.RemoveHediff(carcinoma);
                return true;
            }
            else
            {
                return false;
            }
        }        
    }
}
