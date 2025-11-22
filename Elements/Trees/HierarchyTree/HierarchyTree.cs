using BehaviourGraph.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using BehaviourGraph.Conditions;
using BehaviourGraph.Leafs;
using UnityEditor;

namespace BehaviourGraph.Trees
{
    public class LeafCash
    {
        public LeafCash(ILeaf leaf)
        {
            this.leaf = leaf;
            updatableLeaf = leaf as IUpdatableLeaf;
            FixedUpdatableLeaf = leaf as IFixedUpdatableLeaf;
            lateUpdatableLeaf = leaf as ILateUpdatableLeaf;
            endableLeaf = leaf as IEndableLeaf;
        }

        public ILeaf leaf;
        public IUpdatableLeaf updatableLeaf;
        public IFixedUpdatableLeaf FixedUpdatableLeaf;
        public ILateUpdatableLeaf lateUpdatableLeaf;
        public IEndableLeaf endableLeaf;
    }

    /// <summary>
    /// This tree processes its leaves hierarchically. Only one process can run at a time. Contains global, local and end links.
    /// Need dispose after finishing.
    /// </summary>
    public class HierarchyTree : ITree, IDisposable, IDebugable
    {
        //TODO: optimize with cashing leafs interfaces!
        // protected Dictionary<GUID, LeafCash> _leafs2;

        protected List<ILeaf> _leafs = new List<ILeaf>();
        protected List<Transition> _transitions = new List<Transition>();

        protected Dictionary<ILeaf, EndLinkData> _endLinks = new Dictionary<ILeaf, EndLinkData>();
        protected Dictionary<ILeaf, LocalLinkData> _localLinks = new Dictionary<ILeaf, LocalLinkData>();
        protected Dictionary<ILeaf, GlobalLinkData> _globalLinks = new Dictionary<ILeaf, GlobalLinkData>();

        protected BehaviourMachine _parentGraph;

        /// <summary>
        /// Startable leaf
        /// </summary>
        public ILeaf StartableLeaf { get; set; }

        /// <summary>
        /// Current active leaf
        /// </summary>
        public ILeaf RunningLeaf { get; protected set; }

        public Dictionary<ILeaf, EndLinkData> EndLinks => _endLinks;
        public Dictionary<ILeaf, LocalLinkData> LocalLinks => _localLinks;
        public Dictionary<ILeaf, GlobalLinkData> GlobalLinks => _globalLinks;
        public List<ILeaf> Leafs => _leafs;
        public List<Transition> Transitions => _transitions;
        public string FriendlyName { get; set; }
        public Guid ID { get; }

        /// <summary>
        /// Calls when running leaf is changing. ILeaf param is a new leaf.
        /// </summary>
        public Action<ILeaf> OnChangeRunningLeaf { get; set; }

        /// <summary>
        /// Calls when executes any links by this tree. (local + global + end links).
        /// </summary>
        public Action<Transition> OnExecuteLink { get; set; }

        public bool IsPaused { get; private set; }

        public Action Breakpoint { get; set; }

        public UpdateStatus Status { get; private set; } = UpdateStatus.Failure;

        /// <summary>
        /// Cash for last used leaf. Need for reset state.
        /// </summary>
        public ILeaf CurrentLeaf { get; protected set; }

        protected bool _resetStateAtStart;

        /// <summary>
        /// For creating empty instances
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="leafs"></param>
        /// <param name="resetStateAtStart"></param>
        public HierarchyTree(BehaviourMachine graph, ILeaf[] leafs, bool resetStateAtStart = true)
        {
            _parentGraph = graph;

            ID = Guid.NewGuid();
            FriendlyName = nameof(HierarchyTree);

            for (int i = 0; i < leafs.Length; i++)
            {
                AddLeaf(leafs[i]);
            }

            if (_leafs == null || _leafs.Count == 0)
                UnityEngine.Debug.LogError(
                    $"{FriendlyName} (Gameobject: {_parentGraph.gameObject.name}) does not have leafs!");
            StartableLeaf = _leafs[0];
            CurrentLeaf = StartableLeaf;

            _resetStateAtStart = resetStateAtStart;
        }

