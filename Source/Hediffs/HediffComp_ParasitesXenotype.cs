using RimWorld;
using Verse;
using Verse.Sound;
using System;
using System.Collections.Generic;
using RimWorld.Planet;
using AlphaGenes;

// Based on AlphaGenes HediffComp_Parasites, but the eggs will belong to a specific xenotype, that doesn't need to match either xenotype. e.g. Alien movies.

namespace OMW_Samhaphage
{
    public class HediffComp_ParasitesXenotype : HediffComp
    {
        Logger Log = new Logger("Hediff");

        public Pawn mother;
        public Faction motherFaction;
        public PawnKindDef motherDef;
        public XenotypeDef motherXenotypeDef;
        public int numBabiesMin = 1;
        public int numBabiesMax = 1;
        // this gets set to true if there is an error
        private bool disableHediff = false;


        public HediffCompProperties_ParasitesXenotype Props
        {
            get
            {
                return (HediffCompProperties_ParasitesXenotype)this.props;
            }
        }

        // I have zero idea what this does
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref this.mother, nameof(this.mother));
            Scribe_References.Look(ref this.motherFaction, nameof(this.motherFaction));
            Scribe_Defs.Look(ref this.motherDef, nameof(this.motherDef));
            Scribe_Defs.Look(ref this.motherXenotypeDef, nameof(this.motherXenotypeDef));
            Scribe_Values.Look(ref this.numBabiesMin, nameof(this.numBabiesMin));
            Scribe_Values.Look(ref this.numBabiesMax, nameof(this.numBabiesMax));
            Scribe_Values.Look(ref this.disableHediff, nameof(this.disableHediff), false);
        }


        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
           
            float severityToTurn = Props.severityToTurn;

            Map map = this.parent.pawn.Corpse.Map;
            if (map != null && this.parent.Severity > severityToTurn)
            {
                int numBabies = UnityEngine.Random.Range(numBabiesMin, numBabiesMax + 1);                
                if (numBabiesMin == numBabiesMax) numBabies = numBabiesMin;

                for(int ii=0; ii<numBabies; ii++)
                    Hatch();

                for (int i = 0; i < 20; i++)
                {
                    IntVec3 c;
                    CellFinder.TryFindRandomReachableCellNearPosition(this.parent.pawn.Corpse.Position, this.parent.pawn.Corpse.Position, map, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c);

                    FilthMaker.TryMakeFilth(c, this.parent.pawn.Corpse.Map, ThingDefOf.Filth_Blood);

                }


                InternalDefOf.Hive_Spawn.PlayOneShot(new TargetInfo(this.parent.pawn.Corpse.Position, map, false));
                

            }

        }

        private void Hatch()
        {
            if (disableHediff) return;

            try
            {
                PawnGenerationRequest request;
                Faction motherFaction = this.motherFaction;
                
                // Use a generic Colonist kind to avoid "disabled requiredWorkTags" conflicts 
                // caused by specialized kinds like Mechanitors or Highmates.
                PawnKindDef spawnKind = PawnKindDefOf.Colonist;

                Log.Debug(
                    $"Hatch() as {spawnKind.defName} with Xenotype {motherXenotypeDef?.defName}. Faction: {motherFaction?.Name}");
                request = new PawnGenerationRequest(spawnKind, motherFaction, PawnGenerationContext.NonPlayer, -1,
                    forceGenerateNewPawn: false, allowDead: false, allowDowned: true, canGeneratePawnRelations: true,
                    mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true,
                    allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false,
                    certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false,
                    worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null,
                    null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: true, forbidAnyTitle: false,
                    forceDead: false, null, null, motherXenotypeDef, null, null, 0f, DevelopmentalStage.Newborn);



                Pawn pawn = PawnGenerator.GeneratePawn(request);
                if (PawnUtility.TrySpawnHatchedOrBornPawn(pawn, this.parent.pawn.Corpse))
                {
                    if (pawn != null)
                    {
                        if (mother != null)
                        {
                            if (pawn.playerSettings != null && mother.playerSettings != null)
                            {
                                pawn.playerSettings.AreaRestrictionInPawnCurrentMap =
                                    mother.playerSettings.AreaRestrictionInPawnCurrentMap;
                            }

                            if (pawn.RaceProps.IsFlesh)
                            {
                                pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, mother);
                            }
                        }

                    }


                    Find.LetterStack.ReceiveLetter("AG_ParasitesHatchedLabel".Translate(pawn.NameShortColored),
                        "AG_ParasitesHatched".Translate(pawn.NameShortColored), LetterDefOf.PositiveEvent,
                        (TargetInfo)pawn);
                }
                else
                {
                    Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Exception during Hatch: {ex.Message}\n{ex.StackTrace}");
                disableHediff = true;
            }
        }
    }

    public class HediffCompProperties_ParasitesXenotype : HediffCompProperties
    {
        
        public float severityToTurn = 0.9f;
       
        public HediffCompProperties_ParasitesXenotype()
        {
            this.compClass = typeof(HediffComp_ParasitesXenotype);
        }
    }
}
    
