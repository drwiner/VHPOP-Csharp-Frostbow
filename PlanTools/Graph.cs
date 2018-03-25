using BoltFreezer.Interfaces;
using BoltFreezer.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace BoltFreezer.PlanTools
{
    
    public class Graph<T>
    {
        private HashSet<T> nodes;
        private HashSet<Tuple<T, T>> edges;

        public Graph()
        {
            nodes = new HashSet<T>();
            edges = new HashSet<Tuple<T, T>>();
        }

        public Graph(HashSet<T> _nodes, HashSet<Tuple<T,T>> _edges)
        {
            nodes = _nodes;
            edges = _edges;
        }

        //public void AddEdge(Tuple<T,T> edge)
        //{
        //    if (!nodes.Contains(edge.First))
        //        nodes.Add(edge.First);
        //    if (!nodes.Contains(edge.Second))
        //        nodes.Add(edge.Second);
        //    edges.Add(edge);
        //}

        public void Insert(T elm1, T elm2)
        {
            
            if (!nodes.Contains(elm1))
                nodes.Add(elm1);
            if (!nodes.Contains(elm2))
                nodes.Add(elm2);

            var newEdge = new Tuple<T,T>(elm1, elm2);
            if (!edges.Contains(newEdge))
                edges.Add(newEdge);
        }

        public List<T> GetDescendants(T element)
        {
            if (!nodes.Contains(element))
            {
                throw new System.Exception();
            }

            var descendants = new List<T>();
            var unexplored = new Stack<T>();
            unexplored.Push(element);
            
            while(unexplored.Count > 0)
            {
                var elm = unexplored.Pop();
                var tails = edges.Where(x => x.First.Equals(elm)).Select(x => x.Second);
                //var tails = edges.FindAll(edge => edge.First.Equals(elm)).Select(edge => edge.Second);
                foreach (var tail in tails)
                {
                    if (!descendants.Contains(tail))
                    {
                        unexplored.Push(tail);
                        descendants.Add(tail);
                    }

                }
            }
            return descendants;
        }

        private bool InDescendants(T start, T goal)
        {
            var descendants = new List<T>();
            var unexplored = new Stack<T>();
            unexplored.Push(start);

            while (unexplored.Count > 0)
            {
                var elm = unexplored.Pop();
                var tails = edges.Where(x => x.First.Equals(elm)).Select(x => x.Second);
                //var tails = edges.FindAll(edge => edge.First.Equals(elm)).Select(edge => edge.Second);
                foreach (var tail in tails)
                {
                    if (tail.Equals(goal))
                        return true;
                    if (!descendants.Contains(tail))
                    {
                        unexplored.Push(tail);
                        descendants.Add(tail);
                    }

                }
            }

            return false;
        }

        private bool AnyInDescendants(T start, List<T> goals)
        {
            var descendants = new List<T>();
            var unexplored = new Stack<T>();
            unexplored.Push(start);
            descendants.Add(start);
            while (unexplored.Count > 0)
            {
                var elm = unexplored.Pop();
                var tails = edges.Where(x => x.First.Equals(elm)).Select(x => x.Second);
                //var tails = edges.FindAll(edge => edge.First.Equals(elm)).Select(edge => edge.Second);
                foreach (var tail in tails)
                {
                    if (goals.Contains(tail))
                        return true;
                    //if (tail.Equals(goal))
                    //    return true;
                    if (!descendants.Contains(tail))
                    {
                        unexplored.Push(tail);
                        descendants.Add(tail);
                    }

                }
            }

            return false;
        }

        public bool IsPath(T elm1, T elm2)
        {
            if(!nodes.Contains(elm1) || !nodes.Contains(elm2))
            {
                throw new System.Exception();
            }

            return InDescendants(elm1, elm2);
            //return false/*;*/
        }

        public bool HasCycle()
        {
            foreach (var elm in nodes)
            {
                //var descendants = GetDescendants(elm);
                var predecessors = edges.Where(e => e.Second.Equals(elm)).Select(e => e.First) as List<T>;
                if (predecessors == null)
                    continue;

                if (AnyInDescendants(elm, predecessors))
                    return true;
                //if (descendants.Intersect(predecessors).Any()){
                //    return true;
                //}
                ////var predecessors = edges.FindAll(edge => edge.Second.Equals(elm)).Select(edge => edge.First);
                //foreach (var desc in descendants)
                //{
                //    if (predecessors.Contains(desc))
                //        return true;
                //}
            }
            return false;
        }

        public List<T> TopoSort(T start)
        {
            var edgeList = edges.ToList();
            var L = new List<T>();
            var S = new Stack<T>();
            S.Push(start);

            while (S.Count > 0)
            {
                var n = S.Pop();
                L.Add(n);
                var markedForRemoval = edgeList.ToList();
                var edgesFromN = edgeList.Where(e => e.First.Equals(n));
                foreach (var nmEdge in edgesFromN)
                {
                    markedForRemoval.Remove(nmEdge);
                    if (!markedForRemoval.Any(e => e.Second.Equals(nmEdge.Second)))
                        S.Push(nmEdge.Second);
                }
                edgeList = markedForRemoval;
            }

            return L;
        }
        

        /// <summary>
        /// Clones just the graph, and not the members. The members will never be mutated.
        /// </summary>
        /// <returns> new Graph<typeparamref name="T"/> (nodes, edges) </returns>
        public Object Clone()
        {
            var newNodes = new HashSet<T>(nodes);
            var newEdges = new HashSet<Tuple<T, T>>(edges);
            return new Graph<T>(newNodes, newEdges);
        }

    }



}