using BoltFreezer.Enums;
using BoltFreezer.FileIO;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
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
            Console.WriteLine("Creating Ground Operators");
            GroundActionFactory.PopulateGroundActions(testDomain, testProblem);
            //.Operators, testDomain.ObjectTypes, testProblem.ObjectsByType);
            //BinarySerializer.SerializeObject(FileName, GroundActionFactory.GroundActions);

            // Remove existing cached operators in this domain
            //var di = new DirectoryInfo(Parser.GetTopDirectory() + @"Cached\CachedOperators\");
            //foreach(var file in di.GetFiles())
            //{
            //    var isRightDomain = file.ToString().StartsWith(testDomainName);
            //    if (file.Extension.Equals(".CachedOperator") && isRightDomain)
            //    {
            //        file.Delete();
            //    }
            //}

            foreach (var op in GroundActionFactory.GroundActions)
            {
                BinarySerializer.SerializeObject(FileName + op.GetHashCode().ToString() + ".CachedOperator", op);
            }

            CacheMaps.CacheLinks(GroundActionFactory.GroundActions);
            CacheMaps.CacheGoalLinks(GroundActionFactory.GroundActions, testProblem.Goal);

            BinarySerializer.SerializeObject(CausalMapFileName + ".CachedCausalMap", CacheMaps.CausalTupleMap);
            BinarySerializer.SerializeObject(ThreatMapFileName + ".CachedThreatMap", CacheMaps.ThreatTupleMap);
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

            Console.WriteLine("Finding Statics");
            GroundActionFactory.DetectStatics();
            foreach (var stat in GroundActionFactory.Statics)
            {
                Console.WriteLine(stat);
            }

            Console.WriteLine("\nCmap\n");

            var cmap = BinarySerializer.DeSerializeObject<TupleMap<IPredicate, List<int>>>(CausalMapFileName + ".CachedCausalMap");
            CacheMaps.CausalTupleMap = cmap;

            Console.WriteLine("\nTmap\n");
            var tcmap = BinarySerializer.DeSerializeObject<TupleMap<IPredicate, List<int>>>(ThreatMapFileName + ".CachedThreatMap");
            CacheMaps.ThreatTupleMap = tcmap;

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
