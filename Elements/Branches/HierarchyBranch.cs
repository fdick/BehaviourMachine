using System;
using BehaviourGraph.Leafs;
using UnityEngine;

namespace BehaviourGraph.Trees
{
    /// <summary>
    /// This branch processes its leaves hierarchically. Only one process can run at a time. Can be a leaf and a tree. Contains global, local and end links.
    /// Need dispose after finishing.
    /// </summary>
    public class HierarchyBranch : HierarchyTree, ILeaf, IUpdatableLeaf, IFixedUpdatableLeaf, ILateUpdatableLeaf
    {
        public HierarchyBranch(BehaviourMachine graph, bool resetStateAtStart = true) : base(graph, resetStateAtStart)
        {
        }

        public HierarchyBranch(BehaviourMachine graph, ILeaf[] leafs, bool resetStateAtStart = true) : base(graph,
            leafs, resetStateAtStart)
        {
        }

        public string Tag { get; set; }
        public Action<Transition> OnEnter { get; set; }
        public Action OnExit { get; set; }
        protected GameObject _gameObject;
        private float _lastProcCD;
        private Transition _executedTransition;


        /// <summary>
        /// Called when init Behaviour Graph
        /// </summary>
        public void InitLeaf()
        {
            AwakeTree();
            OnInitBranch();
        }

        public void EnterLeaf(Transition transition = null)
        {
            OnEnter?.Invoke(transition);
            OnEnterBranch(transition);
            StartTree();
            
            if (transition != null && transition.CooldownDuration > 0)
            {
                if (transition.CooldownType == CoolDownTypes.OnEnterDestinationLeaf)
                    transition.SetCooldownTime();
                else
                    _executedTransition = transition;
            }
        }

        public void ExitLeaf()
        {
            if (_executedTransition != null && _executedTransition.CooldownDuration > 0)
            {
                if (_executedTransition.CooldownType == CoolDownTypes.OnExitDestinationLeaf)
                {
                    _executedTransition.SetCooldownTime();
                    _executedTransition = null;
                }
            }
            OnExitBranch();
            EndTree();
            _lastProcCD = Time.time;
            OnExit?.Invoke();
        }

        public void UpdateLeaf()
        {
            UpdateTree();
            OnUpdateBranch();
        }

        public void FixedUpdateLeaf()
        {
            FixedUpdateTree();
            OnFixedUpdateBranch();
        }

        public void LateUpdateLeaf()
        {
            LateUpdateTree();
            OnLateUpdateBranch();
        }


        public void SetGameobject(GameObject go) => _gameObject = go;

        public override void Dispose()
        {
            base.Dispose();
            OnEnter = null;
            OnExit = null;
        }

        protected virtual void OnInitBranch()
        {
        }
        protected virtual void OnEnterBranch(Transition activatedLink = null)
        {
        }
        
        protected virtual void OnUpdateBranch()
        {
        }
        
        protected virtual void OnFixedUpdateBranch()
        {
        }
        
        protected virtual void OnLateUpdateBranch()
        {
        }
        
        protected virtual void OnExitBranch()
        {
        }

        public bool CheckCD(float duration)
        {
            return Time.time >= _lastProcCD + duration || _lastProcCD == 0;
        }
    }
}