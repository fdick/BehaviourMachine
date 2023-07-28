using System;
using System.Linq;

namespace BehaviourGraph.Trees
{
    public class ParallelTree : ITree
    {

        public ParallelTree(AIBehaviourGraph graph, ILeaf mainLeaf, params ILeaf[] parallelLeafs)
        {
            mainLeaf.SetGameobject(graph.CustomGameobject);
            foreach (var l in parallelLeafs)
                l.SetGameobject(graph.CustomGameobject);

            _mainLeaf = mainLeaf;
            _parallelLeafs = parallelLeafs;
            _graph = graph;
            FriendlyName = "ParallelTree";


        }

        public string FriendlyName { get; set; }
        private ILeaf _mainLeaf;
        private ILeaf[] _parallelLeafs;
        private AIBehaviourGraph _graph;

        public void StartTree()
        {
            foreach (var l in _parallelLeafs)
                l.OnStart();

            _mainLeaf.OnStart();
        }

        public LeafStatus UpdateTree()
        {
            foreach (var l in _parallelLeafs)
                l.OnUpdate();

            return _mainLeaf.OnUpdate();
        }

        public void EndTree()
        {
            foreach (var l in _parallelLeafs)
                l.OnEnd();

            _mainLeaf.OnEnd();
        }

        public void PauseTree() { }
        public void UnPauseTree() { }

        public ILeaf GetMainLeaf() => _mainLeaf;
        public ILeaf[] GetParallelingLeafs() => _parallelLeafs;

        public void AddLeaf(ILeaf leaf)
        {
        }

        public void RemoveLeaf(ILeaf leaf)
        {
        }

        public ILeaf GetRunningLeaf() => _mainLeaf;

        public ILeaf GetStartableLeaf()
        {
            return null;
        }


        public ILeaf[] GetLeafs()
        {
            var m = new ILeaf[] { _mainLeaf };
           return _parallelLeafs.Concat(m).ToArray();
        }


        public AIBehaviourGraph GetGraph() => _graph;
    }
}
