using BoltFreezer.Interfaces;
using BoltFreezer.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace BoltFreezer.PlanTools
{
    [Serializable]
    public static class GroundActionFactory
    {
        public static Dictionary<int, IOperator> GroundLibrary;

        public static List<IOperator> GroundActions;

        // TypeDict returns list of all constants given Term type
        public static Dictionary<string, List<string>> TypeDict;

        // Those predicates which are not established by an effect of an action but which are a precondition. They either hold initially or not at all.
        public static List<IPredicate> Statics = new List<IPredicate>();

        public static void Reset()
        {
            GroundLibrary = new Dictionary<int, IOperator>();
            GroundActions = new List<IOperator>();
            Statics = new List<IPredicate>();
        }

        public static void InsertOperator(IOperator newOperator)
        {
            GroundLibrary[newOperator.ID] = newOperator;
            GroundActions.Add(newOperator);
        }

        public static void PopulateGroundActions(Domain domain, Problem problem)
        {
            GroundActions = new List<IOperator>();
            GroundLibrary = new Dictionary<int, IOperator>();
            TypeDict = new Dictionary<string, List<string>>();
            var ops = domain.Operators;
            var objectHierarchy = EnumerableExtension.HashtableToDictionary<string, List<string>>(domain.objectTypes);
            var typeToObjectDict = EnumerableExtension.HashtableToDictionary<string, List<string>>(problem.ObjectsByType);

            foreach (var keyvalue in typeToObjectDict)
            {
                TypeDict[keyvalue.Key] = keyvalue.Value;
            }
            // Foreach object key that isn't a typedict key
            var excludedObjectKeys = objectHierarchy.Keys.Where(okey => !typeToObjectDict.ContainsKey(okey));
            foreach (var okey in excludedObjectKeys)
            {
                var newValues = typeToObjectDict.Keys.Where(tkey => objectHierarchy[okey].Contains(tkey)).Select(tkey => typeToObjectDict[tkey]);
                TypeDict[okey] = newValues.SelectMany(x => x).ToList();
            }


            FromOperators(ops);
        }

        private static void FromOperator(IOperator op)
        {

            var permList = new List<List<string>>();
            foreach (Term variable in op.Terms)
            {
                permList.Add(TypeDict[variable.Type] as List<string>);
            }

            foreach (var combination in EnumerableExtension.GenerateCombinations(permList))
            {
                // Add bindings
                var opClone = op.Clone() as Operator;
                var termStringList = from term in opClone.Terms select term.Variable;
                var constantStringList = combination;

                opClone.AddBindings(termStringList.ToList(), constantStringList.ToList());

                if (!opClone.NonEqualTermsAreNonequal())
                    continue;

                //Debug.Log("operator: " + opClone.ToString());

                // this ensures that this ground operator has a unique ID
                var groundOperator = new Operator(opClone.Name, opClone.Terms, opClone.Bindings, opClone.Preconditions, opClone.Effects);

                if (GroundActionFactory.GroundActions.Contains(groundOperator))
                {
                    continue;
                }

                if (GroundLibrary.ContainsKey(groundOperator.ID))
                    throw new System.Exception();

                InsertOperator(groundOperator as IOperator);
            }
        }

        private static void FromOperators(List<IOperator> operators)
        {
            foreach (var op in operators)
            {
                FromOperator(op);
            }
        }

        public static void DetectStatics()
        {
            // A static is a condition that, irrespective of sign, does not appear as an effect of an action.
            var preconds = GroundActions.SelectMany(op => op.Preconditions);
            var effs = GroundActions.SelectMany(op => op.Effects);
            foreach (var precon in preconds)
            {
                if (Statics.Contains(precon))
                {
                    continue;
                }
                if (effs.Contains(precon))
                {
                    continue;
                }
                if (effs.Contains(precon.GetReversed()))
                {
                    continue;
                }
                Statics.Add(precon);
            }

        }
    }
}