using System;

namespace BehaviourGraph
{
    public interface IForestObject
    {
        public string FriendlyName { get; set; }
        public Guid ID { get; }
    }
}