using BehaviourGraph.Trees;
using UnityEngine;

namespace BehaviourGraph.Visualizer
{
    public abstract class VisualizedBranch : MonoBehaviour
    {
        [field:SerializeField] public string FriendlyName { get; private set; }
        public abstract HierarchyBranch GetInstance(AIBehaviourGraph graph);
    }
}