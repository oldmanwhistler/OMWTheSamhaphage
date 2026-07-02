using RimWorld;
using Verse;
using UnityEngine;

namespace OMW_Samhaphage
{
    public enum NullThrumAbilityMenuType
    {
        ByXenotype,
        None,
        FluxspawnHiveling,
        FluxspawnBrute,
        FluxspawnFlicker,
        Cradlemold,
        Echovessel,
        Hallowbound,
        Samhaphage,
        SovereignStillness
    }

    [StaticConstructorOnStartup]
    public static class NullThrumAbilityMenu
    {
        static Logger Log = new Logger("UI");

        private static readonly Texture2D SovereignStillness =
            ContentFinder<Texture2D>.Get("UI/Menu/SovereignStillnessBG", true) ??
            BaseContent.BadTex;

        private static readonly Texture2D Samhaphage =
            ContentFinder<Texture2D>.Get("UI/Menu/SamhaphageBG", true) ??
            BaseContent.BadTex;

        private static readonly Texture2D Hallowbound =
            ContentFinder<Texture2D>.Get("UI/Menu/HallowboundBG", true) ??
            BaseContent.BadTex;

        private static readonly Texture2D Echovessel =
            ContentFinder<Texture2D>.Get("UI/Menu/EchovesselBG", true) ??
            BaseContent.BadTex;

        private static readonly Texture2D Cradlemold =
            ContentFinder<Texture2D>.Get("UI/Menu/CradlemoldBG", true) ??
            BaseContent.BadTex;

        private static readonly Texture2D FluxspawnHiveling =
            ContentFinder<Texture2D>.Get("UI/Menu/FluxspawnHivelingBG", true) ??
            BaseContent.BadTex;

        private static readonly Texture2D FluxspawnBrute =
            ContentFinder<Texture2D>.Get("UI/Menu/FluxspawnBruteBG", true) ??
            BaseContent.BadTex;

        private static readonly Texture2D FluxspawnFlicker =
            ContentFinder<Texture2D>.Get("UI/Menu/FluxspawnFlickerBG", true) ??
            BaseContent.BadTex;


        public static Texture2D GetMenuBackgroundXenotype(Pawn pawn)
        {
            if (pawn == null || pawn.genes == null || pawn.genes.Xenotype == null)
            {
                Log.Debug($"NullThrumAbilityMenu.GetMenuBackgroundXenotype, pawn is null or has no xenotype. pawn: {pawn?.Name}, genes: {pawn?.genes}, xenotype: {pawn?.genes?.Xenotype}");
                return null;
            }

            var xenotype = pawn.genes.Xenotype;
            if (xenotype == null)
                return null;

            Log.Debug($"NullThrumAbilityMenu.GetMenuBackgroundXenotype, xenotype: {xenotype.defName}");

            switch (xenotype.defName)
            {
                case "omw_sovereign_stillness":
                    return SovereignStillness;
                case "omw_samhaphage":
                    return Samhaphage;
                case "omw_hallowbound":
                    return Hallowbound;
                case "omw_echovessel":
                    return Echovessel;
                case "omw_cradlemold":
                    return Cradlemold;
                case "omw_fluxspawn_hiveling":
                    return FluxspawnHiveling;
                case "omw_fluxspawn_brute":
                    return FluxspawnBrute;
                case "omw_fluxspawn_flicker":
                    return FluxspawnFlicker;
                default:
                    Log.Debug($"NullThrumAbilityMenu.GetMenuBackground, unknown abilityMenuType: {OMW_Mod.settings.abilityMenuType}");
                    return BaseContent.BadTex;
            }
        }

        public static Texture2D GetMenuBackground(Pawn pawn)
        {
            Log.Debug($"NullThrumAbilityMenu.GetMenuBackground, abilityMenuType: {OMW_Mod.settings.abilityMenuType}, pawn: {pawn?.Name}");

            switch (OMW_Mod.settings.abilityMenuType)
            {
                case NullThrumAbilityMenuType.ByXenotype:
                    return GetMenuBackgroundXenotype(pawn);
                case NullThrumAbilityMenuType.None:
                    return BaseContent.BadTex;
                case NullThrumAbilityMenuType.FluxspawnHiveling:
                    return FluxspawnHiveling;
                case NullThrumAbilityMenuType.FluxspawnBrute:
                    return FluxspawnBrute;
                case NullThrumAbilityMenuType.FluxspawnFlicker:
                    return FluxspawnFlicker;
                case NullThrumAbilityMenuType.Cradlemold:
                    return Cradlemold;
                case NullThrumAbilityMenuType.Echovessel:
                    return Echovessel;
                case NullThrumAbilityMenuType.Hallowbound:
                    return Hallowbound;
                case NullThrumAbilityMenuType.Samhaphage:
                    return Samhaphage;
                case NullThrumAbilityMenuType.SovereignStillness:
                    return SovereignStillness;
                default:
                    Log.Debug($"NullThrumAbilityMenu.GetMenuBackground, unknown abilityMenuType: {OMW_Mod.settings.abilityMenuType}");
                    return BaseContent.BadTex;
            }
        }
    }
}
