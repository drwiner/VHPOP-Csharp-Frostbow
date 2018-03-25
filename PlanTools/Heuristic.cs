
using BoltFreezer.Enums;
using BoltFreezer.Interfaces;
using System.Collections.Generic;

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

        private static Dictionary<IOperator, int> visitedOps = new Dictionary<IOperator, int>();
        private static Dictionary<IPredicate, int> visitedPreds = new Dictionary<IPredicate, int>();

        private static List<IPredicate> currentlyEvaluatedPreds;

        // h^r_add(pi) = sum_(oc in plan) 0 if exists a step possibly preceding oc.step and h_add(oc.precondition) otherwise.
        public static int AddReuseHeuristic(IPlan plan)
        {

            int sumo = 0;
            foreach (var oc in plan.Flaws.OpenConditions)
            {
                
                // Refresh to new list
                currentlyEvaluatedPreds = new List<IPredicate>();

                if (visitedPreds.ContainsKey(oc.precondition))
                {
                    sumo += visitedPreds[oc.precondition];
                    continue;
                }

                // Does there exist a step in the plan that can establish this needed precondition?
                var existsA = false;
                foreach (var existingStep in plan.Steps)
                {
                    if (plan.Orderings.IsPath(oc.step, existingStep))
                        continue;

                    if (CacheMaps.IsCndt(oc.precondition, existingStep))
                    {
                        existsA = true;
                        break;
                    }
                }

                // append heuristic for open condition
                if (!existsA)
                {
                    currentlyEvaluatedPreds.Add(oc.precondition);
                    sumo += AddHeuristic(plan.Initial, oc.precondition);
                }
            }
            return sumo;
        }

        // h_add(q) = 0 if holds initially, min a in GA, and infinite otherwise
        public static int AddHeuristic(IState initial, IPredicate condition)
        {
            if (initial.InState(condition))
                return 0;

            // if we have a value for this, return it.
            if (visitedPreds.ContainsKey(condition))
            {
                return visitedPreds[condition];
            }

            int minSoFar = 1000;
            // Then this is a static condition that can never be true... we should avoid this plan.
            if (!CacheMaps.CausalMap.ContainsKey(condition))
            {
                return minSoFar;
            }

            // find the gorund action that minimizes the heuristic estimate
            foreach (var groundAction in CacheMaps.GetCndts(condition))
            {
                // same with above: cannot insert a dummy step. These will get inserted when composite step is inserted.
                if (groundAction.Name.Split(':')[0].Equals("begin") || groundAction.Name.Split(':')[0].Equals("finish"))
                    continue;

                int thisVal;
                if (visitedOps.ContainsKey(groundAction))
                {
                    thisVal = visitedOps[groundAction];
                }
                else
                {
                    thisVal = AddHeuristic(initial, groundAction);
                }


                if (thisVal < minSoFar)
                {
                    minSoFar = thisVal;
                }
            }

            visitedPreds[condition] = minSoFar;
            return minSoFar;
        }

        // h_add(a) = 1 + h_add (Prec(a))
        public static int AddHeuristic(IState initial, IOperator op)
        {
            if (visitedOps.ContainsKey(op))
            {
                return visitedOps[op];
            }

            int sumo = 1;
            foreach (var precond in op.Preconditions)
            {
                if (currentlyEvaluatedPreds.Contains(precond))
                {
                    continue;
                }

                currentlyEvaluatedPreds.Add(precond);
                sumo += AddHeuristic(initial, precond);
            }
            visitedOps[op] = sumo;
            return sumo;
        }

        // Number of open conditions heuristic
        public static int NumOCs(IPlan plan)
        {
            return plan.Flaws.OpenConditions.Count;
        }
    }
}