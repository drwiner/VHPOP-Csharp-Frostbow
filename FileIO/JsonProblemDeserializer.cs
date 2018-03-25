using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestFreezer;

namespace BoltFreezer.FileIO
{
    public class JsonProblemDeserializer
    {
        public static List<ITerm> JsonTermListtoTerms(JsonArray jsontermlist)
        {
            var Terms = new List<ITerm>();

            for (int i = 0; i < jsontermlist.Count; i++)
            {
                var jsonterm = jsontermlist[i] as JsonObject;
                var name = jsonterm["Name"].ToString();
                var _type = jsonterm["Types"].ToString();
                var term = new Term(i.ToString(), name, _type);
                Terms.Add(term);
            }

            return Terms;
        }

        public static List<int> IntListFromJsonArray(JsonArray jsonintlist)
        {
            var enumItems = from item in jsonintlist select int.Parse(item.ToString());
            return enumItems.ToList() as List<int>;
        }

        public static Tuple<IOperator, IOperator> IntTupleFromJsonArray(JsonArray jsonintlist)
        {
            var enumItems = from item in jsonintlist select GroundActionFactory.GroundLibrary[int.Parse(item.ToString())];
            var intList = enumItems.ToList() as List<IOperator>;
            return new Tuple<IOperator, IOperator>(intList[0], intList[1]);
        }

        public static List<Tuple<IOperator, IOperator>> IntTupleListFromJsonArray(JsonArray jsontuplelist)
        {
            var tupleList = new List<Tuple<IOperator, IOperator>>();
            foreach (var jsonTuple in jsontuplelist)
            {
                var tupleItem = IntTupleFromJsonArray(jsonTuple as JsonArray);
                tupleList.Add(tupleItem);
            }
            return tupleList;
        }

        public static CausalLink<IOperator> CausalLinkFromJsonArray(JsonArray jsonlink)
        {
            var pred = PredicateFromJsonObject(jsonlink[1] as JsonObject);
            var source = GroundActionFactory.GroundLibrary[int.Parse(jsonlink[0].ToString())];
            var sink = GroundActionFactory.GroundLibrary[int.Parse(jsonlink[2].ToString())];
            return new CausalLink<IOperator>(pred, source, sink);
        }

        public static List<CausalLink<IOperator>> CausalLinksFromJsonArray(JsonArray jsoncausallinkslist)
        {
            var clinks = new List<CausalLink<IOperator>>();
            foreach (var jsonlink in jsoncausallinkslist)
            {
                var clink = CausalLinkFromJsonArray(jsonlink as JsonArray);
                clinks.Add(clink);
            }
            return clinks;
        }

        public static Predicate PredicateFromJsonObject(JsonObject jsonpredicate)
        {
            var Name = jsonpredicate["name"].ToString();
            var Terms = JsonTermListtoTerms(jsonpredicate["Terms"] as JsonArray);
            var sign = jsonpredicate["Sign"].ToString();
            bool Sign = false;
            if (sign.Equals("True"))
            {
                Sign = true;
            }

            return new Predicate(Name, Terms, Sign);
        }

        public static Predicate JsonPreconToPrecon(JsonObject jsonPrecon)
        {
            var newPredicate = PredicateFromJsonObject(jsonPrecon);
            if (jsonPrecon["Static"].ToString().Equals("True"))
            {
                GroundActionFactory.Statics.Add(newPredicate);
            }

            return newPredicate;
        }

        public static List<IPredicate> JsonPreconditionsToPreconditions(JsonArray jsonPreconditions)
        {
            var Preconditions = new List<IPredicate>();
            foreach (JsonObject p in jsonPreconditions)
            {
                var precondition = JsonPreconToPrecon(p);
                Preconditions.Add(precondition);
            }
            return Preconditions;
        }

        public static Predicate StringToPredicate(string formattedString)
        {
            bool Sign = true;
            string predName = formattedString.Split('[')[0];
            var possibleSign = formattedString.Split('-')[0];
            var afterBracket = formattedString.Split('[')[1];
            if (possibleSign.Equals("not"))
            {
                var afterHyphen = formattedString.Split('-')[1];
                Sign = false;
                predName = afterHyphen.Split('[')[0];
                afterBracket = afterHyphen.Split('[')[1];
            }
            var betweenBrackets = afterBracket.Split(']')[0];
            var indArgs = betweenBrackets.Split(',') as string[];

            var termList = new List<ITerm>();
            foreach (var arg in indArgs)
            {
                termList.Add(new Term(arg.Trim(' ').Trim('\''), true));
            }

            return new Predicate(predName, termList, Sign);
        }

        public static List<IPlanStep> SubStepsFromJson(JsonArray substepjson)
        {
            var substeps = new List<IPlanStep>();
            // each item is another list of form <intID, openCondition JsonObject predicates>
            foreach (JsonArray substepArray in substepjson)
            {
                var intID = int.Parse(substepArray[0].ToString());
                var underlyingComposite = GroundActionFactory.GroundLibrary[intID];
                IPlanStep plansubstep;
                if (underlyingComposite.Height == 0) {
                    plansubstep = new PlanStep(underlyingComposite as IOperator);
                }
                else
                {
                    plansubstep = new CompositePlanStep(underlyingComposite as IComposite);
                }

                var openConditions = new List<IPredicate>();
                foreach(JsonObject openCondition in substepArray[1] as JsonArray)
                {
                    var removeThis = PredicateFromJsonObject(openCondition);
                }

                plansubstep.OpenConditions = openConditions;
                substeps.Add(plansubstep);
                
            }
            return substeps;
        }       
    
