
using UnityEngine;
using BehaviourGraph.Visualizer;
using BehaviourGraph;
using BehaviourGraph.Trees;

namespace BehaviourGraph.Conditions
{
    public class VisualizedISEndedLeaf : VisualizedCondition
    {
        public int leaf_ID = 0;

        public override IConditional GetInstance(ITree tree)
        {
            return new ISEndedLeaf(tree.GetLeafs()[leaf_ID]);
        }
    }
}