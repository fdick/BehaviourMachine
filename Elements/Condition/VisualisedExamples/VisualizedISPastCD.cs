
using UnityEngine;
using BehaviourGraph.Visualizer;
using BehaviourGraph;
using BehaviourGraph.Trees;

public class VisualizedISPastCD : VisualizedCondition
{
    public int leaf_ID;
    public System.Single duration;


    public override IConditional GetInstance(ITree tree)
    {
        return new ISPastCD(tree.GetLeafs()[leaf_ID], duration);
    }
}