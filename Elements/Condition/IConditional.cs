using System.Collections;
using UnityEngine;

namespace BehaviourGraph
{
    public interface IConditional 
    {
        public string FriendlyName { get; set; }
        public LeafStatus OnUpdate();
    }
}