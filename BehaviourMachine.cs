using System;
using BehaviourGraph.Trees;
using BehaviourGraph.Visualizer;
using UnityEngine;

namespace BehaviourGraph
{
    public class BehaviourMachine : MonoBehaviour
    {
        [Space] [SerializeField] private bool _initOnAwake = true;
        [field: SerializeField] public GameObject CustomGameobject { get; private set; }
        [field: SerializeField] public VisualizedOakTree VisualizedTree { get; set; }
        [field: SerializeField] public GraphStatuses GraphStatus { get; private set; }

        public ITree MainTree { get; private set; }
        private const string VIS_ROOT_TREE_NAME = "VisualizedRootTree";
        private const string ROOT_TREE = "RootTree";

#if UNITY_EDITOR
        [field: Space] [field: SerializeField] public DebugModes DebugMode { get; private set; } = DebugModes.Enabled;
        [SerializeField] private Debug.GraphDebuger _debug;
#endif

        private void Awake()
        {
            if (!_initOnAwake)
            {
                enabled = false;
                return;
            }

            InitGraph(CustomGameobject);
        }

        private void Start()
        {
            if (!_initOnAwake)
                return;
            StartGraph();
        }

        private void Update()
        {
            if (!_initOnAwake)
                return;
            UpdateGraph();
        }

        private void FixedUpdate()
        {
            if (!_initOnAwake)
                return;
            FixedUpdateGraph();
        }

        private void LateUpdate()
        {
            if (!_initOnAwake)
                return;
            LateUpdateGraph();
        }

        private void OnDestroy()
        {
            if (GraphStatus == GraphStatuses.Ended)
                return;
#if UNITY_EDITOR
            if (DebugMode >= DebugModes.Enabled)
                _debug.StopUpdator();
#endif
            if (!_initOnAwake)
                return;
            StopGraph();
            DealocateGraph();
        }

        public void OnDisable()
        {
            PauseGraph();
        }

        public void OnEnable()
        {
            if (MainTree?.GetRunningState() == null)
                return;

            UnPauseGraph();

            //restart last activated branch

            // var last = GetLastHierarchyTree(MainTree.GetRunningState());
            //
            // if (last == null) return;
            // last.ForceEnter(last.GetLeafIndex(last.RunningLeaf));
        }

        public void StartGraph()
        {
            if (GraphStatus != GraphStatuses.Inited && GraphStatus != GraphStatuses.Ended)
                return;
            if (MainTree.GetStates().Length == 0 || MainTree.GetStartableState() == null)
                return;
#if UNITY_EDITOR
            if (DebugMode >= DebugModes.Enabled)
            {
                _debug = new Debug.GraphDebuger(
                    this,
                    DebugMode == DebugModes.DetailedDebug ? true : false);

                _debug.StartUpdator();
            }
#endif
            //start tree
            MainTree.StartTree();
            GraphStatus = GraphStatuses.Started;
        }

        public void StopGraph()
        {
            if (GraphStatus < GraphStatuses.Started)
                return;
            if (GraphStatus == GraphStatuses.Paused)
                UnPauseGraph();
            MainTree.StopTree();
            GraphStatus = GraphStatuses.Ended;
        }

        public void PauseGraph()
        {
            if (GraphStatus != GraphStatuses.Started)
                return;

            MainTree.PauseTree();

            GraphStatus = GraphStatuses.Paused;
        }

        public void UnPauseGraph()
        {
            if (GraphStatus != GraphStatuses.Paused)
                return;

            MainTree.UnPauseTree();

            GraphStatus = GraphStatuses.Started;
        }

        public void DealocateGraph()
        {
            if (GraphStatus >= GraphStatuses.Ended)
                StopGraph();

            if (GraphStatus >= GraphStatuses.Inited)
            {
                MainTree.Dispose();
                MainTree = null;
                VisualizedTree = null;
            }

            GraphStatus = GraphStatuses.None;
        }

        public void InitGraph(GameObject customGameobject)
        {
            if (GraphStatus > GraphStatuses.None)
                return;

            if (customGameobject != null)
                CustomGameobject = customGameobject;
            else
            {
                if (CustomGameobject == null)
                    CustomGameobject = gameObject;
            }

            if (VisualizedTree != null)
            {
                MainTree = VisualizedTree.GetInstance(this);

                if (MainTree == null)
                    UnityEngine.Debug.LogError("Init visualized tree has errors!");
                MainTree.FriendlyName = ROOT_TREE;

                for (int i = 0; i < MainTree.GetStates().Length; i++)
                {
                    MainTree.GetStates()[i].InitializeState();
                }
            }
            else
            {
                MainTree = new OakTree(this);
            }

            GraphStatus = GraphStatuses.Inited;
        }

        public void UpdateGraph()
        {
            if (GraphStatus < GraphStatuses.Started)
                return;

            MainTree.UpdateTree();
        }

        public void FixedUpdateGraph()
        {
            if (GraphStatus < GraphStatuses.Started)
                return;

            MainTree.FixedUpdateTree();
        }

        public void LateUpdateGraph()
        {
            if (GraphStatus < GraphStatuses.Started)
                return;

            MainTree.LateUpdateTree();
            MainTree.EndCondition();
        }

        // public HierarchyTree GetLastHierarchyTree(ITree tree)
        // {
        //     if (tree == null)
        //         return null;
        //
        //     if (tree.GetRunningState() == null)
        //         return tree;
        //
        //     if (tree.GetRunningState() is HierarchyTree t)
        //     {
        //         return GetLastHierarchyTree(t);
        //     }
        //
        //
        //     return tree;
        // }

        [InspectorButton("Init Visualized Root Tree")]
        public void PrepareHierarchy()
        {
            if (!transform.Find(VIS_ROOT_TREE_NAME))
            {
                var root = new GameObject(VIS_ROOT_TREE_NAME);
                VisualizedOakTree visTree =
                    (VisualizedOakTree)root.AddComponent(typeof(VisualizedOakTree));
                root.transform.SetParent(transform);
                root.transform.localPosition = Vector3.zero;
                visTree.FriendlyName = ROOT_TREE;

                VisualizedTree = visTree;
            }
        }
    }
}