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
    /// This tree processes its leaves hierarchically. Only one process can run at a time. Contain global, local and end links.
    /// Need dispose after finishing.
    /// </summary>
    public class HierarchyTree : ITree, IDisposable, IDebugable
    {
        //TODO: optimize with cashing leafs interfaces!
        // protected Dictionary<GUID, LeafCash> _leafs2;


        protected List<ILeaf> _leafs = new List<ILeaf>();
        protected List<Transition> _conditions = new List<Transition>();

        protected Dictionary<ILeaf, ILeaf> _endLinks = new Dictionary<ILeaf, ILeaf>();
        protected Dictionary<ILeaf, LocalLinkData> _localLinks = new Dictionary<ILeaf, LocalLinkData>();
        protected Dictionary<ILeaf, GlobalLinkData> _globalLinks = new Dictionary<ILeaf, GlobalLinkData>();

        private BehaviourMachine _parentGraph;

        /// <summary>
        /// Startable leaf
        /// </summary>
        public ILeaf StartableLeaf { get; set; }

        /// <summary>
        /// Current active leaf
        /// </summary>
        public ILeaf RunningLeaf { get; protected set; }

        public Dictionary<ILeaf, ILeaf> EndLinks => _endLinks;
        public Dictionary<ILeaf, LocalLinkData> LocalLinks => _localLinks;
        public Dictionary<ILeaf, GlobalLinkData> GlobalLinks => _globalLinks;
        public List<ILeaf> Leafs => _leafs;
        public List<Transition> Conditions => _conditions;
        public string FriendlyName { get; set; }
        public GUID ID { get; }

        /// <summary>
        /// Calls when running leaf is changing. ILeaf param is a new leaf.
        /// </summary>
        public Action<ILeaf> OnChangeRunningLeaf { get; set; }

        /// <summary>
        /// Calls when executes any links by this tree. (local + global + end links). If Icondition == null than it is an end link.
        /// </summary>
        public Action<Transition> OnExecuteLink { get; set; }

        public bool IsPaused { get; private set; }

        public Action Breakpoint { get; set; }

        public UpdateStatus Status { get; private set; } = UpdateStatus.Failure;

        /// <summary>
        /// Cash for last used leaf. Need for reset state.
        /// </summary>
        public ILeaf CurrentLeaf { get; protected set; }

        private bool _resetStateAtStart;

        /// <summary>
        /// For creating empty instances
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="leafs"></param>
        /// <param name="resetStateAtStart"></param>
        public HierarchyTree(BehaviourMachine graph, ILeaf[] leafs, bool resetStateAtStart = true)
        {
            _parentGraph = graph;

            ID = GUID.Generate();
            FriendlyName = nameof(HierarchyTree);
            // FriendlyName = ToString().Split('.').Last();

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
                return;

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
                    foreach (var c in d.Value.toLeafConditions)
                    {
                        if (RunningLeaf == d.Key)
                            continue;
                        if (c.ExecutedCondition.ConditionUpdate() == UpdateStatus.Successed)
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
                foreach (var d in to.toLeafs)
                {
                    foreach (var c in d.Value)
                    {
                        if (c.ExecutedCondition.ConditionUpdate() == UpdateStatus.Successed)
                        {
                            OnExecuteLink?.Invoke(c);
                            // OnExecuteLink?.Invoke(c);
                            c.FromLeaf = RunningLeaf;
                            ChangeRunningLeaf(d.Key, c);
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
            }

            //end link
            if (treeStatus == UpdateStatus.Successed &&
                _endLinks.TryGetValue(RunningLeaf, out toLeaf))
            {
                var t = new Transition(null, RunningLeaf, toLeaf);
                OnExecuteLink?.Invoke(t);
                ChangeRunningLeaf(toLeaf, t);
                return treeStatus;
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
        public GUID Link(ILeaf origin, ILeaf destination, ICondition condition)
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
                var conditionData = new Transition(condition, origin, destination);

                if (data.toLeafs.TryGetValue(destination, out var conds))
                {
                    //i cant contains same condition
                    if (conds.Where((x) => x.ExecutedCondition == condition).Count() > 0)
                    {
                        UnityEngine.Debug.LogError($"{condition.FriendlyName} already contains!");
                        return default;
                    }

                    conds.Add(conditionData);
                }
                else
                    data.toLeafs.Add(destination, new List<Transition> { conditionData });

                _conditions.Add(conditionData);
            }
            else
            {
                var linkData = new Dictionary<ILeaf, List<Transition>>();
                var conditionData = new Transition(condition, origin, destination);
                linkData.Add(destination, new List<Transition> { conditionData });
                var localLinkData = new LocalLinkData() { toLeafs = linkData };

                _localLinks.Add(origin, localLinkData);
                _conditions.Add(conditionData);
            }

            return ID;
        }

        /// <summary>
        /// Add end link. If this leaf already has end link, that replace it
        /// </summary>
        public void Link(ILeaf origin, ILeaf destination)
        {
            if (origin == null || destination == null)
            {
                throw new NullReferenceException(
                    $"{_parentGraph} : You try to add a confusing end link. Origin leaf is {origin}. Destination leaf is {destination}");
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

            _endLinks.Add(origin, destination);
        }

        /// <summary>
        /// Add global link. Return unique ID for this link
        /// </summary>
        public GUID Link(ILeaf toLeaf, ICondition condition)
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

            var conditionData = new Transition(condition, null, toLeaf);

            if (_globalLinks.TryGetValue(toLeaf, out var l))
                l.toLeafConditions.Add(conditionData);
            else
                _globalLinks.Add(toLeaf,
                    new GlobalLinkData() { toLeafConditions = new List<Transition>() { conditionData } });

            _conditions.Add(conditionData);
            return ID;
        }

        /// <summary>
        /// Remove end link
        /// </summary>
        public void RemoveEndLink(ILeaf origin)
        {
            if (_endLinks.ContainsKey(origin))
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
                if (!data.toLeafs.ContainsKey(destination))
                    return;

            data.toLeafs.Remove(destination);

            //delete empty link
            if (data.toLeafs.Count == 0)
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

            _globalLinks.Remove(globalLeaf);
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
            // OnExecuteLink = null;
            Breakpoint = null;
        }

        public ILeaf[] GetLeafs() => Leafs.ToArray();

        public BehaviourMachine GetGraph() => _parentGraph;

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

        private void ChangeRunningLeaf(ILeaf newLeaf, Transition transition)
        {
            Breakpoint?.Invoke();
            OnChangeRunningLeaf?.Invoke(newLeaf);

            AbortRunningLeaf();
            ApplyRunningLeaf(newLeaf);

            RunningLeaf.EnterLeaf(transition);
        }
    }
}