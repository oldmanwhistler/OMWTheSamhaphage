using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public static class OMWXenotypes
    {
        private static Pawn theSovereignStillness = null;

        private static Pawn GetSovereignStillness()
        {
            if (theSovereignStillness != null && theSovereignStillness.Spawned && !theSovereignStillness.Dead)
            {
                // The pawn is physically on the map and alive
                if (theSovereignStillness.genes?.Xenotype == OMW_XenotypeDefOf.omw_sovereign_stillness)
                {
                    return theSovereignStillness;
                }
            }

            // if we get here then we don't have a valid cached Sovereign Stillness
            theSovereignStillness = null;

            ThereCanOnlyBeOne();

            return theSovereignStillness;
        }

        public static void ThereCanOnlyBeOne()
        {
            IEnumerable<Pawn> playerPawns = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction;

            // 2. Filter for your specific xenotype
            // Use the '?' null-conditional operator to avoid crashing if a pawn has no genes
            List<Pawn> specificXenos = playerPawns.Where(p => p.genes?.Xenotype == OMW_XenotypeDefOf
                .omw_sovereign_stillness).ToList();

            if (specificXenos.Count == 0)
            {
                theSovereignStillness = null;
                return;
            }

            if (specificXenos.Count == 1)
            {
                theSovereignStillness = specificXenos[0];
                return;
            }

            // find the "best" Sovereign. Default to the first entry to ensure a choice is made.
            Pawn choice = specificXenos[0];
            int complexity = OMWGenes.CalculateComplexity(choice);

            for (int i = 1; i < specificXenos.Count; i++)
            {
                Pawn pawn = specificXenos[i];
                int currComplexity = OMWGenes.CalculateComplexity(pawn);
                if (currComplexity > complexity)
                {
                    complexity = currComplexity;
                    choice = pawn;
                }
            }

            theSovereignStillness = choice;

            // convert the rest to Samhaphage
            foreach (Pawn pawn in specificXenos)
            {
                if (pawn == choice)
                {
                    Log.Message($"{pawn.Name} remains the Sovereign Stillness.");
                }
                else 
                {
                    Log.Message($"{pawn.Name} forced to become a Samhaphage from ThereCanOnlyBeOne().");
                    OMWGenes.ChangeEndotype(pawn, OMW_XenotypeDefOf.omw_samhaphage);
                    Find.LetterStack.ReceiveLetter($"{pawn.LabelShort} xenotype changed.", $"{pawn.LabelShort} lost the role of Sovereign Stillness and has returned to being a Samhaphage.", LetterDefOf.NegativeEvent,(TargetInfo)pawn);
                }
            }
        }

        public static bool IsSovereignStillnessInPlayerFaction()
        {
            if (null == GetSovereignStillness())
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private static readonly Texture2D LogoSovereignStillness =
            ContentFinder<Texture2D>.Get("UI/Menu/LogoSovereignStillness", false) ??
            BaseContent.BadTex;

        private static readonly Texture2D LogoSamhaphage =
            ContentFinder<Texture2D>.Get("UI/Menu/LogoSamhaphage", false) ??
            BaseContent.BadTex;

        private static readonly Texture2D LogoHallowbound =
            ContentFinder<Texture2D>.Get("UI/Menu/LogoHallowbound", false) ??
            BaseContent.BadTex;

        private static readonly Texture2D LogoEchovessel =
            ContentFinder<Texture2D>.Get("UI/Menu/LogoEchovessel", false) ??
            BaseContent.BadTex;

        private static readonly Texture2D LogoCradlemold =
            ContentFinder<Texture2D>.Get("UI/Menu/LogoCradlemold", false) ??
            BaseContent.BadTex;

        private static readonly Texture2D LogoFluxspawnHiveling =
            ContentFinder<Texture2D>.Get("UI/Menu/LogoFluxspawnHiveling", false) ??
            BaseContent.BadTex;

        private static readonly Texture2D LogoFluxspawnBrute =
            ContentFinder<Texture2D>.Get("UI/Menu/LogoFluxspawnBrute", false) ??
            BaseContent.BadTex;

        private static readonly Texture2D LogoFluxspawnFlicker =
            ContentFinder<Texture2D>.Get("UI/Menu/LogoFluxspawnFlicker", false) ??
            BaseContent.BadTex;

        private static readonly Texture2D ResonanceBG =
            ContentFinder<Texture2D>.Get("UI/Menu/ResonanceBG", false) ??
            BaseContent.BadTex;

        public static Texture2D GetXenotypeLogo(Pawn pawn)
        {
            if (pawn == null || pawn.genes == null || pawn.genes.Xenotype == null)
                return null;

            var xenotype = pawn.genes.Xenotype;
            if (xenotype == null)
                return null;

            Texture2D logo = null;
            switch (xenotype.defName)
            {
                case "omw_sovereign_stillness":
                    logo = LogoSovereignStillness;
                    break;
                case "omw_samhaphage":
                    logo = LogoSamhaphage;
                    break;
                case "omw_hallowbound":
                    logo = LogoHallowbound;
                    break;
                case "omw_echovessel":
                    logo = LogoEchovessel;
                    break;
                case "omw_cradlemold":
                    logo = LogoCradlemold;
                    break;
                case "omw_fluxspawn_hiveling":
                    logo = LogoFluxspawnHiveling;
                    break;
                case "omw_fluxspawn_brute":
                    logo = LogoFluxspawnBrute;
                    break;
                case "omw_fluxspawn_flicker":
                    logo = LogoFluxspawnFlicker;
                    break;
            }

            if (logo == null)
                return BaseContent.BadTex;

            return logo;
        }        
    }
}