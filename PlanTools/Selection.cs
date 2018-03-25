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
            if (plan.Flaws.Count == 0 && plan.Hdepth > 0)
            {
                return -10000f - plan.Steps.Count;
            }

            return plan.Steps.Count - (2 * plan.Decomps) + HMethod.Heuristic(plan);
        }
    }

    public class E1 : ISelection
    {
        private IHeuristic HMethod;

        public E1(IHeuristic hmethod)
        {
            HMethod = hmethod;
        }

        public SelectionType EType => SelectionType.E1;

        public new string ToString()
        {
            return EType.ToString() + "-" + HMethod.ToString();
        }

        public float Evaluate(IPlan plan)
        {
            if (plan.Flaws.Count == 0 && plan.Hdepth > 0)
            {
                return -10000f - plan.Steps.Count;
            }

            return plan.Steps.Count - (2 * plan.Decomps) + HMethod.Heuristic(plan) - plan.Hdepth;
        }
    }

    public class E2 : ISelection
    {
        private IHeuristic HMethod;

        public E2(IHeuristic hmethod)
        {
            HMethod = hmethod;
        }

        public SelectionType EType => SelectionType.E2;

        public new string ToString()
        {
            return EType.ToString() + "-" + HMethod.ToString();
        }

        public float Evaluate(IPlan plan)
        {
            if (plan.Flaws.Count == 0 && plan.Hdepth > 0)
            {
                return -10000f - plan.Steps.Count;
            }

            return plan.Steps.Count - (2 * plan.Decomps) + HMethod.Heuristic(plan) - plan.Hdepth - (float)System.Math.Log((double)plan.Hdepth + 1f, 2);
        }
    }

    public class E3 : ISelection
    {
        private IHeuristic HMethod;

        public E3(IHeuristic hmethod)
        {
            HMethod = hmethod;
        }

        public SelectionType EType => SelectionType.E3;

        public new string ToString()
        {
            return EType.ToString() + "-" + HMethod.ToString();
        }

        public float Evaluate(IPlan plan)
        {
            if (plan.Flaws.Count == 0 && plan.Hdepth > 0)
            {
                return -10000f - plan.Steps.Count;
            }

            return HMethod.Heuristic(plan) +
                ((plan.Steps.Count - (2 * plan.Decomps)) /
                (float)(1 + System.Math.Log((double)plan.Hdepth + 1f, 2)));
        }
    }

    public class E4 : ISelection
    {
        private IHeuristic HMethod;

        public E4(IHeuristic hmethod)
        {
            HMethod = hmethod;
        }

        public SelectionType EType => SelectionType.E4;

        public new string ToString()
        {
            return EType.ToString() + "-" + HMethod.ToString();
        }

        public float Evaluate(IPlan plan)
        {
            if (plan.Flaws.Count == 0 && plan.Hdepth > 0)
            {
                return -10000f - plan.Steps.Count;
            }

            return HMethod.Heuristic(plan) + plan.Steps.Count - 2 * plan.Decomps;
        }
    }

    public class E5 : ISelection
    {
        private IHeuristic HMethod;

        public E5(IHeuristic hmethod)
        {
            HMethod = hmethod;
        }

        public SelectionType EType => SelectionType.E5;

        public new string ToString()
        {
            return EType.ToString() + "-" + HMethod.ToString();
        }

        public float Evaluate(IPlan plan)
        {
            if (plan.Flaws.Count == 0 && plan.Hdepth > 0)
            {
                return -10000f - plan.Steps.Count;
            }

            return HMethod.Heuristic(plan) + ((plan.Steps.Count - 2 * plan.Decomps) / (1 + plan.Decomps));
        }
    }

    public class E6 : ISelection
    {
        private IHeuristic HMethod;

        public E6(IHeuristic hmethod)
        {
            HMethod = hmethod;
        }

        public SelectionType EType => SelectionType.E6;

        public new string ToString()
        {
            return EType.ToString() + "-" + HMethod.ToString();
        }

        public float Evaluate(IPlan plan)
        {
            if (plan.Flaws.Count == 0 && plan.Hdepth > 0)
            {
                return -10000f - plan.Steps.Count;
            }

            return HMethod.Heuristic(plan) + plan.Decomps;
        }
    }
}