        /// <summary>
        /// For creating inherits
        /// </summary>
        /// <param name="graph"></param>
        public HierarchyTree(BehaviourMachine graph, bool resetStateAtStart = true)
        {
            _parentGraph = graph;
            FriendlyName = ToString().Split('.').Last();
            _resetStateAtStart = resetStateAtStart;
        }

        public void AwakeTree()
        {
            for (int i = 0; i < _leafs.Count; i++)
            {
                _leafs[i].InitLeaf();
            }
        }

        public void StartTree()
        {
            if (StartableLeaf == null)
            {
                // throw new NullReferenceException($"{FriendlyName}: Startable leaf is null!");
#if UNITY_EDITOR
                UnityEngine.Debug.Log($"{FriendlyName}: Startable leaf is null!");
#endif
                return;
            }

            if (!_resetStateAtStart && CurrentLeaf != null)
                ChangeRunningLeaf(CurrentLeaf, null);
            else
                ChangeRunningLeaf(StartableLeaf, null);
        }

        public void UpdateTree()
        {
            if (IsPaused)
                return;
            if (RunningLeaf == null)
                return;

            if (RunningLeaf is IUpdatableLeaf updatable)
            {
                updatable.UpdateLeaf();
            }
        }

        public void FixedUpdateTree()
        {
            if (IsPaused)
                return;
            if (RunningLeaf == null)
                return;

            if (RunningLeaf is IFixedUpdatableLeaf fixedUpdatable)
            {
                fixedUpdatable.FixedUpdateLeaf();
            }
        }

        public UpdateStatus LateUpdateTree()
        {
            if (IsPaused)
                return UpdateStatus.Failure;

            if (RunningLeaf == null)
                return UpdateStatus.Failure;

            //late update
            if (RunningLeaf is ILateUpdatableLeaf lateUpdatable)
            {
                lateUpdatable.LateUpdateLeaf();
            }

            UpdateStatus treeStatus = UpdateStatus.Running;

            Status = treeStatus;

            //global links update
            if (_globalLinks.Count > 0)
            {
                foreach (var d in _globalLinks)
                {
                    foreach (var c in d.Value.toLeafTransitions)
                    {
                        if (RunningLeaf == d.Key)
                            continue;
                        if (c.ExecutedCondition.ConditionUpdate() == UpdateStatus.Successed && c.CheckCooldown())
                        {
                            c.FromLeaf = RunningLeaf;
                            OnExecuteLink?.Invoke(c);

                            ChangeRunningLeaf(d.Key, c);
                            return treeStatus;
                        }
                    }
                }
            }

            //local links update
            if (_localLinks.TryGetValue(RunningLeaf, out var to))
            {
                foreach (var d in to.toLeafsTransitions)
                {
                    foreach (var t in d.Value)
                    {
                        if (t.ExecutedCondition.ConditionUpdate() == UpdateStatus.Successed && t.CheckCooldown())
                        {
                            OnExecuteLink?.Invoke(t);
                            t.FromLeaf = RunningLeaf;
                            ChangeRunningLeaf(d.Key, t);
                            return treeStatus;
                        }
                    }
                }
            }

            //end update
            if (_endLinks.TryGetValue(RunningLeaf, out var toLeaf)
                && RunningLeaf is IEndableLeaf endable)
            {
                treeStatus = endable.EndCondition();

                if (treeStatus == UpdateStatus.Successed && toLeaf.toLeafCondition.CheckCooldown())
                {
                    OnExecuteLink?.Invoke(toLeaf.toLeafCondition);
                    ChangeRunningLeaf(toLeaf.toLeafCondition.ToLeaf, toLeaf.toLeafCondition);
                    return treeStatus;
                }
            }

            return treeStatus;
        }

