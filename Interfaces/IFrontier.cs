using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.Interfaces
{
    public interface IFrontier
    {

        int Count { get; }

        void Enqueue(IPlan Pi, float estimate);

        IPlan Dequeue();
    }
}
