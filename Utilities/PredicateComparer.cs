using BoltFreezer.Interfaces;
using System;

namespace BoltFreezer.Utilities
{
    public class PredicateComparer
    {
        public static int CompareByName (IPredicate x, IPredicate y)
        {
            return String.Compare(x.Name, y.Name);
        }

        public static int InverseCompareByName (IPredicate x, IPredicate y)
        {
            return String.Compare(x.Name, y.Name) * -1;
        }

        public static int CompareTo(IPredicate x, IPredicate y)
        {

            if (x.Name != y.Name)
            {
                if (x.Name.GetHashCode() == y.Name.GetHashCode())
                {
                    throw new System.Exception();
                }
                else if (x.Name.GetHashCode() < y.Name.GetHashCode())
                    return -1;
                else
                    return 1;
            }
            else if (x.Sign != y.Sign)
            {
                if (!x.Sign && y.Sign)
                {
                    return -1;
                }
                else
                    return 1;
            }
            else if (x.Arity != y.Arity)
            {
                if (x.Arity < y.Arity)
                    return -1;
                else
                    return 1;
            }
            // Predicates x and y must have similar terms
            foreach (var kp in x.Terms.Zip(y.Terms))
            {
                if (!kp.Key.Constant.Equals(kp.Value.Constant))
                {
                    if (kp.Key.Constant.GetHashCode() < kp.Value.Constant.GetHashCode())
                    {
                        return -1;
                    }
                    else
                        return 1;
                }

            }
            // All args are the same
            throw new System.Exception();
        }
    }


}
