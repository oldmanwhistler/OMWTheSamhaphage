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
        public static float PercentageXenotype(XenotypeDef xeno)
        {
            int total = TotalColonist();
            if (total == 0) return 0;
            return (float)TotalXenotype(xeno) / (float)total * 100f;
        }
    }
}