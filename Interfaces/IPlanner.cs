using BoltFreezer.PlanTools;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.Interfaces
{
    public interface IPlanner
    {
        ISearch Search { get; }

        List<IPlan> Solve(int k, float cutoff);

        float Score(IPlan pi);

        void Insert(IPlan pi);

        void AddStep(IPlan pi, OpenCondition oc);

        void Reuse(IPlan plan, OpenCondition oc);

        void RepairThreat(IPlan plan, ThreatenedLinkFlaw tclf);

        bool Console_log { get; }

        // Must be a Plan for now, may need upate for IPlan
        void WriteToFile(long elapsedMs, Plan plan);

        int Expanded { get; set; }

        int Open { get; set; }

    }

}
