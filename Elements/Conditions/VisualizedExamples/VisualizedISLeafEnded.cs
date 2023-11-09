using BehaviourGraph.Visualizer;
using BehaviourGraph.Leafs;
using BehaviourGraph.Trees;

namespace BehaviourGraph.Conditions
{
    public class VisualizedISLeafEnded : VisualizedCondition
    {
        public int leaf_ID = 0;

        public override ICondition GetInstance(ITree tree)
        {
            return new ISLeafEnded(tree.GetLeafs()[leaf_ID] as IEndableLeaf);
        }
    }
}