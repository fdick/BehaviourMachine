using BehaviourGraph.Trees;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourGraph.Visualizer
{
    [Serializable]
    public class VisualizedEmptyHierarchyTree : VisualizedTree
    {
        public List<VisualizedLeaf> leafs;
        public List<VisualizedLink> links = new List<VisualizedLink>();
        public int startableLeaf_ID = 0;

        private string _visLeafsName = "Leafs";
        private string _visLinkName = "Links";

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
            VisualizedEmptyHierarchyBranch branchComp = (VisualizedEmptyHierarchyBranch)go.AddComponent(typeof(VisualizedEmptyHierarchyBranch));
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;

            leafs.Add(branchComp);
        }

        [InspectorButton("Get Child Links")]
        public void GetVisualizedLinks()
        {
            links.Clear();

            var ls = transform.GetComponentsInChildren<VisualizedLink>();

            foreach (var l in ls)
            {
                links.Add(l);
            }
        }

        [InspectorButton("Get Child Leafs")]
        public void GetVisualizedLeafs()
        {
            leafs.Clear();

            List<VisualizedEmptyHierarchyBranch> ls = new List<VisualizedEmptyHierarchyBranch>();
            foreach (Transform c in transform)
            {
                if (c.name == _visLeafsName)
                {
                    for (int i = 0; i < c.childCount; i++)
                    {
                        var c2 = c.GetChild(i);
                        if (c2.TryGetComponent<VisualizedEmptyHierarchyBranch>(out var outLeaf))
                            ls.Add(outLeaf);
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
                        if (c2.TryGetComponent<VisualizedEmptyHierarchyBranch>(out var outLeaf))
                            ls.Add(outLeaf);
                    }
                }
            }

            //foreach (var l in ls)
            //    leafs.Add(l);

            leafs.AddRange(ls);


        }


        public override ITree GetInstance(AIBehaviourGraph graph)
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
                lfs[i].OnAwake();

                //set custom name
                if (leafs[i].FriendlyName != string.Empty)
                    lfs[i].FriendlyName = leafs[i].FriendlyName;
            }

            var instance = new HierarchyBranch(graph, lfs);

            //add links
            foreach (var li in links)
            {
                for (int i = 0; i < li.froms.Length; i++)
                {
                    var from = instance.Leafs[leafs.IndexOf(li.froms[i])];
                    var to = instance.Leafs[leafs.IndexOf(li.to)];

                    if (li.linkType == Visualizer.LinkType.FromTo)
                        instance.Link(
                            from,
                            to,
                            li.condition.GetInstance(instance));
                    else if (li.linkType == Visualizer.LinkType.Ended)
                        instance.Link(
                            from,
                            to);
                    else
                        instance.Link(
                            to,
                            li.condition.GetInstance(instance));
                }
            }

            //set startable leaf
            instance.StartableLeaf = instance.Leafs[startableLeaf_ID];

            return instance;
        }
    }
}