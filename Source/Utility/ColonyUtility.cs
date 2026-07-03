using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public static class ColonyUtility
    {
        /// <summary>
        /// Counts the total number of living, free colonists across all maps.
        /// </summary>
        public static int TotalColonist()
        {
            return PawnsFinder.AllMaps_FreeColonists.Count(p => !p.Dead);
        }

        /// <summary>
        /// Counts the number of living colonists of a specific xenotype.
        /// </summary>
        public static int TotalXenotype(XenotypeDef xeno)
        {
            return PawnsFinder.AllMaps_FreeColonists.Count(p => !p.Dead && p.genes?.Xenotype == xeno);
        }

        /// <summary>
        /// Returns the percentage (0-100) of the colony that belongs to a specific xenotype.
        /// </summary>
        public static int PercentageXenotype(XenotypeDef xeno, int offset = 0)
        {
            int total = TotalColonist();
            if (total == 0) return 0;
            return Mathf.FloorToInt((float)(TotalXenotype(xeno) + offset) / (float)(total + offset) * 100f);
        }

        // This function returns the number of additional pawns of the given xenotype that can be added to the colony before reaching the limit.
        public static int MaxPossibleXenotypeIncrease(XenotypeDef xenotypeDef)
        {
            if (!OMW_Mod.settings.limitPercentage.enabled)
            {
                return int.MaxValue;
            }

            int maxPercentage = OMW_Mod.settings.limitPercentage.GetLimit(xenotypeDef);
            for (int i = 0; i < 100; i++)
            {
                int curPercentage = ColonyUtility.PercentageXenotype(xenotypeDef, i);
                if (curPercentage >= maxPercentage)
                {
                    return i;
                }
            }

            return 0;
        }

        public static bool CradlemoldPregenancy(Pawn mother, Pawn father)
        {
            // FIXME: add the logic to create a samhaphage by switching this to a samhaphage infestation

            if (mother == null || father == null) return false;

            if (mother.health.hediffSet.HasHediff(HediffDefOf.Sterilized)) return false;
            if (father.health.hediffSet.HasHediff(HediffDefOf.Sterilized)) return false;
            if (mother.health.hediffSet.HasHediff(HediffDefOf.PregnantHuman))
            {
                Log.Message($"[OMW_Samhaphage] {mother.LabelShort} is already pregnant, cannot apply pregnancy.");
                return false;
            }

            Hediff_Pregnant hediff_Pregnant =
                (Hediff_Pregnant)HediffMaker.MakeHediff(HediffDefOf.PregnantHuman, mother);
            hediff_Pregnant.Severity = PregnancyUtility.GeneratedPawnPregnancyProgressRange.TrueMin;
            hediff_Pregnant.SetParents(mother, father, null);
            mother.health.AddHediff(hediff_Pregnant);        
            return true;
        }
    }
}