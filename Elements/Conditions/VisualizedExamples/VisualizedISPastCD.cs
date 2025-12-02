using BehaviourGraph.Visualizer;
using BehaviourGraph.States;

namespace BehaviourGraph.Conditions
{
    public class VisualizedISPastCD : VisualizedCondition
    {
        public int leaf_ID;
        public System.Single duration;


        public override ICondition GetInstance(Sequence seq)
        {
            return new ISPastCD(seq.GetStates()[leaf_ID], duration);
        }
    }
}