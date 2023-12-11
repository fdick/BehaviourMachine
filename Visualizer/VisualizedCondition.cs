using BehaviourGraph.Conditions;
using BehaviourGraph.Trees;

namespace BehaviourGraph.Visualizer
{
    public abstract class VisualizedCondition : VisualizedForestObject, IVisualizedCondition
    {
        /// <summary>
        /// Get instance from visualized condition
        /// </summary>
        /// <param name="tree">Tree which contains this condition</param>
        /// <returns></returns>
        public abstract ICondition GetInstance(ITree tree);
    }
}