using BehaviourGraph.Conditions;
using BehaviourGraph.Trees;

namespace BehaviourGraph.Visualizer
{
    public class VisualizedSequencer : VisualizedCondition
    {
        public VisualizedCondition[] conditions;

        public override IConditional GetInstance(HierarchyBranch branch)
        {
            IConditional[] iCons = new IConditional[conditions.Length];
            for (int i = 0; i < conditions.Length; i++)
                iCons[i] = conditions[i].GetInstance(branch);

            return new Sequencer(iCons);
        }

    }
}
