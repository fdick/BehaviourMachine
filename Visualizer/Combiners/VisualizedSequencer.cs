using BehaviourGraph.Conditions;
using BehaviourGraph.Trees;

namespace BehaviourGraph.Visualizer
{
    public class VisualizedSequencer : VisualizedCondition
    {
        public VisualizedCondition[] conditions;

        public override ICondition GetInstance(ITree tree)
        {
            ICondition[] iCons = new ICondition[conditions.Length];
            for (int i = 0; i < conditions.Length; i++)
                iCons[i] = conditions[i].GetInstance(tree);

            return new Sequencer(iCons);
        }

    }
}
