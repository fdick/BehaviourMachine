
using UnityEngine;
using BehaviourGraph.Visualizer;
using BehaviourGraph;
using BehaviourGraph.Trees;

namespace BehaviourGraph.Conditions
{
    public class VisualizedISEndedLeaf : VisualizedCondition
    {
        public int leaf_ID = 0;

        public override IConditional GetInstance(HierarchyBranch branch)
        {
            return new ISEndedLeaf(branch.Leafs[leaf_ID]);
        }
    }
}