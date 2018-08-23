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
        protected HashSet<T> nodes;
        public HashSet<Tuple<T, T>> edges;

        protected Dictionary<T, HashSet<T>> DescendantMap;

        public Graph()
        {
            nodes = new HashSet<T>();
            edges = new HashSet<Tuple<T, T>>();
            DescendantMap = new Dictionary<T, HashSet<T>>();
        }

        public Graph(HashSet<T> _nodes, HashSet<Tuple<T, T>> _edges, Dictionary<T, HashSet<T>> _descMap)
        {
            nodes = _nodes;
            edges = _edges;
            DescendantMap = _descMap;
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
            if (elm1.Equals(elm2))
            {
                Console.WriteLine("mistake in ordering");
                throw new System.Exception();
            }

            if (!nodes.Contains(elm1))
            {
                DescendantMap[elm1] = new HashSet<T>();
                nodes.Add(elm1);
            }

            if (!nodes.Contains(elm2))
            {
                nodes.Add(elm2);
                DescendantMap[elm2] = new HashSet<T>();
            }
            DescendantMap[elm1].Add(elm2);
            var newEdge = new Tuple<T, T>(elm1, elm2);
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

            while (unexplored.Count > 0)
            {
                var elm = unexplored.Pop();

                var tails = edges.Where(x => x.First.Equals(elm)).Select(x => x.Second);
                //var tails = edges.FindAll(edge => edge.First.Equals(elm)).Select(edge => edge.Second);
                foreach (var tail in tails)
                {
                    if (!DescendantMap[element].Contains(elm))
                        DescendantMap[element].Add(elm);
                    if (!descendants.Contains(tail))
                    {
                        unexplored.Push(tail);
                        descendants.Add(tail);
                    }

                }
            }
            return descendants;
        }

        protected bool InDescendants(T start, T goal)
        {
            if (DescendantMap[start].Contains(goal))
            {
                return true;
            }

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
                    if (!DescendantMap[start].Contains(tail))
                        DescendantMap[start].Add(tail);

                    if (tail.Equals(goal))
                    {
                        return true;
                    }
                    if (!descendants.Contains(tail))
                    {
                        unexplored.Push(tail);
                        descendants.Add(tail);
                    }

                }
            }

            return false;
        }

        protected bool AnyInDescendants(T start, List<T> goals)
        {
            if (DescendantMap[start].Any(desc => goals.Contains(desc)))
                return true;

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
                    if (!DescendantMap[start].Contains(tail))
                        DescendantMap[start].Add(tail);

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
            if (!nodes.Contains(elm1) || !nodes.Contains(elm2))
            {
                throw new System.Exception();
            }

            return InDescendants(elm1, elm2);
        }

        public bool HasCycle()
        {
            foreach (var elm in nodes)
            {
                //var descendants = GetDescendants(elm);
                var predecessors = edges.Where(e => e.Second.Equals(elm)).Select(e => e.First).ToList();
                if (predecessors == null || predecessors.Count == 0)
                    continue;

                if (AnyInDescendants(elm, predecessors))
                    return true;
            }
            return false;
        }

        public List<T> TopoSort(T start)
        {
            List<Tuple<T, T>> edgeList = edges.ToList();
            var L = new List<T>();
            var S = new Stack<T>();
            S.Push(start);

            while (S.Count > 0)
            {
                var n = S.Pop();
                L.Add(n);
                List<Tuple<T, T>> markedForRemoval = edgeList.ToList();
                var edgesFromN = edgeList.Where(e => e.First.Equals(n));
                foreach (var nmEdge in edgesFromN)
                {
                    markedForRemoval.Remove(nmEdge);
                    if (!markedForRemoval.Any(e => e.Second.Equals(nmEdge.Second))) // && !e.First.Equals(n)))
                        S.Push(nmEdge.Second);
                    //else
                    //{
                    //    markedForRemoval.Add(nmEdge);
                    //}
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
            var newDescendantMap = new Dictionary<T, HashSet<T>>();
            foreach (var keyvalue in DescendantMap)
            {
                newDescendantMap[keyvalue.Key] = new HashSet<T>(keyvalue.Value);
            }
            return new Graph<T>(newNodes, newEdges, newDescendantMap);
        }

    }



}