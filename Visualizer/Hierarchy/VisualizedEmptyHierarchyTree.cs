﻿using BehaviourGraph.Trees;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourGraph.Visualizer
{
    [Serializable]
    public class VisualizedEmptyHierarchyTree : VisualizedTree
    {
        [Tooltip("If enabled then the tree starts the leaf according to the startableLeaf_ID." +
                 " Otherwise, the last leaf on which the work of this tree has ended is launched.")]
        public bool resetStateAtStart = true;

        public List<VisualizedLeaf> leafs = new List<VisualizedLeaf>();
        public List<VisualizedLink> links = new List<VisualizedLink>();
        public int startableLeaf_ID = 0;

        private string _visLeafsName = "Leafs";
        private string _visLinkName = "Links";

        public override ITree GetInstance(BehaviourMachine graph)
        {
            //add leafs
            var lfs = new ILeaf[leafs.Count];
            for (int i = 0; i < leafs.Count; i++)
            {
                //if child leaf is tree
                if (leafs[i] is IVisualizedTree lt)
                {
                    var childTree = lt.GetInstance(graph);

                    lfs[i] = (ILeaf)childTree;
                }
                else
                {
                    lfs[i] = leafs[i].GetInstance();
                }

                //set custom name for leafs
                if (leafs[i].FriendlyName != string.Empty)
                    lfs[i].FriendlyName = leafs[i].FriendlyName;
                if (leafs[i].Tag != string.Empty)
                    lfs[i].Tag = leafs[i].Tag;
            }


            var instance = new HierarchyTree(graph, lfs, resetStateAtStart);

            //set custom name for myself
            if (FriendlyName != string.Empty)
                instance.FriendlyName = FriendlyName;

            //add links
            foreach (var li in links)
            {
                if (li == null)
                    throw new NullReferenceException($"{graph.name} graph: One of the Links is null!");

                var to = instance.Leafs[leafs.IndexOf(li.to)];
                var condition = li.condition?.GetInstance(instance);

                if (li.linkType is LinkType.Ended or LinkType.FromTo)
                {
                    for (int i = 0; i < li.froms.Length; i++)
                    {
                        var from = instance.Leafs[leafs.IndexOf(li.froms[i])];

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
            instance.StartableLeaf = instance.Leafs[startableLeaf_ID];

            return instance;
        }

        [InspectorButton("Add Link")]
        public void AddVisualizedLink()
        {
            var parent = transform.Find(_visLinkName)?.gameObject;
            if (parent == null)
            {
                parent = new GameObject(_visLinkName);
                parent.transform.SetParent(transform);
                parent.transform.localPosition = Vector3.zero;
            }

            var go = new GameObject("Link1");
            VisualizedLink vLink = (VisualizedLink)go.AddComponent(typeof(VisualizedLink));
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;
            links.Add(vLink);
        }

        [InspectorButton("Add Leaf")]
        public void AddVisualizedLeaf()
        {
            var parent = transform.Find(_visLeafsName)?.gameObject;
            if (parent == null)
            {
                parent = new GameObject(_visLeafsName);
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
            var parent = transform.Find(_visLeafsName)?.gameObject;
            if (parent == null)
            {
                parent = new GameObject(_visLeafsName);
                parent.transform.SetParent(transform);
                parent.transform.localPosition = Vector3.zero;
            }

            var go = new GameObject("Branch1");
            VisualizedEmptyHierarchyBranch branchComp =
                (VisualizedEmptyHierarchyBranch)go.AddComponent(typeof(VisualizedEmptyHierarchyBranch));
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;

            leafs.Add(branchComp);
        }

        [InspectorButton("Get Child Links")]
        public void GetVisualizedLinks()
        {
            links.Clear();

            foreach (Transform c in transform)
            {
                if (c.name == _visLinkName)
                {
                    for (int i = 0; i < c.childCount; i++)
                    {
                        var c2 = c.GetChild(i);
                        if (c2.TryGetComponent<VisualizedLink>(out var outLeaf))
                            links.Add(outLeaf);
                    }
                }
            }
        }

        [InspectorButton("Get Child Leafs")]
        public void GetVisualizedLeafs()
        {
            leafs.Clear();

            foreach (Transform c in transform)
            {
                if (c.name == _visLeafsName)
                {
                    for (int i = 0; i < c.childCount; i++)
                    {
                        var c2 = c.GetChild(i);
                        if (c2.TryGetComponent<VisualizedLeaf>(out var outLeaf))
                            leafs.Add(outLeaf);
                    }
                }
                else
                {
                    var cL = c.Find(_visLeafsName);
                    if (cL == null)
                        continue;

                    for (int i = 0; i < cL.childCount; i++)
                    {
                        var c2 = cL.GetChild(i);
                        if (c2.TryGetComponent<VisualizedLeaf>(out var outLeaf))
                            leafs.Add(outLeaf);
                    }
                }
            }
        }
    }
}