
using BoltFreezer.Enums;

namespace BoltFreezer.Interfaces
{
    public interface IHeuristic
    {
        HeuristicType HType { get; }

        float Heuristic(IPlan plan);

        string ToString();
    }
}
