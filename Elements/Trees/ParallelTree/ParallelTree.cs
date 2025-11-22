using System;
using System.Collections.Generic;
using System.Linq;
using BehaviourGraph.Leafs;
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
                if (p is IEndableLeaf ep)
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

        public void StartTree()
        {
            foreach (var l in _parallelLeafs)
                l.EnterLeaf();

            _mainLeaf.EnterLeaf();
        }

        public void UpdateTree()
        {
            if (IsPaused)
                return;
            if (_mainLeaf == null)
                return;

            if (_mainLeaf is IUpdatableLeaf updatable)
            {
                updatable.UpdateLeaf();
            }

            foreach (var l in _parallelLeafs)
                if (l is IUpdatableLeaf pfu)
                    pfu.UpdateLeaf();
        }

        public void FixedUpdateTree()
        {
            if (IsPaused)
                return;
            if (_mainLeaf == null)
                return;

            if (_mainLeaf is IFixedUpdatableLeaf fixedUpdatable)
            {
                fixedUpdatable.FixedUpdateLeaf();
            }

            foreach (var l in _parallelLeafs)
                if (l is IFixedUpdatableLeaf pfu)
                    pfu.FixedUpdateLeaf();
        }

        public UpdateStatus LateUpdateTree()
        {
            if (IsPaused)
                return UpdateStatus.Failure;

            //late update
            if (_mainLeaf is ILateUpdatableLeaf lateUpdatable)
            {
                lateUpdatable.LateUpdateLeaf();
            }

            foreach (var l in _parallelLeafs)
                if (l is ILateUpdatableLeaf pfu)
                    pfu.LateUpdateLeaf();

            UpdateStatus treeStatus = UpdateStatus.Running;

            //end update
            if (_mainLeaf is IEndableLeaf endable)
            {
                treeStatus = endable.EndCondition();
            }

            Status = treeStatus;
            return treeStatus;
        }

        public void EndTree()
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

        /// <summary>
        /// Find a first parallel child leaf by type. Searching in all hierarchy.
        /// </summary>
        /// <typeparam name="T"> is finding type</typeparam>
        /// <returns></returns>
        public T QLeaf<T>() where T : class, ILeaf
        {
            foreach (var l in _parallelLeafs)
            {
                if (l is T tLeaf)
                    return tLeaf;
                if (l is ITree t)
                {
                    var r = t.QLeaf<T>();
                    if (r != null)
                        return r;
                }
            }

            return null;
        }

        /// <summary>
        /// Find a first parallel child leaf by tag. Searching in all hierarchy.
        /// </summary>
        /// <param name="tag">Tag of a leaf.</param>
        public ILeaf QLeaf(string tag)
        {
            foreach (var l in _parallelLeafs)
            {
                if (string.Equals(l.Tag, tag))
                    return l;
                if (l is ITree t)
                {
                    var r = t.QLeaf(tag);
                    if (r != null)
                        return r;
                }
            }

            return null;
        }
    }
}