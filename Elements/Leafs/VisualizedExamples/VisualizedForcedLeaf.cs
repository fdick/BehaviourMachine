using BehaviourGraph.Visualizer;
using UnityEngine;
using UnityEngine.Events;

namespace BehaviourGraph.Leafs
{
    public class VisualizedForcedLeaf : VisualizedLeaf
    {
        [SerializeField] private UnityEvent _onEnter;
        [SerializeField] private UnityEvent _onExit;


        public override ILeaf GetInstance()
        {
            return new ForcedLeaf(
                () => _onEnter?.Invoke(),
                () => _onExit?.Invoke());
        }
    }
}