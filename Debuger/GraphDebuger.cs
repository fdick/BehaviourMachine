using BehaviourGraph.Trees;
using System;
using System.Collections;
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

            _graph.MainTree.OnChangeLastActiveLeaf += (tree, from, to, fromStatus) =>
            {
                // _previousLeafDetailed.value = string.Empty;
                // _previousLeafDetailed.value = GetDetailedInfoAbout_Tree_Recursive(_graph.MainTree, fromStatus, tree);
                // _previousLeafDetailed.value = string.Empty;
                // _previousLeafDetailed.value = GetDetailedInfoAbout_Tree_Recursive(_graph.MainTree, LeafStatus.Running);
            };

            _graph.MainTree.OnChangedLastActiveLeaf += (tree) =>
            {
                _runningLeaf = string.Empty;
                _runningLeaf = GetHierarchyName(_graph.MainTree);

                _runningLeafDetailed.value = string.Empty;
                _runningLeafDetailed.value = GetDetailedInfoAbout_Tree_Recursive(_graph.MainTree, LeafStatus.Running);

                if (enabledDebugMode)
                    PauseEditor();
            };

            int cTrees = GetCountTrees(_graph.MainTree);
            int cLeafs = GetCountLeafs(_graph.MainTree) - cTrees + 1; // +1 так как не следует считать рута
            var cLinks = GetCountLinks(_graph.MainTree);
            _graphInfo =
                $"Count Leafs: {cLeafs} \nCount Trees: {cTrees} \n   Sum: {cLeafs + cTrees}\n----------------------\n";
            _graphInfo +=
                $"Count Global Links: {cLinks.z} \nCount Local   Links {cLinks.x} \nCount Ended Links: {cLinks.y} \n   Sum: {cLinks.x + cLinks.y + cLinks.z}";
        }

        /// <summary>
        /// Set editor on pause when running leaf in main tree is changing
        /// </summary>
        [SerializeField] private string _runningLeaf;

        [SerializeField] private HidedString _runningLeafDetailed = new HidedString();
        [SerializeField] private HidedString _previousLeafDetailed = new HidedString();

        [Multiline(9)] [SerializeField] private string _graphInfo;


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

        private string GetDetailedInfoAbout_Tree_Recursive(ITree tree, LeafStatus previousLeafStatus)
        {
            if (tree == null)
                return null;

            var returnString = string.Empty;

            if (tree is HierarchyTree hTree)
            {
                returnString += GetDetailedInfoAbout_Tree(hTree, previousLeafStatus) + "\n\n\n";

                if (tree.GetRunningLeaf() is ITree)
                {
                    returnString +=
                        GetDetailedInfoAbout_Tree_Recursive(tree.GetRunningLeaf() as ITree, previousLeafStatus);
                }
                // else
                // returnString += GetDetailedInfoAbout_Tree(hTree, previousLeafStatus);
            }
            else if (tree is ParallelTree pTree)
            {
                returnString += GetDetailedInfoAbout_ParallelTree(pTree, previousLeafStatus);
                if (pTree.GetRunningLeaf() is HierarchyBranch hTree2)
                    returnString += GetDetailedInfoAbout_Tree(hTree2, previousLeafStatus) + "\n\n\n";
            }

            return returnString;
        }


        private string GetDetailedInfoAbout_Tree(HierarchyTree tree, LeafStatus previousLeafStatus)
        {
            if (tree == null || tree.GetRunningLeaf() == null)
                return null;

            var returnString = string.Empty;

            //1 stroke
            string text;
            returnString += GetHierarchyName(_graph.MainTree, tree);
            returnString = InsertStatus(returnString, previousLeafStatus, 55);
            returnString += "\n----------------------------------------\n";

            //global links
            string label = "Global Links \n";
            label = label.ToUpper();
            returnString += label;

            int it = 0;
            foreach (var l in tree.GlobalLinks)
            {
                it++;
                returnString += it + ". To " + l.Key.FriendlyName + "\n";

                if (l.Value.toLeafConditions.Count > 0)
                {
                    int i = 0;
                    foreach (var c in l.Value.toLeafConditions)
                    {
                        i++;
                        text = "     " + i + ". " + c.condition.FriendlyName;
                        returnString += InsertStatus(text, c.condition.OnUpdate(), 55) + "\n";
                    }
                }
            }

            returnString += "\n";

            //local links
            label = "Local Links \n";
            label = label.ToUpper();
            returnString += label;

            if (tree.LocalLinks.TryGetValue(tree.GetRunningLeaf(), out var d))
            {
                it = 0;
                foreach (var l in d.toLeafs)
                {
                    it++;
                    returnString += it + ". To " + l.Key.FriendlyName + "\n";

                    if (l.Value.Count > 0)
                    {
                        int i = 0;
                        foreach (var c in l.Value)
                        {
                            i++;
                            text = "     " + i + ". " + c.condition.FriendlyName;
                            returnString += InsertStatus(text, c.condition.OnUpdate(), 55) + "\n";
                        }
                    }
                }
            }

            returnString += "\n";


            label = "End Link \n";
            label = label.ToUpper();
            returnString += label;
            //ended link
            if (tree.EndLinks.TryGetValue(tree.GetRunningLeaf(), out var leaf))
            {
                text = "     " + "To " + leaf.FriendlyName;
                returnString += InsertStatus(
                    text,
                    tree.GetRunningLeaf().OnUpdate() == LeafStatus.Running ? LeafStatus.Failure : LeafStatus.Successed,
                    30);
            }

            //}
            return returnString;
        }

        private string GetDetailedInfoAbout_ParallelTree(ParallelTree tree, LeafStatus previousLeafStatus)
        {
            if (tree == null)
                return null;

            var returnString = string.Empty;


            //label
            returnString += GetHierarchyName(_graph.MainTree, tree);
            returnString = InsertStatus(returnString, previousLeafStatus, 55);
            returnString += "\n----------------------------------------\n";

            //main leaf status
            var text = "MAIN LEAF \n" + "     " + tree.GetMainLeaf().FriendlyName;
            returnString += InsertStatus(text, previousLeafStatus, 55);

            //parallel leafs statuses
            returnString += "\n\nPARALLEL LEAFS";
            int it = 1;
            foreach (var l in tree.GetParallelingLeafs())
            {
                text = $"\n     {it}. " + l.FriendlyName;
                returnString += InsertStatus(text, l.OnUpdate(), 55);
                it++;
            }

            return returnString;
        }


        private string InsertStatus(string modifyableText, LeafStatus status, int offset)
        {
            var deltaOfsset = offset - modifyableText.Length;
            if (deltaOfsset < 0)
                deltaOfsset = 0;
            else
            {
                for (int i = 0; i < deltaOfsset; i++)
                    modifyableText += " ";
            }

            modifyableText += "| " + status;

            return modifyableText;
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
    }
}