        public void EndTree()
        {
            //if running leaf is running than stop it
            AbortRunningLeaf();
        }

        public void PauseTree()
        {
            IsPaused = true;
        }

        public void UnPauseTree()
        {
            IsPaused = false;
        }

        public void AddLeaf(ILeaf leaf)
        {
            if (leaf == null)
                return;
            if (_leafs.Contains(leaf))
                return;

            leaf.SetGameobject(_parentGraph.CustomGameobject);
            _leafs.Add(leaf);
        }

        public void RemoveLeaf(ILeaf leaf)
        {
            //remove link from this leaf
            if (_localLinks.ContainsKey(leaf))
                _localLinks.Remove(leaf);
            //remove leaf
            if (_leafs.Contains(leaf))
                _leafs.Remove(leaf);
        }

        /// <summary>
        /// Add local link. Return unique ID for this link
        /// </summary>
        public Guid Link(ILeaf origin, ILeaf destination, ICondition condition,
            ExecutingTypes executingType = ExecutingTypes.Infinity, int maxExecuteQuantity = 1,
            float cooldownDuration = 0, CoolDownTypes cooldownType = CoolDownTypes.OnExitDestinationLeaf)
        {
            if (origin == null || destination == null || condition == null)
            {
                throw new NullReferenceException(
                    $"{_parentGraph} : You try to add a confusing local link. Origin leaf is {origin}. Destination leaf is {destination}. Condition is {condition}");
            }

            if (!_leafs.Contains(origin))
            {
                throw new Exception($"{_parentGraph} : Behaviour machine doesn't contain the leaf {origin}");
            }

            if (!_leafs.Contains(destination))
            {
                throw new Exception($"{_parentGraph} : Behaviour machine doesn't contain the leaf {destination}");
            }


            //add link
            if (_localLinks.TryGetValue(origin, out var data))
            {
                var transition = new Transition(
                    condition, origin, destination, TransitionTypes.Local,
                    executingType, maxExecuteQuantity,
                    cooldownDuration, cooldownType);

                if (data.toLeafsTransitions.TryGetValue(destination, out var conds))
                {
                    //i cant contains same condition
                    if (conds.Where((x) => x.ExecutedCondition == condition).Count() > 0)
                    {
                        UnityEngine.Debug.LogError($"{condition.FriendlyName} already contains!");
                        return default;
                    }

                    conds.Add(transition);
                }
                else
                    data.toLeafsTransitions.Add(destination, new List<Transition> { transition });

                _transitions.Add(transition);
            }
            else
            {
                var linkData = new Dictionary<ILeaf, List<Transition>>();
                var transition = new Transition(condition, origin, destination, TransitionTypes.Local,
                    executingType, maxExecuteQuantity,
                    cooldownDuration, cooldownType);
                linkData.Add(destination, new List<Transition> { transition });
                var localLinkData = new LocalLinkData() { toLeafsTransitions = linkData };

                _localLinks.Add(origin, localLinkData);
                _transitions.Add(transition);
            }

            return condition.ID;
        }

        /// <summary>
        /// Add end link. If this leaf already has end link, that replace it
        /// </summary>
        public void Link(ILeaf origin, ILeaf destination,
            ExecutingTypes executingType = ExecutingTypes.Infinity, int maxExecuteQuantity = 1,
            float cooldownDuration = 0, CoolDownTypes cooldownType = CoolDownTypes.OnExitDestinationLeaf)
        {
            if (origin == null || destination == null)
            {
                throw new NullReferenceException(
                    $"{_parentGraph} : You try to add a confusing end link. Origin leaf is {origin}. Destination leaf is {destination}");
            }
            
            if (origin is not IEndableLeaf)
            {
                throw new NullReferenceException(
                    $"{_parentGraph} : Leaf {origin} is not endable!");
            }

            if (!_leafs.Contains(origin))
            {
                throw new Exception($"{_parentGraph} : Behaviour machine doesn't contain the leaf {origin}");
            }

            if (!_leafs.Contains(destination))
            {
                throw new Exception($"{_parentGraph} : Behaviour machine doesn't contain the leaf {destination}");
            }

            if (_endLinks.TryGetValue(origin, out var _))
                _endLinks.Remove(origin);

            var transition = new Transition(
                null, origin, destination, TransitionTypes.End,
                executingType, maxExecuteQuantity,
                cooldownDuration, cooldownType);
            var endLinkData = new EndLinkData() { toLeafCondition = transition };
            _endLinks.Add(origin, endLinkData);
        }

