using BehaviourGraph.Conditions;
using UnityEditor;

namespace BehaviourGraph.Trees
{
    public class Transition
    {
        public Transition(ICondition executedCondition, ILeaf fromLeaf, ILeaf toLeaf)
        {
            this.ExecutedCondition = executedCondition;
            this.FromLeaf = fromLeaf;
            this.ToLeaf = toLeaf;
        }

        public GUID ID => ExecutedCondition.ID;
        public ICondition ExecutedCondition { get; }
        public ILeaf FromLeaf { get; set; }
        public ILeaf ToLeaf { get; set; }
    }
}