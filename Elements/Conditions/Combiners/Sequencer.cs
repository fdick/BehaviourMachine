using System.Collections;
using System.Linq;

namespace BehaviourGraph.Conditions
{
    /// <summary>
    /// Like <and> for all child conditions
    /// </summary>
    public class Sequencer : ConditionsCombiner
    {
        public Sequencer(params ICondition[] conditions)
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

        public override UpdateStatus ConditionUpdate()
        {
            foreach (var c in Conditions)
            {
                if (c.ConditionUpdate() == UpdateStatus.Failure)
                    return UpdateStatus.Failure;
            }

            return UpdateStatus.Successed;
        }
    }
}