
using UnityEngine;
using BehaviourGraph.Visualizer;
using BehaviourGraph;
using BehaviourGraph.Trees;

public class VisualizedISPastCD : VisualizedCondition
{
    public int leaf_ID;
    public System.Single duration;


    public override IConditional GetInstance(HierarchyBranch branch)
    {
        return new ISPastCD(branch.Leafs[leaf_ID], duration);
    }
}