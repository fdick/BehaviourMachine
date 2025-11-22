using BehaviourGraph.Trees;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace BehaviourGraph.Visualizer
{
    [Serializable]
    public class VisualizedEmptyHierarchyBranch : VisualizedLeaf, IVisualizedTree
    {
        [SerializeField] public bool _resetStateAtStart = true;
        [SerializeField] private List<VisualizedLeaf> _leafs = new List<VisualizedLeaf>();
        [SerializeField] private List<VisualizedLink> _links = new List<VisualizedLink>();

        [FormerlySerializedAs("_startableLeafID")] [SerializeField]
        public int startableLeafID = 0;

        private const string VIS_LEAFS_NAME = "Leafs";
        private const string VIS_LINKS_NAME = "Links";


        public ITree GetInstance(BehaviourMachine graph)
        {
            //add leafs
            var lfs = new ILeaf[_leafs.Count];
            for (int i = 0; i < _leafs.Count; i++)
            {
                if (_leafs[i] == null)
                    throw new NullReferenceException(
                        $"{graph.name}: Instance ID {graph.GetInstanceID()} graph: At {i} array position Leaf is null!");

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

                //leaf events
                if (_leafs[i].OnStartLeaf != null)
                {
                    int savedI = i;
                    lfs[i].OnEnter += (t) => _leafs[savedI].OnStartLeaf.Invoke();
                }
                if (_leafs[i].OnEndLeaf != null)
                {
                    int savedI = i;
                    lfs[i].OnExit += () => _leafs[savedI].OnEndLeaf.Invoke();
                }

                //set custom name
                if (_leafs[i].FriendlyName != string.Empty)
                    lfs[i].FriendlyName = _leafs[i].FriendlyName;
                if (_leafs[i].Tag != string.Empty)
                    lfs[i].Tag = _leafs[i].Tag;
            }

            var instance = new HierarchyBranch(graph, lfs, _resetStateAtStart);


            //add events
            if (OnStartLeaf != null)
                instance.OnEnter += (t) => OnStartLeaf?.Invoke();

            if (OnEndLeaf != null)
                instance.OnExit += () => OnEndLeaf?.Invoke();

            //set custom name for myself
            if (FriendlyName != string.Empty)
                instance.FriendlyName = FriendlyName;
            if (Tag != string.Empty)
                instance.Tag = Tag;

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
            if (instance.Leafs.Count == 0)
                UnityEngine.Debug.LogError($"{transform.name}: Quantity of Leafs equals 0!");
            if (startableLeafID < 0 || startableLeafID >= instance.Leafs.Count)
                UnityEngine.Debug.LogError(
                    $"{transform.name}: Startable Leaf ID not in range! Quantity leafs {instance.Leafs.Count}. Startable Leaf ID {startableLeafID}");

            instance.StartableLeaf = instance.Leafs[startableLeafID];

            return instance;
        }

        public override ILeaf GetInstance()
        {
            UnityEngine.Debug.LogError("I am not a leaf!");
            return null;
        }


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

        public void AddVisualizedParallelBranch()
        {
            var parent = transform.Find(VIS_LEAFS_NAME)?.gameObject;
            if (parent == null)
            {
                parent = new GameObject(VIS_LEAFS_NAME);
                parent.transform.SetParent(transform);
                parent.transform.localPosition = Vector3.zero;
            }

            var go = new GameObject("ParallelBranch1");
            VisualizedEmptyParallelBranch branchComp =
                (VisualizedEmptyParallelBranch)go.AddComponent(typeof(VisualizedEmptyParallelBranch));
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;

            _leafs.Add(branchComp);
        }

        public void GetVisualizedLinks()
        {
            _links.Clear();
            var p = transform.Find(VIS_LINKS_NAME);
            foreach (Transform c in p)
            {
                if (c.TryGetComponent<VisualizedLink>(out var outLink))
                {
                    //check for contains leafs from link in tree
                    switch (outLink.linkType)
                    {
                        case LinkType.FromTo:
                            foreach (var l in outLink.froms)
                            {
                                if (!_leafs.Contains(l))
                                    UnityEngine.Debug.LogError(
                                        $"Tree {transform} does not contain the leaf {l.FriendlyName} which stay link {outLink.FriendlyName}");
                            }

                            break;
                        case LinkType.Ended:
                            foreach (var l in outLink.froms)
                            {
                                if (!_leafs.Contains(l))
                                    UnityEngine.Debug.LogError(
                                        $"Tree {transform} does not contain the leaf {l.FriendlyName} which set inside link {outLink.FriendlyName}");
                            }

                            break;
                    }

                    if (!_leafs.Contains(outLink.to))
                        UnityEngine.Debug.LogError(
                            $"Tree {transform} does not contain the leaf {outLink.to.FriendlyName} which set inside link {outLink.FriendlyName}");
                    _links.Add(outLink);
                }
            }
        }

        public void GetVisualizedLeafs()
        {
            _leafs.Clear();
            var p = transform.Find(VIS_LEAFS_NAME);
            foreach (Transform c in p)
            {
                if (c.TryGetComponent<VisualizedLeaf>(out var outLeaf))
                    _leafs.Add(outLeaf);
            }
        }
    }
}