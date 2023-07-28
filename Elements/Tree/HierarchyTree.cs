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
    /// —тейт машина не €вл€етс€ стейтом. “о есть не может находитьс€ с остальными стейтами в одном списке.
    /// «апускает стейт, который идет первый в очереди. ѕриоритет условий линейный - условие, что первое было прописано имеет высший приоритет по сравнению с тем,
    /// что прописано было позже.
    /// </summary>
    public class HierarchyTree : ITree, IDisposable
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

        /// <summary>
        /// Leaf which was paused
        /// </summary>
        public ILeaf PausedLeaf { get; protected set; }

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
        /// Calls  when active leaf was changed but not started.
        /// </summary>
        public Action<ITree> OnChangedLastActiveLeaf { get; set; }

        /// <summary>
        /// Calls when my child is changing. From - To - from Status
        /// </summary>
        public Action<ILeaf, ILeaf, LeafStatus> OnChangeActiveLeaf { get; set; }

        /// <summary>
        /// Calls when any child is changing. Tree - From leaf - To leaf - from Status
        /// </summary>
        public Action<ITree, ILeaf, ILeaf, LeafStatus> OnChangeLastActiveLeaf { get; set; }

        /// <summary>
        /// Calls when proc any condition. Return proced condition
        /// </summary>
        public Action<IConditional> OnProcCondiiton { get; set; }

        /// <summary>
        /// Calls when execute child links. Only for local and global links
        /// </summary>
        public Action<GUID> OnExecuteLink { get; set; }

        public HierarchyTree(AIBehaviourGraph graph, ILeaf[] leafs)
        {
            for (int i = 0; i < leafs.Length; i++)
            {
                AddLeaf(leafs[i]);
            }

            _parentGraph = graph;

            FriendlyName = ToString().Split('.').Last();
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

            ChangeRunningLeaf(StartableLeaf, LeafStatus.Successed, null);
        }

        public LeafStatus UpdateTree()
        {
            if (RunningLeaf == null)
                return LeafStatus.Failure;

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
                        if (c.condition.OnUpdate() == LeafStatus.Successed)
                        {
                            OnProcCondiiton?.Invoke(c.condition);
                            OnExecuteLink?.Invoke(c.ID);
                            ChangeRunningLeaf(d.Key, status, c);
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
                        if (c.condition.OnUpdate() == LeafStatus.Successed)
                        {
                            OnProcCondiiton?.Invoke(c.condition);
                            OnExecuteLink?.Invoke(c.ID);
                            ChangeRunningLeaf(d.Key, status, c);
                            return status;
                        }
                    }
                }
            }

            //end link update
            if (status == LeafStatus.Successed &&
                _endLinks.TryGetValue(RunningLeaf, out var toLeaf))
            {
                OnProcCondiiton?.Invoke(null);
                ChangeRunningLeaf(toLeaf, status, null);
                return status;
            }

            return status;
        }


        public void EndTree()
        {
            //if running leaf is running than stop it
            AbortRunningLeaf();
        }

        public void PauseTree()
        {
            PausedLeaf = RunningLeaf;
            AbortRunningLeaf();
        }

        public void UnPauseTree()
        {
            if (PausedLeaf == null)
                return;

            ApplyRunningLeaf(PausedLeaf);
            RunningLeaf.OnStart();
        }

        public void AddLeaf(ILeaf leaf)
        {
            if (leaf == null)
                return;
            if (_leafs.Contains(leaf))
                return;

            leaf.SetGameobject(_parentGraph.CustomGameobject);
            _leafs.Add(leaf);

            if (leaf is HierarchyBranch)
            {
                var leafTree = leaf as HierarchyBranch;

                leafTree.OnChangeLastActiveLeaf += (q, w, e, r) => OnChangeLastActiveLeaf?.Invoke(q, w, e, r);
                leafTree.OnChangedLastActiveLeaf += (t) => OnChangedLastActiveLeaf?.Invoke(t);
            }
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
            OnChangedLastActiveLeaf = null;
            OnChangeLastActiveLeaf = null;
            OnChangeActiveLeaf = null;
            OnProcCondiiton = null;
            OnExecuteLink = null;
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

        private void ChangeRunningLeaf(ILeaf newLeaf, LeafStatus previewsLeafStatus, ConditionData conditionData)
        {
            OnChangeActiveLeaf?.Invoke(RunningLeaf, newLeaf, previewsLeafStatus);
            if (RunningLeaf is ILeaf)
                OnChangeLastActiveLeaf?.Invoke(this, RunningLeaf, newLeaf, previewsLeafStatus);
            
            AbortRunningLeaf();
            ApplyRunningLeaf(newLeaf);

            RunningLeaf.OnStart(conditionData);
            
            if (RunningLeaf is ILeaf)
                OnChangedLastActiveLeaf?.Invoke(this);
        }
    }
}