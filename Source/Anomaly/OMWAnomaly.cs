using RimWorld;
using Verse;
using System.Linq;

namespace OMW_Samhaphage
{
    public static class OMWAnomaly
    {
        private static bool debug = true;

        public static bool ApplyBrainDamage(Pawn caster, Pawn victim)
        {
            if (victim == null) return false;

            BodyPartRecord brain = victim.health.hediffSet.GetNotMissingParts()
                .FirstOrDefault(x => x.def == BodyPartDefOf.Head);

            // if the brain is null then it will be general damage

            // DamageInfo(def, amount, armorPen, angle, instigator, hitPart, weapon, category, intendedTarget)
            DamageInfo dinfo = new DamageInfo(
                DamageDefOf.ExecutionCut,
                999f, // Amount
                999f, // Armor Penetration
                -1f, // Angle (Calculated automatically if -1)
                caster, // THE INSTIGATOR
                brain, // The specific part
                null, // Weapon (null if it's a psychic/gene power)
                DamageInfo.SourceCategory.ThingOrUnknown,
                victim // Intended Target
            );

            victim.TakeDamage(dinfo);
            return true;
        }

        // safe shamblerization of corpses only if AnomalyDLC is present.
        public static bool CorpseToShamblerOrDestroy(Corpse corpse)
        {
            if (corpse == null)
            {
                if (debug)
                    Log.Message($"CorpseToShamblerOrDestroy was called with a null corpse");
                return false;
            }

            if (!ModsConfig.AnomalyActive)
            {
                corpse.Destroy();
                if (debug)
                    Log.Message($"CorpseToShamblerOrDestroy anomaly isn't active so destroyed {corpse.LabelShort}");
                return true;
            }
            if (MutantUtility.CanResurrectAsShambler(corpse, true))
            {
                int lifespanTicks = 60000 * 3; // 3 Days
                MutantUtility.ResurrectAsShambler(corpse.InnerPawn, lifespanTicks, corpse.Faction);
                if (debug) Log.Message($"CorpseToShamblerOrDestroy make a shambler from {corpse.LabelShort}");
                return true;
            }
            else
            {
                corpse.Destroy();
                if (debug) Log.Message($"CorpseToShamblerOrDestroy couldn't make a shambler so destroyed {corpse.LabelShort}");
                return true;
            }
        }


        public static bool PawnToShamblerOrKillDestroy(Pawn caster, Pawn victim)
        {
            if (victim == null)
            {
                if (debug)
                    Log.Message($"PawnToShamblerOrKillDestroy was called with a null pawn");
                return false;
            }

            if (ModsConfig.AnomalyActive)
            {
                if (debug)
                    Log.Message($"PawnToShamblerOrKillDestroy make {victim.LabelShort} into a shambler");
                MutantUtility.SetPawnAsMutantInstantly(victim, MutantDefOf.Shambler);
                return true;
            }
            else if (ApplyBrainDamage(caster, victim))
            {
                if (debug)
                    Log.Message($"PawnToShamblerOrKillDestroy is killing {victim.LabelShort} brain damage");
                return true;
            }
            else
            {
                if (debug)
                    Log.Message($"PawnToShamblerOrKillDestroy is killing {victim.LabelShort} with misc damage");
                // we couldn't apply damage!?
                DamageInfo dinfo = new DamageInfo(
                    DamageDefOf.ExecutionCut,
                    999f, // Amount
                    999f, // Armor Penetration
                    -1f, // Angle (Calculated automatically if -1)
                    caster, // THE INSTIGATOR
                    null, // The specific part
                    null, // Weapon (null if it's a psychic/gene power)
                    DamageInfo.SourceCategory.ThingOrUnknown,
                    victim // Intended Target
                );
                Hediff exactCulpritHediff = new Hediff();
                exactCulpritHediff.pawn = caster;
                victim.Kill(dinfo, exactCulpritHediff);
                victim.Destroy();
                return true;
            }
        }
    }
}