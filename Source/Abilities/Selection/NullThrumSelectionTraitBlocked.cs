using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Collections.Concurrent;
namespace OMW_Samhaphage
{
    public class NullThrumSelectionTraitBlocked
    {
        private Dictionary<TraitDef, string> dict = new Dictionary<TraitDef, string>();

        public void Append(TraitDef key, string value)
        {
            this.dict[key] = this.dict.TryGetValue(key, out var existing) ? $"{existing}, {value}" : value;
        }

        public bool Has(TraitDef key)
        {
            return this.dict.TryGetValue(key, out var existing);
        }

        public string Str(TraitDef key)
        {
            string blockedStr;
            if (this.dict.TryGetValue(key, out var existing))
            {
                blockedStr = existing;
            }
            else
            {
                blockedStr = "";
            }
            return blockedStr;
        }

        public int Count
        {
            get
            {
                return this.dict.Count;
            }
        }
    
    }
}