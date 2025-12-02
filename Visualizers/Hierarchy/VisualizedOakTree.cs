using BehaviourGraph.Trees;
using System;
using System.Collections.Generic;
using BehaviourGraph.States;
using UnityEngine;

namespace BehaviourGraph.Visualizer
{
    [Serializable]
    public class VisualizedOakTree : VisualizedTree
    {
        [Tooltip("If enabled then the tree starts the leaf according to the startableLeaf_ID." +
                 " Otherwise, the last leaf on which the work of this tree has ended is launched.")]
        public bool resetStateAtStart = true;

        public List<VisualizedState> states = new List<VisualizedState>();
        public List<VisualizedLink> links = new List<VisualizedLink>();
        public int startableLeaf_ID = 0;

        private string _visStatesName = "States";
        private string _visLinkName = "Links";

        public override ITree GetInstance(BehaviourMachine graph)
        {
            //add leafs
            var lfs = new IState[states.Count];
            for (int i = 0; i < states.Count; i++)
            {
                //if child leaf is tree
                if (states[i] is IVisualizedTree lt)
                {
                    var childTree = lt.GetInstance(graph);

                    lfs[i] = (IState)childTree;
                }
                else
                {
                    lfs[i] = states[i].GetInstance(graph);
                }
                
                //leaf events
                if (states[i].OnStartState != null)
                {
                    int savedI = i;
                    lfs[i].OnEnter += (t) => states[savedI].OnStartState.Invoke();
                }
                if (states[i].OnEndState != null)
                {
                    int savedI = i;
                    lfs[i].OnExit += () => states[savedI].OnEndState.Invoke();
                }

                //set custom name for leafs
                if (states[i].FriendlyName != string.Empty)
                    lfs[i].FriendlyName = states[i].FriendlyName;
                if (states[i].Tag != string.Empty)
                    lfs[i].Tag = states[i].Tag;
            }

            var seq = new Sequence(graph, lfs);
            seq.StartableState = seq.States[0];
            var instance = new OakTree(graph, seq, resetStateAtStart);

            //set custom name for myself
            if (FriendlyName != string.Empty)
                instance.FriendlyName = FriendlyName;

            //add links
            foreach (var li in links)
            {
                if (li == null)
                    throw new NullReferenceException($"{graph.name} graph: One of the Links is null!");

                var to = instance.Sequence.GetStates()[states.IndexOf(li.to)];
                var condition = li.condition?.GetInstance(seq);

                if (li.linkType is LinkType.HasEnded or LinkType.FromTo)
                {
                    for (int i = 0; i < li.froms.Length; i++)
                    {
                        var from = instance.Sequence.GetStates()[states.IndexOf(li.froms[i])];

                        //set custom name for condition
                        if (condition != null && li.condition.FriendlyName != string.Empty)
                            condition.FriendlyName = li.condition.FriendlyName;

                        switch (li.linkType)
                        {
                            case LinkType.FromTo:
                                instance.Sequence.Link(
                                    from,
                                    to,
                                    condition,
                                    li.executingType,
                                    li.executesQuantity,
                                    li.coolDown,
                                    li.setCoolDownOn);
                                break;
                            case LinkType.HasEnded:
                                instance.Sequence.Link(
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
                    instance.Sequence.Link(
                        to,
                        condition,
                        li.executingType,
                        li.executesQuantity,
                        li.coolDown,
                        li.setCoolDownOn);
                }
            }

            //set startable leaf
            instance.Sequence.StartableState = instance.Sequence.GetStates()[startableLeaf_ID];

            return instance;
        }

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

        public void AddVisualizedLeaf()
        {
            var parent = transform.Find(_visStatesName)?.gameObject;
            if (parent == null)
            {
                parent = new GameObject(_visStatesName);
                parent.transform.SetParent(transform);
                parent.transform.localPosition = Vector3.zero;
            }

            var go = new GameObject("State1");
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;
        }

        public void AddVisualizedSequence()
        {
            var parent = transform.Find(_visStatesName)?.gameObject;
            if (parent == null)
            {
                parent = new GameObject(_visStatesName);
                parent.transform.SetParent(transform);
                parent.transform.localPosition = Vector3.zero;
            }

            var go = new GameObject("Sequence1");
            VisualizedSequence branchComp =
                (VisualizedSequence)go.AddComponent(typeof(VisualizedSequence));
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;

            states.Add(branchComp);
        }

        public void AddVisualizedParallel()
        {
            var parent = transform.Find(_visStatesName)?.gameObject;
            if (parent == null)
            {
                parent = new GameObject(_visStatesName);
                parent.transform.SetParent(transform);
                parent.transform.localPosition = Vector3.zero;
            }
        
            var go = new GameObject("Parallel1");
            VisualizedParallel branchComp =
                (VisualizedParallel)go.AddComponent(typeof(VisualizedParallel));
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;
        
            states.Add(branchComp);
        }

        public void GetVisualizedLinks()
        {
            links.Clear();
            var p = transform.Find(_visLinkName);
            
            if(p == null)
                return;
            
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
                                if (!states.Contains(l))
                                    UnityEngine.Debug.LogError(
                                        $"Tree {transform} does not contain the leaf {l.FriendlyName} which set inside link {outLink.FriendlyName}");
                            }

                            break;
                        case LinkType.HasEnded:
                            foreach (var l in outLink.froms)
                            {
                                if (!states.Contains(l))
                                    UnityEngine.Debug.LogError(
                                        $"Tree {transform} does not contain the leaf {l.FriendlyName} which set inside link {outLink.FriendlyName}");
                            }

                            break;
                    }

                    if (!states.Contains(outLink.to))
                        UnityEngine.Debug.LogError(
                            $"Tree {transform} does not contain the leaf {outLink.to.FriendlyName} which set inside link {outLink.FriendlyName}");
                    
                    links.Add(outLink);
                }
            }
        }

        public void GetVisualizedLeafs()
        {
            states.Clear();
            var p = transform.Find(_visStatesName);
            foreach (Transform c in p)
            {
                if (c.TryGetComponent<VisualizedState>(out var outLeaf))
                    states.Add(outLeaf);
            }
        }
    }
}