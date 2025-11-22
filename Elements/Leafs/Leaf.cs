using System;
using System.Runtime.InteropServices.WindowsRuntime;
using BehaviourGraph.Trees;
using UnityEditor;
using UnityEngine;

namespace BehaviourGraph
{
    public class Leaf : ILeaf, IDisposable
    {
        public string FriendlyName { get; set; }
        public string Tag { get; set; }
        public Guid ID { get; }
        protected GameObject _gameObject;
        protected bool _isRunning;
        protected float _lastProcCD;

        public Leaf(string tag = null)
        {
            ID = Guid.NewGuid();
            FriendlyName = this.ToString();
            Tag = tag;
        }

        public Leaf(Action onEnter, Action onExit, string tag = null)
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


        public virtual void InitLeaf()
        {
        }

        public virtual void EnterLeaf(Transition transition)
        {
            OnEnter?.Invoke(transition);
            _isRunning = true;

            if (transition != null && transition.CooldownDuration > 0)
            {
                if (transition.CooldownType == CoolDownTypes.OnEnterDestinationLeaf)
                    transition.SetCooldownTime();
                else
                    _executedTransition = transition;
            }
        }

        public virtual void ExitLeaf()
        {
            if (_executedTransition != null && _executedTransition.CooldownDuration > 0)
            {
                if (_executedTransition.CooldownType == CoolDownTypes.OnExitDestinationLeaf)
                {
                    _executedTransition.SetCooldownTime();
                    _executedTransition = null;
                }
            }

            _lastProcCD = Time.time;
            _isRunning = false;
            OnExit?.Invoke();
        }

        public void SetGameobject(GameObject go)
        {
            if(!go)
                return;
            _gameObject = go;
        }

        public void Dispose()
        {
            OnEnter = null;
            OnExit = null;
        }

        public bool CheckCD(float duration)
        {
            return !_isRunning && (Time.time >= _lastProcCD + duration || _lastProcCD == 0);
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(FriendlyName) ? this.GetType().ToString() : FriendlyName;
        }
    }
}