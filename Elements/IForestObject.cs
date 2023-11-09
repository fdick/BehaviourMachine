using UnityEditor;

namespace BehaviourGraph
{
    public interface IForestObject
    {
        public string FriendlyName { get; set; }
        public GUID ID { get; }
    }
}