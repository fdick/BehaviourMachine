using BehaviourGraph.Visualizer;
using UnityEngine;
using UnityEngine.Events;

namespace BehaviourGraph.States
{
    public class VisualizedForcedState : VisualizedState
    {
        [SerializeField] private UnityEvent _onEnter;
        [SerializeField] private UnityEvent _onExit;
        [SerializeField] private string _tag;


        public override IState GetInstance(BehaviourMachine graph)
        {
            return new State(() => _onEnter?.Invoke(), () => _onExit?.Invoke(), _tag);
        }
    }
}