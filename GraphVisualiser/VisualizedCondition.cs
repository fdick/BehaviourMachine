using BehaviourGraph.Trees;
using UnityEngine;

namespace BehaviourGraph.Visualizer
{
    public abstract class VisualizedCondition : MonoBehaviour
    {
        public abstract IConditional GetInstance(HierarchyBranch branch);
    }
}