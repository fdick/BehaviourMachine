using BehaviourGraph.Trees;

namespace BehaviourGraph.Visualizer
{
    public abstract class VisualizedHierarchyBranch : VisualizedLeaf, IVisualizedTree
    {
        public abstract ITree GetInstance(AIBehaviourGraph graph);
        public override ILeaf GetInstance()
        {
            UnityEngine.Debug.LogError($"{this} - is not a leaf!");
            return null;
        }
    }

}