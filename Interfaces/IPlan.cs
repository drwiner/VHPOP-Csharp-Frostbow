using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using BoltFreezer.PlanTools;

namespace BoltFreezer.Interfaces
{
    public interface IPlan
    {

        // Plans have an ordered list of steps.
        List<IPlanStep> Steps { get; set; }

        // Ordering Graph
        Graph<IPlanStep> Orderings { get; set; }

        // CausalLinkGraph
        List<CausalLink<IPlanStep>> CausalLinks { get; set; }

        // The plan will have an initial state.
        IState Initial { get; set; }

        IPlanStep InitialStep { get; }

        // The plan will have a goal state.
        IState Goal { get; set; }

        IPlanStep GoalStep { get; }

        // Keep track of flaws in each plan.
        Flawque Flaws { get; set; }

        // Insert step
        void Insert(IPlanStep newStep);

        // Repair Condition; also checks if this new causal link is threatened
        void Repair(OpenCondition oc, IPlanStep repairStep);

        // Detect threats for this specific step
        void DetectThreats(IPlanStep newStep);

        IPlanStep Find(IPlanStep stepClonedFromOpenCondition);

        // The plan can be cloned.
        Object Clone();

        string ToStringOrdered();
    }
}
