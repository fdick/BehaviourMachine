using BehaviourGraph.Trees;
using BehaviourGraph.Visualizer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviourGraph.States;
using UnityEngine;

namespace BehaviourGraph.Debug
{
    [Serializable]
    public class HidedString
    {
        [Multiline(30)] public string value;
    }

    [Serializable]
    public class GraphDebuger
    {
        public GraphDebuger(BehaviourMachine graph, bool enabledDebugMode = false)
        {
            if (graph == null)
                UnityEngine.Debug.LogError("Graph is null!");
            if (graph.MainTree == null)
                UnityEngine.Debug.LogError("Main tree is null!");

            _graph = graph;
            _enabledDebugMode = enabledDebugMode;

            if (enabledDebugMode)
                InitBreakpoint();

            int cTrees = GetCountTrees(_graph.MainTree);
            int cLeafs = GetCountLeafs(_graph.MainTree) - cTrees + 1; // +1 так как не следует считать рута
            var cLinks = GetCountLinks(_graph.MainTree.GetStartableState() as Sequence);
            _graphInfo =
                $"Count Leafs: {cLeafs} \nCount Trees: {cTrees} \n   Sum: {cLeafs + cTrees}\n----------------------\n";
            _graphInfo +=
                $"Count Global Links: {cLinks.z} \nCount Local   Links {cLinks.x} \nCount Ended Links: {cLinks.y} \n   Sum: {cLinks.x + cLinks.y + cLinks.z}";

            SetInspectors();
        }

        /// <summary>
        /// Set editor on pause when running leaf in main tree is changing
        /// </summary>
        [SerializeField] private string _runningBranchFullName;

        [SerializeField] private HidedString _runningBranch = new HidedString();

        [Multiline(9)] [SerializeField] private string _graphInfo;


        private Dictionary<Type, object> _inspectors;

        private BehaviourMachine _graph;
        private bool _enabledDebugMode;
        private Coroutine _updatorCoro;


        public void StartUpdator()
        {
            if (_updatorCoro != null)
                return;
            _updatorCoro = _graph.StartCoroutine(Updator());
        }

        public void StopUpdator()
        {
            if (_updatorCoro == null)
                return;
            _graph.StopCoroutine(_updatorCoro);
        }

        private IEnumerator Updator()
        {
            do
            {
                yield return null;
                OnUpdate();
            } while (true);
        }

        private void OnUpdate()
        {
            _runningBranchFullName = string.Empty;
            _runningBranchFullName = GetHierarchyName(_graph.MainTree.GetStartableState() as Sequence);

            _runningBranch.value = string.Empty;
            _runningBranch.value = GetDebugInfoTreeRecursive(_graph.MainTree.GetStartableState() as Sequence);
        }

        private void SetInspectors()
        {
            _inspectors = new Dictionary<Type, object>();

            //get all scripts which inherit from InspectorBase
            System.Type[] types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
            System.Type[] inherits =
                (from System.Type type in types where type.IsSubclassOf(typeof(InspectorBase)) select type).ToArray();

            // i = 1 bcz first is Inspector<>, we dont need it
            for (int i = 1; i < inherits.Length; i++)
            {
                Type inh = inherits[i];
                string name = inh.BaseType.ToString();
                var arr = name.Split('[', ']');
                var type = Type.GetType(arr[1]);

                _inspectors.Add(type, Activator.CreateInstance(inh));
            }
        }

        //recursive
        private string GetHierarchyName(Sequence seq, Sequence beforePoint = null)
        {
            if (seq == null)
                return null;

            var returnString = string.Empty;

            returnString += " / " + seq.FriendlyName;

            if (seq.RunningState is Sequence && (beforePoint == null || seq != beforePoint))
                returnString += GetHierarchyName(seq.RunningState as Sequence);
            else if (seq.RunningState is IState)
                returnString += " / " + seq.RunningState.FriendlyName;
            
            return returnString;
        }

        private string GetDebugInfoTreeRecursive(Sequence tree)
        {
            if (tree == null)
                return null;

            var returnString = string.Empty;

            //get inspector
            var inspector = GetInspectorRecursive(tree.GetType());

            //get visualize
            var t = inspector.GetType();
            var str = t.GetMethod("Visualize").Invoke(inspector, new object[] { tree });
            returnString += str + "\n\n\n";

            //if running leaf is tree
            if (tree.RunningState is Sequence childTree)
                returnString += GetDebugInfoTreeRecursive(childTree);

            return returnString;
        }

        private object GetInspectorRecursive(Type key)
        {
            if (key == null)
            {
                UnityEngine.Debug.LogError("Key is null!");
            }

            if (!_inspectors.TryGetValue(key, out var inspector))
                inspector = GetInspectorRecursive(key.BaseType);
            return inspector;
        }

        private void PauseEditor()
        {
            UnityEngine.Debug.Break();
        }

        private int GetCountLeafs(ITree tree)
        {
            int countLeafs = tree.GetStates().Length;
            foreach (var l in tree.GetStates())
            {
                if (l is ITree localTree)
                    countLeafs += GetCountLeafs(localTree);
            }

            return countLeafs;
        }

        private int GetCountTrees(ITree tree)
        {
            int countTrees = 1;
            foreach (var l in tree.GetStates())
            {
                if (l is ITree localTree)
                    countTrees += GetCountTrees(localTree);
            }

            return countTrees;
        }

        private Vector3Int GetCountLinks(Sequence seq)
        {
            if (seq == null)
                return Vector3Int.zero;
            Vector3Int countLinks = new Vector3Int(seq.LocalLinks.Count, seq.EndLinks.Count, seq.GlobalLinks.Count);

            foreach (var l in seq.GetStates())
            {
                if (l is Sequence localSequence)
                    countLinks += GetCountLinks(localSequence);
            }

            return countLinks;
        }

        private void InitBreakpoint()
        {
            //find all breakpoint
            var brps = GetBreakpoints(_graph.MainTree.GetStartableState() as Sequence);

            if (brps == null)
            {
                UnityEngine.Debug.LogError("I can't find breakpoints!");
                return;
            }

            UnityEngine.Debug.Log($"Count breakpoint: {brps.Count}");

            //init them
            foreach (var b in brps)
            {
                b.Breakpoint += PauseEditor;
            }
        }

        private List<IDebugable> GetBreakpoints(Sequence seq)
        {
            var leafs = seq.GetStates();
            if (leafs.Length == 0)
                return null;

            var brps = new List<IDebugable>();
            //check myself
            if (seq is IDebugable treeB)
                brps.Add(treeB);

            for (int i = 0; i < leafs.Length; i++)
            {
                var l = leafs[i];

                if (l is Sequence t)
                    brps.AddRange(GetBreakpoints(t));
            }

            return brps;
        }
    }
}