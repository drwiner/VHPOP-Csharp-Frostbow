
using BoltFreezer.Enums;
using BoltFreezer.Interfaces;
using BoltFreezer.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace BoltFreezer.PlanTools
{


    public class AddReuseHeuristic : IHeuristic
    {
        public new string ToString()
        {
            return HType.ToString();
        }

        public HeuristicType HType
        {
            get { return HeuristicType.AddReuseHeuristic; }
        }

        public float Heuristic(IPlan plan)
        {
            return HeuristicMethods.AddReuseHeuristic(plan);
        }

    }

    public class NumOpenConditionsHeuristic : IHeuristic
    {
        public HeuristicType HType
        {
            get { return HeuristicType.NumOCsHeuristic; }
        }

        public new string ToString()
        {
            return HType.ToString();
        }

        public float Heuristic(IPlan plan)
        {
            return HeuristicMethods.NumOCs(plan);
        }
    }

    public class ZeroHeuristic : IHeuristic
    {
        public new string ToString()
        {
            return HType.ToString();
        }

        public HeuristicType HType
        {
            get { return HeuristicType.ZeroHeuristic; }
        }

        public float Heuristic(IPlan plan)
        {
            return 0f;
        }
    }


    public static class HeuristicMethods
    {
        // These may be stored in preprocessing step
        public static TupleMap<IPredicate, int> visitedPreds = new TupleMap<IPredicate, int>();

        // h^r_add(pi) = sum_(oc in plan) 0 if exists a step possibly preceding oc.step and h_add(oc.precondition) otherwise.
        public static int AddReuseHeuristic(IPlan plan)
        {
           // Logger.LogThingToType("AddReuse", " ", "HeuristicNew");
            //Logger.LogThingToType("AddReuse", " ", "HeuristicOld");

            var tuplemapping = new TupleMap<IPredicate, List<IPlanStep>>();
            // var posmapping = new Dictionary<IPredicate, List<int>>();
            //var negmapping = new Dictionary<IPredicate, List<int>>();
            var effList = new List<IPredicate>();
            int sumo = 0;

            if (plan.Flaws.OpenConditions.Count > plan.Steps.Count)
            {
                //var beforePreamble = Logger.Log();
                foreach (var existingStep in plan.Steps)
                {
                    if (existingStep.Height > 0)
                    {
                        continue;
                    }

                    foreach (var eff in existingStep.Effects)
                    {
                        effList.Add(eff);
                        if (!tuplemapping.Get(eff.Sign).ContainsKey(eff))
                        {
                            tuplemapping.Get(eff.Sign)[eff] = new List<IPlanStep>() { existingStep };
                        }
                        else
                        {
                            tuplemapping.Get(eff.Sign)[eff].Add(existingStep);
                        }
                    }
                }

                //Logger.LogTimeToType("collectStepsPreamble", Logger.Log() - beforePreamble, "HeuristicNew");

                foreach (var oc in plan.Flaws.OpenConditions)
                {
                    //var beforeEachOc = Logger.Log();
                    var existsA = false;
                    if (effList.Contains(oc.precondition))
                    {

                        foreach (var action in tuplemapping.Get(oc.precondition.Sign)[oc.precondition])
                        {
                            if (plan.Orderings.IsPath(oc.step, action))
                                continue;

                            existsA = true;
                            break;
                        }
                    }
                    if (!existsA)
                    {
                        // we should always have the conditions in the visitedPreds dictionary if we processed correctly
                        if (visitedPreds.Get(oc.precondition.Sign).ContainsKey(oc.precondition))
                        {
                            sumo += visitedPreds.Get(oc.precondition.Sign)[oc.precondition];
                            continue;
                        }
                        // Fail gracefully
                        sumo += 100000;
                        //throw new System.Exception("visitedPreds does not contain " + oc.precondition.ToString());
                    }
                    //Logger.LogTimeToType("EachOC", Logger.Log() - beforeEachOc, "HeuristicNew");
                }

                return sumo;
            }


            // we are just taking the sum of the visitedPreds values of the open conditions, unless there is a step that establishes the condition already in plan (reuse).

            foreach (var oc in plan.Flaws.OpenConditions)
            {

               // var wayBefore = Logger.Log();
                // Does there exist a step in the plan that can establish this needed precondition?
                var existsA = false;
                foreach (var existingStep in plan.Steps)
                {

                    if (existingStep.Height > 0)
                        continue;

                    //var before = Logger.Log();
                    if (CacheMaps.IsCndt(oc.precondition, existingStep))
                    {
                        existsA = true;
                        break;
                    }
                    //L//ogger.LogTimeToType("IsCndt", Logger.Log() - before, "HeuristicOld");

                    if (plan.Orderings.IsPath(oc.step, existingStep))
                        continue;
                }

                // append heuristic for open condition
                if (!existsA)
                {
                    // we should always have the conditions in the visitedPreds dictionary if we processed correctly
                    if (visitedPreds.Get(oc.precondition.Sign).ContainsKey(oc.precondition))
                    {
                        sumo += visitedPreds.Get(oc.precondition.Sign)[oc.precondition];
                        continue;
                    }
                    sumo += 100000;
                    //throw new System.Exception("visitedPreds does not contain " + oc.precondition.ToString());
                }

                //Logger.LogTimeToType("EachOC", Logger.Log() - wayBefore, "HeuristicOld");
            }
            return sumo;
        }

        // Number of open conditions heuristic
        public static int NumOCs(IPlan plan)
        {
            return plan.Flaws.OpenConditions.Count;
        }
    }
}