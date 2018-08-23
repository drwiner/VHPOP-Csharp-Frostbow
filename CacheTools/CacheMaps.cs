using System;
using System.Collections.Generic;
using BoltFreezer.Interfaces;
using System.Linq;
using BoltFreezer.Utilities;

namespace BoltFreezer.PlanTools
{
    [Serializable]
    public static class CacheMaps
    {
        /// <summary>
        /// Stored mappings for repair applicability. Do Not Use unless you are serializing
        /// </summary>
        /// 
        public static TupleMap<IPredicate, List<int>> CausalTupleMap = new TupleMap<IPredicate, List<int>>();
        public static TupleMap<IPredicate, List<int>> ThreatTupleMap = new TupleMap<IPredicate, List<int>>();
        

        public static void Reset()
        {
            CausalTupleMap = new TupleMap<IPredicate, List<int>>();
            ThreatTupleMap = new TupleMap<IPredicate, List<int>>();
        }

        public static IEnumerable<IOperator> GetCndts(IPredicate pred)
        {
            var whichMap = CausalTupleMap.Get(pred.Sign);
            if (whichMap.ContainsKey(pred))
                return from intID in whichMap[pred] select GroundActionFactory.GroundLibrary[intID];

            return new List<IOperator>();
        }

        public static IEnumerable<IOperator> GetThreats(IPredicate pred)
        {
            var whichMap = ThreatTupleMap.Get(pred.Sign);
            if (whichMap.ContainsKey(pred))
                return whichMap[pred].Select(intID => GroundActionFactory.GroundLibrary[intID]);

            return new List<IOperator>();
        }

        public static bool IsCndt(IPredicate pred, IPlanStep ps)
        {
            var whichMap = CausalTupleMap.Get(pred.Sign);

            if (!whichMap.ContainsKey(pred))
                return false;
            return whichMap[pred].Contains(ps.Action.ID);

        }

        public static bool IsThreat(IPredicate pred, IPlanStep ps)
        {
            var whichMap = ThreatTupleMap.Get(pred.Sign);
            if (!whichMap.ContainsKey(pred))
                return false;
            return whichMap[pred].Contains(ps.Action.ID);
        }

