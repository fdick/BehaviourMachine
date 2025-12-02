using System;
using BehaviourGraph.States;

namespace BehaviourGraph.Trees
{
    public interface ITree : IPartOfMachine, IEndableState, IDisposable
    {
        public void InitTree();
        
        public void StartTree();
        public void StopTree();
        
        public void UpdateTree();
        public void FixedUpdateTree();
        public void LateUpdateTree();
        
        public void PauseTree();
        public void UnPauseTree();
        public bool IsPaused { get; }

        
        public UpdateStatus Status { get; }
        public void AddState(IState state);
        public void RemoveState(IState leaf);
        public IState GetRunningState();
        public IState GetStartableState();
        public IState[] GetStates();
        public BehaviourMachine GetGraph();
        public T QState<T>() where T : class, IState;
        public IState QState(string tag);
    }
}
