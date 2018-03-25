using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using System;
using System.Collections.Generic;

namespace BoltFreezer.Interfaces
{

    public interface IPlanStep : IOperator
    {
        // The representative operator   
        IOperator Action { get; set; }

        // Identification
        new int ID { get; }

        // Actions keep track of open preconditions
        List<IPredicate> OpenConditions { get; set; }

        // remove from open conditions
        void Fulfill(IPredicate condition);

        // Actions can be cloned.
        new Object Clone();
    }

    
}
