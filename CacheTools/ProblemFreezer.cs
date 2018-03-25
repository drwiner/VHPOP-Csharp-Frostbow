using BoltFreezer.Enums;
using BoltFreezer.FileIO;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BoltFreezer.CacheTools
{
    public class ProblemFreezer
    {
        public string testDomainName;
        public string testDomainDirectory;
        public Domain testDomain;
        public Problem testProblem;

        string FileName;
        string CausalMapFileName;
        string ThreatMapFileName;

        public ProblemFreezer()
        {
            testDomainName = "batman";
            testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
            testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.PlanSpace);
            testProblem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl");

            FileName = Parser.GetTopDirectory() + @"Cached\CachedOperators\" + testDomainName + "_" + testProblem.Name;
            CausalMapFileName = Parser.GetTopDirectory() + @"Cached\CausalMaps\" + testDomainName + "_" + testProblem.Name;
            ThreatMapFileName = Parser.GetTopDirectory() + @"Cached\ThreatMaps\" + testDomainName + "_" + testProblem.Name;

        }

        public ProblemFreezer(string _testDomainName, string _testDomainDirectory, Domain _testDomain, Problem _testProblem)
        {
            testDomainName = _testDomainName;
            testDomainDirectory = _testDomainDirectory;
            testDomain = _testDomain;
            testProblem = _testProblem;

            FileName = Parser.GetTopDirectory() + @"Cached\CachedOperators\" + testDomainName + "_" + testProblem.Name;
            CausalMapFileName = Parser.GetTopDirectory() + @"Cached\CausalMaps\" + testDomainName + "_" + testProblem.Name;
            ThreatMapFileName = Parser.GetTopDirectory() + @"Cached\ThreatMaps\" + testDomainName + "_" + testProblem.Name;
        }

        public void Serialize()
        {
            Console.Write("Creating Ground Operators");
            GroundActionFactory.PopulateGroundActions(testDomain.Operators, testProblem);
            //BinarySerializer.SerializeObject(FileName, GroundActionFactory.GroundActions);
            foreach (var op in GroundActionFactory.GroundActions)
            {
                BinarySerializer.SerializeObject(FileName + op.GetHashCode().ToString() + ".CachedOperator", op);
            }

            CacheMaps.CacheLinks(GroundActionFactory.GroundActions);
            CacheMaps.CacheGoalLinks(GroundActionFactory.GroundActions, testProblem.Goal);
            BinarySerializer.SerializeObject(CausalMapFileName + ".CachedCausalMap", CacheMaps.CausalMap);
            BinarySerializer.SerializeObject(ThreatMapFileName + ".CachedThreatMap", CacheMaps.ThreatMap);
        }

        public void Deserialize()
        {
            GroundActionFactory.GroundActions = new List<IOperator>();
            GroundActionFactory.GroundLibrary = new Dictionary<int, IOperator>();
            foreach (var file in Directory.GetFiles(Parser.GetTopDirectory() + @"Cached\CachedOperators\", testDomainName + "_" + testProblem.Name + "*.CachedOperator"))
            {
                var op = BinarySerializer.DeSerializeObject<IOperator>(file);
                GroundActionFactory.GroundActions.Add(op);
                GroundActionFactory.GroundLibrary[op.ID] = op;
            }
            // THIS is so that initial and goal steps created don't get matched with these
            Operator.SetCounterExternally(GroundActionFactory.GroundActions.Count + 1);

            Console.WriteLine("\nCmap\n");

            var cmap = BinarySerializer.DeSerializeObject<Dictionary<IPredicate, List<int>>>(CausalMapFileName + ".CachedCausalMap");
            CacheMaps.CausalMap = cmap;

            Console.WriteLine("\nTmap\n");
            var tcmap = BinarySerializer.DeSerializeObject<Dictionary<IPredicate, List<int>>>(ThreatMapFileName + ".CachedThreatMap");
            CacheMaps.ThreatMap = tcmap;

            Console.WriteLine("Finding Statics");
            GroundActionFactory.DetectStatics(CacheMaps.CausalMap, CacheMaps.ThreatMap);
            foreach (var stat in GroundActionFactory.Statics)
            {
                Console.WriteLine(stat);
            }
        }

        public void FreezeProblem(bool serialize, bool deserialize)
        {
            if (serialize)
            {
                Serialize();
            }
            else if (deserialize)
            {
                Deserialize();
            }
        }
    }
}
