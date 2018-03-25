using BoltFreezer.Interfaces;
using BoltFreezer.Utilities;
using BoltFreezer.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace BoltFreezer.PlanTools
{
    // A Flawque is a flaw selection queue, has the .Next Property
    public class Flawque : IFlawLibrary
    {
        // Cannot use heap because the values are mutable and won't be kept sorted unless they are all re-inserted. If we have to do that, there's no benefit
        public List<OpenCondition> OpenConditions;
        
        // LIFO
        private Stack<ThreatenedLinkFlaw> threatenedLinks;

        public Flawque()
        {
            OpenConditions = new List<OpenCondition>();
            threatenedLinks = new Stack<ThreatenedLinkFlaw>();
        }

        //public Flawque(Heap<OpenCondition> ocs, Heap<ThreatenedLinkFlaw> tclfs)
        public Flawque(List<OpenCondition> ocs, Stack<ThreatenedLinkFlaw> tclfs)
        {
            OpenConditions = ocs;
            threatenedLinks = tclfs;
        }

        public void Add(IPlan plan, OpenCondition oc)
        {
            // Don't add an open condition if it's already in the Flawque.
            if (OpenConditions.Contains(oc))
                return;

            // Predicates are labeled as static before planning
            if (GroundActionFactory.Statics.Contains(oc.precondition))
            {
                oc.isStatic = true;
                return;
            }

            // Calculate risks and cndts
            foreach (var step in plan.Steps)
            {
                
                if (step.ID == oc.step.ID)
                    continue;

                if (plan.Orderings.IsPath(oc.step, step))
                    continue;

                if (CacheMaps.IsCndt(oc.precondition, step))
                    oc.cndts += 1;

                if (CacheMaps.IsThreat(oc.precondition, step))
                    oc.risks += 1;

            }


            if (oc.risks == 0 && plan.Initial.InState(oc.precondition))
            {
                oc.isInit = true;
            }

            OpenConditions.Add(oc);
        }

        public void Add(ThreatenedLinkFlaw tclf)
        {

            threatenedLinks.Push(tclf);
        }

        public IEnumerable<OpenCondition> OpenConditionGenerator()
        {
            foreach (var item in OpenConditions.ToList())
            {
                yield return item.Clone();
            }
        }

        public int Count
        {
            get { return OpenConditions.Count + threatenedLinks.Count; }
        }

        /// <summary>
        /// Returns next flaw. A threatened link has priority. Then statics, inits without risk, risky ocs, reuse ocs, then nonreuse ocs
        /// </summary>
        /// <returns> Flaw that implements interface IFlaw </returns>
        public IFlaw Next()
        {
            // repair threatened links first
            if (threatenedLinks.Count != 0)
                return threatenedLinks.Pop();

            if (OpenConditions.Count == 0)
                return null;

            var best_flaw = OpenConditions[0].Clone() as OpenCondition;

            foreach(var oc in OpenConditions.Skip(1))
            {
                if (oc < best_flaw)
                    best_flaw = oc;
            }

            //if (!OpenConditions.IsEmpty())
            //    return OpenConditions.PopRoot();
            OpenConditions.Remove(best_flaw);
            return best_flaw;
        }

        // When we add a new step, update cndts and risks
        public void UpdateFlaws(IPlan plan, IPlanStep action)
        {

            foreach (var oc in OpenConditions.ToList())
            {
                // ignore any open conditions that cannot possibly be affected by this action's effects, such as those occurring after
                if (plan.Orderings.IsPath(oc.step, action))
                    continue;

                if (CacheMaps.IsCndt(oc.precondition, action))
                    //if (action.Effects.Contains(oc.precondition))
                    oc.cndts += 1;

                if (CacheMaps.IsThreat(oc.precondition, action))
                    //if (action.Effects.Any(x => oc.precondition.IsInverse(x)))
                    oc.risks += 1;
            }
        }

        // Clone of Flawque requires clone of individual flaws because these have mutable properties
        public Object Clone()
        {

            // Move open condition to new container and clone each one
            var newOpenConditions = new List<OpenCondition>();
            foreach (var oc in OpenConditions)
            {
                newOpenConditions.Add(oc.Clone());
            }

            // Move tclfs to new stack, no need to clone.
            var newThreatenedLinks = new Stack<ThreatenedLinkFlaw>();
            foreach (var tclf in threatenedLinks.ToList())
            {
                newThreatenedLinks.Push(tclf);
            }

            return new Flawque(newOpenConditions, newThreatenedLinks);
        }

    }
}