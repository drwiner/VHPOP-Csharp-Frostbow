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
        private IFrontier frontier;
        //private Func<IPlan, float> heuristic;
        //private IHeuristic heuristic;
        private ISelection selection;
        //private SearchType search;
        private ISearch search;
        //private Func<IPlanner, int, float, List<IPlan>> search;
        private bool console_log;
        private int opened, expanded = 0;
        public int problemNumber;
        public string directory;

        // TODO: keep track of plan-space search tree and not just frontier
        //private List<PlanSpaceEdge> PlanSpaceGraph;

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

        public IFrontier Frontier
        {
            get { return frontier; }
        }

        public PlanSpacePlanner(IPlan initialPlan, ISelection _selection, ISearch _search, bool consoleLog)
        {
            console_log = consoleLog;
            selection = _selection;
            search = _search;
            frontier = new PriorityQueue();
            Insert(initialPlan);
        }

        public PlanSpacePlanner(IPlan initialPlan)
        {
            console_log = false;
            selection = new E0(new AddReuseHeuristic());
            frontier = new PriorityQueue();
            Insert(initialPlan);
        }

        public static IPlan CreateInitialPlan(ProblemFreezer PF)
        {
            var initialPlan = new Plan(new State(PF.testProblem.Initial) as IState, new State(PF.testProblem.Goal) as IState);
            foreach (var goal in PF.testProblem.Goal)
                initialPlan.Flaws.Insert(initialPlan, new OpenCondition(goal, initialPlan.GoalStep as IPlanStep));
            initialPlan.Orderings.Insert(initialPlan.InitialStep, initialPlan.GoalStep);
            return initialPlan;
        }

        public void Insert(IPlan plan)
        {
            if (!plan.Orderings.HasCycle())
            {
                frontier.Enqueue(plan, Score(plan));
                opened++;
            }
            else
                Console.WriteLine("CHeck");
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
                if (cndt == null)
                    continue;
                // only possible for reading in python json
                if (cndt.ID == plan.InitialStep.Action.ID)
                    continue;
                // same with above: cannot insert a dummy step. These will get inserted when composite step is inserted.
                if (cndt.Name.Split(':')[0].Equals("begin") || cndt.Name.Split(':')[0].Equals("finish"))
                    continue;
                //if (cndt.Height > 0)
                //    continue;

                var planClone = plan.Clone() as IPlan;
                IPlanStep newStep;
                if (cndt.Height > 0)
                {
                    newStep = new CompositePlanStep(cndt.Clone() as IComposite);
                }
                else
                {
                    newStep = new PlanStep(cndt.Clone() as IOperator);
                }
                //newStep.Height = cndt.Height;
                planClone.Insert(newStep);
                planClone.Repair(oc, newStep);

                // check if inserting new Step (with orderings given by Repair) add cndts/risks to existing open conditions, affecting their status in the heap
                //planClone.Flaws.UpdateFlaws(planClone, newStep);
                planClone.DetectThreats(newStep);
                Insert(planClone);
            }
        }

        public void Reuse(IPlan plan, OpenCondition oc)
        {
            // if repaired by initial state
            if (plan.Initial.InState(oc.precondition))
            {
                var planClone = plan.Clone() as IPlan;
                planClone.Repair(oc, planClone.InitialStep);
                Insert(planClone);
                
            }

            if (oc.step.Name.Split(':')[0].Equals("finish"))
            {
                Console.WriteLine("");
            }

            foreach (var step in plan.Steps)
            {
                if (oc.step.ID == step.ID)
                {
                    continue;
                }
                if (CacheMaps.IsCndt(oc.precondition, step)){
                    // before adding a repair, check if there is a path.
                    if (plan.Orderings.IsPath(oc.step, step))
                        continue;
                    
                    var planClone = plan.Clone() as IPlan;
                    planClone.Repair(oc, step);
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
            var decomps = plan.Decomps;
            var namedData = new List<Tuple<string, string>>
                        {
                            new Tuple<string, string>("problem", problemNumber.ToString()),
                            new Tuple<string, string>("selection", selection.ToString()),
                            //new Tuple<string, string>("heuristic", heuristic.ToString()),
                            new Tuple<string, string>("search", search.ToString()),
                            new Tuple<string,string>("runtime", elapsedMs.ToString()),
                            new Tuple<string, string>("opened", opened.ToString()),
                            new Tuple<string, string>("expanded", expanded.ToString()),
                            new Tuple<string, string>("primitives", primitives.ToString() ),
                            new Tuple<string, string>("decomps", decomps.ToString() ),
                            new Tuple<string, string>("composites", composites.ToString() ),
                            new Tuple<string, string>("hdepth", plan.Hdepth.ToString() )
                        };

            var file = directory + problemNumber.ToString() + "-" + search.ToString() + "-" + selection.ToString() + ".txt";
            using (StreamWriter writer = new StreamWriter(file, false))
            {
                foreach (Tuple<string, string> dataItem in namedData)
                {
                    writer.WriteLine(dataItem.First + "\t" + dataItem.Second);
                }
            }
        }


    }
}
