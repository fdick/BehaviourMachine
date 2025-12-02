using BehaviourGraph.Conditions;
using BehaviourGraph.States;
using BehaviourGraph.Trees;

namespace BehaviourGraph.Visualizer
{
    public class VisualizedSequencer : VisualizedCondition
    {
        public VisualizedCondition[] conditions;

        public override ICondition GetInstance(Sequence seq)
        {
            ICondition[] iCons = new ICondition[conditions.Length];
            for (int i = 0; i < conditions.Length; i++)
                iCons[i] = conditions[i].GetInstance(seq);

            return new Sequencer(iCons);
        }

    }
}
