using BoltFreezer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.Interfaces
{
    public interface ISearch
    {
        IFrontier Frontier { get; }

        SearchType SType { get; }

        List<IPlan> Search(IPlanner P);

        List<IPlan> Search(IPlanner P, int k, float cutoff);

        string ToString();
    }
}
