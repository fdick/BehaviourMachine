using BehaviourGraph.Trees;

namespace BehaviourGraph.Visualizer
{
    public class DefaultTreeInspector : Inspector<ITree>
    {
        public override string Visualize(ITree node)
        {
            return $"Running Leaf: {node.GetRunningState().FriendlyName}";
        }
    }

}