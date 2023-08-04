using System.Collections.Generic;

namespace BehaviourGraph.Conditions
{
    public abstract class CombinerConditions : IConditional
    {
        public string FriendlyName { get; set; }
        public List<IConditional> Conditions { get; protected set; }

        public abstract UpdateStatus OnUpdate();
    }
}