using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;
using AlphaGenes;

namespace OMW_Samhaphage
{
    public class Gene_ResourceResonance : Gene_Resource
    {
        static Logger Log = new Logger("Resonance");
        private const int PassiveGainIntervalTicks = 1000; // Avoids CS0108 name conflict

        public override float InitialResourceMax => OMW_Mod.settings.resonanceMax;
        public override int MaxForDisplay => Mathf.RoundToInt(OMW_Mod.settings.resonanceMax);
        protected override Color BarColor => new Color(0.36f, 0.22f, 0.42f); // Bruise-Purple
        protected override Color BarHighlightColor => new Color(0.54f, 0.17f, 0.89f); // Neon-Violet
        public override float MinLevelForAlert => 10f;
        public override string ResourceLabel => "resonance";


        public override int PostProcessValue(float value)
        {
            return Mathf.RoundToInt(value);
        }

        public override void PostAdd()
        {
            base.PostAdd();
            // Initialize max from settings immediately
            this.max = InitialResourceMax;
            // We override that here to ensure the pawn starts with a low random amount instead.
            Value = Rand.Range(30f, 50f);
        }

        public override void Tick()
        {
            base.Tick();

            // Use the hash interval to prevent performance hits every frame
            if (pawn.IsHashIntervalTick(PassiveGainIntervalTicks))
            {
                // Ensure the internal max field stays in sync with mod settings
                this.max = OMW_Mod.settings.resonanceMax;
                ApplyPassiveGain();
            }
        }

        private void ApplyPassiveGain()
        {
            // FIXME: This doesn't work.
            if (pawn.genes == null)
            {
                Log.Error($"DONE: {pawn.LabelShort}.ApplyPassiveGain -- because pawn.genes == null");
                return;
            }

            // Dynamically fetch the gain from the Pawn's stats
            // This looks for OMW_StatResonance defined in your StatDefs.xml
            float dailyGain = pawn.GetStatValue(StatDef.Named("OMW_StatResonance"));

            if (dailyGain != 0)
            {
                Log.Debug($"START: {pawn.LabelShort}.ApplyPassiveGain");
                // Calculate gain: (Daily Amount / 60000 ticks in a day) * Ticks Passed
                float gainPerInterval = (dailyGain / 60000f) * (float)PassiveGainIntervalTicks;
                Log.Debug($"{pawn.LabelShort}.ApplyPassiveGain: {gainPerInterval}");
                OffsetResonance(gainPerInterval);
                Log.Debug($"DONE: {pawn.LabelShort}.ApplyPassiveGain");
            }

        }

        public void OffsetResonance(float offset)
        {
            // Value and MaxForDisplay are built-in fields from Gene_Resource
            Value = Mathf.Clamp(Value + offset, 0f, MaxForDisplay);
        }
    }

    public class GeneGizmo_ResourceResonance : GeneGizmo_Resource
    {
        Gene_ResourceResonance ResourceGene => gene as Gene_ResourceResonance;

        public GeneGizmo_ResourceResonance(Gene_Resource gene, List<IGeneResourceDrain> drainGenes, Color barColor,
            Color barhighlightColor)
            : base(gene, drainGenes, barColor, barhighlightColor)
        {
        }

        protected override bool IsDraggable
        {
            get
            {
                return false;
            }
        }

        // Prevents the player from clicking/dragging to set threshold targets.
        protected override bool DraggingBar { get => false; set { } }
        
        protected override string GetTooltip()
        {
            string text =
                $"{gene.ResourceLabel.CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor)}: {gene.ValueForDisplay} / {gene.MaxForDisplay}\n";

            if (!gene.def.resourceDescription.NullOrEmpty())
            {
                text = text + "\n\n" + gene.def.resourceDescription.Formatted(gene.pawn.Named("PAWN")).Resolve();
            }

            return text;
        }        
    }

    public class StatWorker_Resonance : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            if (!base.ShouldShowFor(req)) return false;

            // The Numbers mod passes a StatRequest where req.Def is the PawnKindDef (Human).
            // We must return true here so the stat appears as an option in the Numbers UI.
            if (req.Def is PawnKindDef pk && pk.RaceProps.Humanlike)
            {
                return true;
            }

            // Unwrap pawn from Thing or Corpse
            Pawn p = req.Thing as Pawn;
            if (req.Thing is Corpse corpse) p = corpse.InnerPawn;

            if (p != null)
            {
                return ResonanceUtility.HasGene(p);
            }

            return false;
        }

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            // Handle requests for the Def itself (e.g., UI menus/Numbers)
            if (!req.HasThing) return 0f;

            Pawn p = req.Thing as Pawn;
            if (req.Thing is Corpse corpse) p = corpse.InnerPawn;

            // Only calculate for pawns that actually have the resonance gene
            if (p != null && ResonanceUtility.HasGene(p))
            {
                // This should return your desired "Daily Gain" amount.
                // You can link this to your mod settings or a base value.
                return 5f; 
            }

            return 0f;
        }
    }
}