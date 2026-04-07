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

        // TODO: this should be static? It doesn't need to be stored on each individual hediff instance, and it would be more efficient to cache it instead of looking it up every time a new hediff is created.
        public List<GeneDef> blacklist = new List<GeneDef>();

        public List<string> defnameStrings = new List<string>();

        public bool Active = false;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Collections.Look(ref this.geneDefs, nameof(this.geneDefs));
            Scribe_Collections.Look(ref this.blacklist, nameof(this.blacklist));
            Scribe_Collections.Look(ref this.defnameStrings, nameof(this.defnameStrings));
            Scribe_Values.Look(ref this.Active, nameof(this.Active));
            CompPostMake();

        }

        public override void CompPostMake()
        {
            base.CompPostMake();

            blacklist?.Clear();
            defnameStrings?.Clear();

            // TODO: make this more efficient by caching the blacklist and defnameStrings instead of looking them up every time a new hediff is created. This is especially important if there are a lot of wretchblacklistdefs, but even with just a few it would be better to cache it.
            List<AlphaGenes.WretchBlacklistDef> allWretchBlacklistedGenes = DefDatabase<AlphaGenes.WretchBlacklistDef>.AllDefsListForReading;
            foreach (AlphaGenes.WretchBlacklistDef individualList in allWretchBlacklistedGenes)
            {
                if (!individualList.blackListedGenes.NullOrEmpty())
                {
                    blacklist.AddRange(individualList.blackListedGenes);
                }
                if (!individualList.blackListedDefNameStrings.NullOrEmpty())
                {
                    defnameStrings.AddRange(individualList.blackListedDefNameStrings);
                }


            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (!Active && this.parent.pawn.Map != null)
            {
                Active = true;
                this.geneDefs?.Clear();
                for (int i = 0; i < Props.numberOfGenes; i++)
                {
                    // TODO: make this more efficient by caching the list of valid genes instead of looking it up every time a mutation is applied. This is especially important if there are a lot of genes, but even with just a few it would be better to cache it.
                    GeneDef gene = DefDatabase<GeneDef>.AllDefs.Where((GeneDef x) => x.exclusionTags?.Contains("AG_OnlyOnCharacterCreation") == false &&
                    x.prerequisite == null && x.biostatArc == 0 && x.biostatMet > Props.minMetabolism && x.biostatMet < Props.maxMetabolism && x.modContentPack?.PackageId != "vanillaracesexpanded.insector" && !defnameStrings.Any(s => x.defName.Contains(s))
                    && !blacklist.Contains(x)).RandomElement();
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