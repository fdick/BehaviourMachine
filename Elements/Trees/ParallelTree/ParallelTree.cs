using System;
using System.Collections.Generic;
using System.Linq;
using BehaviourGraph.States;
using UnityEditor;

namespace BehaviourGraph.Trees
{
    /// <summary>
    /// This tree processes its leaves in parallel. Two or more processes can be running at the same time.
    /// The tree contains one main process and may contains two or more processes that will run simultaneously
    /// and in parallel with the main process. The tree will end when the main process ends.
    /// </summary>
    public class ParallelTree : ITree
    {
        public ParallelTree(BehaviourMachine graph, ILeaf mainLeaf, params ILeaf[] parallelLeafs)
        {
            ID = Guid.NewGuid();
            FriendlyName = nameof(ParallelTree);

            foreach (var p in parallelLeafs)
            {
                if (p is IEndableState ep)
                    UnityEngine.Debug.LogWarning(
                        $"{FriendlyName} is parallel Tree! And it is not support Endable Leaf - {p.FriendlyName}! ");
            }

            mainLeaf.SetGameobject(graph.CustomGameobject);
            foreach (var l in parallelLeafs)
                l.SetGameobject(graph.CustomGameobject);

            _mainLeaf = mainLeaf;
            _parallelLeafs = parallelLeafs.ToList();
            _graph = graph;
        }

        public string FriendlyName { get; set; }
        public Guid ID { get; }

        public bool IsPaused { get; private set; }
        public UpdateStatus Status { get; private set; } = UpdateStatus.Failure;
        public void AddState(IState state)
        {
            throw new NotImplementedException();
        }

        public void RemoveState(IState leaf)
        {
            throw new NotImplementedException();
        }

        public IState GetRunningState()
        {
            throw new NotImplementedException();
        }

        public IState GetStartableState()
        {
            throw new NotImplementedException();
        }

        public IState[] GetStates()
        {
            throw new NotImplementedException();
        }


        protected ILeaf _mainLeaf;
        protected List<ILeaf> _parallelLeafs;
        protected BehaviourMachine _graph;


        public void AwakeTree()
        {
            _mainLeaf.InitLeaf();
            foreach (var l in _parallelLeafs)
            {
                l.InitLeaf();
            }
        }

        public void InitTree()
        {
            throw new NotImplementedException();
        }

        public void StartTree()
        {
            foreach (var l in _parallelLeafs)
                l.EnterLeaf();

            _mainLeaf. EnterLeaf();
        }

        public void UpdateTree()
        {
            if (IsPaused)
                return;
            if (_mainLeaf == null)
                return;

            if (_mainLeaf is IUpdatableState updatable)
            {
                updatable.UpdateState();
            }

            foreach (var l in _parallelLeafs)
                if (l is IUpdatableState pfu)
                    pfu.UpdateState();
        }

        public void FixedUpdateTree()
        {
            if (IsPaused)
                return;
            if (_mainLeaf == null)
                return;

            if (_mainLeaf is IFixedUpdatableState fixedUpdatable)
            {
                fixedUpdatable.FixedUpdateState();
            }

            foreach (var l in _parallelLeafs)
                if (l is IFixedUpdatableState pfu)
                    pfu.FixedUpdateState();
        }

        public void LateUpdateTree()
        {
            // if (IsPaused)
                // return UpdateStatus.Failure;

            //late update
            if (_mainLeaf is ILateUpdatableState lateUpdatable)
            {
                lateUpdatable.LateUpdateState();
            }

            foreach (var l in _parallelLeafs)
                if (l is ILateUpdatableState pfu)
                    pfu.LateUpdateState();

            UpdateStatus treeStatus = UpdateStatus.Running;

            //end update
            if (_mainLeaf is IEndableState endable)
            {
                treeStatus = endable.EndCondition();
            }

            Status = treeStatus;
            // return treeStatus;
        }

        public void StopTree()
        {
            foreach (var l in _parallelLeafs)
                l.ExitLeaf();

            _mainLeaf.ExitLeaf();
        }

        public void PauseTree()
        {
            IsPaused = true;
        }

        public void UnPauseTree()
        {
            IsPaused = false;
        }

        public ILeaf GetMainLeaf() => _mainLeaf;
        public List<ILeaf> GetParallelLeafs() => _parallelLeafs;

        /// <summary>
        /// Add parallel leaf
        /// </summary>
        /// <param name="leaf"></param>
        public void AddLeaf(ILeaf leaf)
        {
            _parallelLeafs.Add(leaf);
        }

        public void SetMainLeaf(ILeaf leaf)
        {
            _mainLeaf = leaf;
        }

        /// <summary>
        /// Remove parallel leaf
        /// </summary>
        /// <param name="leaf"></param>
        public void RemoveLeaf(ILeaf leaf)
        {
            _parallelLeafs.Remove(leaf);
        }

        public ILeaf GetRunningLeaf() => _mainLeaf;

        public ILeaf GetStartableLeaf()
        {
            return _mainLeaf;
        }

        public ILeaf[] GetLeafs()
        {
            var m = new ILeaf[] { _mainLeaf };
            return _parallelLeafs.Concat(m).ToArray();
        }

        public BehaviourMachine GetGraph() => _graph;
        public T QState<T>() where T : class, IState
        {
            throw new NotImplementedException();
        }

        public IState QState(string tag)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Find a first parallel child leaf by type. Searching in all hierarchy.
        /// </summary>
        /// <typeparam name="T"> is finding type</typeparam>
        /// <returns></returns>
        // public T QState<T>() where T : class, ILeaf
        // {
        //     foreach (var l in _parallelLeafs)
        //     {
        //         if (l is T tLeaf)
        //             return tLeaf;
        //         if (l is ITree t)
        //         {
        //             var r = t.QState<T>();
        //             if (r != null)
        //                 return r;
        //         }
        //     }
        //
        //     return null;
        // }
        //
        // /// <summary>
        // /// Find a first parallel child leaf by tag. Searching in all hierarchy.
        // /// </summary>
        // /// <param name="tag">Tag of a leaf.</param>
        // public ILeaf QState(string tag)
        // {
        //     foreach (var l in _parallelLeafs)
        //     {
        //         if (string.Equals(l.Tag, tag))
        //             return l;
        //         if (l is ITree t)
        //         {
        //             var r = t.QState(tag);
        //             if (r != null)
        //                 return r;
        //         }
        //     }
        //
        //     return null;
        // }

        public void Dispose()
        {
            // TODO release managed resources here
        }

        public UpdateStatus EndCondition()
        {
            throw new NotImplementedException();
        }
    }
}