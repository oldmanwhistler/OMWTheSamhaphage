using RimWorld;
using Verse;
using System.Collections.Generic;
namespace OMW_Samhaphage
{
    public class NullThrumSelectionGeneBlocked
    {
        Dictionary<GeneDef, string> dict = new Dictionary<GeneDef, string>();

        public void Append(GeneDef key, string value)
        {
            this.dict[key] = this.dict.TryGetValue(key, out var existing) ? $"{existing}, {value}" : value;
        }
    }
}