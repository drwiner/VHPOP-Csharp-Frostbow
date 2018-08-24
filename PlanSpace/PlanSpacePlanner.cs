using BoltFreezer.CacheTools;
using BoltFreezer.Enums;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.IO;

namespace BoltFreezer.PlanSpace
{
    public class PlanSpacePlanner : IPlanner
    {
        private ISelection selection;
        private ISearch search;

        private bool console_log;
        private int opened, expanded = 0;
        public int problemNumber;
        public string directory;

        public bool Console_log
        {
            get { return console_log; }
            set { console_log = value; }
        }

        public int Expanded
        {
            get { return expanded; }
            set { expanded = value; }
        }

        public int Open
        {
            get { return opened; }
            set { opened = value; }
        }

        public ISearch Search
        {
            get { return search; }
        }

        public PlanSpacePlanner(IPlan initialPlan, ISelection _selection, ISearch _search, bool consoleLog)
        {
            console_log = consoleLog;
            selection = _selection;
            search = _search;
            Insert(initialPlan);
        }

        public PlanSpacePlanner(IPlan initialPlan)
        {
            console_log = false;
            selection = new E0(new AddReuseHeuristic());
            search = new ADstar();
            Insert(initialPlan);
        }

        public static IPlan CreateInitialPlan(Problem problem)
        {
            var initialPlan = new Plan(new State(problem.Initial) as IState, new State(problem.Goal) as IState);
            foreach (var goal in problem.Goal)
                initialPlan.Flaws.Add(initialPlan, new OpenCondition(goal, initialPlan.GoalStep as IPlanStep));
            initialPlan.Orderings.Insert(initialPlan.InitialStep, initialPlan.GoalStep);
            return initialPlan;
        }

        public static IPlan CreateInitialPlan(List<IPredicate> Initial, List<IPredicate> Goal)
        {
            var initialPlan = new Plan(new State(Initial) as IState, new State(Goal) as IState);
            foreach (var goal in Goal)
                initialPlan.Flaws.Add(initialPlan, new OpenCondition(goal, initialPlan.GoalStep as IPlanStep));
            initialPlan.Orderings.Insert(initialPlan.InitialStep, initialPlan.GoalStep);
            return initialPlan;
        }

        public static IPlan CreateInitialPlan(ProblemFreezer PF)
        {
            var initialPlan = new Plan(new State(PF.testProblem.Initial) as IState, new State(PF.testProblem.Goal) as IState);
            foreach (var goal in PF.testProblem.Goal)
                initialPlan.Flaws.Add(initialPlan, new OpenCondition(goal, initialPlan.GoalStep as IPlanStep));
            initialPlan.Orderings.Insert(initialPlan.InitialStep, initialPlan.GoalStep);
            return initialPlan;
        }

        public void Insert(IPlan plan)
        {
            if (!plan.Orderings.HasCycle())
            {
                var score = Score(plan);
                if (score > 600)
                {
                    Console.WriteLine(score);
                    // reduce size of frontier
                    return;
                }
                search.Frontier.Enqueue(plan, score);
                opened++;
            }
            else
            {
                plan = null;
            }
        }

        public float Score(IPlan plan)
        {
            return selection.Evaluate(plan);
        }

        public List<IPlan> Solve(int k, float cutoff)
        {
            return search.Search(this, k, cutoff);
        }

        public void AddStep(IPlan plan, OpenCondition oc)
        {
                
            foreach(var cndt in CacheMaps.GetCndts(oc.precondition))
            {
                var planClone = plan.Clone() as IPlan;
                var newStep = new PlanStep(cndt.Clone() as IOperator);

                planClone.Insert(newStep);
                planClone.Repair(oc, newStep);

                // Check if inserting new Step (with orderings given by Repair) add cndts/risks to existing open conditions, affecting their status in the heap
                //planClone.Flaws.UpdateFlaws(planClone, newStep);

                // Detect if this new step threatens existing causal links
                planClone.DetectThreats(newStep);

                Insert(planClone);
            }
        }

        public void Reuse(IPlan plan, OpenCondition oc)
        {
            // If repaired by initial state
            if (plan.Initial.InState(oc.precondition))
            {
                var planClone = plan.Clone() as IPlan;
                planClone.Repair(oc, planClone.InitialStep);
                Insert(planClone);
                
            }

            // For each existing step, check if it is a candidate for repair
            foreach (var step in plan.Steps)
            {
                if (oc.step.ID == step.ID)
                {
                    continue;
                }

                if (CacheMaps.IsCndt(oc.precondition, step)){

                    // Before adding a repair, check if there is a path.
                    if (plan.Orderings.IsPath(oc.step, step))
                        continue;
                    
                    // Create child plan-space node
                    var planClone = plan.Clone() as IPlan;

                    // Make repair, check if new causal link is threatened
                    planClone.Repair(oc, step);

                    // Insert plan as child on search frontier
                    Insert(planClone);
                }
            }
        }

        public void RepairThreat(IPlan plan, ThreatenedLinkFlaw tclf)
        {
            
            var cl = tclf.causallink;
            var threat = tclf.threatener;

            // Promote
            if (!plan.Orderings.IsPath(threat, cl.Tail))
            {
                var promote = plan.Clone() as IPlan;
                promote.Orderings.Insert(cl.Tail, threat);
                Insert(promote);
            }

            // Demote
            if (!plan.Orderings.IsPath(cl.Head, threat))
            {
                var demote = plan.Clone() as IPlan;
                demote.Orderings.Insert(threat, cl.Head);
                Insert(demote);
            }

        }

        public void WriteToFile(long elapsedMs, Plan plan) {
            var primitives = plan.Steps.FindAll(step => step.Height == 0).Count;
            var composites = plan.Steps.FindAll(step => step.Height > 0).Count;
            var namedData = new List<Tuple<string, string>>
                        {
                            new Tuple<string, string>("problem", problemNumber.ToString()),
                            new Tuple<string, string>("selection", selection.ToString()),
                            new Tuple<string, string>("search", search.ToString()),
                            new Tuple<string,string>("runtime", elapsedMs.ToString()),
                            new Tuple<string, string>("opened", opened.ToString()),
                            new Tuple<string, string>("expanded", expanded.ToString()),
                            new Tuple<string, string>("primitives", primitives.ToString() ),
                        };

            var file = directory + problemNumber.ToString() + "-" + search.ToString() + "-" + selection.ToString() + ".txt";
            using (StreamWriter writer = new StreamWriter(file, false))
            {
                foreach (Tuple<string, string> dataItem in namedData)
                {
                    writer.WriteLine(dataItem.First + "\t" + dataItem.Second);
                }
                writer.WriteLine("\n");
                writer.WriteLine(plan.ToStringOrdered());
                writer.WriteLine("\nOrderings:\n");
                foreach (var ord in plan.Orderings.edges)
                {
                    writer.WriteLine(ord.ToString());
                }
            }
        }


    }
}
