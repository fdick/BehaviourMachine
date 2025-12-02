using BehaviourGraph.Trees;
using System;
using System.Collections.Generic;
using BehaviourGraph.States;
using UnityEngine;
using UnityEngine.Serialization;

namespace BehaviourGraph.Visualizer
{
    [Serializable]
    public class VisualizedSequence : VisualizedState
    {
        [SerializeField] public bool _resetStateAtStart = true;
        [SerializeField] private List<VisualizedState> _states = new List<VisualizedState>();
        [SerializeField] private List<VisualizedLink> _links = new List<VisualizedLink>();

        [SerializeField] public int startableStateID = 0;

        private const string VIS_STATES_NAME = "States";
        private const string VIS_LINKS_NAME = "Links";

        public override IState GetInstance(BehaviourMachine graph)
        {
            //add states
            var lfs = new IState[_states.Count];
            for (int i = 0; i < _states.Count; i++)
            {
                if (_states[i] == null)
                    throw new NullReferenceException(
                        $"{graph.name}: Instance ID {graph.GetInstanceID()} graph: At {i} array position State is null!");

                //if child state is tree
                if (_states[i] is IVisualizedTree lt)
                {
                    var childTree = lt.GetInstance(graph);

                    lfs[i] = (IState)childTree;
                }
                else
                {
                    lfs[i] = _states[i].GetInstance(graph);
                }

                //state events
                if (_states[i].OnStartState != null)
                {
                    int savedI = i;
                    lfs[i].OnEnter += (t) => _states[savedI].OnStartState.Invoke();
                }

                if (_states[i].OnEndState != null)
                {
                    int savedI = i;
                    lfs[i].OnExit += () => _states[savedI].OnEndState.Invoke();
                }

                //set custom name
                if (_states[i].FriendlyName != string.Empty)
                    lfs[i].FriendlyName = _states[i].FriendlyName;
                if (_states[i].Tag != string.Empty)
                    lfs[i].Tag = _states[i].Tag;
            }

            var instance = new Sequence(graph, lfs, _resetStateAtStart);
            instance.StartableState = instance.States[0];

            //add events
            if (OnStartState != null)
                instance.OnEnter += (t) => OnStartState?.Invoke();

            if (OnEndState != null)
                instance.OnExit += () => OnEndState?.Invoke();

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

                var to = instance.GetStates()[_states.IndexOf(li.to)];
                var condition = li.condition?.GetInstance(instance);

                if (li.linkType is LinkType.HasEnded or LinkType.FromTo)
                {
                    for (int i = 0; i < li.froms.Length; i++)
                    {
                        var from = instance.GetStates()[_states.IndexOf(li.froms[i])];

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
                            case LinkType.HasEnded:
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

            //set startable state
            if (instance.GetStates().Length == 0)
                UnityEngine.Debug.LogError($"{transform.name}: Quantity of States equals 0!");
            if (startableStateID < 0 || startableStateID >= instance.GetStates().Length)
                UnityEngine.Debug.LogError(
                    $"{transform.name}: Startable State ID not in range! Quantity states {instance.GetStates().Length}. Startable State ID {startableStateID}");

            instance.StartableState = instance.GetStates()[startableStateID];

            return instance;
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

        public void AddVisualizedState()
        {
            var parent = transform.Find(VIS_STATES_NAME)?.gameObject;
            if (parent == null)
            {
                parent = new GameObject(VIS_STATES_NAME);
                parent.transform.SetParent(transform);
                parent.transform.localPosition = Vector3.zero;
            }

            var go = new GameObject("State1");
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;
        }

        public void AddVisualizedSequence()
        {
            var parent = transform.Find(VIS_STATES_NAME)?.gameObject;
            if (parent == null)
            {
                parent = new GameObject(VIS_STATES_NAME);
                parent.transform.SetParent(transform);
                parent.transform.localPosition = Vector3.zero;
            }

            var go = new GameObject("Sequence1");
            VisualizedSequence branchComp =
                (VisualizedSequence)go.AddComponent(typeof(VisualizedSequence));
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;

            _states.Add(branchComp);
        }

        public void AddVisualizedParallel()
        {
            var parent = transform.Find(VIS_STATES_NAME)?.gameObject;
            if (parent == null)
            {
                parent = new GameObject(VIS_STATES_NAME);
                parent.transform.SetParent(transform);
                parent.transform.localPosition = Vector3.zero;
            }

            var go = new GameObject("Parallel1");
            VisualizedParallel comp =
                (VisualizedParallel)go.AddComponent(typeof(VisualizedParallel));
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;

            _states.Add(comp);
        }

        public void GetVisualizedLinks()
        {
            _links.Clear();
            var p = transform.Find(VIS_LINKS_NAME);
            if (p == null)
                return;
            foreach (Transform c in p)
            {
                if (c.TryGetComponent<VisualizedLink>(out var outLink))
                {
                    //check for contains states from link in tree
                    switch (outLink.linkType)
                    {
                        case LinkType.FromTo:
                            foreach (var l in outLink.froms)
                            {
                                if (!_states.Contains(l))
                                    UnityEngine.Debug.LogError(
                                        $"Tree {transform} does not contain the state {l.FriendlyName} which stay link {outLink.FriendlyName}");
                            }

                            break;
                        case LinkType.HasEnded:
                            foreach (var l in outLink.froms)
                            {
                                if (!_states.Contains(l))
                                    UnityEngine.Debug.LogError(
                                        $"Tree {transform} does not contain the state {l.FriendlyName} which set inside link {outLink.FriendlyName}");
                            }

                            break;
                    }

                    if (!_states.Contains(outLink.to))
                        UnityEngine.Debug.LogError(
                            $"Tree {transform} does not contain the state {outLink.to.FriendlyName} which set inside link {outLink.FriendlyName}");
                    _links.Add(outLink);
                }
            }
        }

        public void GetVisualizedStates()
        {
            _states.Clear();
            var p = transform.Find(VIS_STATES_NAME);
            foreach (Transform c in p)
            {
                if (c.TryGetComponent<VisualizedState>(out var outState))
                    _states.Add(outState);
            }
        }
    }
}