        public static Plan DeserializeJsonTravelDomain(int whichOne)
        {
            //var problemFile = @"D:\Unity projects\BoltFreezer\travel_domain.travel\travel_domain.travel\1\1.json";
            var problemFile = @"D:\Documents\workspace\travel_domain.travel\" + whichOne.ToString() + @"\" + whichOne.ToString() + @".json";
            //var problemFile = @"D:\Unity projects\BoltFreezer\travel_domain.travel\travel_domain.travel\" + whichOne.ToString() + @"\" + whichOne.ToString() + @".json";
            //problemFile

            var travelProblem = Parser.GetProblem(@"D:\Documents\workspace\travel_domain.travel\" + whichOne.ToString() + @"\travel-" + whichOne.ToString() + @".pddl");

            var problemText = System.IO.File.ReadAllText(problemFile);

            var jsonArray = SimpleJson.DeserializeObject(problemText) as JsonArray;
            //Console.WriteLine("CHERE");

            GroundActionFactory.GroundActions = new List<IOperator>();
            GroundActionFactory.GroundLibrary = new Dictionary<int, IOperator>();
            CacheMaps.CausalMap = new Dictionary<IPredicate, List<int>>();
            CacheMaps.ThreatMap = new Dictionary<IPredicate, List<int>>();

            Operator initialOp = new Operator();
            Operator goalOp = new Operator();
            // for each operator in the list
            foreach (JsonObject jsonObject in jsonArray)
            {
                // ID, Name, Terms, Preconditions, Effects
                var ID = jsonObject["ID"];
                var Name = jsonObject["Name"].ToString();
                var Terms = JsonTermListtoTerms(jsonObject["Terms"] as JsonArray);
                var Preconditions = JsonPreconditionsToPreconditions(jsonObject["Preconditions"] as JsonArray) as List<IPredicate>;
                var Effects = new List<IPredicate>();
                var Height = int.Parse(jsonObject["height"].ToString());

                if (Name.Equals("dummy_goal"))
                {
                    Name = "goal";
                }
                if (Name.Equals("dummy_init"))
                {
                    Name = "initial";
                    Effects = travelProblem.Initial;
                    Preconditions = new List<IPredicate>();
                }

                var action = new Operator(Name, Terms, new Hashtable(), Preconditions, Effects, int.Parse(ID.ToString()))
                {
                    Height = Height
                };

                if (Height > 0)
                {
                    //Console.Write("here");
                    var init = GroundActionFactory.GroundLibrary[int.Parse(jsonObject["DummyInitial"].ToString())];
                    var goal = GroundActionFactory.GroundLibrary[int.Parse(jsonObject["DummyGoal"].ToString())];
                    var subSteps = SubStepsFromJson(jsonObject["SubSteps"] as JsonArray);
                    
                    //var subSteps = from item in IntListFromJsonArray(jsonObject["SubSteps"] as JsonArray) select GroundActionFactory.GroundLibrary[item] as IOperator;
                    var subOrderingTuples = IntTupleListFromJsonArray(jsonObject["SubOrderings"] as JsonArray);
                    var subLinks = CausalLinksFromJsonArray(jsonObject["SubLinks"] as JsonArray);

                    action = new Composite(action, init, goal, subSteps, subOrderingTuples, subLinks);

                }

                //Console.WriteLine(action.ToString() + "  " + action.Height);

                if (Name.Equals("goal"))
                    goalOp = action;
                else if (Name.Equals("initial"))
                    initialOp = action;
                else
                {
                    GroundActionFactory.GroundActions.Add(action);
                    GroundActionFactory.GroundLibrary[action.ID] = action;
                }

                if (jsonObject.ContainsKey("CausalMap"))
                {
                    var CausalMap = jsonObject["CausalMap"] as JsonObject;
                    foreach (var keyvalue in CausalMap)
                    {
                        var predKey = StringToPredicate(keyvalue.Key);
                        if (!CacheMaps.CausalMap.ContainsKey(predKey))
                        {
                            CacheMaps.CausalMap[predKey] = IntListFromJsonArray(keyvalue.Value as JsonArray);
                        }

                    }
                }
                if (jsonObject.ContainsKey("ThreatMap"))
                {
                    var ThreatMap = jsonObject["ThreatMap"] as JsonObject;
                    foreach (var keyvalue in ThreatMap)
                    {
                        var predKey = StringToPredicate(keyvalue.Key);
                        if (!CacheMaps.ThreatMap.ContainsKey(predKey))
                        {
                            var intList = new List<object>();
                            var jsonList = keyvalue.Value as JsonArray;
                            var enumItems = from item in jsonList select int.Parse(item.ToString());
                        }
                    }
                }

            }

            GroundActionFactory.GroundLibrary[initialOp.ID] = null;
            GroundActionFactory.GroundLibrary[goalOp.ID] = null;

            Operator.SetCounterExternally(GroundActionFactory.GroundActions.Count + 1);

            var plan = new Plan(initialOp, goalOp);
            foreach (var goal in plan.Goal.Predicates)
            {
                plan.Flaws.Insert(plan, new OpenCondition(goal, plan.GoalStep as IPlanStep));
            }

            Console.WriteLine("Insert First Ordering");
            plan.Orderings.Insert(plan.InitialStep, plan.GoalStep);
            return plan;
        }
    }
}
