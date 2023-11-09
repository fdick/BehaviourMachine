using UnityEngine;
using BehaviourGraph.Visualizer;
using BehaviourGraph;
using BehaviourGraph.Trees;

namespace BehaviourGraph.Conditions
{
    public class VisualizedISPastCD : VisualizedCondition
    {
        public int leaf_ID;
        public System.Single duration;


        public override ICondition GetInstance(ITree tree)
        {
            return new ISPastCD(tree.GetLeafs()[leaf_ID], duration);
        }
    }
}