using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;

namespace BoltFreezer.PlanSpace
{ 
    public class PlanSpaceEdge
    {
        public Operator action;
        public CausalLink<PlanStep> clobberedLink;
        public State state;

        public PlanSpaceEdge ()
        {
            action = new Operator();
            clobberedLink = new CausalLink<PlanStep>();
            state = new State();
        }

        public PlanSpaceEdge (Operator action, CausalLink<PlanStep> clobberedLink, State state)
        {
            this.action = action;
            this.clobberedLink = clobberedLink;
            this.state = state;
        }

        // Displays the contents of the exceptional action.
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("ACTION");
            sb.AppendLine(action.ToString());
            sb.AppendLine("LINK");
            sb.AppendLine(clobberedLink.ToString());

            return sb.ToString();
        }
    }
}
