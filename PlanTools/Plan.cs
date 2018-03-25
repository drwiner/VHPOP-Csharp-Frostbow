using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;

using BoltFreezer.Interfaces;

namespace BoltFreezer.PlanTools
{
    [Serializable]
    public class Plan : IPlan
    {
        private List<IPlanStep> steps;
        private IState initial;
        private IState goal;
        private IPlanStep initialStep = null;
        private IPlanStep goalStep = null;

        private Graph<IPlanStep> orderings;
        private List<CausalLink<IPlanStep>> causalLinks;
        private Flawque flaws;

        // Access the plan's steps.
        public List<IPlanStep> Steps
        {
            get { return steps; }
            set { steps = value; }
        }

        // Access the plan's initial state.
        public IState Initial
        {
            get { return initial; }
            set { initial = value; }
        }

        // Access the plan's goal state.
        public IState Goal
        {
            get { return goal; }
            set { goal = value; }
        }

        // Access to plan's flaw library
        public Flawque Flaws
        {
            get { return flaws; }
            set { Flaws = value;  }
        }

        // Access the plan's initial step.
        public IPlanStep InitialStep
        {
            get { return initialStep; }
            set { initialStep = value; }
        }

        // Access the plan's goal step.
        public IPlanStep GoalStep
        {
            get { return goalStep; }
            set { goalStep = value; }
        }

        // Access to plan's ordering graph
        public Graph<IPlanStep> Orderings
        {
            get { return orderings; }
            set   { throw new NotImplementedException(); }
        }

        // Access to plan's causal links
        public List<CausalLink<IPlanStep>> CausalLinks
        {
            get { return causalLinks; }
            set { causalLinks = value; }
        }

        public Plan ()
        {
            // S
            steps = new List<IPlanStep>();
            // O
            orderings = new Graph<IPlanStep>();
            // L
            causalLinks = new List<CausalLink<IPlanStep>>();
            
            flaws = new Flawque();
            initial = new State();
            goal = new State();
            initialStep = new PlanStep(new Operator("initial", new List<IPredicate>(), initial.Predicates));
            goalStep = new PlanStep(new Operator("goal", goal.Predicates, new List<IPredicate>()));
        }

        public Plan(IState _initial, IState _goal)
        {
            steps = new List<IPlanStep>();
            causalLinks = new List<CausalLink<IPlanStep>>();
            orderings = new Graph<IPlanStep>();
            flaws = new Flawque();
            initial = _initial;
            goal = _goal;
            initialStep = new PlanStep(new Operator("initial", new List<IPredicate>(), initial.Predicates));
            goalStep = new PlanStep(new Operator("goal", goal.Predicates, new List<IPredicate>()));
        }

        public Plan(Operator _initial, Operator _goal)
        {
            steps = new List<IPlanStep>();
            causalLinks = new List<CausalLink<IPlanStep>>();
            orderings = new Graph<IPlanStep>();
            flaws = new Flawque();
            initial = new State(_initial.Effects);
            goal = new State(_goal.Preconditions);
            initialStep = new PlanStep(_initial);
            goalStep = new PlanStep(_goal);
        }

        // Used when cloning a plan: <S, O, L>, F
        public Plan(List<IPlanStep> steps, IState initial, IState goal, IPlanStep initialStep, IPlanStep goalStep, Graph<IPlanStep> orderings, List<CausalLink<IPlanStep>> causalLinks, Flawque flaws)
        {
            this.steps = steps;
            this.causalLinks = causalLinks;
            this.orderings = orderings;
            this.flaws = flaws;
            this.initial = initial;
            this.goal = goal;
            this.initialStep = initialStep;
            this.goalStep = goalStep;
        }

        public void Insert(IPlanStep newStep)
        {
            steps.Add(newStep);
            orderings.Insert(InitialStep, newStep);
            orderings.Insert(newStep, GoalStep);

            // Add new flaws
            foreach (var pre in newStep.OpenConditions)
            {
                Flaws.Add(this, new OpenCondition(pre, newStep));
            }
        }

