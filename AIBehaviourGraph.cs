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

    public enum DebugModes
    {
        Disabled,
        Enabled,
        DetailedDebug
    }

    public enum GraphStatuses
    {
        None,
        Inited,
        Ended,
        Started,
        Paused
    }

    public class AIBehaviourGraph : MonoBehaviour
    {
        [SerializeField] private bool _startGraphOnStart = false;
        [SerializeField] private GraphUpdateType _updateType;
        [SerializeField] private float _tickValue = 0.1f;
        [field: SerializeField] public GameObject CustomGameobject;
        [field: SerializeField] public VisualizedEmptyHierarchyTree VisualizedTree { get; private set; }
        [field: SerializeField] public GraphStatuses GraphStatus { get; private set; }

        public HierarchyTree MainTree { get; private set; }
        private Coroutine _updatorCoro;
        readonly string visRootTreeName = "VisualizedRootTree";

        [field: Space]
        [field: SerializeField] public DebugModes DebugMode { get; private set; }
        [SerializeField] private Debug.GraphDebuger _debug;

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
            if (GraphStatus == GraphStatuses.Ended)
                return;
            if (DebugMode >= DebugModes.Enabled)
                _debug.StopUpdator();
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
            //start update graph
            StartUpdator();
            GraphStatus = GraphStatuses.Started;
        }

        public void StopGraph()
        {
            if (GraphStatus != GraphStatuses.Started || GraphStatus != GraphStatuses.Paused)
                return;
            if (GraphStatus == GraphStatuses.Paused)
                UnPauseGraph();

            MainTree.EndTree();
            StopUpdator();
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

            GraphStatus = GraphStatuses.Paused;
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

        public void InitGraph()
        {
            if (VisualizedTree == null || GraphStatus > GraphStatuses.None)
                return;

            MainTree = (HierarchyTree)VisualizedTree.GetInstance(this);

            if (MainTree == null)
                UnityEngine.Debug.LogError("Init visualized tree have errors!");
            MainTree.FriendlyName = "RootTree";

            GraphStatus = GraphStatuses.Inited;
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
                VisualizedEmptyHierarchyTree visTree = (VisualizedEmptyHierarchyTree)root.AddComponent(typeof(VisualizedEmptyHierarchyTree));
                root.transform.SetParent(transform);
                root.transform.localPosition = Vector3.zero;
                visTree.FriendlyName = "RootTree";

                VisualizedTree = visTree;
            }
        }
    }
}