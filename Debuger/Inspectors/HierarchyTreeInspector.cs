using BehaviourGraph.Trees;
using System;

namespace BehaviourGraph.Visualizer
{
    public class HierarchyTreeInspector : Inspector<HierarchyTree>
    {
        public override string Visualize(HierarchyTree tree)
        {
            if (tree == null || tree.GetRunningLeaf() == null)
                return null;

            var returnString = string.Empty;

            //1 stroke
            string text;
            returnString += tree.FriendlyName;
            returnString += "\n----------------------------------------\n";

            //running leaf
            returnString += $"RUNNING LEAF - {tree.RunningLeaf.FriendlyName}\n\n";

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

                        UpdateStatus status;
                        try
                        {
                            status = c.condition.OnUpdate();
                        }
                        catch (Exception)
                        {
                            status = UpdateStatus.Failure;
                        }

                        returnString += InsertStatus(text, status, 55) + "\n";
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
                            UpdateStatus status;
                            try
                            {
                                status = c.condition.OnUpdate();
                            }
                            catch (Exception)
                            {
                                status = UpdateStatus.Failure;
                            }
                            returnString += InsertStatus(text, status, 55) + "\n";
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
                    tree.GetRunningLeaf().OnUpdate() == UpdateStatus.Running ? UpdateStatus.Failure : UpdateStatus.Successed,
                    30);
            }

            returnString += "\n----------------------------------------\n";

            return returnString;

        }

        private string InsertStatus(string modifyableText, UpdateStatus status, int offset)
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
    }

}