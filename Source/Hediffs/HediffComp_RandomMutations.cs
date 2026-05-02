using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
// TODO: is this needed? using static HarmonyLib.Code;
using AlphaGenes;

// Based on AlphaGenes' RandomMutation hediff comp, but changed to match the headcanon for my xenotypes. Credit to Sarg for the original code and idea.

namespace OMW_Samhaphage
{
    public class HediffCompProperties_RandomMutation : HediffCompProperties
    {
        public int numberOfGenes = 1;
        public int period = 60000;
        public int minMetabolism = -100;
        public int maxMetabolism = 100;

        public HediffCompProperties_RandomMutation()
        {
            compClass = typeof(HediffComp_RandomMutation);
        }
    }
    public class HediffComp_RandomMutation : HediffComp
    {
        private HediffCompProperties_RandomMutation Props => (HediffCompProperties_RandomMutation)props;

        public List<GeneDef> geneDefs = new List<GeneDef>();

        // Caching these globally to prevent redundant iterations across multiple pawns
        private static List<GeneDef> cachedBlacklist;
        private static List<string> cachedDefnameStrings;
        private static List<GeneDef> cachedValidGenes;

        private static void EnsureCacheLoaded()
        {
            if (cachedBlacklist != null) return;

            cachedBlacklist = new List<GeneDef>();
            cachedDefnameStrings = new List<string>();

            List<AlphaGenes.WretchBlacklistDef> allWretchBlacklistedGenes = DefDatabase<AlphaGenes.WretchBlacklistDef>.AllDefsListForReading;
            foreach (AlphaGenes.WretchBlacklistDef individualList in allWretchBlacklistedGenes)
            {
                if (!individualList.blackListedGenes.NullOrEmpty())
                    cachedBlacklist.AddRange(individualList.blackListedGenes);
                
                if (!individualList.blackListedDefNameStrings.NullOrEmpty())
                    cachedDefnameStrings.AddRange(individualList.blackListedDefNameStrings);
            }
        }

        public bool Active = false;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Collections.Look(ref this.geneDefs, nameof(this.geneDefs));
            Scribe_Values.Look(ref this.Active, nameof(this.Active));
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            EnsureCacheLoaded();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (!Active && this.parent.pawn.Map != null)
            {
                Active = true;
                EnsureCacheLoaded();
                this.geneDefs?.Clear();
                for (int i = 0; i < Props.numberOfGenes; i++)
                {
                    GeneDef gene = DefDatabase<GeneDef>.AllDefs.Where((GeneDef x) => 
                        x.exclusionTags?.Contains("AG_OnlyOnCharacterCreation") == false &&
                        x.prerequisite == null && x.biostatArc == 0 && x.biostatMet > Props.minMetabolism && x.biostatMet < Props.maxMetabolism && 
                        x.modContentPack?.PackageId != "vanillaracesexpanded.insector" && 
                        !cachedDefnameStrings.Any(s => x.defName.Contains(s)) && 
                        !cachedBlacklist.Contains(x)).RandomElement();
                    
                    if (gene != null)
                    {
                        this.geneDefs.Add(gene);
                        this.parent.pawn.genes?.AddGene(gene, true);
                    }
                }
            }

            if (this.parent.pawn.IsHashIntervalTick(Props.period))
            {

                if (!this.geneDefs.NullOrEmpty())
                {
                    // only remove genes if they didn't become endogenes, otherwise the player would lose the gene permanently and it would be more frustrating than fun. This also means that if a gene becomes an endogene, it will stay with the pawn permanently, which fits with the headcanon of these mutations being a way for the xenotypes to evolve and adapt to their environment over time.
                    for (int i = 0; i < this.geneDefs.Count; i++)
                    {
                        if (this.parent.pawn.genes?.HasXenogene(this.geneDefs[i]) == true)
                        {
                            Gene gene = this.parent.pawn.genes?.GetGene(this.geneDefs[i]);
                            if (gene != null)
                            {
                                this.parent.pawn.genes?.RemoveGene(gene);
                            }
                        }
                    }
                    this.geneDefs?.Clear();
                }
                Active = false;
            }

        }


        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            // only remove genes if they didn't become endogenes, otherwise the player would lose the gene permanently and it would be more frustrating than fun. This also means that if a gene becomes an endogene, it will stay with the pawn permanently, which fits with the headcanon of these mutations being a way for the xenotypes to evolve and adapt to their environment over time.
            for (int i = 0; i < this.geneDefs.Count; i++)
            {
                if (this.parent.pawn.genes?.HasXenogene(this.geneDefs[i]) == true)
                {
                    Gene gene = this.parent.pawn.genes?.GetGene(this.geneDefs[i]);
                    if (gene != null)
                    {
                        this.parent.pawn.genes?.RemoveGene(gene);
                    }
                }                
            }
            Active = false;
            this.geneDefs?.Clear();
        }

    }
}