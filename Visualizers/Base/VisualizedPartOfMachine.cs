using BehaviourGraph.Trees;
using System;
using BehaviourGraph.Conditions;
using BehaviourGraph.States;
using UnityEngine;
using UnityEngine.Events;

namespace BehaviourGraph.Visualizer
{
    [Serializable]
    public abstract class VisualizedPartOfMachine : MonoBehaviour
    {
        [field: SerializeField] public string FriendlyName { get; set; }
        
        
        [field: Header("Events")]
        [field: SerializeField] public UnityEvent OnStartState { get; set; }
        [field: SerializeField] public UnityEvent OnEndState { get; set; }
    }

    [Serializable]
    public abstract class VisualizedTree : VisualizedPartOfMachine, IVisualizedTree
    {
        public abstract ITree GetInstance(BehaviourMachine graph);
    }

    [Serializable]
    public abstract class VisualizedState : VisualizedPartOfMachine, IVisualizedState
    {
        [field: SerializeField] public string Tag { get; set; }
        public abstract IState GetInstance(BehaviourMachine graph);
    }


    public interface IVisualizedPartOfMachine
    {
        public string FriendlyName { get; set; }
    }

    public interface IVisualizedTree : IVisualizedPartOfMachine
    {
        public ITree GetInstance(BehaviourMachine graph);
    }

    public interface IVisualizedState : IVisualizedPartOfMachine
    {
        public string Tag { get; set; }
        public IState GetInstance(BehaviourMachine graph);
    }

    public interface IVisualizedCondition : IVisualizedPartOfMachine
    {
        public ICondition GetInstance(Sequence seq);
    }
}