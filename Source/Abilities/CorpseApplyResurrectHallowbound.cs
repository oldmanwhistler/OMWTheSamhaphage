using UnityEngine;
using Verse;

namespace OMW_Samhaphage
{
    public class CorpseApplyResurrectHallowbound : CorpseApplyResurrectBase
    {
        public override Verse.HediffDef TargetHediff => OMW_HediffDefOf.OMW_SilentServitude;
        public override RimWorld.XenotypeDef TargetXenotype => OMW_XenotypeDefOf.omw_hallowbound;
        public override bool SacrificeCaster => true;
        public override Texture2D Icon => ContentFinder<Texture2D>.Get("UI/Abilities/ResurrectHallowbound");

        public override string VerbName => "Resurrect";
        public override string VerbDescription => "as a Hallowbound by sacrificing yourself.";

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