        /// <summary>
        /// Add global link. Return unique ID for this link
        /// </summary>
        public Guid Link(ILeaf toLeaf, ICondition condition,
            ExecutingTypes executingType = ExecutingTypes.Infinity, int maxExecuteQuantity = 1,
            float cooldownDuration = 0, CoolDownTypes cooldownType = CoolDownTypes.OnExitDestinationLeaf)
        {
            if (toLeaf == null || condition == null)
            {
                throw new NullReferenceException(
                    $"{_parentGraph} : You try to add a confusing global link. Leaf is {toLeaf}. Condition is {condition}");
            }

            if (!_leafs.Contains(toLeaf))
            {
                throw new Exception($"{_parentGraph} : Behaviour machine doesn't contain the leaf {toLeaf}");
            }

            var transition = new Transition(
                condition, null, toLeaf, TransitionTypes.Global,
                executingType, maxExecuteQuantity,
                cooldownDuration, cooldownType);

            if (_globalLinks.TryGetValue(toLeaf, out var l))
                l.toLeafTransitions.Add(transition);
            else
                _globalLinks.Add(toLeaf,
                    new GlobalLinkData() { toLeafTransitions = new List<Transition>() { transition } });

            _transitions.Add(transition);
            return condition.ID;
        }

        /// <summary>
        /// Remove end link
        /// </summary>
        public void RemoveEndLink(ILeaf origin)
        {
            if (!_endLinks.ContainsKey(origin))
                return;

            var transition = _endLinks[origin];
            _transitions.Remove(transition.toLeafCondition);

            _endLinks.Remove(origin);
        }

        /// <summary>
        /// Remove local link
        /// </summary>
        public void RemoveLink(ILeaf origin, ILeaf destination)
        {
            if (!_leafs.Contains(origin) ||
                !_leafs.Contains(destination) ||
                !_localLinks.ContainsKey(origin))
                return;

            if (_localLinks.TryGetValue(origin, out var data))
                if (!data.toLeafsTransitions.ContainsKey(destination))
                    return;

            foreach (var t in data.toLeafsTransitions[destination])
            {
                _transitions.Remove(t);
            }

            data.toLeafsTransitions.Remove(destination);

            //delete empty link
            if (data.toLeafsTransitions.Count == 0)
                _localLinks.Remove(origin);
        }

        /// <summary>
        /// Remove global link
        /// </summary>
        public void RemoveLink(ILeaf globalLeaf)
        {
            if (!_leafs.Contains(globalLeaf))
                return;
            if (!_globalLinks.ContainsKey(globalLeaf))
                return;

            foreach (var t in _globalLinks[globalLeaf].toLeafTransitions)
            {
                _transitions.Remove(t);
            }

            _globalLinks.Remove(globalLeaf);
        }

        public void RemoveLink(Transition transition)
        {
            if(transition == null)
                return;
            if (!_transitions.Contains(transition))
                return;

            switch (transition.TransitionType)
            {
                case TransitionTypes.End:
                    RemoveEndLink(transition.FromLeaf);
                    break;
                case TransitionTypes.Local:
                    RemoveLink(transition.FromLeaf, transition.ToLeaf);
                    break;
                case TransitionTypes.Global:
                    RemoveLink(transition.ToLeaf);
                    break;
            }
        }

