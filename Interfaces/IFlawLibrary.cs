using BoltFreezer.PlanTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.Interfaces
{
    public interface IFlawLibrary
    {
        // Add a flaw into the library
        void Add(IPlan plan, OpenCondition oc);

        // Add a threatened causal link flaw into the library
        void Add(ThreatenedLinkFlaw tclf);

        IFlaw Next();

        Object Clone();
    }
}
