using System;
using BehaviourGraph.States;
using BehaviourGraph.Trees;
using UnityEngine;

namespace BehaviourGraph
{
    public class State : IState
    {
        public string FriendlyName { get; set; }
        public string Tag { get; set; }
        public Guid ID { get; }
        protected GameObject _gameObject;
        protected bool _isRunning;
        protected float _lastProcCD;

        public State(string tag = null)
        {
            ID = Guid.NewGuid();
            FriendlyName = this.ToString();
            Tag = tag;
        }

        public State(Action onEnter, Action onExit, string tag = null)
        {
            ID = Guid.NewGuid();
            FriendlyName = this.ToString();
            OnEnter += (c) => onEnter?.Invoke();
            OnExit += () => onExit?.Invoke();
            Tag = tag;
        }

        public Action<Transition> OnEnter { get; set; }
        public Action OnExit { get; set; }

        private Transition _executedTransition;




        public virtual void InitializeState()
        {
        }

        public void Enter(Transition transition = null)
        {
            OnEnter?.Invoke(transition);
            OnEnterState(transition);
            _isRunning = true;

            if (transition != null && transition.CooldownDuration > 0)
            {
                if (transition.CooldownType == CoolDownTypes.OnEnterDestinationState)
                    transition.SetCooldownTime();
                else
                    _executedTransition = transition;
            }
        }

        public void Exit()
        {
            if (_executedTransition != null && _executedTransition.CooldownDuration > 0)
            {
                if (_executedTransition.CooldownType == CoolDownTypes.OnExitDestinationState)
                {
                    _executedTransition.SetCooldownTime();
                    _executedTransition = null;
                }
            }

            _lastProcCD = Time.time;
            _isRunning = false;
            OnExitState();
            OnExit?.Invoke();
        }


        public void SetGameobject(GameObject go)
        {
            if (!go)
                return;
            _gameObject = go;
        }

        public virtual void Dispose()
        {
            OnEnter = null;
            OnExit = null;
        }

        public bool CheckCD(float duration)
        {
            return !_isRunning && (Time.time >= _lastProcCD + duration || _lastProcCD == 0);
        }
        
        public float GetRemainingCD()
        {
            if (!_isRunning)
                return 0;
            return Time.time - _lastProcCD;
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(FriendlyName) ? this.GetType().ToString() : FriendlyName;
        }

        protected virtual void OnEnterState(Transition transition)
        {
        }

        protected virtual void OnExitState()
        {
        }
    }
}