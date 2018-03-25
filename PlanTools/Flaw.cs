using BoltFreezer.Interfaces;
using BoltFreezer.Utilities;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using BoltFreezer.Enums;

namespace BoltFreezer.PlanTools
{

    [Serializable]
    public class OpenCondition : IComparable<OpenCondition>, IFlaw
    {
        public IPredicate precondition;
        public IPlanStep step;
        public bool isStatic = false;
        public bool isInit = false;
        public int risks = 0;
        public int cndts = 0;
        

        FlawType IFlaw.Ftype
        {
            get{ return FlawType.Condition;}
        }

        public OpenCondition ()
        {
            precondition = new Predicate();
            step = new PlanStep();
        }

        public OpenCondition (IPredicate precondition, IPlanStep step)
        {
            this.precondition = precondition;
            this.step = step;
        }

        // Displays the contents of the flaw.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Flaw: " + precondition);
                
            sb.AppendLine("Step: " + step);

            return sb.ToString();
        }

        public int CompareTo(OpenCondition other)
        {
            if (other == null)
            {
                return 1;
            }

            // check static
            if (isStatic && !other.isStatic)
                return -1;
            else if (other.isStatic && !isStatic)
                return 1;

            // check init
            if (isInit && !other.isInit)
                return -1;
            else if (other.isInit && !isInit)
                return 1;

            // check risks
            if (risks > 0 || other.risks > 0)
            {
                if (risks > other.risks)
                    return -1;
                else if (risks < other.risks)
                    return 1;
            }

            // check cndts
            if (cndts > other.cndts)
                return -1;
            else if (cndts < other.cndts)
                return 1;

            // resort to tiebreak criteria
            if (step.ID == other.step.ID)
            {
                if (precondition.Equals(other.precondition))
                {
                    throw new System.Exception();
                }
                else
                    return PredicateComparer.CompareTo(precondition, other.precondition);

            }
            else if (step.ID < other.step.ID)
            {
                return -1;
            }
            else
                return 1;
        }

        public static bool operator < (OpenCondition self, OpenCondition other)
        {
            if (self.CompareTo(other) < 0)
                return true;
            return false;
        }

        public static bool operator > (OpenCondition self, OpenCondition other)
        {
            if (self.CompareTo(other) > 0)
                return true;
            return false;
        }

        public override bool Equals(object obj)
        {
            var oc = obj as OpenCondition;
            if (oc.step.Equals(step) && oc.precondition.Equals(precondition))
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.step.GetHashCode() * 23 + this.precondition.GetHashCode();
        }

        public OpenCondition Clone()
        {
            var oc = new OpenCondition(precondition.Clone() as IPredicate, step.Clone() as IPlanStep)
            {
                risks = risks,
                isInit = isInit,
                isStatic = isStatic,
                cndts = cndts
            };
            return oc;
        }
    }

    [Serializable]
    public class ThreatenedLinkFlaw : IComparable<ThreatenedLinkFlaw>, IFlaw
    {
        public CausalLink<IPlanStep> causallink;
        public IPlanStep threatener;

        public ThreatenedLinkFlaw()
        {
            causallink = new CausalLink<IPlanStep>();
            threatener = new Operator() as IPlanStep;
        }

        public ThreatenedLinkFlaw(CausalLink<IPlanStep> _causallink, IPlanStep _threat)
        {
            this.causallink = _causallink;
            this.threatener = _threat;
        }

        FlawType IFlaw.Ftype
        {
            get { return FlawType.Link; }
        }

        public int CompareTo(ThreatenedLinkFlaw other)
        {
            if (other == null)
            {
                return 1;
            }
            if (threatener.ID == other.threatener.ID)
            {
                if (causallink.Predicate.Equals(other.causallink.Predicate))
                {
                    if (causallink.Head.ID != other.causallink.Head.ID)
                    {
                        if (causallink.Head.ID < other.causallink.Head.ID)
                            return -1;
                        else
                            return 1;
                    }
                    else if (causallink.Tail.ID != other.causallink.Tail.ID)
                    {
                        if (causallink.Tail.ID < other.causallink.Tail.ID)
                            return -1;
                        else
                            return 1;
                    }
                    // causal link is the same, and the threat is the same
                    throw new System.Exception();
                }
                else
                    return PredicateComparer.CompareTo(causallink.Predicate, other.causallink.Predicate);
            }
            else if (threatener.ID < other.threatener.ID)
            {
                return -1;
            }
            else
                return 1;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("CausalLink: " + causallink);

            sb.AppendLine("Threat: " + threatener);

            return sb.ToString();
        }

        public ThreatenedLinkFlaw Clone()
        {
            var cl = causallink.Clone() as CausalLink<IPlanStep>;
            var thrt = threatener.Clone() as IPlanStep;
            return new ThreatenedLinkFlaw(cl, thrt);
        }
    }


    
}
