using System;
using System.Collections.Generic;
using BoltFreezer.Interfaces;
using System.Linq;

namespace BoltFreezer.PlanTools
{
    [Serializable]
    public static class CacheMaps
    {
        /// <summary>
        /// Stored mappings for repair applicability. Do Not Use unless you are serializing
        /// </summary>
        public static Dictionary<IPredicate, List<int>> CausalMap = new Dictionary<IPredicate, List<int>>();

        /// <summary>
        /// Stored mappings for threat detection. Do Not Use unless you are serializing
        /// </summary>
        public static Dictionary<IPredicate, List<int>> ThreatMap = new Dictionary<IPredicate, List<int>>();

        public static IEnumerable<IOperator> GetCndts(IPredicate pred)
        {
            if (CausalMap.ContainsKey(pred))
                return from intID in CausalMap[pred] select GroundActionFactory.GroundLibrary[intID];

            return new List<IOperator>();
        }

        public static IEnumerable<IOperator> GetThreats(IPredicate pred)
        {
            if (ThreatMap.ContainsKey(pred))
                return ThreatMap[pred].Select(intID => GroundActionFactory.GroundLibrary[intID]);
            //        Where(intID => x.First.Equals(elm)).Select(x => x.Second);
            //return from intID in ThreatMap[pred] select GroundActionFactory.GroundLibrary[intID];
            return new List<IOperator>();
        }

        public static bool IsCndt(IPredicate pred, IPlanStep ps)
        {
            if (!CausalMap.ContainsKey(pred))
                return false;

            return CausalMap[pred].Contains(ps.Action.ID);
        }

        public static bool IsThreat(IPredicate pred, IPlanStep ps)
        {
            if (!ThreatMap.ContainsKey(pred))
                return false;

            return ThreatMap[pred].Contains(ps.Action.ID);
        }

        // Checks for mappings pairwise
        public static void CacheLinks(List<IOperator> groundSteps)
        {
            foreach (var tstep in groundSteps)
            {
                foreach (var tprecond in tstep.Preconditions)
                {
                    if (CausalMap.ContainsKey(tprecond) || ThreatMap.ContainsKey(tprecond))
                    {
                        // Then this precondition has already been evaluated.
                        continue;
                    }

                    foreach (var hstep in groundSteps)
                    {
                        if (hstep.Effects.Contains(tprecond))
                        {
                            if (!CausalMap.ContainsKey(tprecond))
                                CausalMap.Add(tprecond, new List<int>() { hstep.ID });
                            else
                                CausalMap[tprecond].Add(hstep.ID);
                        }
                        if (hstep.Effects.Contains(tprecond.GetReversed()))
                        {
                            if (!ThreatMap.ContainsKey(tprecond))
                                ThreatMap.Add(tprecond, new List<int>() { hstep.ID });
                            else
                                ThreatMap[tprecond].Add(hstep.ID);
                        }
                    }

                }
            }
        }

        // Limits checks to just those from heads to tails.
        public static void CacheLinks(List<IOperator> heads, List<IOperator> tails)
        {
            foreach (var tstep in tails)
            {
                foreach (var tprecond in tstep.Preconditions)
                {

                    foreach (var hstep in heads)
                    {
                        if (CausalMap[tprecond].Contains(hstep.ID) || ThreatMap[tprecond].Contains(hstep.ID))
                        {
                            // then this head step has already been checked for this condition
                            continue;
                        }

                        if (hstep.Effects.Contains(tprecond))
                        {
                            if (!CausalMap.ContainsKey(tprecond))
                                CausalMap.Add(tprecond, new List<int>() { hstep.ID });
                            else
                                CausalMap[tprecond].Add(hstep.ID);
                        }
                        if (hstep.Effects.Contains(tprecond.GetReversed()))
                        {
                            if (!ThreatMap.ContainsKey(tprecond))
                                ThreatMap.Add(tprecond, new List<int>() { hstep.ID });
                            else
                                ThreatMap[tprecond].Add(hstep.ID);
                        }
                    }
                }

            }
        }

        public static void CacheGoalLinks(List<IOperator> groundSteps, List<IPredicate> goal)
        {

            foreach( var goalCondition in goal)
            {
                foreach(var gstep in groundSteps)
                {
                    if (gstep.Effects.Contains(goalCondition))
                    {
                        if (!CausalMap.ContainsKey(goalCondition))
                            CausalMap.Add(goalCondition, new List<int>() { gstep.ID });
                        else
                            CausalMap[goalCondition].Add(gstep.ID);
                    }
                    if (gstep.Effects.Contains(goalCondition.GetReversed()))
                    {
                        if (!ThreatMap.ContainsKey(goalCondition))
                            ThreatMap.Add(goalCondition, new List<int>() { gstep.ID });
                        else
                            ThreatMap[goalCondition].Add(gstep.ID);
                    }
                }
            }
        }
    }
}