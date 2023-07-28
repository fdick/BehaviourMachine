using System.Collections;
using System.Linq;

namespace BehaviourGraph.Conditions
{
    /// <summary>
    /// Like <or> for all child conditions
    /// </summary>
    public class Selector : CombinerCondiitons
    {
        public Selector(params IConditional[] conditions)
        {
            Conditions = conditions.ToList();

            FriendlyName = "Selector: ";
            foreach (var c in Conditions)
                FriendlyName += c.FriendlyName + " - ";
        }

        public override LeafStatus OnUpdate()
        {
            foreach (var c in Conditions)
            {
                if (c.OnUpdate() == LeafStatus.Successed)
                    return LeafStatus.Successed;
            }
            return LeafStatus.Failure;
        }
    }
}