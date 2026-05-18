using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace OMW_Samhaphage
{
    public class NullThrumTracker : WorldComponent
    {
        private Dictionary<NullThrumAbilityType, int> descriptionCounts = new Dictionary<NullThrumAbilityType, int>();

        public NullThrumTracker(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref descriptionCounts, "descriptionCounts", LookMode.Value, LookMode.Value);
            if (descriptionCounts == null)
            {
                descriptionCounts = new Dictionary<NullThrumAbilityType, int>();
            }
        }

        public int GetCount(NullThrumAbilityType ability)
        {
            return descriptionCounts.TryGetValue(ability, out int count) ? count : 0;
        }

        public void IncrementCount(NullThrumAbilityType ability)
        {
            if (!descriptionCounts.ContainsKey(ability))
                descriptionCounts[ability] = 1;
            else
                descriptionCounts[ability]++;
        }
    }
}