using System;
using BehaviourGraph.Trees;
using UnityEngine;

namespace BehaviourGraph
{
    public enum LeafStatus
    {
        Successed,
        Failure,
        Running
    }
    public interface ILeaf
    {
        public Action<ConditionData> OnStarting { get; set; }
        public Action OnEnded { get; set; }
        public void OnAwake();
        public void OnStart(ConditionData activatedLink = null);
        public void OnEnd();
        public LeafStatus OnUpdate();
        public void SetGameobject(GameObject go);
        public bool CheckCD(float duration);

        public string FriendlyName { get; }
    }
}
