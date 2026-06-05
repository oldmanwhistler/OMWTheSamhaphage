using RimWorld;
using Verse;
using System.Linq;

namespace OMW_Samhaphage
{
    public static class OMWAnomaly
    {
        static Logger Log = new Logger("Anomaly");

        public static bool ApplyBrainDamage(Pawn victim, Pawn caster)
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
                Log.Debug($"CorpseToShamblerOrDestroy was called with a null corpse");
                return false;
            }

            // if (!ModsConfig.AnomalyActive)
            // {
            //     corpse.Destroy();
            //     Log.Debug($"CorpseToShamblerOrDestroy anomaly isn't active so destroyed {corpse.LabelShort}");
            //     return true;
            // }
            // if (MutantUtility.CanResurrectAsShambler(corpse, true))
            // {
            //     int lifespanTicks = 60000 * 3; // 3 Days
            //     // Passing null for faction ensures the shambler is hostile to the player (and everyone else).
            //     MutantUtility.ResurrectAsShambler(corpse.InnerPawn, lifespanTicks, null);
                
            //     // Rebuild the graphics node immediately to prevent "Node is null" drawing errors
            //     corpse.InnerPawn.Drawer.renderer.EnsureGraphicsInitialized();
            //     Log.Debug($"CorpseToShamblerOrDestroy made a shambler from {corpse.LabelShort}");
            //     return true;
            // }
            // else
            // {
                corpse.Destroy();
                // Log.Debug($"CorpseToShamblerOrDestroy couldn't make a shambler so destroyed {corpse.LabelShort}");
                return true;
            // }
        }


        public static bool PawnToShamblerOrKillDestroy(Pawn victim, Pawn caster)
        {
            if (victim == null)
            {
                Log.Debug($"PawnToShamblerOrKillDestroy was called with a null pawn");
                return false;
            }

            // if (ModsConfig.AnomalyActive)
            // {
                
            //     Log.Debug($"PawnToShamblerOrKillDestroy make {victim.LabelShort} into a hostile shambler");
            //     // Strip the faction before conversion to ensure it becomes a hostile entity.
            //     victim.SetFaction(null);
            //     MutantUtility.SetPawnAsMutantInstantly(victim, MutantDefOf.Shambler);
            //     // Fix for "Node is null": Force graphics initialization 
            //     // after the pawn state changes to Shambler.
            //     victim.Drawer.renderer.EnsureGraphicsInitialized();
            //     return true;
            // }
            // else 
            if (ApplyBrainDamage(victim, caster))
            {
                
                Log.Debug($"PawnToShamblerOrKillDestroy is killing {victim.LabelShort} brain damage");
                return true;
            }
            else
            {
                
                Log.Debug($"PawnToShamblerOrKillDestroy is killing {victim.LabelShort} with misc damage");
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