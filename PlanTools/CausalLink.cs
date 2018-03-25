using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using BoltFreezer.Interfaces;

namespace BoltFreezer.PlanTools
{
    [Serializable]
    public class CausalLink<T> where T : IOperator
    {
        private static int Counter = -1;

        private IPredicate predicate;
        private T head;
        private T tail;
        private int id;
        //private List<IOperator> span;


        // Access the operator's ID.
        public int ID
        {
            get { return id; }
        }

        // Access the link's predicate.
        public IPredicate Predicate
        {
            get { return predicate; }
            set { predicate = value; }
        }

        // Access the link's head.
        public T Head
        {
            get { return head; }
            set { head = value; }
        }

        // Access the link's tail.
        public T Tail
        {
            get { return tail; }
            set { tail = value; }
        }

        public CausalLink ()
        {
            predicate = new Predicate();
            head = default(T);
            tail = default(T);
            id = System.Threading.Interlocked.Increment(ref Counter);
        }

        public CausalLink (IPredicate predicate, T head, T tail)
        {
            this.predicate = predicate;
            this.head = head;
            this.tail = tail;
            id = System.Threading.Interlocked.Increment(ref Counter);
        }

        public CausalLink(IPredicate predicate, T head, T tail, int _id)
        {
            this.predicate = predicate;
            this.head = head;
            this.tail = tail;
            id = _id;
        }

        // Checks if two causal links are equal.
        public bool Equals(CausalLink<T> link)
        {
            if (link.head.Equals(head) && link.tail.Equals(tail) && link.predicate.Equals(predicate))
            {
                if (link.ID == ID)
                {
                    return true;
                }
            }
            return false;
        }

        // Returns a bound copy of the predicate.
        public Predicate GetBoundPredicate ()
        {
            Predicate boundPred = (Predicate)predicate.Clone();
            return boundPred;
        }

        // Displays the contents of the causal link.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Predicate: " + predicate);

            sb.AppendLine("Tail: " + tail.ToString());
            
            sb.AppendLine("Head: " + head.ToString());

            sb.AppendLine("ID: " + ID.ToString());

            return sb.ToString();
        }

        // Create a clone of the causal link.
        public Object Clone()
        {
            return new CausalLink<T>(predicate.Clone() as IPredicate, (T)head.Clone(), (T)tail.Clone(), ID);
        }

        public Object ShallowCopy()
        {
            return new CausalLink<T>(predicate as IPredicate, head, tail, ID);
        }
    }
}
