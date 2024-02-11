using BehaviourGraph.Trees;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourGraph.Visualizer
{
    [Serializable]
    public class VisualizedEmptyHierarchyBranch : VisualizedLeaf, IVisualizedTree
    {
        [SerializeField] private bool _resetStateAtStart = true;
        [SerializeField] private List<VisualizedLeaf> _leafs = new List<VisualizedLeaf>();
        [SerializeField] private List<VisualizedLink> _links = new List<VisualizedLink>();

        [SerializeField] private int _startableLeafID = 0;

        private const string VIS_LEAFS_NAME = "Leafs";
        private const string VIS_LINKS_NAME = "Links";


        public ITree GetInstance(BehaviourMachine graph)
        {
            //add leafs
            var lfs = new ILeaf[_leafs.Count];
            for (int i = 0; i < _leafs.Count; i++)
            {
                if (_leafs[i] == null)
                    throw new NullReferenceException($"{graph.name} graph: At {i} array position Leaf is null!");

                //if child leaf is tree
                if (_leafs[i] is IVisualizedTree lt)
                {
                    var childTree = lt.GetInstance(graph);

                    lfs[i] = (ILeaf)childTree;
                }
                else
                {
                    lfs[i] = _leafs[i].GetInstance();
                }

                //set custom name
                if (_leafs[i].FriendlyName != string.Empty)
                    lfs[i].FriendlyName = _leafs[i].FriendlyName;
            }

            var instance = new HierarchyBranch(graph, lfs, _resetStateAtStart);

            //set custom name for myself
            if (FriendlyName != string.Empty)
                instance.FriendlyName = FriendlyName;

            //add links
            foreach (var li in _links)
            {
                if (li == null)
                    throw new NullReferenceException($"{graph.name} graph: One of the Links is null!");

                var to = instance.Leafs[_leafs.IndexOf(li.to)];
                var condition = li.condition?.GetInstance(instance);

                if (li.linkType is LinkType.Ended or LinkType.FromTo)
                {
                    for (int i = 0; i < li.froms.Length; i++)
                    {
                        var from = instance.Leafs[_leafs.IndexOf(li.froms[i])];

                        //set custom name for condition
                        if (condition != null && li.condition.FriendlyName != string.Empty)
                            condition.FriendlyName = li.condition.FriendlyName;

                        switch (li.linkType)
                        {
                            case LinkType.FromTo:
                                instance.Link(
                                    from,
                                    to,
                                    condition,
                                    li.executingType,
                                    li.executesQuantity,
                                    li.coolDown,
                                    li.setCoolDownOn);
                                break;
                            case LinkType.Ended:
                                instance.Link(
                                    from,
                                    to,
                                    li.executingType,
                                    li.executesQuantity,
                                    li.coolDown,
                                    li.setCoolDownOn);
                                break;
                        }
                    }
                }
                else
                {
                    instance.Link(
                        to,
                        condition,
                        li.executingType,
                        li.executesQuantity,
                        li.coolDown,
                        li.setCoolDownOn);
                }
            }

            //set startable leaf
            instance.StartableLeaf = instance.Leafs[_startableLeafID];

            return instance;
        }

        public override ILeaf GetInstance()
        {
            UnityEngine.Debug.LogError("I am not a leaf!");
            return null;
        }


        [InspectorButton("Add Link")]
        public void AddVisualizedLink()
        {
            var parent = transform.Find(VIS_LINKS_NAME)?.gameObject;
            if (parent == null)
            {
                parent = new GameObject(VIS_LINKS_NAME);
                parent.transform.SetParent(transform);
                parent.transform.localPosition = Vector3.zero;
            }

            var go = new GameObject("Link1");
            VisualizedLink vLink = (VisualizedLink)go.AddComponent(typeof(VisualizedLink));
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;
            _links.Add(vLink);
        }

        [InspectorButton("Add Leaf")]
        public void AddVisualizedLeaf()
        {
            var parent = transform.Find(VIS_LEAFS_NAME)?.gameObject;
            if (parent == null)
            {
                parent = new GameObject(VIS_LEAFS_NAME);
                parent.transform.SetParent(transform);
                parent.transform.localPosition = Vector3.zero;
            }

            var go = new GameObject("Leaf1");
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;
        }

        [InspectorButton("Add Branch")]
        public void AddVisualizedBranch()
        {
            var parent = transform.Find(VIS_LEAFS_NAME)?.gameObject;
            if (parent == null)
            {
                parent = new GameObject(VIS_LEAFS_NAME);
                parent.transform.SetParent(transform);
                parent.transform.localPosition = Vector3.zero;
            }

            var go = new GameObject("Branch1");
            VisualizedEmptyHierarchyBranch branchComp =
                (VisualizedEmptyHierarchyBranch)go.AddComponent(typeof(VisualizedEmptyHierarchyBranch));
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;

            _leafs.Add(branchComp);
        }

        [InspectorButton("Get Child Links")]
        public void GetVisualizedLinks()
        {
            _links.Clear();

            var ls = transform.GetComponentsInChildren<VisualizedLink>();

            foreach (var l in ls)
            {
                _links.Add(l);
            }
        }

        [InspectorButton("Get Child Leafs")]
        public void GetVisualizedLeafs()
        {
            _leafs.Clear();

            List<VisualizedLeaf> ls = new List<VisualizedLeaf>();
            foreach (Transform c in transform)
            {
                if (c.name == VIS_LEAFS_NAME)
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
                    var cL = c.Find(VIS_LEAFS_NAME);
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

            _leafs.AddRange(ls);
        }
    }
}