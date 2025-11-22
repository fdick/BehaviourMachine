using BehaviourGraph.Trees;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourGraph.Visualizer
{
    [Serializable]
    public class VisualizedEmptyParallelBranch : VisualizedLeaf, IVisualizedTree
    {
        public VisualizedLeaf mainLeaf;
        public List<VisualizedLeaf> _parallelLeafs = new List<VisualizedLeaf>();


        public ITree GetInstance(BehaviourMachine graph)
        {
            //main leaf
            var mainLf = mainLeaf is IVisualizedTree mt ? (ILeaf)mt.GetInstance(graph) : mainLeaf.GetInstance();

            //set custom name for main leaf
            if (mainLeaf.FriendlyName != string.Empty)
                mainLf.FriendlyName = mainLeaf.FriendlyName;

            //parallel leafs
            var lfs = new ILeaf[_parallelLeafs.Count];
            for (int i = 0; i < _parallelLeafs.Count; i++)
            {
                //if child leaf is tree
                if (_parallelLeafs[i] is IVisualizedTree lt)
                {
                    var childTree = lt.GetInstance(graph);

                    lfs[i] = (ILeaf)childTree;
                }
                else
                {
                    lfs[i] = _parallelLeafs[i].GetInstance();
                }

                //leaf events
                if (_parallelLeafs[i].OnStartLeaf != null)
                {
                    int savedI = i;
                    lfs[i].OnEnter += (t) => _parallelLeafs[savedI].OnStartLeaf.Invoke();
                }
                if (_parallelLeafs[i].OnEndLeaf != null)
                {
                    int savedI = i;
                    lfs[i].OnExit += () => _parallelLeafs[savedI].OnEndLeaf.Invoke();
                }

                //set custom names for parallel leafs
                if (_parallelLeafs[i].FriendlyName != string.Empty)
                    lfs[i].FriendlyName = _parallelLeafs[i].FriendlyName;
            }

            var instance = new ParallelBranch(graph, mainLf, lfs);

            if (OnStartLeaf != null)
                instance.OnEnter += (t) => OnStartLeaf?.Invoke();

            if (OnEndLeaf != null)
                instance.OnExit += () => OnEndLeaf?.Invoke();

            //set custom name for myself
            if (FriendlyName != string.Empty)
                instance.FriendlyName = FriendlyName;

            return instance;
        }

        public override ILeaf GetInstance()
        {
            UnityEngine.Debug.LogError("I am not a leaf!");
            return null;
        }


        private string _mainLeafName = "Main";
        private string _parallelLefsName = "Parallels";


        [InspectorButton("Add Main Branch")]
        public void AddVisualizedMainBranch()
        {
            var parent = transform.Find(_mainLeafName)?.gameObject;
            if (parent == null)
            {
                parent = new GameObject(_mainLeafName);
                parent.transform.SetParent(transform);
                parent.transform.localPosition = Vector3.zero;
            }

            var go = new GameObject("MainBranch");
            VisualizedEmptyHierarchyBranch branchComp =
                (VisualizedEmptyHierarchyBranch)go.AddComponent(typeof(VisualizedEmptyHierarchyBranch));
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;

            mainLeaf = branchComp;
        }

        [InspectorButton("Add Parallel Leaf")]
        public void AddVisualizedParallelLeaf()
        {
            var parent = transform.Find(_parallelLefsName)?.gameObject;
            if (parent == null)
            {
                parent = new GameObject(_parallelLefsName);
                parent.transform.SetParent(transform);
                parent.transform.localPosition = Vector3.zero;
            }

            var go = new GameObject("ParallelLeaf");
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;
        }

        [InspectorButton("Get Paralell Leafs")]
        public void GetVisualizedParallelLeafs()
        {
            _parallelLeafs.Clear();

            List<VisualizedLeaf> ls = new List<VisualizedLeaf>();
            foreach (Transform c in transform)
            {
                if (c.name == _parallelLefsName)
                {
                    for (int i = 0; i < c.childCount; i++)
                    {
                        var c2 = c.GetChild(i);
                        if (c2.TryGetComponent<VisualizedLeaf>(out var outLeaf))
                            ls.Add(outLeaf);
                    }
                }
                else
                {
                    var cL = c.Find(_parallelLefsName);
                    if (cL == null)
                        continue;

                    for (int i = 0; i < cL.childCount; i++)
                    {
                        var c2 = cL.GetChild(i);
                        if (c2.TryGetComponent<VisualizedLeaf>(out var outLeaf))
                            ls.Add(outLeaf);
                    }
                }
            }

            _parallelLeafs.AddRange(ls);
        }
    }
}