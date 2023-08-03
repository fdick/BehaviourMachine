using System;
using BehaviourGraph.Trees;
using UnityEngine;

namespace BehaviourGraph
{
    public enum UpdateStatus
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
        public UpdateStatus OnUpdate();
        public void SetGameobject(GameObject go);
        public bool CheckCD(float duration);

        public string FriendlyName { get; set; }
    }

    public interface IHasFriendlyName
    {
        public string FriendlyName { get; }
    }
}
