using BehaviourGraph.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace BehaviourGraph.Trees
{
    public class ConditionData
    {
        public GUID ID;
        public IConditional condition;
    }

    public class LocalLinkData
    {
        public Dictionary<ILeaf, List<ConditionData>> toLeafs;
    }

    public class GlobalLinkData
    {
        public List<ConditionData> toLeafConditions;
    }


    /// <summary>
    /// This tree processes its leaves hierarchically. Only one process can run at a time. Need dispose after finishing.
    /// </summary>
    public class HierarchyTree : ITree, IDisposable, IDebugBreakpoint
    {
        protected List<ILeaf> _leafs = new List<ILeaf>();
        protected List<ITree> _childTrees = new List<ITree>();
        protected List<ConditionData> _conditions = new List<ConditionData>();

        protected Dictionary<ILeaf, ILeaf> _endLinks = new Dictionary<ILeaf, ILeaf>();
        protected Dictionary<ILeaf, LocalLinkData> _localLinks = new Dictionary<ILeaf, LocalLinkData>();
        protected Dictionary<ILeaf, GlobalLinkData> _globalLinks = new Dictionary<ILeaf, GlobalLinkData>();


        private AIBehaviourGraph _parentGraph;

        /// <summary>
        /// Startable leaf
        /// </summary>
        public ILeaf StartableLeaf { get; set; }

        /// <summary>
        /// Current active leaf
        /// </summary>
        public ILeaf RunningLeaf { get; protected set; }

        /// <summary>
        /// Ended active leaf.
        /// </summary>
        public ILeaf EndedRunningLeaf { get; protected set; }


        public Dictionary<ILeaf, ILeaf> EndLinks
        {
            get => _endLinks;
        }

        public Dictionary<ILeaf, LocalLinkData> LocalLinks
        {
            get => _localLinks;
        }

        public Dictionary<ILeaf, GlobalLinkData> GlobalLinks
        {
            get => _globalLinks;
        }


        public List<ILeaf> Leafs
        {
            get => _leafs;
        }

        public List<ConditionData> Conditions
        {
            get => _conditions;
        }

        public string FriendlyName { get; set; }

        /// <summary>
        /// Calls when running leaf is changing. ILeaf param is a new leaf.
        /// </summary>
        public Action<ILeaf> OnChangeRunningLeaf { get; set; }

        /// <summary>
        /// Calls when proc any condition. Return proced condition
        /// </summary>
        public Action<IConditional> OnProcCondiiton { get; set; }

        /// <summary>
        /// Calls when execute child links. Only for local and global links
        /// </summary>
        public Action<GUID> OnExecuteLink { get; set; }

        public bool IsPaused { get; private set; }

        public Action Breakpoint { get; set; }

        public UpdateStatus Status { get; private set; } = UpdateStatus.Failure;

        public HierarchyTree(AIBehaviourGraph graph, ILeaf[] leafs)
        {
            _parentGraph = graph;

            FriendlyName = ToString().Split('.').Last();

            for (int i = 0; i < leafs.Length; i++)
            {
                AddLeaf(leafs[i]);
            }
        }

        public HierarchyTree(AIBehaviourGraph graph)
        {
            _parentGraph = graph;
            FriendlyName = ToString().Split('.').Last();
        }


        public void StartTree()
        {
            if (StartableLeaf == null)
                return;

            ChangeRunningLeaf(StartableLeaf, null);
        }

        public UpdateStatus UpdateTree()
        {
            if (IsPaused)
                return UpdateStatus.Failure;

            if (RunningLeaf == null)
                return UpdateStatus.Failure;

            var status = RunningLeaf.OnUpdate();

            //global links update
            if (_globalLinks.Count > 0)
            {
                foreach (var d in _globalLinks)
                {
                    foreach (var c in d.Value.toLeafConditions)
                    {
                        if (RunningLeaf == d.Key)
                            continue;
                        if (c.condition.OnUpdate() == UpdateStatus.Successed)
                        {
                            OnProcCondiiton?.Invoke(c.condition);
                            OnExecuteLink?.Invoke(c.ID);
                            ChangeRunningLeaf(d.Key, c);
                            Status = status;
                            return status;
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
                        if (c.condition.OnUpdate() == UpdateStatus.Successed)
                        {
                            OnProcCondiiton?.Invoke(c.condition);
                            OnExecuteLink?.Invoke(c.ID);
                            ChangeRunningLeaf(d.Key, c);
                            Status = status;
                            return status;
                        }
                    }
                }
            }

            //end link update
            if (status == UpdateStatus.Successed &&
                _endLinks.TryGetValue(RunningLeaf, out var toLeaf))
            {
                OnProcCondiiton?.Invoke(null);
                ChangeRunningLeaf(toLeaf, null);
                Status = status;
                return status;
            }

            Status = status;
            return status;
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
        public GUID Link(ILeaf origin, ILeaf destination, IConditional condition)
        {
            if (!_leafs.Contains(origin) ||
                !_leafs.Contains(destination) ||
                condition == null)
                return default;
            var ID = GUID.Generate();

            //add link
            if (_localLinks.TryGetValue(origin, out var data))
            {
                var conditionData = new ConditionData() { condition = condition, ID = ID };

                if (data.toLeafs.TryGetValue(destination, out var conds))
                {
                    //i cant contains same condition
                    if (conds.Where((x) => x.condition == condition).Count() > 0)
                    {
                        UnityEngine.Debug.LogError($"{condition.FriendlyName} already contains!");
                        return default;
                    }

                    conds.Add(conditionData);
                }
                else
                    data.toLeafs.Add(destination, new List<ConditionData> { conditionData });

                _conditions.Add(conditionData);
            }
            else
            {
                var linkData = new Dictionary<ILeaf, List<ConditionData>>();
                var conditionData = new ConditionData() { condition = condition, ID = ID };
                linkData.Add(destination, new List<ConditionData> { conditionData });
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
            if (_endLinks.TryGetValue(origin, out var toLeaf))
            {
                toLeaf = destination;
                return;
            }

            _endLinks.Add(origin, destination);
        }

        /// <summary>
        /// Add global link. Return unique ID for this link
        /// </summary>
        public GUID Link(ILeaf toLeaf, IConditional condition)
        {
            if (toLeaf == null || condition == null)
                return default;

            if (!_leafs.Contains(toLeaf))
                return default;

            var ID = GUID.Generate();
            var conditionData = new ConditionData() { condition = condition, ID = ID };

            if (_globalLinks.TryGetValue(toLeaf, out var l))
                l.toLeafConditions.Add(conditionData);
            else
                _globalLinks.Add(toLeaf,
                    new GlobalLinkData() { toLeafConditions = new List<ConditionData>() { conditionData } });

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
        public ILeaf GetEndedRunningLeaf() => EndedRunningLeaf;
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
            OnProcCondiiton = null;
            OnExecuteLink = null;
            Breakpoint = null;
        }

        public ILeaf[] GetLeafs() => Leafs.ToArray();

        public AIBehaviourGraph GetGraph() => _parentGraph;

        private void AbortRunningLeaf()
        {
            if (RunningLeaf == null)
                return;

            RunningLeaf.OnEnd();
            RunningLeaf = null;
        }

        private void ApplyRunningLeaf(ILeaf leaf)
        {
            if (RunningLeaf != null)
                return;

            RunningLeaf = leaf;
            EndedRunningLeaf = leaf;
        }

        private void ChangeRunningLeaf(ILeaf newLeaf, ConditionData conditionData)
        {
            Breakpoint?.Invoke();
            OnChangeRunningLeaf?.Invoke(newLeaf);

            AbortRunningLeaf();
            ApplyRunningLeaf(newLeaf);

            RunningLeaf.OnStart(conditionData);
        }
    }
}