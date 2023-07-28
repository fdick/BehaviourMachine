using System;

namespace BehaviourGraph.Conditions
{
    public class DefaultCondition : IConditional
    {
        public DefaultCondition()
        {
            FriendlyName = this.ToString();
        }

        public string FriendlyName { get; set; }

        public virtual LeafStatus OnUpdate()
        {
            return LeafStatus.Failure;
        }
    }
}