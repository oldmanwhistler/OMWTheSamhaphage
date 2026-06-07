using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;

namespace OMW_Samhaphage
{
    public static class KillUtility

    {
        static Logger Log = new Logger("Kill");

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

        public static void PurgeBionics(Pawn victim)
        {
            if (victim?.health?.hediffSet == null) return;

            Log.Debug($"PurgeBionics::START::({victim.LabelShort})");
            
            Map map = victim.MapHeld;
            IntVec3 pos = victim.PositionHeld;

            // Identify all hediffs that count as artificial (bionics, prosthetics, implants)
            List<Hediff> artificialHediffs = victim.health.hediffSet.hediffs
                .Where(h => h.def.countsAsAddedPartOrImplant)
                .ToList();

            Log.Debug($"PurgeBionics found {artificialHediffs.Count} bionics");

            foreach (Hediff hediff in artificialHediffs)
            {
                Log.Debug($"PurgeBionics working on bionic: {hediff.Label}");

                // If the bionic/implant has a defined item product, spawn it on the ground.
                if (map != null && hediff.def.spawnThingOnRemoved != null)
                {
                    Log.Debug($"PurgeBionics is creating bionic: {hediff.def.spawnThingOnRemoved.label}");
                    Thing thing = ThingMaker.MakeThing(hediff.def.spawnThingOnRemoved);
                    GenPlace.TryPlaceThing(thing, pos, map, ThingPlaceMode.Near);
                    Messages.Message($"Rendered {victim.LabelShort} and extracted {hediff.def.spawnThingOnRemoved.label}.", MessageTypeDefOf.PositiveEvent);
                }

                BodyPartRecord part = hediff.Part;
                victim.health.RemoveHediff(hediff);

                // For AddedParts (like bionic limbs), removal results in a missing part.
                // We restore the biological part to keep the victim intact but "natural".
                if (part != null && hediff is Hediff_AddedPart)
                {
                    victim.health.RestorePart(part);
                }
            }

            Log.Debug($"PurgeBionics::DONE::({victim.LabelShort})");
        }

        // safe shamblerization of corpses only if AnomalyDLC is present.
        public static bool CorpseDestroy(Corpse corpse)
        {
            if (corpse == null)
            {
                Log.Debug($"CorpseDestroy was called with a null corpse");
                return false;
            }

            Yum(corpse.InnerPawn);
            corpse.Destroy();
            return true;
            // }
        }


        public static bool PawnKillDestroy(Pawn victim, Pawn caster)
        {
            if (victim == null)
            {
                Log.Debug($"PawnKillDestroy was called with a null pawn");
                return false;
            }

            if (ApplyBrainDamage(victim, caster))
            {

                Log.Debug($"PawnKillDestroy is killing {victim.LabelShort} brain damage");
                return true;
            }
            else
            {

                Log.Debug($"PawnKillDestroy is killing {victim.LabelShort} with misc damage");
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
                Yum(victim);
                victim.Destroy();
                return true;
            }
        }

        private static void Yum(Pawn victim)
        {
            if (victim == null)
            {
                Log.Debug($"Yum() was called with a null pawn");
                return;
            }      
            
            Map map = victim.MapHeld;
            if (map == null) return;

            victim.Strip();

            IntVec3 pos = victim.PositionHeld;

            // We use .Cleanup() because our sub-effecters are SprayerTriggered one-shots.
            EffecterDef yumEffect = OMW_EffecterDefOf.OMW_YumEffect;
            yumEffect?.Spawn(pos, map).Cleanup();

            // Leave lasting filth on the ground
            for (int i = 0; i < 9; i++)
            {
                IntVec3 c = pos + GenRadial.RadialPattern[i];
                if (c.InBounds(map))
                {
                    FilthMaker.TryMakeFilth(c, map, victim.RaceProps.BloodDef ?? ThingDefOf.Filth_Blood);
                }
            }
            
            // Spawn meat, leather, and other butcher products
            IEnumerable<Thing> products = victim.ButcherProducts(null, 1.5f);
            if (products != null)
            {
                foreach (Thing product in products)
                {
                    GenPlace.TryPlaceThing(product, pos, map, ThingPlaceMode.Near);
                }
            }
            
        }

        public static void ApplyRenderOrAttenuate(Pawn victim, Pawn caster)
        {
            if (victim == null || caster == null) return;

            XenotypeDef xeno = caster.genes.Xenotype;
            if (victim.Dead && victim.Corpse != null)
            {
                if ((xeno == OMW_XenotypeDefOf.omw_samhaphage) || (xeno == OMW_XenotypeDefOf.omw_sovereign_stillness))
                {
                    // master xenotypes can render
                    CorpseApplyRender render = new CorpseApplyRender();
                    render.ApplyCorpse(victim.Corpse, caster);
                    return;
                }
            }

            // lesser xenotypes can attenuate
            ThingApplyAttenuate attenuate = new ThingApplyAttenuate();
            attenuate.ApplyPawn(victim, caster);
        }

        public static void ApplyRenderOrAttenuate(Corpse corpse, Pawn caster)
        {
            if (corpse?.InnerPawn == null || caster == null) return;
            ApplyRenderOrAttenuate(corpse.InnerPawn, caster);
        }
            
    }
}