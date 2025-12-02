using BehaviourGraph.States;

namespace BehaviourGraph.Visualizer
{
    public class ParallelInspector : Inspector<Parallel>
    {
        public override string Visualize(Parallel tree)
        {
            if (tree == null)
                return null;

            var returnString = string.Empty;


            returnString += tree.FriendlyName;
            returnString += "\n----------------------------------------\n";

            //main leaf status
            var text = "MAIN STATE \n" + "     " + tree.MainState.FriendlyName;
            returnString += text;

            //parallel leafs statuses
            returnString += "\n\nPARALLEL STATE";
            int it = 1;
            
            var l = tree.ParallelState;
            var endableLeaf = l as IEndableState;
            text = $"\n     {it}. " + l.FriendlyName;
            returnString += InsertStatus(text,
                endableLeaf != null ? endableLeaf.EndCondition() : UpdateStatus.Running, 55);
            it++;

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