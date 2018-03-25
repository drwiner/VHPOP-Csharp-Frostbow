using BoltFreezer.Enums;
using BoltFreezer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.PlanTools
{

    public class Nada : ISelection
    {
        private IHeuristic HMethod;

        public Nada(IHeuristic hmethod)
        {
            HMethod = hmethod;
        }

        public SelectionType EType => SelectionType.Nada;

        public new string ToString()
        {
            return EType.ToString() + "-" + HMethod.ToString();
        }

        public float Evaluate(IPlan plan)
        {
            return 0f;
        }
    }

    public class E0 : ISelection
    {
        private IHeuristic HMethod;

        public E0(IHeuristic hmethod)
        {
            HMethod = hmethod;
        }

        public SelectionType EType => SelectionType.E0;

        public new string ToString()
        {
            return EType.ToString() + "-" + HMethod.ToString();
        }

        public float Evaluate(IPlan plan)
        {
            if (plan.Flaws.Count == 0)
            {
                return -10000f - plan.Steps.Count;
            }

            return plan.Steps.Count + HMethod.Heuristic(plan);
        }
    }

}
