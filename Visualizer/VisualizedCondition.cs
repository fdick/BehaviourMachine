using BehaviourGraph.Trees;

namespace BehaviourGraph.Visualizer
{
    public abstract class VisualizedCondition : VisualizedObject, IVisualizedCondition
    {
        /// <summary>
        /// Get instance from visualized condition
        /// </summary>
        /// <param name="tree">Tree which contains this condition</param>
        /// <returns></returns>
        public abstract IConditional GetInstance(ITree tree);
    }
}