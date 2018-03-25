using System;
using BoltFreezer.CacheTools;
using BoltFreezer.Enums;
using BoltFreezer.FileIO;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanSpace;
using BoltFreezer.PlanTools;

namespace TestVHPOP
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
            var directory = Parser.GetTopDirectory() + @"/Results/";
            var cutoff = 6000f;
            var k = 1;

            var testDomainName = "batman";
            var testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
            var testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.PlanSpace);
            var testProblem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl");

            var problemFreezer = new ProblemFreezer(testDomainName, testDomainDirectory, testDomain, testProblem);

            //problemFreezer.Serialize();
            problemFreezer.Deserialize();
            var initPlan = PlanSpacePlanner.CreateInitialPlan(problemFreezer);

            RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E0(new AddReuseHeuristic()), k, cutoff, directory, 1);
            //RunPlanner(initPlan.Clone() as IPlan, new ADstar(), new E0(new NumOpenConditionsHeuristic()), k, cutoff, directory, 1);
            //RunPlanner(initPlan.Clone() as IPlan, new DFS(), new Nada(new ZeroHeuristic()), k, cutoff, directory, 1);
            //RunPlanner(initPlan.Clone() as IPlan, new BFS(), new Nada(new ZeroHeuristic()), k, cutoff, directory, 1);
        }
    }
}
