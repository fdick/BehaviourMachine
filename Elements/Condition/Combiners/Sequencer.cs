using System.Collections;
using System.Linq;

namespace BehaviourGraph.Conditions
{
    /// <summary>
    /// Like <and> for all child conditions
    /// </summary>
    public class Sequencer : CombinerCondiitons
    {
        public Sequencer(params IConditional[] conditions)
        {
            Conditions = conditions.ToList();

            FriendlyName = "[ ";
            for (int i = 0; i < Conditions.Count; i++)
            {
                if (i > 0)
                    FriendlyName += " && ";
                FriendlyName += Conditions[i].FriendlyName;
                if (i == Conditions.Count - 1)
                    FriendlyName += " ]";
            }
        }

        public override LeafStatus OnUpdate()
        {
            foreach (var c in Conditions)
            {
                if (c.OnUpdate() == LeafStatus.Failure)
                    return LeafStatus.Failure;
            }

            return LeafStatus.Successed;
        }
    }
}