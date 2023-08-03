using System;
using System.Collections.Generic;
using System.Linq;

namespace BehaviourGraph.Trees
{
    /// <summary>
    /// This tree processes its leaves in parallel. Two or more processes can be running at the same time.
    /// The tree contains one main process and may contain two or more processes that will run simultaneously
    /// and in parallel with the main process. The tree will end when the main process ends.
    /// </summary>
    public class ParallelTree : ITree
    {
        public ParallelTree(AIBehaviourGraph graph, ILeaf mainLeaf, params ILeaf[] parallelLeafs)
        {
            mainLeaf.SetGameobject(graph.CustomGameobject);
            foreach (var l in parallelLeafs)
                l.SetGameobject(graph.CustomGameobject);

            _mainLeaf = mainLeaf;
            _parallelLeafs = parallelLeafs.ToList();
            _graph = graph;
            FriendlyName = nameof(ParallelTree);


        }

        public string FriendlyName { get; set; }

        public bool IsPaused { get; private set; }

        public UpdateStatus Status { get; private set; } = UpdateStatus.Failure;

        private ILeaf _mainLeaf;
        private List<ILeaf> _parallelLeafs;
        private AIBehaviourGraph _graph;

        public void StartTree()
        {
            foreach (var l in _parallelLeafs)
                l.OnStart();

            _mainLeaf.OnStart();
        }

        public UpdateStatus UpdateTree()
        {
            if (IsPaused)
                return UpdateStatus.Failure;

            var status = _mainLeaf.OnUpdate();

            foreach (var l in _parallelLeafs)
                l.OnUpdate();

            Status = status;

            return status;
        }

        public void EndTree()
        {
            foreach (var l in _parallelLeafs)
                l.OnEnd();

            _mainLeaf.OnEnd();
        }

        public void PauseTree() { IsPaused = true; }
        public void UnPauseTree() { IsPaused = false; }

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


        public AIBehaviourGraph GetGraph() => _graph;
    }
}
