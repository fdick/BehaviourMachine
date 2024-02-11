using System;
using BehaviourGraph.Leafs;
using UnityEngine;

namespace BehaviourGraph.Trees
{
    /// <summary>
    /// This branch processes its leaves in parallel. Two or more processes can be running at the same time.
    /// The branch contains one main process and may contains two or more processes that will run simultaneously
    /// and in parallel with the main process. The tree will end when the main process ends. Can be a leaf and a tree. Need dispose after finishing.
    /// </summary>
    public class ParallelBranch : ParallelTree, ILeaf, IUpdatableLeaf, IFixedUpdatableLeaf, ILateUpdatableLeaf,
        IDisposable
    {
        public ParallelBranch(BehaviourMachine graph, ILeaf mainLeaf, params ILeaf[] parallelLeafs) : base(graph,
            mainLeaf, parallelLeafs)
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

        public void Dispose()
        {
            OnEnter = null;
            OnExit = null;
        }

        protected virtual void OnEnterBranch(Transition condData = null)
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