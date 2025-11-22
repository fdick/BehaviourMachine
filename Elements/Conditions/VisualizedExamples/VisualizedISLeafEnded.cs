using System;
using BehaviourGraph.Visualizer;
using BehaviourGraph.Leafs;
using BehaviourGraph.Trees;

namespace BehaviourGraph.Conditions
{
    public class VisualizedISLeafEnded : VisualizedCondition
    {
        public string leafTag;
        public int leaf_ID = 0;

        public override ICondition GetInstance(ITree tree)
        {
            if (leafTag != String.Empty)
                return new ISLeafEnded(tree.QLeaf(leafTag) as IEndableLeaf);
            else
                return new ISLeafEnded(tree.GetLeafs()[leaf_ID] as IEndableLeaf);
        }
    }
    
    
}