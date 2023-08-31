using System;
using UnityEngine;

namespace BehaviourGraph.Trees
{
    /// <summary>
    /// This branch processes its leaves in parallel. Two or more processes can be running at the same time.
    /// The branch contains one main process and may contain two or more processes that will run simultaneously
    /// and in parallel with the main process. The tree will end when the main process ends. Can be a leaf and a tree. Need dispose after finishing.
    /// </summary>
    public class ParallelBranch : ParallelTree, ILeaf, IDisposable
    {
        public ParallelBranch(AIBehaviourGraph graph, ILeaf mainLeaf, params ILeaf[] parallelLeafs) : base(graph,
            mainLeaf, parallelLeafs)
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
            AwakeTree();
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


        public UpdateStatus OnUpdate()
        {
            return UpdateTree();
        }

        public void SetGameobject(GameObject go) => gameObject = go;

        public void Dispose()
        {
            OnStarting = null;
            OnEnded = null;
        }

        protected virtual void OnStarted(ConditionData condData = null)
        {
        }

        protected virtual void OnEnding()
        {
        }

        public bool CheckCD(float duration)
        {
            return Time.time >= _lastProcCD + duration || _lastProcCD == 0;
        }
    }
}