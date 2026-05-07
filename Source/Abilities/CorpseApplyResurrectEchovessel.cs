using UnityEngine;
using Verse;

namespace OMW_Samhaphage
{
    public class CorpseApplyResurrectEchovessel : CorpseApplyResurrectBase
    {
        public override Verse.HediffDef TargetHediff => OMW_HediffDefOf.OMW_SilentServitude;
        public override RimWorld.XenotypeDef TargetXenotype => OMW_XenotypeDefOf.omw_echovessel;
        public override bool SacrificeCaster => false;
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/OMW/ResurrectEchovessel");

        public override string VerbName => "Resurrect";
        public override string VerbDescription(Pawn victim, Pawn caster)
        {
            if (this.SacrificeCaster)
                return $"Resurrect {victim.LabelShort} as an Echovessel by sacrificing yourself.";
            else
                return $"Resurrect {victim.LabelShort} as an Echovessel.";
        }


        public override bool CanApplyOnCorpse(Corpse corpse, Pawn caster, out string reason)
        {
            reason = "unknown reason";
            if (corpse == null)
            {
                reason = "Target is null.";
                return false;
            }

            // 2. Is there a pawn inside?
            Pawn pawn = corpse.InnerPawn;
            if (pawn == null)
            {
                reason = "Corpse is missing InnerPawn or InnerPawn is invalid.";
                return false;
            }

            if (pawn.RaceProps?.Humanlike != true)
            {
                reason = "Target is not humanlike.";
                return false;
            }


            // 4. Is the pawn already being resurrected/interacted with?
            if (pawn.Spawned && !corpse.Spawned)
            {
                reason = "Pawn is already being processed.";
                return false;
            }

            // 5. Is the head missing? (RimWorld standard resurrection fails without a head)
            if (pawn.health.hediffSet.GetBrain() == null)
            {
                reason = "Vessel is decapitated; the frequency cannot be anchored.";
                return false;
            }

            return true;
        }
    }
}