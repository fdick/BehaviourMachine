using System;
using UnityEngine;

namespace BehaviourGraph.Trees
{
    public class ParallelBranch : ParallelTree, ILeaf, IDisposable
    {
        public ParallelBranch(AIBehaviourGraph graph, ILeaf mainLeaf, params ILeaf[] parallelLeafs) : base(graph, mainLeaf, parallelLeafs)
        {
        }

        public Action<ConditionData> OnStarting { get; set; }
        public Action OnEnded { get; set; }
        protected GameObject gameObject;
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
            StartTree();
            OnStarted(condData);
        }

        public void OnEnd()
        {
            OnEnding();
            EndTree();
            OnEnded?.Invoke();
        }


        public LeafStatus OnUpdate()
        {
            return UpdateTree();
        }

        public void SetGameobject(GameObject go) => gameObject = go;

        public void Dispose()
        {
            OnStarting = null;
            OnEnded = null;
        }

        protected virtual void OnStarted(ConditionData condData = null) { }
        protected virtual void OnEnding() { }

        public bool CheckCD(float duration)
        {
            return Time.time >= _lastProcCD + duration || _lastProcCD == 0;
        }
    }
}