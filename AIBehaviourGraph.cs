using System;
using BehaviourGraph.Trees;
using BehaviourGraph.Visualizer;
using System.Collections;
using UnityEngine;


namespace BehaviourGraph
{
    public enum GraphUpdateType
    {
        Update,
        FixedUpdate,
        CustomTick,
    }

    public class AIBehaviourGraph : MonoBehaviour
    {
        [SerializeField] private bool _startGraphOnStart = false;
        [SerializeField] private GraphUpdateType _updateType;
        [SerializeField] private float _tickValue = 0.1f;
        [field: SerializeField] public GameObject CustomGameobject;
        public HierarchyTree MainTree { get; private set; }
        [field: SerializeField] public VisualizedBranch VisualizeTree { get; private set; }
        private Coroutine _updatorCoro;

        readonly string visRootTreeName = "VisualizedRootTree";

        [Space] [SerializeField] private bool _enableDebug = true;
        [SerializeField] private bool _enableDebugMode = false;
        [SerializeField] private Debug.GraphDebuger _debug;

        public bool IsStarted { get; private set; } = false;

        private void Awake()
        {
            if (CustomGameobject == null)
                CustomGameobject = gameObject;

            //init graph
            if (_startGraphOnStart)
                InitGraph();
        }

        private void Start()
        {
            if (_startGraphOnStart)
                StartGraph();
        }

        private void OnDestroy()
        {
            if (!IsStarted)
                return;
            if (_enableDebug)
                _debug.StopUpdator();
            DealocateGraph();
        }

        public void StartGraph()
        {
            if (IsStarted)
                return;
#if UNITY_EDITOR
            if (_enableDebug)
            {
                _debug = new Debug.GraphDebuger(this, _enableDebugMode);
                _debug.StartUpdator();
            }
#endif
            //start tree
            MainTree.StartTree();
            //start update graph
            StartUpdator();
            IsStarted = true;
        }

        public void StopGraph()
        {
            if (!IsStarted)
                return;
            MainTree.EndTree();
            StopUpdator();
            IsStarted = false;
        }

        public void PauseGraph()
        {
        }

        public void UnPauseGraph()
        {
        }

        public void DealocateGraph()
        {
            StopGraph();
            MainTree.Dispose();
            MainTree = null;
            VisualizeTree = null;
        }

        public void InitGraph()
        {
            if (VisualizeTree != null)
            {
                MainTree = InitVisualizedTree(VisualizeTree);
                if (MainTree == null)
                    UnityEngine.Debug.LogError("Init visualized tree has errors!");
                MainTree.FriendlyName = "RootTree";
            }
        }

        private HierarchyBranch InitVisualizedTree(VisualizedBranch branch)
        {
            if (branch is VisualizedEmptyBranch emptyBranch)
            {
                var tree = new HierarchyBranch(this);

                //add leafs
                foreach (var l in emptyBranch.leafs)
                {
                    Leaf cLeaf = default;
                    HierarchyBranch cBranch = default;
                    if (l is VisualizedLeaf ll)
                    {
                        cLeaf = ll.GetInstance();
                        tree.AddLeaf(cLeaf);
                    }
                    else
                    {
                        cBranch = l.GetInstance(this);
                        tree.AddLeaf(cBranch);
                    }

                    if (l.FriendlyName != String.Empty)
                    {

                        if (cLeaf != null)
                            cLeaf.FriendlyName = l.FriendlyName;
                        else
                            cBranch.FriendlyName = l.FriendlyName;
                    }
                        
                }

                //set startable leaf
                tree.StartableLeaf = tree.Leafs[emptyBranch.startableLeaf_ID];

                //add links
                foreach (var li in emptyBranch.links)
                {
                    for (int i = 0; i < li.froms.Length; i++)
                    {
                        var from = tree.Leafs[emptyBranch.leafs.IndexOf(li.froms[i])];
                        var to = tree.Leafs[emptyBranch.leafs.IndexOf(li.to)];

                        if (li.linkType == Visualizer.LinkType.FromTo)
                            tree.Link(
                                from,
                                to,
                                li.condition.GetInstance(tree));
                        else if (li.linkType == Visualizer.LinkType.Ended)
                            tree.Link(
                                from,
                                to);
                        else
                            tree.Link(
                                to,
                                li.condition.GetInstance(tree));
                    }
                }

                for (int i = 0; i < emptyBranch.leafs.Count; i++)
                {
                    var vl = emptyBranch.leafs[i];

                    if (vl is VisualizedEmptyBranch)
                        tree.Leafs[i] = InitVisualizedTree(vl);
                }

                return tree;
            }
            else if (branch is VisualizedBranch)
            {
                return branch.GetInstance(this);
            }

            return null;
        }

        private void StartUpdator()
        {
            if (_updatorCoro != null)
                return;

            _updatorCoro = StartCoroutine(GraphUpdator());
        }

        private void StopUpdator()
        {
            if (_updatorCoro == null)
                return;

            StopCoroutine(_updatorCoro);
            _updatorCoro = null;
        }

        private IEnumerator GraphUpdator()
        {
            do
            {
                if (MainTree == null)
                {
                    UnityEngine.Debug.LogError("Main Tree is not init!");
                    yield break;
                }

                switch (_updateType)
                {
                    case GraphUpdateType.Update:
                        yield return null;
                        break;
                    case GraphUpdateType.FixedUpdate:
                        yield return new WaitForFixedUpdate();
                        break;
                    case GraphUpdateType.CustomTick:
                        yield return new WaitForSeconds(_tickValue);
                        break;
                    default:
                        break;
                }

                MainTree.UpdateTree();
            } while (true);
        }

        [InspectorButton("Init Visualized Root Tree")]
        public void PrepareHierarchy()
        {
            if (!transform.Find(visRootTreeName))
            {
                var root = new GameObject(visRootTreeName);
                VisualizedEmptyBranch visTree = (VisualizedEmptyBranch)root.AddComponent(typeof(VisualizedEmptyBranch));
                root.transform.SetParent(transform);
                root.transform.localPosition = Vector3.zero;

                VisualizeTree = visTree;
            }
        }
    }
}