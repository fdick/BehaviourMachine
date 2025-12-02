using System;

namespace BehaviourGraph
{
    public interface IPartOfMachine
    {
        public string FriendlyName { get; set; }
        public Guid ID { get; }
    }
}