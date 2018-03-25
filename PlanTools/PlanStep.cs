using BoltFreezer.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoltFreezer.PlanTools
{
    [Serializable]
    public class PlanStep : IPlanStep
    {
        private static int Counter = -1;
        private IOperator action;
        private List<IPredicate> openConditions;

        private int id;

        public IOperator Action
        {
            get { return action; }
            set { action = value; }
        }

        public int ID
        {
            get { return id; }
        }

        public int Height
        {
            get { return Action.Height; }
            set { Action.Height = value; }
        }

        // Access the operator's preconditions.
        public List<IPredicate> OpenConditions
        {
            get { return openConditions; }
            set { openConditions = value; }
        }

        // Inherit from IOperator
        public IPredicate Predicate { get => Action.Predicate; set => Action.Predicate=  value; }
        public string Name { get => Action.Name; set => Action.Name = value; }
        public List<ITerm> Terms { get => Action.Terms; set => Action.Terms = value; }
        public int Arity => Action.Arity;
        public List<IPredicate> Preconditions { get => Action.Preconditions; set => Action.Preconditions = value; }
        public List<IPredicate> Effects { get => Action.Effects; set => Action.Effects = value; }
        public List<IAxiom> Conditionals { get => Action.Conditionals; set => Action.Conditionals = value; }
        public List<IPredicate> ExceptionalEffects { get => Action.ExceptionalEffects; set => Action.ExceptionalEffects = value; }
        public string Actor => Action.Actor;
        public List<ITerm> ConsentingAgents { get => Action.ConsentingAgents; set => Action.ConsentingAgents = value; }

        public PlanStep()
        {
            action = new Operator();
            id = System.Threading.Interlocked.Increment(ref Counter);
            openConditions = new List<IPredicate>();
        }

        public PlanStep(IOperator groundAction)
        {
            action = groundAction;
            id = System.Threading.Interlocked.Increment(ref Counter);
            openConditions = new List<IPredicate>();
            foreach (var precondition in groundAction.Preconditions)
            {
                openConditions.Add(precondition);
            }
        }

        public PlanStep(IPlanStep planStep)
        {
            action = planStep.Action as IOperator;
            id = System.Threading.Interlocked.Increment(ref Counter);

            // Generate open conditions to fulfill
            openConditions = new List<IPredicate>();
            foreach (var precondition in planStep.OpenConditions)
            {
                openConditions.Add(precondition);
            }
        }

        public PlanStep(IOperator groundAction, List<IPredicate> ocs, int _id)
        {
            action = groundAction;
            id = _id;

            // Copy open conditions from existing plan step
            openConditions = new List<IPredicate>();
            foreach (var precondition in ocs)
            {
                openConditions.Add(precondition);
            }
        }

        public PlanStep (IOperator groundAction, int _id)
        {
            action = groundAction;
            id = _id;
            openConditions = new List<IPredicate>();
            foreach (var precondition in groundAction.Preconditions)
            {
                openConditions.Add(precondition);
            }
        }

        public PlanStep(IPlanStep planStep, int _id)
        {
            action = planStep.Action;
            id = _id;
            openConditions = new List<IPredicate>();
            foreach (var precondition in planStep.Preconditions)
            {
                openConditions.Add(precondition);
            }
        }

        // Removes an open condition
        public void Fulfill(IPredicate condition)
        {
            if (!action.Preconditions.Contains(condition))
            {
                throw new System.Exception();
            }

            if (!OpenConditions.Contains(condition))
            {
                throw new System.Exception();
            }

            OpenConditions.Remove(condition);
        }

        // A special method for displaying fully ground steps.
        public override string ToString()
        {
            return Action.ToString();
        }

        // Checks if two operators are equal.
        public override bool Equals(Object obj)
        {
            // Store the object as a state space action.
            PlanStep step = obj as PlanStep;

            if (step.ID == ID)
            {
                return true;
            }
            //if (step.Action.ID == Action.ID)
            //{
            //    return true;
            //}

            return false;
        }

        // Returns a hashcode.
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + Name.GetHashCode();

                foreach (ITerm term in Terms)
                    if (term.Bound)
                        hash = hash * 23 + term.Constant.GetHashCode();
                    else
                        hash = hash * 23 + term.Variable.GetHashCode();

                hash = hash * 23 + ID.GetHashCode();

                return hash;
            }
        }

        // the clone doesn't need to mutate the underlying action (Action)
        public Object Clone()
        {
            return new PlanStep(Action, OpenConditions, ID);
        }

        public string TermAt(int position)
        {
            return Action.TermAt(position);
        }

        public object Template()
        {
            return Action.Template();
        }
    }
}