        public void RemoveLink(int transitionIndex)
        {
            if (transitionIndex < 0 || transitionIndex >= _transitions.Count)
            {
                throw new IndexOutOfRangeException(
                    $"{transitionIndex} out of range! Range is : 0 - {_transitions.Count}");
            }

            var t = _transitions[transitionIndex];
            RemoveLink(t);
        }

        public ILeaf GetRunningLeaf() => RunningLeaf;
        public ILeaf GetStartableLeaf() => StartableLeaf;

        public virtual void Dispose()
        {
            foreach (var l in _leafs)
            {
                if (l is IDisposable tDis)
                    tDis.Dispose();
            }

            EndLinks.Clear();
            GlobalLinks.Clear();
            LocalLinks.Clear();

            OnChangeRunningLeaf = null;
            OnExecuteLink = null;
            Breakpoint = null;
        }

        public ILeaf[] GetLeafs() => Leafs.ToArray();

        public BehaviourMachine GetGraph() => _parentGraph;

        public int GetLeafIndex(ILeaf leaf)
        {
            if (leaf == null)
                return -1;
            return _leafs.IndexOf(leaf);
        }

        /// <summary>
        /// Force enter to Leaf. Find first leaf by T type. Only for this tree childs. 
        /// </summary>
        public void ForceEnter<T>() where T : ILeaf
        {
            var leaf = _leafs.FirstOrDefault(x => x.GetType() == typeof(T));

            if (leaf == null)
                return;

            var id = _leafs.IndexOf(leaf);
            ForceEnter(id);
        }

        /// <summary>
        /// Force enter to Leaf. Only for this tree childs. 
        /// </summary>
        public void ForceEnter(int leafID)
        {
            if (leafID >= _leafs.Count)
                return;

            var temporaryTransition = new Transition(null, RunningLeaf, _leafs[leafID],
                TransitionTypes.End, ExecutingTypes.Infinity);
            ChangeRunningLeaf(_leafs[leafID], temporaryTransition);
        }

        /// <summary>
        /// Find a first child leaf by type. Searching in all hierarchy.
        /// </summary>
        /// <typeparam name="T"> is finding type</typeparam>
        /// <returns></returns>
        public T QLeaf<T>() where T : class, ILeaf
        {
            foreach (var l in _leafs)
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
        /// Find a first child leaf by tag. Searching in all hierarchy.
        /// </summary>
        /// <param name="tag">Tag of a leaf.</param>
        public ILeaf QLeaf(string tag)
        {
            foreach (var l in _leafs)
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

        private void AbortRunningLeaf()
        {
            if (RunningLeaf == null)
                return;

            RunningLeaf.ExitLeaf();
            RunningLeaf = null;
        }

        private void ApplyRunningLeaf(ILeaf leaf)
        {
            if (RunningLeaf != null)
                return;
            CurrentLeaf = leaf;
            RunningLeaf = leaf;
        }

        private void ChangeRunningLeaf(ILeaf newLeaf, Transition transition = null)
        {
#if UNITY_EDITOR
            Breakpoint?.Invoke();
#endif
            OnChangeRunningLeaf?.Invoke(newLeaf);

            AbortRunningLeaf();
            ApplyRunningLeaf(newLeaf);

            RunningLeaf.EnterLeaf(transition);

            //remove transition if quantity executes equals max executes
            if (transition != null && transition.ExecutingType == ExecutingTypes.Custom)
            {
                transition.ExecutedTimes++;
                if (transition.ExecutedTimes >= transition.MaxExecuteQuantities)
                {
                    //remove end link
                    if (transition.ExecutedCondition == null)
                        RemoveEndLink(transition.FromLeaf);
                    //remove global link
                    else if (transition.FromLeaf == null)
                        RemoveLink(transition.ToLeaf);
                    //remove local link
                    else
                        RemoveLink(transition.FromLeaf, transition.ToLeaf);
                }
            }
        }
    }
}