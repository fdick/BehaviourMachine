using BehaviourGraph.Leafs;
using BehaviourGraph.Trees;

namespace BehaviourGraph.Visualizer
{
    public class ParallelTreeInspector : Inspector<ParallelBranch>
    {
        public override string Visualize(ParallelBranch tree)
        {
            if (tree == null)
                return null;

            var returnString = string.Empty;


            returnString += tree.FriendlyName;
            returnString += "\n----------------------------------------\n";

            //main leaf status
            var text = "MAIN LEAF \n" + "     " + tree.GetMainLeaf().FriendlyName;
            returnString += text;

            //parallel leafs statuses
            returnString += "\n\nPARALLEL LEAFS";
            int it = 1;
            foreach (var l in tree.GetParallelLeafs())
            {
                var endableLeaf = l as IEndableLeaf;
                text = $"\n     {it}. " + l.FriendlyName;
                returnString += InsertStatus(text,
                    endableLeaf != null ? endableLeaf.EndCondition() : UpdateStatus.Running, 55);
                it++;
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