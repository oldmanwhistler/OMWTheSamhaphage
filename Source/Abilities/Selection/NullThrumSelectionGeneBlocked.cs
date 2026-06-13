using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Collections.Concurrent;
namespace OMW_Samhaphage
{
    public class NullThrumSelectionGeneBlocked
    {
        private Dictionary<GeneDef, string> dict = new Dictionary<GeneDef, string>();

        public void Append(GeneDef key, string value)
        {
            this.dict[key] = this.dict.TryGetValue(key, out var existing) ? $"{existing}, {value}" : value;
        }

        public bool Has(GeneDef key)
        {
            return this.dict.TryGetValue(key, out var existing);
        }

        public string Str(GeneDef key)
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