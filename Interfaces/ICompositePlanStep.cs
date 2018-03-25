using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.Interfaces
{
    public interface ICompositePlanStep : IPlanStep
    {
        IComposite CompositeAction { get; set; }

        IPlanStep InitialStep { get; set; }
        IPlanStep GoalStep { get; set; }
        List<IPlanStep> SubSteps { get; }
        List<Tuple<IOperator, IOperator>> SubOrderings { get; }
        List<CausalLink<IOperator>> SubLinks { get; }
        new Object Clone();
    }
}
