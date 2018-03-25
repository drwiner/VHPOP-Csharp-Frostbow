using BoltFreezer.Interfaces;
using BoltFreezer.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.PlanTools
{
    public class Composite : Operator, IComposite
    {
        private IOperator initialStep;
        private IOperator goalStep;
        private List<Tuple<IOperator, IOperator>> subOrderings;
        private List<CausalLink<IOperator>> subLinks;
        private List<IPlanStep> subSteps;

        public IOperator InitialStep {
            get { return initialStep; }
            set { initialStep = value; }
        }

        public IOperator GoalStep
        {
            get { return goalStep; }
            set { goalStep = value; }
        }

        public List<IPlanStep> SubSteps
        {
            get { return subSteps;  }
            set { subSteps = value; }
        }

        public List<Tuple<IOperator,IOperator>> SubOrderings
        {
            get { return subOrderings; }
            set { subOrderings = value; }
        }

        public List<CausalLink<IOperator>> SubLinks
        {
            get { return subLinks; }
            set { subLinks = value; }
        }

        public Composite() : base()
        {
            subOrderings = new List<Tuple<IOperator, IOperator>>();
            subLinks = new List<CausalLink<IOperator>>();
            subSteps = new List<IPlanStep>();
            initialStep = new Operator();
            goalStep = new Operator();
        }

        public Composite(string name, List<ITerm> terms, IOperator init, IOperator dummy, List<IPredicate> Preconditions, List<IPredicate> Effects, int ID) 
            : base(name, terms, new Hashtable(), Preconditions, Effects, ID)
        {
            subOrderings = new List<Tuple<IOperator, IOperator>>();
            subLinks = new List<CausalLink<IOperator>>();
            subSteps = new List<IPlanStep>();
            initialStep = new Operator();
            goalStep = new Operator();
        }

        public Composite(IOperator core, IOperator init, IOperator goal, List<IPlanStep> substeps, List<Tuple<IOperator, IOperator>> suborderings, List<CausalLink<IOperator>> sublinks)
            : base(core.Name, core.Terms, new Hashtable(), core.Preconditions, core.Effects, core.ID)
        {
            subOrderings = suborderings;
            subLinks = sublinks;
            subSteps = substeps;
            initialStep = init;
            goalStep = goal;
            Height = core.Height;
        }

        public new Object Clone()
        {
            return new Composite(this as IOperator, InitialStep, GoalStep, SubSteps, SubOrderings, SubLinks);
        }
    }
}
