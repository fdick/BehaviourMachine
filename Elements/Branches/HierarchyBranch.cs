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

        public Action<Transition> OnEnter { get; set; }
        public Action OnExit { get; set; }
        protected GameObject _gameObject;
        private float _lastProcCD;


        /// <summary>
        /// Called when init Behaviour Graph
        /// </summary>
        public void InitLeaf()
        {
            AwakeTree();
        }

        public void EnterLeaf(Transition condData = null)
        {
            OnEnter?.Invoke(condData);
            OnEnterBranch(condData);
            StartTree();
        }

        public void ExitLeaf()
        {
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