        public IPlanStep Find(IPlanStep stepClonedFromOpenCondition)
        {
            if (GoalStep.Equals(stepClonedFromOpenCondition))
                return GoalStep;

            // For now, this condition is impossible
            if (InitialStep.Equals(stepClonedFromOpenCondition))
                return InitialStep;

            if (!Steps.Contains(stepClonedFromOpenCondition))
            {
                throw new System.Exception();
            }
            return Steps.Single(s => s.ID == stepClonedFromOpenCondition.ID);
        }

        public void DetectThreats(IPlanStep possibleThreat)
        {
            foreach (var clink in causalLinks)
            {
                // Let it be for now that a newly inserted step cannot already be in a causal link in the plan (a head or tail). If not true, then check first.
                if (!CacheMaps.IsThreat(clink.Predicate, possibleThreat))
                {
                    continue;
                }
                // new step can possibly threaten 
                if (Orderings.IsPath(clink.Tail as IPlanStep, possibleThreat))
                {
                    continue;
                }
                if (Orderings.IsPath(possibleThreat, clink.Head as IPlanStep))
                {
                    continue;
                }
                
                Flaws.Add(new ThreatenedLinkFlaw(clink, possibleThreat));
            }
        }


        public void Repair(OpenCondition oc, IPlanStep repairStep)
        {
            // oc = <needStep, needPrecond>. Need to find needStep in plan, because open conditions have been mutated before arrival.
            var needStep = Find(oc.step);
            needStep.Fulfill(oc.precondition);

            orderings.Insert(repairStep, needStep);
            var clink = new CausalLink<IPlanStep>(oc.precondition as Predicate, repairStep, needStep);
            causalLinks.Add(clink);

            foreach (var step in Steps)
            {
                if (step.ID == repairStep.ID || step.ID == needStep.ID)
                {
                    continue;
                }
                if (!CacheMaps.IsThreat(oc.precondition, step))
                {
                    continue;
                }
                // step is a threat to need precondition
                if (Orderings.IsPath(needStep, step))
                {
                    continue;
                }
                if (Orderings.IsPath(step, repairStep))
                {
                    continue;
                }
                Flaws.Add(new ThreatenedLinkFlaw(clink, step));
            }
        }


        // Return the first state of the plan.
        public State GetFirstState ()
        {
            return (State)Initial.Clone();
        }

        public List<IPlanStep> TopoSort()
        {
            List<IPlanStep> sortedList = new List<IPlanStep>();

            var topoSort = Orderings.TopoSort(InitialStep);

            foreach (var item in topoSort)
            {
                if (item.Equals(InitialStep) || item.Equals(GoalStep))
                    continue;

                sortedList.Add(item);
            }

            return sortedList;

        }

        // Displays the contents of the plan.
        public override string ToString ()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var step in steps)
                sb.AppendLine(step.ToString());

            return sb.ToString();
        }

        // Displays the contents of the plan. THIS IS BROKEN
        public string ToStringOrdered ()
        {
            StringBuilder sb = new StringBuilder();

            var topoSort = TopoSort();
            foreach (var step in topoSort)
                sb.AppendLine(step.ToString());

            return sb.ToString();
        }

        // Creates a clone of the plan. (orderings, and Links are Read-only, so only their host containers are replaced)
        public Object Clone ()
        {
            var newSteps = new List<IPlanStep>();

            foreach (var step in steps)
            {
                // need clone because these have fulfilled conditions that are mutable.
                newSteps.Add(step.Clone() as IPlanStep);
            }

            var newInitialStep = initialStep.Clone() as IPlanStep;
            // need clone of goal step because this as fulfillable conditions
            var newGoalStep = goalStep.Clone() as IPlanStep;

            // Assuming for now that members of the ordering graph are never mutated.  If they are, then a clone will keep references to mutated members
            var newOrderings = orderings.Clone() as Graph<IPlanStep>;

            // Causal Links are containers whose members are not mutated.
            List<CausalLink<IPlanStep>> newLinks = new List<CausalLink<IPlanStep>>();
            foreach (var cl in causalLinks)
            {
                newLinks.Add(cl as CausalLink<IPlanStep>);
            }

            // Inherit all flaws, must clone very flaw
            var flawList = flaws.Clone() as Flawque;

            //return new Plan(newSteps, newInitial, newGoal, newInitialStep, newGoalStep, newOrderings, newLinks, flawList);
            return new Plan(newSteps, Initial, Goal, newInitialStep, newGoalStep, newOrderings, newLinks, flawList);
        }
    }
}