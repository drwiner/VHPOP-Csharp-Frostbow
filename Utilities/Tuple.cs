using System;

namespace BoltFreezer.Utilities
{
    [Serializable]
    public class Tuple<T1, T2>
    {
        public T1 First { get; private set; }
        public T2 Second { get; private set; }
        public Tuple(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }

        public override int GetHashCode()
        {
            return First.GetHashCode() + 23 * Second.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var tuple = obj as Tuple<T1, T2>;
            if (First.Equals(tuple.First) && Second.Equals(tuple.Second))
                return true;
            //if (First.GetHashCode() == tuple.First.GetHashCode().Equals(tuple.First) && Second.Equals(tuple.Second))
            //    return true;
            return false;
        }

        public override string ToString()
        {
            return String.Format("({0}, {1})", First.ToString(), Second.ToString());
        }

        public Object Clone()
        {
            return new Tuple<T1,T2>(First, Second);
        }

    }

    [Serializable]
    public static class Tuple
    {
        public static Tuple<T1, T2> New<T1, T2>(T1 first, T2 second)
        {
            var tuple = new Tuple<T1, T2>(first, second);
            return tuple;
        }
    }
}
