using BehaviourGraph.Trees;
using BehaviourGraph.Visualizer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public GraphDebuger(AIBehaviourGraph graph, bool enabledDebugMode = false)
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
            var cLinks = GetCountLinks(_graph.MainTree);
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

        private AIBehaviourGraph _graph;
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
            _runningBranchFullName = GetHierarchyName(_graph.MainTree);

            _runningBranch.value = string.Empty;
            _runningBranch.value = GetDebugInfoTreeRecursive(_graph.MainTree);
        }

        private void SetInspectors()
        {
            _inspectors = new Dictionary<Type, object>();

            //get all scripts which inherit from InspectorBase
            System.Type[] types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
            System.Type[] inherits = (from System.Type type in types where type.IsSubclassOf(typeof(InspectorBase)) select type).ToArray();

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
        private string GetHierarchyName(ITree tree, ITree beforePoint = null)
        {
            if (tree == null)
                return null;

            var returnString = string.Empty;

            returnString += " / " + tree.FriendlyName;

            if (tree is HierarchyTree)
            {
                if (tree.GetRunningLeaf() is ITree && (beforePoint == null || tree != beforePoint))
                    returnString += GetHierarchyName(tree.GetRunningLeaf() as ITree);
                else if (tree.GetRunningLeaf() is ILeaf)
                    returnString += " / " + tree.GetRunningLeaf().FriendlyName;
            }

            return returnString;
        }

        private string GetDebugInfoTreeRecursive(ITree tree)
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
            if (tree.GetRunningLeaf() is ITree childTree)
                returnString += GetDebugInfoTreeRecursive(childTree);

            return returnString;
        }

        private object GetInspectorRecursive(Type key)
        {
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
            int countLeafs = tree.GetLeafs().Length;
            foreach (var l in tree.GetLeafs())
            {
                if (l is ITree localTree)
                    countLeafs += GetCountLeafs(localTree);
            }

            return countLeafs;
        }

        private int GetCountTrees(ITree tree)
        {
            int countTrees = 1;
            foreach (var l in tree.GetLeafs())
            {
                if (l is ITree localTree)
                    countTrees += GetCountTrees(localTree);
            }

            return countTrees;
        }

        private Vector3Int GetCountLinks(HierarchyTree tree)
        {
            Vector3Int countLinks = new Vector3Int(tree.LocalLinks.Count, tree.EndLinks.Count, tree.GlobalLinks.Count);

            foreach (var l in tree.Leafs)
            {
                if (l is HierarchyTree localTree)
                    countLinks += GetCountLinks(localTree);
            }

            return countLinks;
        }

        private void InitBreakpoint()
        {
            //find all breakpoint
            var brps = GetBreakpoints(_graph.MainTree);

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

        private List<IDebugBreakpoint> GetBreakpoints(ITree tree)
        {
            var leafs = tree.GetLeafs();
            if (leafs.Length == 0)
                return null;

            var brps = new List<IDebugBreakpoint>();
            //check myself
            if (tree is IDebugBreakpoint treeB)
                brps.Add(treeB);

            for (int i = 0; i < leafs.Length; i++)
            {
                var l = leafs[i];

                if (l is ITree t)
                    brps.AddRange(GetBreakpoints(t));
            }

            return brps;
        }
    }
}