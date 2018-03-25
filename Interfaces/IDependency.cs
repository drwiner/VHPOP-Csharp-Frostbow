using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using BoltFreezer.PlanTools;

namespace BoltFreezer.Interfaces
{
    public interface IDependency
    {
        // Dependencies protect a predicate.
        Predicate Predicate { get; set; }

        // Dependencies span from a tail step.
        IOperator Tail { get; set; }

        // Dependencies span to a head step.
        IOperator Head { get; set; }

        // Dependencies span a set of operators.
        List<IOperator> Span { get; set; }

        // Dependencies can be cloned.
        Object Clone();
    }

    public interface ILink
    {
        // Dependencies protect a predicate.
        Predicate Predicate { get; set; }

        // Dependencies span from a tail step.
        IOperator Tail { get; set; }

        // Dependencies span to a head step.
        IOperator Head { get; set; }

        // Dependencies can be cloned.
        Object Clone();
    }

    public interface IOrd
    {
        // Dependencies span from a tail step.
        IOperator Tail { get; set; }

        // Dependencies span to a head step.
        IOperator Head { get; set; }

        // Dependencies can be cloned.
        Object Clone();
    }
}
