using BoltFreezer.Interfaces;
using BoltFreezer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.PlanTools
{
    public class CompositePlanStep : PlanStep, ICompositePlanStep
    {
        private IComposite compositeAction;
        private IPlanStep initialStep;
        private IPlanStep goalStep;

        public IComposite CompositeAction {
            get { return compositeAction; }
            set { compositeAction = value; }
        }

        public List<IPlanStep> SubSteps
        {
            get { return compositeAction.SubSteps; }
        }

        public List<Tuple<IOperator,IOperator>> SubOrderings
        {
            get { return compositeAction.SubOrderings; }
        }

        public List<CausalLink<IOperator>> SubLinks
        {
            get { return compositeAction.SubLinks; }
        }

        public IPlanStep InitialStep
        {
            get { return initialStep; }
            set { initialStep = value; }
        }

        public IPlanStep GoalStep
        {
            get { return goalStep; }
            set { goalStep = value; }
        }

        public CompositePlanStep()
        {
            compositeAction = new Composite();
        }

        public CompositePlanStep(IComposite comp) : base(comp as IOperator)
        {
            compositeAction = comp;
            initialStep = new PlanStep(comp.InitialStep);
            goalStep = new PlanStep(comp.GoalStep);
        }

        public CompositePlanStep(ICompositePlanStep comp) : base(comp as IPlanStep)
        {
            compositeAction = comp.Action as IComposite;
            initialStep = new PlanStep(comp.InitialStep);
            goalStep = new PlanStep(comp.GoalStep);
        }

        public CompositePlanStep(IComposite comp, List<IPredicate> openconditions, IPlanStep init, IPlanStep goal, int ID) : base(comp as IOperator, openconditions, ID)
        {
            compositeAction = comp;
            initialStep = init;
            goalStep = goal;
        }

        public CompositePlanStep(ICompositePlanStep comp, List<IPredicate> openconditions, IPlanStep init, IPlanStep goal, int ID) : base(comp as IPlanStep, openconditions, ID)
        {
            compositeAction = comp.Action as IComposite;
            initialStep = init;
            goalStep = goal;
        }

        public new Object Clone()
        {
            return new CompositePlanStep(CompositeAction, OpenConditions, InitialStep, GoalStep, ID)
            {
                Depth = base.Depth
            };
        }

    }
}
