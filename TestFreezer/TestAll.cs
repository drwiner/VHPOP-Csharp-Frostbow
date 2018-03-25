using System;
using BoltFreezer.FileIO;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanSpace;
using BoltFreezer.PlanTools;

namespace TestFreezer
{
    class TestAll
    {

        public static void RunPlanner(IPlan initPi, ISearch SearchMethod, ISelection SelectMethod, int k, float cutoff, string directoryToSaveTo, int problem)
        {
            var POP = new PlanSpacePlanner(initPi, SelectMethod, SearchMethod, true)
            {
                directory = directoryToSaveTo,
                problemNumber = problem,
            };
            var Solutions = POP.Solve(k, cutoff);
        }

        static void Main(string[] args)
        {

            Console.Write("hello world\n");
            var directory = @"D:\Documents\workspace\travel_domain.travel\ICAPS\";
            var cutoff = 6000f;
            var k = 1;
            for (int i = 1; i < 9; i++)
            {
                var initPlan = JsonProblemDeserializer.DeserializeJsonTravelDomain(i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E0(new AddReuseHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E1(new AddReuseHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E2(new AddReuseHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E3(new AddReuseHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E4(new AddReuseHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E5(new AddReuseHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E6(new AddReuseHeuristic()), k, cutoff, directory, i);

                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E0(new NumOpenConditionsHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E1(new NumOpenConditionsHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E2(new NumOpenConditionsHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E3(new NumOpenConditionsHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E4(new NumOpenConditionsHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E5(new NumOpenConditionsHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E6(new NumOpenConditionsHeuristic()), k, cutoff, directory, i);

                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E0(new ZeroHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E1(new ZeroHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E2(new ZeroHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E3(new ZeroHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E4(new ZeroHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E5(new ZeroHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E6(new ZeroHeuristic()), k, cutoff, directory, i);


                RunPlanner(initPlan.Clone() as IPlan, new DFS(), new Nada(new ZeroHeuristic()), k, cutoff, directory, i);
                RunPlanner(initPlan.Clone() as IPlan, new BFS(), new Nada(new ZeroHeuristic()), k, cutoff, directory, i);
            }
        }
    }
}
