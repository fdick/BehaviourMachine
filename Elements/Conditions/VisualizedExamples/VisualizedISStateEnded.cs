using System;
using BehaviourGraph.Visualizer;
using BehaviourGraph.States;

namespace BehaviourGraph.Conditions
{
    public class VisualizedISStateEnded : VisualizedCondition
    {
        public string leafTag;
        public int leaf_ID = 0;

        public override ICondition GetInstance(Sequence seq)
        {
            if (leafTag != String.Empty)
                return new ISStateEnded(seq.QState(leafTag) as IEndableState);
            else
                return new ISStateEnded(seq.GetStates()[leaf_ID] as IEndableState);
        }
    }
    
    
}