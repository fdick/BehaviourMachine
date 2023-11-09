using BehaviourGraph.Trees;
using BehaviourGraph.Visualizer;
using UnityEngine;

namespace BehaviourGraph
{
    public class BehaviourMachine : MonoBehaviour
    {
        [field: Space] [field: SerializeField] public GameObject CustomGameobject { get; private set; }
        [field: SerializeField] public VisualizedEmptyHierarchyTree VisualizedTree { get; private set; }
        [field: SerializeField] public GraphStatuses GraphStatus { get; private set; }

        public HierarchyTree MainTree { get; private set; }
        private Coroutine _updatorCoro;
        private readonly string _visRootTreeName = "VisualizedRootTree";
        private const string ROOT_TREE = "RootTree";

#if UNITY_EDITOR
        [field: Space] [field: SerializeField] public DebugModes DebugMode { get; private set; }
        [SerializeField] private Debug.GraphDebuger _debug;
#endif

        private void Awake()
        {
            if (!enabled)
                return;
            InitGraph(CustomGameobject == null ? gameObject : CustomGameobject);
        }

        private void Start()
        {
            StartGraph();
        }

        private void Update()
        {
            UpdateGraph();
        }

        private void FixedUpdate()
        {
            FixedUpdateGraph();
        }

        private void LateUpdate()
        {
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
            StopGraph();
            DealocateGraph();
        }

        public void StartGraph()
        {
            if (GraphStatus != GraphStatuses.Inited)
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
            MainTree.EndTree();
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
            if (VisualizedTree == null || GraphStatus > GraphStatuses.None)
                return;

            if (customGameobject != null)
                CustomGameobject = customGameobject;

            MainTree = (HierarchyTree)VisualizedTree.GetInstance(this);

            if (MainTree == null)
                UnityEngine.Debug.LogError("Init visualized tree has errors!");
            MainTree.FriendlyName = ROOT_TREE;

            for (int i = 0; i < MainTree.Leafs.Count; i++)
            {
                MainTree.Leafs[i].InitLeaf();
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
        }

       [InspectorButton("Init Visualized Root Tree")]
        public void PrepareHierarchy()
        {
            if (!transform.Find(_visRootTreeName))
            {
                var root = new GameObject(_visRootTreeName);
                VisualizedEmptyHierarchyTree visTree =
                    (VisualizedEmptyHierarchyTree)root.AddComponent(typeof(VisualizedEmptyHierarchyTree));
                root.transform.SetParent(transform);
                root.transform.localPosition = Vector3.zero;
                visTree.FriendlyName = ROOT_TREE;

                VisualizedTree = visTree;
            }
        }
    }
}