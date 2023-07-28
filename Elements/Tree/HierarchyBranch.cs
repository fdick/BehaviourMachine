using System;
using UnityEngine;

namespace BehaviourGraph.Trees
{
    /// <summary>
    /// Branch can be a leaf and can be a tree. Can be added to childs to other tree.
    /// </summary>
    public class HierarchyBranch : HierarchyTree, ILeaf
    {
        public HierarchyBranch(AIBehaviourGraph graph) : base(graph)
        {
        }

        public HierarchyBranch(AIBehaviourGraph graph, ILeaf[] leafs) : base(graph, leafs)
        {
        }

        public Action<ConditionData> OnStarting { get; set; }
        public Action OnEnded { get; set; }
        protected GameObject gameObject;
        private float _lastProcCD;


        /// <summary>
        /// Called when inin Behaviour Graph
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


        public virtual LeafStatus OnUpdate()
        {
            return UpdateTree();
        }

        public void SetGameobject(GameObject go) => gameObject = go;

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