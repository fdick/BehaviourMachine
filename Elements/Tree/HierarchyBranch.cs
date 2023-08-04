using System;
using UnityEngine;

namespace BehaviourGraph.Trees
{
    /// <summary>
    /// This branch processes its leaves hierarchically. Only one process can run at a time. Can be a leaf and a tree. Contain global, local and end links.
    /// Need dispose after finishing.
    /// </summary>
    public class HierarchyBranch : HierarchyTree, ILeaf
    {
        public HierarchyBranch(AIBehaviourGraph graph, bool resetStateAtStart = true) : base(graph, resetStateAtStart)
        {
        }

        public HierarchyBranch(AIBehaviourGraph graph, ILeaf[] leafs, bool resetStateAtStart = true) : base(graph, leafs, resetStateAtStart)
        {
        }

        public Action<ConditionData> OnStarting { get; set; }
        public Action OnEnded { get; set; }
        protected GameObject _gameObject;
        private float _lastProcCD;


        /// <summary>
        /// Called when init Behaviour Graph
        /// </summary>
        public void OnAwake()
        {

        }

        public void OnStart(ConditionData condData = null)
        {
            OnStarting?.Invoke(condData);
            _lastProcCD = Time.time;
            OnStarted(condData);
            StartTree();
        }

        public void OnEnd()
        {
            OnEnding();
            EndTree();
            OnEnded?.Invoke();
        }


        public virtual UpdateStatus OnUpdate()
        {
            return UpdateTree();
        }

        public void SetGameobject(GameObject go) => _gameObject = go;

        public override void Dispose()
        {
            base.Dispose();
            OnStarting = null;
            OnEnded = null;
        }

        protected virtual void OnStarted(ConditionData activatedLink = null) { }
        protected virtual void OnEnding() { }

        public bool CheckCD(float duration)
        {
            return Time.time >= _lastProcCD + duration || _lastProcCD == 0;
        }
    }

}