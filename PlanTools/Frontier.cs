using BoltFreezer.Interfaces;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.PlanTools
{
    public class PriorityQueue : IFrontier
    {
        public int Count => priorityQueue.Count;

        public SimplePriorityQueue<IPlan, float> priorityQueue;

        public PriorityQueue()
        {
            priorityQueue = new SimplePriorityQueue<IPlan, float>();
        }

        public void Enqueue(IPlan Pi, float estimate)
        {
            priorityQueue.Enqueue(Pi, estimate);
        }

        public IPlan Dequeue()
        {
            return priorityQueue.Dequeue();
        }
    }


    public class DFSFrontier : IFrontier
    {
        public int Count => stack.Count;

        private Stack<IPlan> stack;

        public DFSFrontier()
        {
            stack = new Stack<IPlan>();
        }

        public void Enqueue(IPlan Pi, float estimate)
        {
            stack.Push(Pi);
        }

        public IPlan Dequeue()
        {
            return stack.Pop();
        }
    }

    public class BFSFrontier : IFrontier
    {
        public int Count => queue.Count;

        private Queue<IPlan> queue;

        public BFSFrontier()
        {
            queue = new Queue<IPlan>();
        }

        public void Enqueue(IPlan Pi, float estimate)
        {
            queue.Enqueue(Pi);
        }

        public IPlan Dequeue()
        {
            return queue.Dequeue();
        }
    }
}
