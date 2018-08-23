using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.Utilities
{
    [Serializable]
    public class TupleMap<T1, T2>
    {
        public Dictionary<T1, T2> PosMap = new Dictionary<T1, T2>();
        public Dictionary<T1, T2> NegMap = new Dictionary<T1, T2>();

        public TupleMap(Dictionary<T1, T2> posMap, Dictionary<T1, T2> negMap)
        {
            PosMap = posMap;
            NegMap = negMap;
        }

        public TupleMap()
        {

        }

        public Dictionary<T1, T2> Get(bool which)
        {
            if (which)
            {
                return PosMap;
            }
            return NegMap;
        }

        public void AddKeyIfMissing(bool sign, T1 key, T2 emptyValue)
        {
            var whichDictionary = Get(sign);
            if (!whichDictionary.ContainsKey(key))
            {
                whichDictionary[key] = emptyValue;
            }
        }

    }
}
