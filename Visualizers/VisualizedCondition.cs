using BehaviourGraph.Conditions;
using BehaviourGraph.States;
using BehaviourGraph.Trees;

namespace BehaviourGraph.Visualizer
{
    public abstract class VisualizedCondition : VisualizedPartOfMachine, IVisualizedCondition
    {
        /// <summary>
        /// Get instance from visualized condition
        /// </summary>
        /// <param name="seq">Tree which contains this condition</param>
        /// <returns></returns>
        public abstract ICondition GetInstance(Sequence seq);
    }
}