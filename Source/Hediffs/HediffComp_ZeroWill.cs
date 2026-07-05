using RimWorld;
using Verse;

namespace OMW_Samhaphage
{
    public class HediffComp_ZeroWill : HediffComp
    {
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            // 2500 ticks is roughly 1 in-game hour
            if (this.parent.pawn.IsHashIntervalTick(2500) && this.parent.pawn.Spawned)
            {
                if (this.parent.pawn.guest != null && this.parent.pawn.guest.Resistance > 0)
                {
                    // Shave off 0.5 resistance every hour automatically
                    this.parent.pawn.guest.resistance = System.Math.Max(0f, this.parent.pawn.guest.resistance - 0.5f);
                }

                if (this.parent.pawn.guest != null && this.parent.pawn.guest.will > 0)
                {
                    // Shave off 0.5 resistance every hour automatically
                    this.parent.pawn.guest.will = System.Math.Max(0f, this.parent.pawn.guest.will - 0.4f);
                }
                if (this.parent.pawn.ideo != null && this.parent.pawn.Ideo != Faction.OfPlayer.ideos.PrimaryIdeo)
                {
                    // Reduce it by 5% (0.05)
                    // OffsetCertainty handles the math and ensures it doesn't go below 0
                    this.parent.pawn.ideo.OffsetCertainty(-0.05f);

                    // Optional: If certainty hits 0, you could trigger a conversion check
                    if (this.parent.pawn.ideo.Certainty <= 0.001f)
                    {
                        PerformConversion();
                    }
                }
            }
        }        
        private void PerformConversion()
        {
            // Get the Ideoligion you want to convert them to.
            // Usually, this is the Ideoligion of the Colony/Faction.
            Ideo targetIdeo = Faction.OfPlayer.ideos.PrimaryIdeo;

            if (targetIdeo != null && this.parent.pawn.Ideo != targetIdeo)
            {
                this.parent.pawn.ideo.IdeoConversionAttempt(100f, Faction.OfPlayer.ideos.PrimaryIdeo);

                // Optional: Send a letter or message to the player
                Messages.Message($"{this.parent.pawn.LabelShort}'s mind has finally succumbed to the Absolute Frequency.",
                    this.parent.pawn, MessageTypeDefOf.PositiveEvent);
            }
        }
        // public override void CompPostTick(ref float severityAdjustment)
        // {
        //     // Check if the this.parent.pawn is a prisoner and has a guest tracker
        //     if (this.parent.pawn.guest != null && this.parent.pawn.IsPrisoner)
        //     {
        //         this.parent.pawn.guest.will = 0f;
        //     }
        // }
    }

    public class HediffCompProperties_ZeroWill : HediffCompProperties
    {
        public HediffCompProperties_ZeroWill()
        {
            this.compClass = typeof(HediffComp_ZeroWill);
        }
    }
}