        // Checks for mappings pairwise
        public static void CacheLinks(List<IOperator> groundSteps)
        {
            foreach (var tstep in groundSteps)
            {
                foreach (var tprecond in tstep.Preconditions)
                {
                    var causemap = CausalTupleMap.Get(tprecond.Sign);
                    var threatmap = ThreatTupleMap.Get(tprecond.Sign);

                    if (causemap.ContainsKey(tprecond) || threatmap.ContainsKey(tprecond))
                    {
                        // Then this precondition has already been evaluated.
                        continue;
                    }

                    foreach (var hstep in groundSteps)
                    {
                        if (hstep.Effects.Contains(tprecond))
                        {
                            if (!causemap.ContainsKey(tprecond))
                                causemap.Add(tprecond, new List<int>() { hstep.ID });
                            else if (!causemap[tprecond].Contains(hstep.ID))
                            {
                                causemap[tprecond].Add(hstep.ID);
                            }
                        }
                        if (hstep.Effects.Contains(tprecond.GetReversed()))
                        {
                            if (!threatmap.ContainsKey(tprecond))
                                threatmap.Add(tprecond, new List<int>() { hstep.ID });
                            else
                                threatmap[tprecond].Add(hstep.ID);
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
                    var causeMap = CausalTupleMap.Get(tprecond.Sign);
                    var threatmap = ThreatTupleMap.Get(tprecond.Sign);

                    foreach (var hstep in heads)
                    {

                        if ((causeMap.ContainsKey(tprecond) && causeMap[tprecond].Contains(hstep.ID)) || (threatmap.ContainsKey(tprecond) && threatmap[tprecond].Contains(hstep.ID)))
                        {
                            // then this head step has already been checked for this condition
                            continue;
                        }

                        if (hstep.Effects.Contains(tprecond))
                        {
                            if (!causeMap.ContainsKey(tprecond))
                                causeMap.Add(tprecond, new List<int>() { hstep.ID });
                            else
                            {
                                if (!causeMap[tprecond].Contains(hstep.ID))
                                {
                                    causeMap[tprecond].Add(hstep.ID);
                                }

                            }
                        }
                        if (hstep.Effects.Contains(tprecond.GetReversed()))
                        {
                            if (!threatmap.ContainsKey(tprecond))
                                threatmap.Add(tprecond, new List<int>() { hstep.ID });
                            else
                                threatmap[tprecond].Add(hstep.ID);
                        }
                    }
                }
                
            }
        }

        public static void CacheGoalLinks(List<IOperator> groundSteps, List<IPredicate> goal)
        {

            foreach (var goalCondition in goal)
            {
                var causeMap = CausalTupleMap.Get(goalCondition.Sign);
                var threatmap = ThreatTupleMap.Get(goalCondition.Sign);

                // Hence, it's been processed already
                if (causeMap.ContainsKey(goalCondition) || threatmap.ContainsKey(goalCondition))
                {
                    continue;
                }

                foreach (var gstep in groundSteps)
                {

                    if (gstep.Effects.Contains(goalCondition))
                    {
                        if (!causeMap.ContainsKey(goalCondition))
                            causeMap.Add(goalCondition, new List<int>() { gstep.ID });
                        else
                            causeMap[goalCondition].Add(gstep.ID);
                    }

                    if (gstep.Effects.Contains(goalCondition.GetReversed()))
                    {
                        if (!threatmap.ContainsKey(goalCondition))
                            threatmap.Add(goalCondition, new List<int>() { gstep.ID });
                        else
                            threatmap[goalCondition].Add(gstep.ID);
                    }

                }
            }
        }

        private static TupleMap<IPredicate, int> RecursiveHeuristicCache(TupleMap<IPredicate, int> currentMap, List<IPredicate> InitialConditions)
        {
            // Steps that are executable given the initial conditions. These conditions can represent a state that is logically inconsistent (and (at bob store) (not (at bob store)))
            var initiallyRelevant = GroundActionFactory.GroundActions.Where(action => action.Height == 0 && action.Preconditions.All(pre => InitialConditions.Contains(pre)));

            // a boolean tag to decide whether to continue recursively. If checked, then there is some new effect that isn't in initial conditions.
            bool toContinue = false;

            // for each step whose preconditions are executable given the initial conditions
            foreach (var newStep in initiallyRelevant)
            {
                // sum_{pre in newstep.preconditions} currentMap[pre]
                var thisStepsValue = newStep.Preconditions.Sum(pre => currentMap.Get(pre.Sign)[pre]);

                foreach (var eff in newStep.Effects)
                {
                    // ignore effects we've already seen; these occur "earlier" in planning graph
                    if (currentMap.Get(eff.Sign).ContainsKey(eff))
                        continue;

                    // If we make it this far, then we've reached an unexplored literal effect
                    toContinue = true;

                    // The current value of this effect is 1 (this new step) + the sum of the preconditions of this step in the map.
                    currentMap.Get(eff.Sign)[eff] = 1 + thisStepsValue;

                    // Add this effect to the new initial Condition for subsequent round
                    InitialConditions.Add(eff);
                }
            }

            // Only continue recursively if we've explored a new literal effect. Pass the map along to maintain a global item.
            if (toContinue)
                return RecursiveHeuristicCache(currentMap, InitialConditions);

            // Otherwise, return our current map
            return currentMap;

        }

        public static void CacheAddReuseHeuristic(IState InitialState)
        {
            // Use dynamic programming
            var initialMap = new TupleMap<IPredicate, int>();
            //var initialMap = new Dictionary<IPredicate, int>();
            foreach (var item in InitialState.Predicates)
            {
                initialMap.Get(true)[item] = 0;
            }
            List<IPredicate> newInitialList = InitialState.Predicates;
            foreach (var pre in CacheMaps.CausalTupleMap.Get(false).Keys)
            {
                if (!newInitialList.Contains(pre.GetReversed()))
                {
                    newInitialList.Add(pre);
                    initialMap.Get(false)[pre] = 0;
                }
            }

            HeuristicMethods.visitedPreds = RecursiveHeuristicCache(initialMap, newInitialList);

        }

    }


}