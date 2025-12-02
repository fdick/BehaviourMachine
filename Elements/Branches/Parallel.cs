using System;
using BehaviourGraph.Trees;
using UnityEngine;

namespace BehaviourGraph.States
{
    public class Parallel : IState, IUpdatableState, IFixedUpdatableState, ILateUpdatableState, IEndableState
    {
        public string FriendlyName { get; set; }
        public Guid ID { get; }
        public string Tag { get; set; }
        public Action<Transition> OnEnter { get; set; }
        public Action OnExit { get; set; }

        protected IState _mainState;
        protected IState _parallelState;

        public IState MainState => _mainState;
        public IState ParallelState => _parallelState;
        
        public Parallel(Sequence mainState, Sequence parallelState)
        {
            ID = Guid.NewGuid();
            FriendlyName = nameof(Parallel);
            
            _mainState = mainState;
            _parallelState = parallelState;
        }

        public void Enter(Transition transition = null)
        {
            OnEnter?.Invoke(transition);
            
            _mainState.Enter(transition);
            _parallelState.Enter();
        }

        public void Exit()
        {
            _mainState.Exit();
            _parallelState.Exit();
            
            OnExit?.Invoke();
        }

        public void SetGameobject(GameObject go)
        {
            return;
        }

        public bool CheckCD(float duration)
        {
            return _mainState.CheckCD(duration);
        }

        public float GetRemainingCD()
        {
            return _mainState.GetRemainingCD();
        }

        public void InitializeState()
        {
            _mainState.InitializeState();
            _parallelState.InitializeState();
        }

        public void UpdateState()
        {
            if (_mainState is IUpdatableState u)
                u.UpdateState();

            if (_parallelState is IUpdatableState pu)
                pu.UpdateState();
        }

        public void FixedUpdateState()
        {
            if (_mainState is IFixedUpdatableState u)
                u.FixedUpdateState();

            if (_parallelState is IFixedUpdatableState pu)
                pu.FixedUpdateState();
        }

        public void LateUpdateState()
        {
            if (_mainState is ILateUpdatableState u)
                u.LateUpdateState();

            if (_parallelState is ILateUpdatableState pu)
                pu.LateUpdateState();
        }

        public void Dispose()
        {
            _mainState.Dispose();
            _parallelState.Dispose();
        }

        public UpdateStatus EndCondition()
        {
            if (_mainState is IEndableState eu)
                return eu.EndCondition();

            return UpdateStatus.Failure;
        }
    }
}