using BoltFreezer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.Interfaces
{
    public interface ISelection
    {
        SelectionType EType { get; }

        float Evaluate(IPlan plan);

        string ToString();
    }
}
