using BehaviourGraph.Conditions;
using BehaviourGraph.Trees;

namespace BehaviourGraph.Visualizer
{
    public class VisualizedSelector : VisualizedCondition
    {
        public VisualizedCondition[] conditions;

        public override IConditional GetInstance(ITree tree)
        {
            IConditional[] iCons = new IConditional[conditions.Length];
            for (int i = 0; i < conditions.Length; i++)
                iCons[i] = conditions[i].GetInstance(tree);

            return new Selector(iCons);
        }
    }
}
